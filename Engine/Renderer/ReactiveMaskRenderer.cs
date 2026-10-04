using System.Numerics;
using SlavicGame.Engine.World;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public readonly record struct ReactiveMaskRange(
    uint IndexStart,
    uint IndexCount,
    float Strength);

/// <summary>
/// Builds the FSR reactive mask from already-generated dynamic world geometry.
/// It deliberately reuses the scene depth buffer read-only, so hidden particles,
/// water and actors do not contaminate the temporal mask.
/// </summary>
public sealed class ReactiveMaskRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private Texture? _maskTexture;
    private TextureView? _maskView;
    private Framebuffer? _framebuffer;
    private DeviceBuffer? _parameters;
    private ResourceLayout? _parameterLayout;
    private ResourceSet? _parameterSet;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private Texture? _depthTexture;
    private uint _width;
    private uint _height;
    private bool _disposed;

    public bool IsInitialized => _pipeline is not null;

    public TextureView MaskView =>
        _maskView ?? throw new InvalidOperationException(
            "Reactive-mask renderer is not initialized.");

    public void Initialize(
        GraphicsDevice graphicsDevice,
        ResourceLayout cameraLayout,
        Texture depthTexture,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(depthTexture);

        _graphicsDevice = graphicsDevice;
        _depthTexture = depthTexture;

        var factory = graphicsDevice.ResourceFactory;
        _parameters = factory.CreateBuffer(new BufferDescription(
            16,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _parameterLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "ReactiveParameters",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment)));
        _parameterSet = factory.CreateResourceSet(
            new ResourceSetDescription(
                _parameterLayout,
                _parameters));

        _shaders = ShaderLibrary.LoadPair(factory, "reactive_mask");
        RecreateTarget(width, height);

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementSemantic.Position,
                VertexElementFormat.Float3),
            new VertexElementDescription(
                "Color",
                VertexElementSemantic.Color,
                VertexElementFormat.Float3),
            new VertexElementDescription(
                "Normal",
                VertexElementSemantic.Normal,
                VertexElementFormat.Float3));

        _pipeline = factory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                new RasterizerStateDescription(
                    FaceCullMode.None,
                    PolygonFillMode.Solid,
                    FrontFace.Clockwise,
                    true,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription([vertexLayout], _shaders),
                [cameraLayout, _parameterLayout],
                _framebuffer!.OutputDescription));
    }

    public void SetDepthSource(
        Texture depthTexture,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(depthTexture);
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Reactive-mask renderer is not initialized.");

        _depthTexture = depthTexture;
        width = Math.Max(1u, width);
        height = Math.Max(1u, height);

        if (_width != width || _height != height)
        {
            _graphicsDevice.WaitForIdle();
            RecreateTarget(width, height);
            return;
        }

        RecreateFramebuffer();
    }

    public TextureView Render(
        CommandList commandList,
        ResourceSet cameraSet,
        DeviceBuffer vertexBuffer,
        DeviceBuffer indexBuffer,
        IReadOnlyList<ReactiveMaskRange> ranges)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(cameraSet);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);
        ArgumentNullException.ThrowIfNull(ranges);

        if (_framebuffer is null ||
            _pipeline is null ||
            _parameterSet is null ||
            _parameters is null)
        {
            throw new InvalidOperationException(
                "Reactive-mask renderer is not initialized.");
        }

        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);
        commandList.SetGraphicsResourceSet(1, _parameterSet);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);

        // Low-strength opaque dynamics first, highly unstable translucent/
        // emissive content last. If geometry overlaps at the same depth the
        // stronger temporal reaction wins without needing a max-blend path.
        foreach (var range in ranges
                     .Where(item => item.IndexCount > 0)
                     .OrderBy(item => item.Strength))
        {
            var strength = Math.Clamp(range.Strength, 0f, 1f);
            commandList.UpdateBuffer(
                _parameters,
                0,
                new Vector4(strength, 0f, 0f, 0f));
            commandList.DrawIndexed(
                range.IndexCount,
                1,
                range.IndexStart,
                0,
                0);
        }

        return MaskView;
    }

    private void RecreateTarget(uint width, uint height)
    {
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Reactive-mask renderer is not initialized.");

        _framebuffer?.Dispose();
        _maskView?.Dispose();
        _maskTexture?.Dispose();

        width = Math.Max(1u, width);
        height = Math.Max(1u, height);
        var factory = _graphicsDevice.ResourceFactory;
        _maskTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            1,
            1,
            PixelFormat.R8_UNorm,
            TextureUsage.RenderTarget | TextureUsage.Sampled));
        _maskView = factory.CreateTextureView(_maskTexture);
        _width = width;
        _height = height;
        RecreateFramebuffer();
    }

    private void RecreateFramebuffer()
    {
        if (_graphicsDevice is null ||
            _depthTexture is null ||
            _maskTexture is null)
        {
            return;
        }

        _framebuffer?.Dispose();
        _framebuffer = _graphicsDevice.ResourceFactory.CreateFramebuffer(
            new FramebufferDescription(
                _depthTexture,
                _maskTexture));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pipeline?.Dispose();
        _parameterSet?.Dispose();
        _parameterLayout?.Dispose();
        _parameters?.Dispose();
        _framebuffer?.Dispose();
        _maskView?.Dispose();
        _maskTexture?.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _parameterSet = null;
        _parameterLayout = null;
        _parameters = null;
        _framebuffer = null;
        _maskView = null;
        _maskTexture = null;
        _shaders = null;
        _depthTexture = null;
        _graphicsDevice = null;
    }
}
