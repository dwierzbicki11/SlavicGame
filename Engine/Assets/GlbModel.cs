using System.Numerics;
using System.Text;
using System.Text.Json;

namespace SlavicGame.Engine.Assets;

public sealed record MeshGeometry(Vector3[] Positions, uint[] Indices);

public readonly struct PbrVertex
{
    public const uint SizeInBytes = 32;

    public readonly Vector3 Position;
    public readonly Vector3 Normal;
    public readonly Vector2 TexCoord;

    public PbrVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
    {
        Position = position;
        Normal = normal;
        TexCoord = texCoord;
    }
}

public sealed record GlbMaterialData(
    Vector4 BaseColorFactor,
    float MetallicFactor,
    float RoughnessFactor,
    byte[]? BaseColorImage,
    byte[]? NormalImage,
    byte[]? MetallicRoughnessImage);

public sealed record GlbDrawRange(uint IndexStart, uint IndexCount, int MaterialIndex);

public sealed record PbrMeshGeometry(
    PbrVertex[] Vertices,
    uint[] Indices,
    GlbDrawRange[] DrawRanges,
    GlbMaterialData[] Materials);

public sealed class GlbModel
{
    private const uint GlbMagic = 0x46546C67;
    private const uint JsonChunk = 0x4E4F534A;
    private const uint BinChunk = 0x004E4942;

    private readonly Primitive[][] _meshes;
    private readonly Node[] _nodes;
    private readonly int[] _sceneRoots;
    private readonly Dictionary<string, AnimationClip> _animations;
    private readonly GlbMaterialData[] _materials;

    public IReadOnlyCollection<string> AnimationNames => _animations.Keys;

    public float AnimationDuration(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _animations.TryGetValue(name, out var clip)
            ? clip.Duration
            : throw new KeyNotFoundException($"Animation '{name}' is not present in this GLB.");
    }
    public IReadOnlyList<GlbMaterialData> Materials => _materials;
    public bool HasAuthoredNormals =>
        _meshes.SelectMany(mesh => mesh).All(primitive => primitive.Normals is { Length: > 0 });
    public bool HasTextureCoordinates =>
        _meshes.SelectMany(mesh => mesh).All(primitive => primitive.TexCoords is { Length: > 0 });

    private GlbModel(
        Primitive[][] meshes,
        Node[] nodes,
        int[] sceneRoots,
        Dictionary<string, AnimationClip> animations,
        GlbMaterialData[] materials)
    {
        _meshes = meshes;
        _nodes = nodes;
        _sceneRoots = sceneRoots;
        _animations = animations;
        _materials = materials;
    }

    public static GlbModel Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

        if (reader.ReadUInt32() != GlbMagic)
            throw new InvalidDataException($"'{path}' is not a GLB file.");
        if (reader.ReadUInt32() != 2)
            throw new InvalidDataException($"'{path}' uses an unsupported glTF version.");
        var declaredLength = reader.ReadUInt32();
        if (declaredLength != stream.Length)
            throw new InvalidDataException($"'{path}' has an invalid GLB length.");

        byte[]? jsonBytes = null;
        byte[]? binary = null;
        while (stream.Position < stream.Length)
        {
            var length = reader.ReadUInt32();
            var type = reader.ReadUInt32();
            var data = reader.ReadBytes(checked((int)length));
            if (data.Length != length)
                throw new EndOfStreamException($"Unexpected end of GLB chunk in '{path}'.");
            if (type == JsonChunk) jsonBytes = data;
            else if (type == BinChunk) binary = data;
        }

        if (jsonBytes is null || binary is null)
            throw new InvalidDataException($"'{path}' must contain JSON and BIN chunks.");

        using var document = JsonDocument.Parse(jsonBytes);
        var root = document.RootElement;

