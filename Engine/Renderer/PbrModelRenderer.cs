using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.ImageSharp;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class PbrModelRenderer : IDisposable
{
    private const float ChunkSize = 192f;
    private const float MaxRenderDistance = 650f;
    private readonly List<Renderable> _renderables = [];
    private readonly List<Texture> _ownedTextures = [];
    private readonly List<TextureView> _ownedTextureViews = [];
    private readonly List<DeviceBuffer> _ownedMaterialBuffers = [];
    private readonly List<ResourceSet> _ownedMaterialSets = [];

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
    private int _instanceCount;
    private int _uniqueAssetCount;

    public int RenderableCount => _renderables.Count;
    public int InstanceCount => _instanceCount;
    public int UniqueAssetCount => _uniqueAssetCount;
    public int DrawCallCount => _renderables.Sum(renderable => renderable.Draws.Length);

    public void Initialize(
        GraphicsDevice graphicsDevice,
        ResourceLayout cameraLayout,
        OutputDescription outputDescription,
        WorldState world,
        string assetsRoot)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetsRoot);

        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;

        _materialLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "Material", ResourceKind.UniformBuffer, ShaderStages.Fragment),
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
            new[] { cameraLayout, _materialLayout },
            outputDescription));

        CreateFallbackTextures(graphicsDevice, factory);

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
            var materialSets = new ResourceSet[model.Materials.Count];
            for (var materialIndex = 0; materialIndex < model.Materials.Count; materialIndex++)
                materialSets[materialIndex] = CreateMaterialSet(
                    graphicsDevice,
                    factory,
                    model.Materials[materialIndex]);

            foreach (var chunkGroup in assetGroup.GroupBy(instance => GetChunkKey(instance.Position)))
            {
                var instances = chunkGroup.ToArray();
                var combinedVertices = new List<PbrVertex>();
                var indicesByMaterial = Enumerable
                    .Range(0, model.Materials.Count)
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

                    var vertexOffset = checked((uint)combinedVertices.Count);
                    combinedVertices.AddRange(mesh.Vertices);

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
                    center,
                    radius,
                    vertexBuffer,
                    indexBuffer,
                    draws.ToArray()));
            }
        }

        EngineLog.Info(
            $"PBR model renderer initialized: {_instanceCount} instances, " +
            $"{_uniqueAssetCount} unique assets, {_renderables.Count} spatial batches, " +
            $"{DrawCallCount} material draws.");
    }

    public void Render(CommandList commandList, ResourceSet cameraSet, Vector3 cameraPosition)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pipeline is null)
            throw new InvalidOperationException("PBR model renderer has not been initialized.");

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);

        foreach (var renderable in _renderables)
        {
            var delta = new Vector2(
                renderable.Center.X - cameraPosition.X,
                renderable.Center.Z - cameraPosition.Z);
            var maxDistance = MaxRenderDistance + renderable.Radius;
            if (delta.LengthSquared() > maxDistance * maxDistance)
                continue;

            commandList.SetVertexBuffer(0, renderable.VertexBuffer);
            commandList.SetIndexBuffer(renderable.IndexBuffer, IndexFormat.UInt32);

            foreach (var draw in renderable.Draws)
            {
                commandList.SetGraphicsResourceSet(1, draw.MaterialSet);
                commandList.DrawIndexed(
                    draw.IndexCount,
                    instanceCount: 1,
                    indexStart: draw.IndexStart,
                    vertexOffset: 0,
                    instanceStart: 0);
            }
        }
    }

    private ResourceSet CreateMaterialSet(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        GlbMaterialData material)
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
        var uniform = new MaterialUniform(
            material.BaseColorFactor,
            new Vector4(
                Math.Clamp(material.MetallicFactor, 0f, 1f),
                Math.Clamp(material.RoughnessFactor, 0.04f, 1f),
                0f,
                0f));
        graphicsDevice.UpdateBuffer(materialBuffer, 0, ref uniform);
        _ownedMaterialBuffers.Add(materialBuffer);

        var baseColorView = material.BaseColorImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.BaseColorImage, srgb: true)
            : _whiteView;
        var normalView = material.NormalImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.NormalImage, srgb: false)
            : _flatNormalView;
        var mrView = material.MetallicRoughnessImage is { Length: > 0 }
            ? CreateTextureView(graphicsDevice, factory, material.MetallicRoughnessImage, srgb: false)
            : _defaultMrView;

        var set = factory.CreateResourceSet(new ResourceSetDescription(
            _materialLayout,
            materialBuffer,
            baseColorView,
            normalView,
            mrView,
            graphicsDevice.Aniso4xSampler));
        _ownedMaterialSets.Add(set);
        return set;
    }

    private TextureView CreateTextureView(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        byte[] imageBytes,
        bool srgb)
    {
        using var stream = new MemoryStream(imageBytes, writable: false);
        var image = new ImageSharpTexture(stream, mipmap: true, srgb: srgb);
        var texture = image.CreateDeviceTexture(graphicsDevice, factory);
        var view = factory.CreateTextureView(texture);
        _ownedTextures.Add(texture);
        _ownedTextureViews.Add(view);
        return view;
    }

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
            renderable.VertexBuffer.Dispose();
            renderable.IndexBuffer.Dispose();
        }
        _renderables.Clear();

        foreach (var set in _ownedMaterialSets) set.Dispose();
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

    private static (int X, int Z) GetChunkKey(Vector3 position) =>
        ((int)MathF.Floor(position.X / ChunkSize),
         (int)MathF.Floor(position.Z / ChunkSize));

    private sealed record Renderable(
        string Id,
        Vector3 Center,
        float Radius,
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        DrawBatch[] Draws);

    private sealed record DrawBatch(
        uint IndexStart,
        uint IndexCount,
        ResourceSet MaterialSet);

}
