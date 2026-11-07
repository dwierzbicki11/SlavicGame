using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class SkyRenderer : IDisposable
{
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private bool _disposed;

    public void Initialize(
        ResourceFactory factory,
        ResourceLayout cameraLayout,
        OutputDescription outputDescription)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(cameraLayout);

        _shaders = ShaderLibrary.LoadPair(factory, "sky");

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            // The sky is drawn after opaque scene geometry at depth 1.0. Early
            // depth testing then skips the expensive atmosphere/cloud shader
            // for every pixel already covered by terrain, models or actors.
            DepthStencilStateDescription.DepthOnlyLessEqualRead,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                false,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription([], _shaders),
            [cameraLayout],
            outputDescription));
    }

    public void Render(CommandList commandList, ResourceSet cameraSet)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pipeline is null)
            throw new InvalidOperationException("Sky renderer has not been initialized.");

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);
        commandList.Draw(vertexCount: 3);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pipeline?.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _shaders = null;
    }
}