        var views = ParseBufferViews(root);
        var accessors = ParseAccessors(root);
        var images = ParseImages(root, binary, views, Path.GetDirectoryName(path) ?? ".");
        var textureSources = ParseTextureSources(root);
        var materials = ParseMaterials(root, images, textureSources);
        var meshes = ParseMeshes(root, binary, views, accessors);
        var nodes = ParseNodes(root);
        var roots = ParseSceneRoots(root, nodes);
        var animations = ParseAnimations(root, binary, views, accessors);

        return new GlbModel(meshes, nodes, roots, animations, materials);
    }

    public MeshGeometry BuildMesh(
        Matrix4x4 instanceTransform,
        string? animationName = null,
        float animationTimeSeconds = 0f,
        bool sourceIsZUp = true)
    {
        var pbr = BuildPbrMesh(instanceTransform, animationName, animationTimeSeconds, sourceIsZUp);
        return new MeshGeometry(pbr.Vertices.Select(vertex => vertex.Position).ToArray(), pbr.Indices);
    }

    public PbrMeshGeometry BuildPbrMesh(
        Matrix4x4 instanceTransform,
        string? animationName = null,
        float animationTimeSeconds = 0f,
        bool sourceIsZUp = true)
    {
        var translations = _nodes.Select(node => node.Translation).ToArray();
        var rotations = _nodes.Select(node => node.Rotation).ToArray();
        var scales = _nodes.Select(node => node.Scale).ToArray();

        if (!string.IsNullOrWhiteSpace(animationName))
        {
            if (!_animations.TryGetValue(animationName, out var clip))
                throw new KeyNotFoundException($"Animation '{animationName}' is not present in this GLB.");
            ApplyAnimation(clip, animationTimeSeconds, translations, rotations, scales);
        }

        var vertices = new List<PbrVertex>();
        var indices = new List<uint>();
        var ranges = new List<GlbDrawRange>();
        var sourceToEngine = sourceIsZUp
            ? Matrix4x4.CreateRotationX(-MathF.PI / 2f)
            : Matrix4x4.Identity;

        foreach (var root in _sceneRoots)
            AppendNode(root, Matrix4x4.Identity);

        return new PbrMeshGeometry(
            vertices.ToArray(),
            indices.ToArray(),
            ranges.ToArray(),
            _materials);

        void AppendNode(int nodeIndex, Matrix4x4 parent)
        {
            var node = _nodes[nodeIndex];
            var local = node.Matrix ?? (
                Matrix4x4.CreateScale(scales[nodeIndex]) *
                Matrix4x4.CreateFromQuaternion(rotations[nodeIndex]) *
                Matrix4x4.CreateTranslation(translations[nodeIndex]));
            var world = local * parent;

            if (node.Mesh is int meshIndex)
            {
                var final = world * sourceToEngine * instanceTransform;
                if (!Matrix4x4.Invert(final, out var inverse))
                    inverse = Matrix4x4.Identity;
                var normalMatrix = Matrix4x4.Transpose(inverse);

                foreach (var primitive in _meshes[meshIndex])
                {
                    var startVertex = checked((uint)vertices.Count);
                    var startIndex = checked((uint)indices.Count);
                    var normals = primitive.Normals ?? ComputeNormals(primitive.Positions, primitive.Indices);
                    var uvs = primitive.TexCoords ?? new Vector2[primitive.Positions.Length];

                    for (var i = 0; i < primitive.Positions.Length; i++)
                    {
                        var position = Vector3.Transform(primitive.Positions[i], final);
                        var normal = Vector3.TransformNormal(normals[i], normalMatrix);
                        if (normal.LengthSquared() < 0.000001f)
                            normal = Vector3.UnitY;
                        else
                            normal = Vector3.Normalize(normal);

                        vertices.Add(new PbrVertex(position, normal, uvs[i]));
                    }

                    foreach (var index in primitive.Indices)
                        indices.Add(startVertex + index);

                    ranges.Add(new GlbDrawRange(
                        startIndex,
                        checked((uint)primitive.Indices.Length),
                        Math.Clamp(primitive.MaterialIndex, 0, _materials.Length - 1)));
                }
            }

            foreach (var child in node.Children)
                AppendNode(child, world);
        }
    }

    private static Vector3[] ComputeNormals(Vector3[] positions, uint[] indices)
    {
        var normals = new Vector3[positions.Length];
        for (var i = 0; i + 2 < indices.Length; i += 3)
        {
            var ia = checked((int)indices[i]);
            var ib = checked((int)indices[i + 1]);
            var ic = checked((int)indices[i + 2]);
            var edgeA = positions[ib] - positions[ia];
            var edgeB = positions[ic] - positions[ia];
            var face = Vector3.Cross(edgeA, edgeB);
            if (face.LengthSquared() < 0.0000001f)
                continue;
            normals[ia] += face;
            normals[ib] += face;
            normals[ic] += face;
        }

        for (var i = 0; i < normals.Length; i++)
            normals[i] = normals[i].LengthSquared() < 0.000001f
                ? Vector3.UnitZ
                : Vector3.Normalize(normals[i]);

        return normals;
    }

    private static void ApplyAnimation(
        AnimationClip clip,
        float timeSeconds,
        Vector3[] translations,
        Quaternion[] rotations,
        Vector3[] scales)
    {
        var time = clip.Duration <= 0f
            ? 0f
            : MathF.Max(0f, timeSeconds) % clip.Duration;

        foreach (var channel in clip.Channels)
        {
            if (channel.Node < 0 || channel.Node >= translations.Length)
                continue;

            switch (channel.Path)
            {
                case "translation":
                    translations[channel.Node] = SampleVector3(channel.Sampler, time);
                    break;
                case "scale":
                    scales[channel.Node] = SampleVector3(channel.Sampler, time);
                    break;
                case "rotation":
                    rotations[channel.Node] = SampleQuaternion(channel.Sampler, time);
                    break;
            }
        }
    }

    private static Vector3 SampleVector3(AnimationSampler sampler, float time)
    {
        var (a, b, amount) = FindSample(sampler.Times, time);
        var va = ReadVector3(sampler.Values, a);
        if (sampler.Interpolation == "STEP" || a == b) return va;
        return Vector3.Lerp(va, ReadVector3(sampler.Values, b), amount);
    }

    private static Quaternion SampleQuaternion(AnimationSampler sampler, float time)
    {
        var (a, b, amount) = FindSample(sampler.Times, time);
        var qa = Quaternion.Normalize(ReadQuaternion(sampler.Values, a));
        if (sampler.Interpolation == "STEP" || a == b) return qa;
        return Quaternion.Normalize(Quaternion.Slerp(
            qa,
            Quaternion.Normalize(ReadQuaternion(sampler.Values, b)),
            amount));
    }

    private static (int A, int B, float Amount) FindSample(float[] times, float time)
    {
        if (times.Length == 0) return (0, 0, 0f);
        if (time <= times[0]) return (0, 0, 0f);
        for (var i = 0; i < times.Length - 1; i++)
        {
            if (time > times[i + 1]) continue;
            var span = MathF.Max(0.000001f, times[i + 1] - times[i]);
            return (i, i + 1, Math.Clamp((time - times[i]) / span, 0f, 1f));
        }
        return (times.Length - 1, times.Length - 1, 0f);
    }

    private static Vector3 ReadVector3(float[] values, int index)
    {
        var offset = index * 3;
        return new Vector3(values[offset], values[offset + 1], values[offset + 2]);
    }

    private static Quaternion ReadQuaternion(float[] values, int index)
    {
        var offset = index * 4;
        return new Quaternion(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
    }

    private static BufferView[] ParseBufferViews(JsonElement root) =>
        root.GetProperty("bufferViews").EnumerateArray()
            .Select(view => new BufferView(
                view.TryGetProperty("byteOffset", out var offset) ? offset.GetInt32() : 0,
                view.GetProperty("byteLength").GetInt32(),
                view.TryGetProperty("byteStride", out var stride) ? stride.GetInt32() : null))
            .ToArray();

    private static Accessor[] ParseAccessors(JsonElement root) =>
        root.GetProperty("accessors").EnumerateArray()
            .Select(accessor => new Accessor(
                accessor.GetProperty("bufferView").GetInt32(),
                accessor.TryGetProperty("byteOffset", out var offset) ? offset.GetInt32() : 0,
                accessor.GetProperty("componentType").GetInt32(),
                accessor.GetProperty("count").GetInt32(),
                accessor.GetProperty("type").GetString() ?? throw new InvalidDataException("Accessor type is missing.")))
            .ToArray();

    private static byte[][] ParseImages(
        JsonElement root,
        byte[] binary,
        BufferView[] views,
        string baseDirectory)
    {
        if (!root.TryGetProperty("images", out var imagesElement))
            return [];

        var images = new List<byte[]>();
        foreach (var image in imagesElement.EnumerateArray())
        {
            if (image.TryGetProperty("bufferView", out var viewElement))
            {
                var view = views[viewElement.GetInt32()];
                images.Add(binary.AsSpan(view.Offset, view.Length).ToArray());
                continue;
            }

            if (!image.TryGetProperty("uri", out var uriElement))
                throw new NotSupportedException("GLB image must use bufferView or uri.");

            var uri = uriElement.GetString() ?? string.Empty;
            if (uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = uri.IndexOf(',');
                if (comma < 0) throw new InvalidDataException("Invalid data URI image.");
                images.Add(Convert.FromBase64String(uri[(comma + 1)..]));
                continue;
            }

            var imagePath = Path.GetFullPath(Path.Combine(baseDirectory, Uri.UnescapeDataString(uri)));
            images.Add(File.ReadAllBytes(imagePath));
        }

        return images.ToArray();
    }

    private static int[] ParseTextureSources(JsonElement root)
    {
        if (!root.TryGetProperty("textures", out var texturesElement))
            return [];

        return texturesElement.EnumerateArray()
            .Select(texture => texture.GetProperty("source").GetInt32())
            .ToArray();
    }

    private static GlbMaterialData[] ParseMaterials(
        JsonElement root,
        byte[][] images,
        int[] textureSources)
    {
        var result = new List<GlbMaterialData>();
        if (root.TryGetProperty("materials", out var materialsElement))
        {
            foreach (var material in materialsElement.EnumerateArray())
            {
                var pbr = material.TryGetProperty("pbrMetallicRoughness", out var pbrElement)
                    ? pbrElement
                    : default;

                var baseColorFactor = pbr.ValueKind != JsonValueKind.Undefined &&
                                      pbr.TryGetProperty("baseColorFactor", out var baseFactor)
                    ? new Vector4(
                        baseFactor[0].GetSingle(),
                        baseFactor[1].GetSingle(),
                        baseFactor[2].GetSingle(),
                        baseFactor[3].GetSingle())
                    : Vector4.One;

                var metallic = pbr.ValueKind != JsonValueKind.Undefined &&
                               pbr.TryGetProperty("metallicFactor", out var metallicElement)
                    ? metallicElement.GetSingle()
                    : 1f;

                var roughness = pbr.ValueKind != JsonValueKind.Undefined &&
                                pbr.TryGetProperty("roughnessFactor", out var roughnessElement)
                    ? roughnessElement.GetSingle()
                    : 1f;

                var baseColorImage = pbr.ValueKind != JsonValueKind.Undefined
                    ? ResolveTextureImage(pbr, "baseColorTexture", textureSources, images)
                    : null;
                var metallicRoughnessImage = pbr.ValueKind != JsonValueKind.Undefined
                    ? ResolveTextureImage(pbr, "metallicRoughnessTexture", textureSources, images)
                    : null;
                var normalImage = ResolveTextureImage(material, "normalTexture", textureSources, images);

                result.Add(new GlbMaterialData(
                    baseColorFactor,
                    metallic,
                    roughness,
                    baseColorImage,
                    normalImage,
                    metallicRoughnessImage));
            }
        }

        if (result.Count == 0)
        {
            result.Add(new GlbMaterialData(
                Vector4.One,
                0f,
                1f,
                null,
                null,
                null));
        }

        return result.ToArray();
    }

    private static byte[]? ResolveTextureImage(
        JsonElement owner,
        string property,
        int[] textureSources,
        byte[][] images)
    {
        if (!owner.TryGetProperty(property, out var textureInfo))
            return null;

        var textureIndex = textureInfo.GetProperty("index").GetInt32();
        if ((uint)textureIndex >= (uint)textureSources.Length)
            throw new InvalidDataException($"Texture index {textureIndex} is out of range.");

        var imageIndex = textureSources[textureIndex];
        if ((uint)imageIndex >= (uint)images.Length)
            throw new InvalidDataException($"Texture source {imageIndex} is out of range.");

        return images[imageIndex];
    }

    private static Primitive[][] ParseMeshes(
        JsonElement root,
        byte[] binary,
        BufferView[] views,
        Accessor[] accessors)
    {
        if (!root.TryGetProperty("meshes", out var meshesElement))
            return [];

        return meshesElement.EnumerateArray()
            .Select(mesh => mesh.GetProperty("primitives").EnumerateArray()
                .Select(primitive =>
                {
                    var attributes = primitive.GetProperty("attributes");
                    var positionAccessor = attributes.GetProperty("POSITION").GetInt32();
                    var positions = ReadVector3Accessor(binary, views, accessors[positionAccessor], "POSITION");

                    Vector3[]? normals = null;
                    if (attributes.TryGetProperty("NORMAL", out var normalElement))
                        normals = ReadVector3Accessor(binary, views, accessors[normalElement.GetInt32()], "NORMAL");

                    Vector2[]? texCoords = null;
                    if (attributes.TryGetProperty("TEXCOORD_0", out var texCoordElement))
                        texCoords = ReadVector2Accessor(binary, views, accessors[texCoordElement.GetInt32()]);

                    uint[] indices;
                    if (primitive.TryGetProperty("indices", out var indexElement))
                        indices = ReadIndexAccessor(binary, views, accessors[indexElement.GetInt32()]);
                    else
                        indices = Enumerable.Range(0, positions.Length).Select(value => (uint)value).ToArray();

                    var materialIndex = primitive.TryGetProperty("material", out var materialElement)
                        ? materialElement.GetInt32()
                        : 0;

                    return new Primitive(positions, normals, texCoords, indices, materialIndex);
                }).ToArray())
            .ToArray();
    }

    private static Node[] ParseNodes(JsonElement root)
    {
        if (!root.TryGetProperty("nodes", out var nodesElement))
            return [];

        return nodesElement.EnumerateArray()
            .Select(node =>
            {
                var translation = node.TryGetProperty("translation", out var t)
                    ? ReadVector3Element(t)
                    : Vector3.Zero;
                var rotation = node.TryGetProperty("rotation", out var r)
                    ? Quaternion.Normalize(new Quaternion(
                        r[0].GetSingle(), r[1].GetSingle(), r[2].GetSingle(), r[3].GetSingle()))
                    : Quaternion.Identity;
                var scale = node.TryGetProperty("scale", out var s)
                    ? ReadVector3Element(s)
                    : Vector3.One;
                Matrix4x4? matrix = node.TryGetProperty("matrix", out var m)
                    ? ReadMatrix(m)
                    : null;
                var children = node.TryGetProperty("children", out var c)
                    ? c.EnumerateArray().Select(value => value.GetInt32()).ToArray()
                    : [];
                int? mesh = node.TryGetProperty("mesh", out var meshElement)
                    ? meshElement.GetInt32()
                    : null;
                return new Node(mesh, children, translation, rotation, scale, matrix);
            }).ToArray();
    }

    private static int[] ParseSceneRoots(JsonElement root, Node[] nodes)
    {
        if (root.TryGetProperty("scenes", out var scenes) && scenes.GetArrayLength() > 0)
        {
            var sceneIndex = root.TryGetProperty("scene", out var scene) ? scene.GetInt32() : 0;
            var selected = scenes[sceneIndex];
            if (selected.TryGetProperty("nodes", out var roots))
                return roots.EnumerateArray().Select(value => value.GetInt32()).ToArray();
        }

        var children = new HashSet<int>(nodes.SelectMany(node => node.Children));
        return Enumerable.Range(0, nodes.Length).Where(index => !children.Contains(index)).ToArray();
    }

    private static Dictionary<string, AnimationClip> ParseAnimations(
        JsonElement root,
        byte[] binary,
        BufferView[] views,
        Accessor[] accessors)
    {
        var result = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
        if (!root.TryGetProperty("animations", out var animations))
            return result;

        var animationNumber = 0;
        foreach (var animation in animations.EnumerateArray())
        {
            var samplers = animation.GetProperty("samplers").EnumerateArray()
                .Select(sampler =>
                {
                    var input = ReadFloatAccessor(binary, views, accessors[sampler.GetProperty("input").GetInt32()]);
                    var outputAccessor = accessors[sampler.GetProperty("output").GetInt32()];
                    var output = ReadFloatAccessor(binary, views, outputAccessor);
                    var interpolation = sampler.TryGetProperty("interpolation", out var interpolationElement)
                        ? interpolationElement.GetString() ?? "LINEAR"
                        : "LINEAR";
                    if (interpolation is not ("LINEAR" or "STEP"))
                        throw new NotSupportedException($"GLB interpolation '{interpolation}' is not supported yet.");
                    return new AnimationSampler(input, output, interpolation);
                }).ToArray();

            var channels = animation.GetProperty("channels").EnumerateArray()
                .Select(channel =>
                {
                    var sampler = samplers[channel.GetProperty("sampler").GetInt32()];
                    var target = channel.GetProperty("target");
                    return new AnimationChannel(
                        target.GetProperty("node").GetInt32(),
                        target.GetProperty("path").GetString() ?? string.Empty,
                        sampler);
                }).ToArray();

            var duration = samplers.Length == 0
                ? 0f
                : samplers.Max(sampler => sampler.Times.Length == 0 ? 0f : sampler.Times[^1]);
            var name = animation.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString()
                : null;
            name = string.IsNullOrWhiteSpace(name) ? $"animation-{animationNumber}" : name;
            result.Add(name, new AnimationClip(channels, duration));
            animationNumber++;
        }

        return result;
    }

    private static Vector3[] ReadVector3Accessor(
        byte[] binary,
        BufferView[] views,
        Accessor accessor,
        string semantic)
    {
        if (accessor.ComponentType != 5126 || accessor.Type != "VEC3")
            throw new NotSupportedException($"{semantic} accessors must be FLOAT VEC3.");

        var view = views[accessor.BufferView];
        var stride = view.Stride ?? 12;
        var start = view.Offset + accessor.Offset;
        var values = new Vector3[accessor.Count];
        for (var i = 0; i < values.Length; i++)
        {
            var offset = start + i * stride;
            values[i] = new Vector3(
                BitConverter.ToSingle(binary, offset),
                BitConverter.ToSingle(binary, offset + 4),
                BitConverter.ToSingle(binary, offset + 8));
        }
        return values;
    }

    private static Vector2[] ReadVector2Accessor(byte[] binary, BufferView[] views, Accessor accessor)
    {
        if (accessor.ComponentType != 5126 || accessor.Type != "VEC2")
            throw new NotSupportedException("TEXCOORD_0 accessors must be FLOAT VEC2.");

        var view = views[accessor.BufferView];
        var stride = view.Stride ?? 8;
        var start = view.Offset + accessor.Offset;
        var values = new Vector2[accessor.Count];
        for (var i = 0; i < values.Length; i++)
        {
            var offset = start + i * stride;
            values[i] = new Vector2(
                BitConverter.ToSingle(binary, offset),
                BitConverter.ToSingle(binary, offset + 4));
        }
        return values;
    }

    private static uint[] ReadIndexAccessor(byte[] binary, BufferView[] views, Accessor accessor)
    {
        if (accessor.Type != "SCALAR")
            throw new NotSupportedException("Index accessor must be SCALAR.");

        var view = views[accessor.BufferView];
        var componentSize = accessor.ComponentType switch
        {
            5121 => 1,
            5123 => 2,
            5125 => 4,
            _ => throw new NotSupportedException($"Index component type {accessor.ComponentType} is not supported.")
        };
        var stride = view.Stride ?? componentSize;
        var start = view.Offset + accessor.Offset;
        var indices = new uint[accessor.Count];
        for (var i = 0; i < indices.Length; i++)
        {
            var offset = start + i * stride;
            indices[i] = accessor.ComponentType switch
            {
                5121 => binary[offset],
                5123 => BitConverter.ToUInt16(binary, offset),
                5125 => BitConverter.ToUInt32(binary, offset),
                _ => 0
            };
        }
        return indices;
    }

    private static float[] ReadFloatAccessor(byte[] binary, BufferView[] views, Accessor accessor)
    {
        if (accessor.ComponentType != 5126)
            throw new NotSupportedException("Animation accessors must use FLOAT components.");

        var view = views[accessor.BufferView];
        var components = ComponentCount(accessor.Type);
        var packedSize = components * 4;
        var stride = view.Stride ?? packedSize;
        var start = view.Offset + accessor.Offset;
        var values = new float[checked(accessor.Count * components)];
        for (var i = 0; i < accessor.Count; i++)
        {
            var baseOffset = start + i * stride;
            for (var component = 0; component < components; component++)
                values[i * components + component] = BitConverter.ToSingle(binary, baseOffset + component * 4);
        }
        return values;
    }

    private static int ComponentCount(string type) => type switch
    {
        "SCALAR" => 1,
        "VEC2" => 2,
        "VEC3" => 3,
        "VEC4" => 4,
        _ => throw new NotSupportedException($"Accessor type '{type}' is not supported.")
    };

    private static Vector3 ReadVector3Element(JsonElement element) =>
        new(element[0].GetSingle(), element[1].GetSingle(), element[2].GetSingle());

    private static Matrix4x4 ReadMatrix(JsonElement element)
    {
        var a = element.EnumerateArray().Select(value => value.GetSingle()).ToArray();
        if (a.Length != 16) throw new InvalidDataException("glTF node matrix must contain 16 values.");

        return new Matrix4x4(
            a[0], a[1], a[2], a[3],
            a[4], a[5], a[6], a[7],
            a[8], a[9], a[10], a[11],
            a[12], a[13], a[14], a[15]);
    }

    private sealed record Primitive(
        Vector3[] Positions,
        Vector3[]? Normals,
        Vector2[]? TexCoords,
        uint[] Indices,
        int MaterialIndex);
    private sealed record BufferView(int Offset, int Length, int? Stride);
    private sealed record Accessor(int BufferView, int Offset, int ComponentType, int Count, string Type);
    private sealed record Node(
        int? Mesh,
        int[] Children,
        Vector3 Translation,
        Quaternion Rotation,
        Vector3 Scale,
        Matrix4x4? Matrix);
    private sealed record AnimationSampler(float[] Times, float[] Values, string Interpolation);
    private sealed record AnimationChannel(int Node, string Path, AnimationSampler Sampler);
    private sealed record AnimationClip(AnimationChannel[] Channels, float Duration);
}
