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

    public int RenderableCount => _renderables.Count;
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

        foreach (var instance in world.Models)
        {
            var relative = instance.AssetPath.Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(assetsRoot, relative);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Required PBR world asset '{instance.Id}' was not found.", path);

            var model = GlbModel.Load(path);
            var transform =
                Matrix4x4.CreateScale(instance.Scale) *
                Matrix4x4.CreateRotationY(instance.YawRadians) *
                Matrix4x4.CreateTranslation(instance.Position);
            var mesh = model.BuildPbrMesh(
                transform,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: instance.SourceIsZUp);

            var vertexBuffer = factory.CreateBuffer(new BufferDescription(
                PbrVertex.SizeInBytes * checked((uint)mesh.Vertices.Length),
                BufferUsage.VertexBuffer));
            var indexBuffer = factory.CreateBuffer(new BufferDescription(
                sizeof(uint) * checked((uint)mesh.Indices.Length),
                BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(vertexBuffer, 0, mesh.Vertices);
            graphicsDevice.UpdateBuffer(indexBuffer, 0, mesh.Indices);

            var materialSets = new ResourceSet[mesh.Materials.Length];
            for (var materialIndex = 0; materialIndex < mesh.Materials.Length; materialIndex++)
                materialSets[materialIndex] = CreateMaterialSet(graphicsDevice, factory, mesh.Materials[materialIndex]);

            var draws = mesh.DrawRanges
                .Select(range => new DrawBatch(
                    range.IndexStart,
                    range.IndexCount,
                    materialSets[Math.Clamp(range.MaterialIndex, 0, materialSets.Length - 1)]))
                .ToArray();

            _renderables.Add(new Renderable(instance.Id, vertexBuffer, indexBuffer, draws));
        }

        EngineLog.Info(
            $"PBR model renderer initialized: {_renderables.Count} models, {DrawCallCount} material draws.");
    }

    public void Render(CommandList commandList, ResourceSet cameraSet)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pipeline is null)
            throw new InvalidOperationException("PBR model renderer has not been initialized.");

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);

        foreach (var renderable in _renderables)
        {
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

    private sealed record Renderable(
        string Id,
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        DrawBatch[] Draws);

    private sealed record DrawBatch(
        uint IndexStart,
        uint IndexCount,
        ResourceSet MaterialSet);

}
