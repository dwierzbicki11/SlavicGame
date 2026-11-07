using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class CharacterRenderer : IDisposable
{
    private Shader[]? _shaders;
    private Pipeline? _pipeline;

    public bool IsInitialized => _pipeline is not null;

    public void Initialize(
        ResourceFactory factory,
        VertexLayoutDescription vertexLayout,
        ResourceLayout cameraLayout,
        ResourceLayout shadowSampleLayout,
        OutputDescription sceneOutput)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(shadowSampleLayout);

        Dispose();

        _shaders = ShaderLibrary.LoadPair(factory, "character");
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
            new ShaderSetDescription([vertexLayout], _shaders),
            [cameraLayout, shadowSampleLayout],
            sceneOutput));
    }

    public void Render(
        CommandList commandList,
        ResourceSet cameraSet,
        ResourceSet shadowSampleSet,
        DeviceBuffer vertexBuffer,
        DeviceBuffer indexBuffer,
        uint indexCount,
        uint indexStart = 0)
    {
        if (_pipeline is null || indexCount == 0)
            return;

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);
        commandList.SetGraphicsResourceSet(1, shadowSampleSet);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed(
            indexCount,
            instanceCount: 1,
            indexStart: indexStart,
            vertexOffset: 0,
            instanceStart: 0);
    }

    public void Dispose()
    {
        _pipeline?.Dispose();
        _pipeline = null;

        if (_shaders is not null)
        {
            foreach (var shader in _shaders)
                shader.Dispose();
            _shaders = null;
        }
    }
}
