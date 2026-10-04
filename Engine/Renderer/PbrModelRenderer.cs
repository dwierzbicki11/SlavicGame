using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.ImageSharp;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class PbrModelRenderer : IDisposable
{
    private const float ChunkSize = 192f;
    private readonly List<Renderable> _renderables = [];
    private readonly List<Texture> _ownedTextures = [];
    private readonly List<TextureView> _ownedTextureViews = [];
    private readonly List<DeviceBuffer> _ownedMaterialBuffers = [];
    private readonly List<ResourceSet> _ownedMaterialSets = [];
    private readonly Sampler?[] _qualitySamplers = new Sampler?[4];

    private GraphicsDevice? _graphicsDevice;
    private Pipeline? _pipeline;
    private ResourceLayout? _materialLayout;
    private Shader[]? _shaders;
    private Texture? _whiteTexture;
    private Texture? _flatNormalTexture;
    private Texture? _defaultMrTexture;
    private TextureView? _whiteView;
    private TextureView? _flatNormalView;
    private TextureView? _defaultMrView;
    private bool _disposed;
    private TextureQuality _textureQuality = TextureQuality.High;
    private int _instanceCount;
    private int _uniqueAssetCount;

    public int RenderableCount => _renderables.Count;
    public int InstanceCount => _instanceCount;
    public int UniqueAssetCount => _uniqueAssetCount;
    public int DrawCallCount => _renderables.Sum(renderable => renderable.Lod0.Draws.Length);

    public void Initialize(
        GraphicsDevice graphicsDevice,
        ResourceLayout cameraLayout,
        ResourceLayout shadowLayout,
        OutputDescription outputDescription,
        WorldState world,
        string assetsRoot,
        TextureQuality textureQuality)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(shadowLayout);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetsRoot);
        _textureQuality = textureQuality;

        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;

        _materialLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "Material", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "BaseColorTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "NormalTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "MetallicRoughnessTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "MaterialSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        _shaders = ShaderLibrary.LoadPair(factory, "pbr");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(new[] { vertexLayout }, _shaders),
            new[] { cameraLayout, _materialLayout, shadowLayout },
            outputDescription));

        CreateFallbackTextures(graphicsDevice, factory);

        for (var qualityIndex = 0; qualityIndex < _qualitySamplers.Length; qualityIndex++)
        {
            _qualitySamplers[qualityIndex] = TextureQualityResources.CreateSampler(
                graphicsDevice,
                factory,
                (TextureQuality)qualityIndex);
        }

        _instanceCount = world.Models.Count;
        _uniqueAssetCount = world.Models
            .Select(instance => instance.AssetPath)
            .Distinct(StringComparer.Ordinal)
            .Count();

        foreach (var assetGroup in world.Models.GroupBy(
                     instance => instance.AssetPath,
                     StringComparer.Ordinal))
        {
            var relative = assetGroup.Key.Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(assetsRoot, relative);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Required PBR world asset '{assetGroup.Key}' was not found.",
                    path);

            var model = GlbModel.Load(path);
            var lod1Path = ResolveLodPath(assetsRoot, assetGroup.Key, "lod1");
            var lod2Path = ResolveLodPath(assetsRoot, assetGroup.Key, "lod2");
            var lod1Model = File.Exists(lod1Path) ? GlbModel.Load(lod1Path) : null;
            var lod2Model = File.Exists(lod2Path) ? GlbModel.Load(lod2Path) : null;

            var materialSets = new ResourceSet[model.Materials.Count][];
            for (var materialIndex = 0; materialIndex < model.Materials.Count; materialIndex++)
                materialSets[materialIndex] = CreateMaterialSets(
                    graphicsDevice,
                    factory,
                    model.Materials[materialIndex],
                    assetGroup.Key,
                    textureQuality);

            foreach (var chunkGroup in assetGroup.GroupBy(
                         instance => GetChunkKey(instance.Position)))
            {
                var instances = chunkGroup.ToArray();

                var lod0 = BuildLodGeometry(
                    graphicsDevice,
                    factory,
                    model,
                    materialSets,
                    instances);
                var lod1 = lod1Model is null
                    ? null
                    : BuildLodGeometry(
                        graphicsDevice,
                        factory,
                        lod1Model,
                        materialSets,
                        instances);
                var lod2 = lod2Model is null
                    ? null
                    : BuildLodGeometry(
                        graphicsDevice,
                        factory,
                        lod2Model,
                        materialSets,
                        instances);

                var center = new Vector3(
                    instances.Average(instance => instance.Position.X),
                    instances.Average(instance => instance.Position.Y),
                    instances.Average(instance => instance.Position.Z));
                var radius = instances.Max(instance =>
                    Vector2.Distance(
                        new Vector2(instance.Position.X, instance.Position.Z),
                        new Vector2(center.X, center.Z))) + 40f;

                _renderables.Add(new Renderable(
                    $"{assetGroup.Key}@{chunkGroup.Key.X},{chunkGroup.Key.Z}",
                    ClassifyAsset(assetGroup.Key),
                    center,
                    radius,
                    lod0,
                    lod1,
                    lod2));
            }
        }

        EngineLog.Info(
            $"PBR model renderer initialized: {_instanceCount} instances, " +
            $"{_uniqueAssetCount} unique assets, {_renderables.Count} spatial batches, " +
            $"{DrawCallCount} LOD0 material draws; LOD1/LOD2 enabled where assets exist.");
    }

    public void Render(
        CommandList commandList,
        ResourceSet cameraSet,
        ResourceSet shadowSet,
        Vector3 cameraPosition,
        CameraFrustum frustum,
        float renderDistance,
        float vegetationDistance,
        float groundClutterDistance,
        ModelLodQuality lodQuality,
        FarVegetationMode farVegetationMode)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pipeline is null)
            throw new InvalidOperationException("PBR model renderer has not been initialized.");

        renderDistance = Math.Max(0f, renderDistance);
        vegetationDistance = Math.Max(0f, vegetationDistance);
        groundClutterDistance = Math.Max(0f, groundClutterDistance);

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);
        commandList.SetGraphicsResourceSet(2, shadowSet);

        foreach (var renderable in _renderables)
        {
            var delta = new Vector2(
                renderable.Center.X - cameraPosition.X,
                renderable.Center.Z - cameraPosition.Z);
            var categoryDistance = renderable.Kind switch
            {
                RenderableKind.GroundClutter =>
                    Math.Min(renderDistance, groundClutterDistance),
                RenderableKind.Tree or RenderableKind.Vegetation =>
                    Math.Min(renderDistance, vegetationDistance),
                _ => renderDistance
            };

            if (categoryDistance <= 0f)
                continue;

            var distanceSquared = delta.LengthSquared();
            var maxDistance = categoryDistance + renderable.Radius;
            if (distanceSquared > maxDistance * maxDistance)
                continue;
            if (!frustum.IntersectsSphere(renderable.Center, renderable.Radius))
                continue;

            var distance = MathF.Sqrt(distanceSquared);
            if (renderable.Kind == RenderableKind.Tree &&
                farVegetationMode != FarVegetationMode.FullMeshes &&
                distance >= GraphicsQualityCatalog.VegetationImpostorStart(lodQuality))
            {
                continue;
            }

            var geometry = SelectLodGeometry(
                renderable,
                distance,
                lodQuality);

            commandList.SetVertexBuffer(0, geometry.VertexBuffer);
            commandList.SetIndexBuffer(geometry.IndexBuffer, IndexFormat.UInt32);

            foreach (var draw in geometry.Draws)
            {
                commandList.SetGraphicsResourceSet(
                    1,
                    draw.MaterialSets[(int)_textureQuality]);
                commandList.DrawIndexed(
                    draw.IndexCount,
                    instanceCount: 1,
                    indexStart: draw.IndexStart,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }

    public void RenderShadow(
        CommandList commandList,
        Pipeline shadowPipeline,
        ResourceSet shadowDepthSet,
        Vector3 focusPosition,
        float shadowDistance,
        float vegetationDistance,
        float groundClutterDistance,
        ModelLodQuality lodQuality,
        FarVegetationMode farVegetationMode)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        shadowDistance = Math.Max(0f, shadowDistance);
        vegetationDistance = Math.Max(0f, vegetationDistance);
        groundClutterDistance = Math.Max(0f, groundClutterDistance);

        commandList.SetPipeline(shadowPipeline);
        commandList.SetGraphicsResourceSet(0, shadowDepthSet);

        foreach (var renderable in _renderables)
        {
            var delta = new Vector2(
                renderable.Center.X - focusPosition.X,
                renderable.Center.Z - focusPosition.Z);
            var categoryDistance = renderable.Kind switch
            {
                RenderableKind.GroundClutter =>
                    Math.Min(shadowDistance, groundClutterDistance),
                RenderableKind.Tree or RenderableKind.Vegetation =>
                    Math.Min(shadowDistance, vegetationDistance),
                _ => shadowDistance
            };

            if (categoryDistance <= 0f)
                continue;

            var distanceSquared = delta.LengthSquared();
            var maxDistance = categoryDistance + renderable.Radius;
            if (distanceSquared > maxDistance * maxDistance)
                continue;

            var shadowCasterDistance = MathF.Sqrt(distanceSquared);
            if (renderable.Kind == RenderableKind.Tree &&
                farVegetationMode != FarVegetationMode.FullMeshes &&
                shadowCasterDistance >= GraphicsQualityCatalog.VegetationImpostorStart(lodQuality))
            {
                continue;
            }

            var geometry = SelectLodGeometry(
                renderable,
                shadowCasterDistance,
                lodQuality);

            commandList.SetVertexBuffer(0, geometry.VertexBuffer);
            commandList.SetIndexBuffer(geometry.IndexBuffer, IndexFormat.UInt32);

            foreach (var draw in geometry.Draws)
            {
                commandList.DrawIndexed(
                    draw.IndexCount,
                    instanceCount: 1,
                    indexStart: draw.IndexStart,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }

    private static string ResolveLodPath(
        string assetsRoot,
        string assetPath,
        string lodFolder)
    {
        return Path.Combine(
            assetsRoot,
            "lod",
            lodFolder,
            Path.GetFileName(assetPath));
    }

    private static LodGeometry BuildLodGeometry(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        GlbModel model,
        ResourceSet[][] materialSets,
        IReadOnlyList<WorldModelInstance> instances)
    {
        var combinedVertices = new List<PbrVertex>();
        var indicesByMaterial = Enumerable
            .Range(0, materialSets.Length)
            .Select(_ => new List<uint>())
            .ToArray();

        foreach (var instance in instances)
        {
            var transform =
                Matrix4x4.CreateScale(instance.Scale) *
                Matrix4x4.CreateRotationY(instance.YawRadians) *
                Matrix4x4.CreateTranslation(instance.Position);

            var mesh = model.BuildPbrMesh(
                transform,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: instance.SourceIsZUp);

            var instanceVertices = ApplyTreeFoliageVariation(mesh, instance);
            var vertexOffset = checked((uint)combinedVertices.Count);
            combinedVertices.AddRange(instanceVertices);

            foreach (var range in mesh.DrawRanges)
            {
                var materialIndex = Math.Clamp(
                    range.MaterialIndex,
                    0,
                    indicesByMaterial.Length - 1);
                var bucket = indicesByMaterial[materialIndex];
                var rangeEnd = range.IndexStart + range.IndexCount;
                for (var sourceIndex = range.IndexStart; sourceIndex < rangeEnd; sourceIndex++)
                    bucket.Add(vertexOffset + mesh.Indices[sourceIndex]);
            }
        }

        var combinedIndices = new List<uint>();
        var draws = new List<DrawBatch>();

        for (var materialIndex = 0; materialIndex < indicesByMaterial.Length; materialIndex++)
        {
            var bucket = indicesByMaterial[materialIndex];
            if (bucket.Count == 0)
                continue;

            var indexStart = checked((uint)combinedIndices.Count);
            combinedIndices.AddRange(bucket);
            draws.Add(new DrawBatch(
                indexStart,
                checked((uint)bucket.Count),
                materialSets[materialIndex]));
        }

        var vertexArray = combinedVertices.ToArray();
        var indexArray = combinedIndices.ToArray();

        var vertexBuffer = factory.CreateBuffer(new BufferDescription(
            PbrVertex.SizeInBytes * checked((uint)vertexArray.Length),
            BufferUsage.VertexBuffer));
        var indexBuffer = factory.CreateBuffer(new BufferDescription(
            sizeof(uint) * checked((uint)indexArray.Length),
            BufferUsage.IndexBuffer));

        graphicsDevice.UpdateBuffer(vertexBuffer, 0, vertexArray);
        graphicsDevice.UpdateBuffer(indexBuffer, 0, indexArray);

        return new LodGeometry(
            vertexBuffer,
            indexBuffer,
            draws.ToArray(),
            vertexArray.Length,
            indexArray.Length);
    }

    private static PbrVertex[] ApplyTreeFoliageVariation(
        PbrMeshGeometry mesh,
        WorldModelInstance instance)
    {
        if (!FarVegetationRenderer.IsTreeAsset(instance.AssetPath) ||
            mesh.Vertices.Length == 0 ||
            mesh.DrawRanges.Length == 0)
        {
            return mesh.Vertices;
        }

        var foliageMaterials = mesh.Materials
            .Select(material =>
                material.Name.Contains("leaf", StringComparison.OrdinalIgnoreCase) ||
                material.Name.Contains("foliage", StringComparison.OrdinalIgnoreCase) ||
                material.Name.Contains("needle", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (!foliageMaterials.Any(value => value))
            return mesh.Vertices;

        var foliageVertices = new bool[mesh.Vertices.Length];
        foreach (var range in mesh.DrawRanges)
        {
            var materialIndex = Math.Clamp(
                range.MaterialIndex,
                0,
                foliageMaterials.Length - 1);
            if (!foliageMaterials[materialIndex])
                continue;

            var end = range.IndexStart + range.IndexCount;
            for (var i = range.IndexStart; i < end; i++)
            {
                var vertexIndex = mesh.Indices[i];
                if (vertexIndex < foliageVertices.Length)
                    foliageVertices[vertexIndex] = true;
            }
        }

        var minY = float.MaxValue;
        var maxY = float.MinValue;
        for (var i = 0; i < foliageVertices.Length; i++)
        {
            if (!foliageVertices[i])
                continue;

            var y = mesh.Vertices[i].Position.Y;
            minY = MathF.Min(minY, y);
            maxY = MathF.Max(maxY, y);
        }

        if (!float.IsFinite(minY) ||
            !float.IsFinite(maxY) ||
            maxY - minY < 0.01f)
        {
            return mesh.Vertices;
        }

        var seed = StableTreeHash(instance.Id);
        var widthX = 0.92f + UnitFromHash(seed ^ 0x68BC21EBu) * 0.16f;
        var widthZ = 0.92f + UnitFromHash(seed ^ 0xA53A9E37u) * 0.16f;
        var height = 0.96f + UnitFromHash(seed ^ 0x1B56C4E9u) * 0.09f;
        var asymmetry = 0.12f + UnitFromHash(seed ^ 0xD8163841u) * 0.30f;
        var phase = UnitFromHash(seed ^ 0xC6BC2796u) * MathF.Tau;
        var directionAngle = UnitFromHash(seed ^ 0x9E3779B9u) * MathF.Tau;
        var direction = new Vector2(
            MathF.Cos(directionAngle),
            MathF.Sin(directionAngle));

        var varied = mesh.Vertices.ToArray();
        var crownHeight = maxY - minY;

        for (var i = 0; i < varied.Length; i++)
        {
            if (!foliageVertices[i])
                continue;

            var vertex = varied[i];
            var position = vertex.Position;
            var relative = position - instance.Position;
            var height01 = Math.Clamp(
                (position.Y - minY) / crownHeight,
                0f,
                1f);
            var crownWeight = height01 * height01 * (3f - 2f * height01);
            var localWave = MathF.Sin(
                phase +
                relative.Y * 0.47f +
                relative.X * 0.11f -
                relative.Z * 0.09f);

            relative.X *= widthX;
            relative.Z *= widthZ;
            relative.Y *= height;

            var drift = asymmetry * crownWeight;
            relative.X += direction.X * drift + direction.Y * localWave * 0.08f * crownWeight;
            relative.Z += direction.Y * drift - direction.X * localWave * 0.08f * crownWeight;

            varied[i] = new PbrVertex(
                instance.Position + relative,
                vertex.Normal,
                vertex.TexCoord);
        }

        return varied;
    }

    private static uint StableTreeHash(string value)
    {
        const uint offset = 2166136261u;
        const uint prime = 16777619u;

        var hash = offset;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }

    private static float UnitFromHash(uint value)
    {
        value ^= value >> 16;
        value *= 0x7FEB352Du;
        value ^= value >> 15;
        value *= 0x846CA68Bu;
        value ^= value >> 16;
        return (value & 0x00FFFFFFu) / 16777215f;
    }

    private static LodGeometry SelectLodGeometry(
        Renderable renderable,
        float distance,
        ModelLodQuality quality)
    {
        var (lod1Distance, lod2Distance) =
            GraphicsQualityCatalog.ModelLodDistances(quality);

        // Vegetation tolerates a faster transition because its silhouette
        // dominates long before small branch detail does.
        if (renderable.Kind is RenderableKind.Tree or RenderableKind.Vegetation)
        {
            lod1Distance *= 0.72f;
            lod2Distance *= 0.72f;
        }
        else if (renderable.Kind == RenderableKind.GroundClutter)
        {
            lod1Distance *= 0.55f;
            lod2Distance *= 0.55f;
        }

        if (distance >= lod2Distance && renderable.Lod2 is not null)
            return renderable.Lod2;
        if (distance >= lod1Distance && renderable.Lod1 is not null)
            return renderable.Lod1;

        return renderable.Lod0;
    }

    private ResourceSet[] CreateMaterialSets(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        GlbMaterialData material,
        string assetPath,
        TextureQuality textureQuality)
    {
        if (_materialLayout is null ||
            _whiteView is null ||
            _flatNormalView is null ||
            _defaultMrView is null)
        {
            throw new InvalidOperationException("PBR material resources are not initialized.");
        }

        var materialBuffer = factory.CreateBuffer(new BufferDescription(
            MaterialUniform.SizeInBytes,
            BufferUsage.UniformBuffer));
        var windResponse = FoliageWindResponse(assetPath, material.Name);
        var uniform = new MaterialUniform(
            material.BaseColorFactor,
            new Vector4(
                Math.Clamp(material.MetallicFactor, 0f, 1f),
                Math.Clamp(material.RoughnessFactor, 0.04f, 1f),
                windResponse,
                0f));
        graphicsDevice.UpdateBuffer(materialBuffer, 0, ref uniform);
        _ownedMaterialBuffers.Add(materialBuffer);

        var baseColorView = material.BaseColorImage is { Length: > 0 }
            ? CreateTextureView(
                graphicsDevice,
                factory,
                material.BaseColorImage,
                srgb: true,
                textureQuality)
            : _whiteView;
        var normalView = material.NormalImage is { Length: > 0 }
            ? CreateTextureView(
                graphicsDevice,
                factory,
                material.NormalImage,
                srgb: false,
                textureQuality)
            : _flatNormalView;
        var mrView = material.MetallicRoughnessImage is { Length: > 0 }
            ? CreateTextureView(
                graphicsDevice,
                factory,
                material.MetallicRoughnessImage,
                srgb: false,
                textureQuality)
            : _defaultMrView;

        var sets = new ResourceSet[_qualitySamplers.Length];
        for (var qualityIndex = 0; qualityIndex < sets.Length; qualityIndex++)
        {
            var sampler = _qualitySamplers[qualityIndex]
                ?? throw new InvalidOperationException("Texture quality sampler is missing.");
            sets[qualityIndex] = factory.CreateResourceSet(new ResourceSetDescription(
                _materialLayout,
                materialBuffer,
                baseColorView,
                normalView,
                mrView,
                sampler));
            _ownedMaterialSets.Add(sets[qualityIndex]);
        }

        return sets;
    }

    private TextureView CreateTextureView(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        byte[] imageBytes,
        bool srgb,
        TextureQuality textureQuality)
    {
        using var stream = new MemoryStream(imageBytes, writable: false);
        var texture = TextureQualityResources.CreateTexture(
            graphicsDevice,
            factory,
            stream,
            srgb,
            textureQuality,
            out _,
            out _);
        var view = factory.CreateTextureView(texture);
        _ownedTextures.Add(texture);
        _ownedTextureViews.Add(view);
        return view;
    }

    public void SetTextureQuality(TextureQuality quality) =>
        _textureQuality = quality;

    private void CreateFallbackTextures(GraphicsDevice graphicsDevice, ResourceFactory factory)
    {
        _whiteTexture = CreateSolidTexture(graphicsDevice, factory, 255, 255, 255, 255, srgb: true);
        _flatNormalTexture = CreateSolidTexture(graphicsDevice, factory, 128, 128, 255, 255, srgb: false);
        // glTF metallic-roughness convention: G=roughness, B=metallic.
        _defaultMrTexture = CreateSolidTexture(graphicsDevice, factory, 0, 255, 255, 255, srgb: false);

        _whiteView = factory.CreateTextureView(_whiteTexture);
        _flatNormalView = factory.CreateTextureView(_flatNormalTexture);
        _defaultMrView = factory.CreateTextureView(_defaultMrTexture);
    }

    private static Texture CreateSolidTexture(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        byte r,
        byte g,
        byte b,
        byte a,
        bool srgb)
    {
        var format = srgb
            ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb
            : PixelFormat.R8_G8_B8_A8_UNorm;
        var texture = factory.CreateTexture(TextureDescription.Texture2D(
            1, 1, 1, 1, format, TextureUsage.Sampled));
        graphicsDevice.UpdateTexture(
            texture,
            new byte[] { r, g, b, a },
            0, 0, 0,
            1, 1, 1,
            0, 0);
        return texture;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var renderable in _renderables)
        {
            renderable.Lod0.Dispose();
            renderable.Lod1?.Dispose();
            renderable.Lod2?.Dispose();
        }
        _renderables.Clear();

        foreach (var set in _ownedMaterialSets) set.Dispose();
        foreach (var sampler in _qualitySamplers) sampler?.Dispose();
        foreach (var buffer in _ownedMaterialBuffers) buffer.Dispose();
        foreach (var view in _ownedTextureViews) view.Dispose();
        foreach (var texture in _ownedTextures) texture.Dispose();

        _whiteView?.Dispose();
        _flatNormalView?.Dispose();
        _defaultMrView?.Dispose();
        _whiteTexture?.Dispose();
        _flatNormalTexture?.Dispose();
        _defaultMrTexture?.Dispose();

        _pipeline?.Dispose();
        _materialLayout?.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders) shader.Dispose();

        _ownedMaterialSets.Clear();
        Array.Clear(_qualitySamplers);
        _ownedMaterialBuffers.Clear();
        _ownedTextureViews.Clear();
        _ownedTextures.Clear();
        _pipeline = null;
        _materialLayout = null;
        _shaders = null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct MaterialUniform
    {
        public const uint SizeInBytes = 32;

        public readonly Vector4 BaseColorFactor;
        public readonly Vector4 MaterialFactors;

        public MaterialUniform(Vector4 baseColorFactor, Vector4 materialFactors)
        {
            BaseColorFactor = baseColorFactor;
            MaterialFactors = materialFactors;
        }
    }

    private static float FoliageWindResponse(string assetPath, string materialName)
    {
        if (!FarVegetationRenderer.IsTreeAsset(assetPath))
            return 0f;

        var foliageMaterial =
            materialName.Contains("leaf", StringComparison.OrdinalIgnoreCase) ||
            materialName.Contains("foliage", StringComparison.OrdinalIgnoreCase) ||
            materialName.Contains("needle", StringComparison.OrdinalIgnoreCase);
        if (!foliageMaterial)
            return 0f;

        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        if (fileName.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase))
            return 1.15f;
        if (fileName.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase))
            return 0.92f;
        if (fileName.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase))
            return 0.58f;

        return 0.82f;
    }

    private static RenderableKind ClassifyAsset(string assetPath)
    {
        if (assetPath.Contains(
                "ground_clutter/",
                StringComparison.OrdinalIgnoreCase) ||
            WorldModelRenderPolicy.IsShortRangeProp(assetPath))
        {
            return RenderableKind.GroundClutter;
        }

        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        var vegetationPrefixes = new[]
        {
            "dab_",
            "dab_stary_",
            "brzoza_",
            "sosna_",
            "olsza_",
            "krzak_",
            "paproc_",
            "korzenie_",
            "grzyby_",
            "trzciny_",
            "pien_bagienny_"
        };

        if (FarVegetationRenderer.IsTreeAsset(assetPath))
            return RenderableKind.Tree;

        return vegetationPrefixes.Any(prefix =>
            fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            ? RenderableKind.Vegetation
            : RenderableKind.World;
    }

    private static (int X, int Z) GetChunkKey(Vector3 position) =>
        ((int)MathF.Floor(position.X / ChunkSize),
         (int)MathF.Floor(position.Z / ChunkSize));

    private enum RenderableKind
    {
        World,
        Tree,
        Vegetation,
        GroundClutter
    }

    private sealed record Renderable(
        string Id,
        RenderableKind Kind,
        Vector3 Center,
        float Radius,
        LodGeometry Lod0,
        LodGeometry? Lod1,
        LodGeometry? Lod2);

    private sealed record LodGeometry(
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        DrawBatch[] Draws,
        int VertexCount,
        int IndexCount) : IDisposable
    {
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }

    private sealed record DrawBatch(
        uint IndexStart,
        uint IndexCount,
        ResourceSet[] MaterialSets);

}
