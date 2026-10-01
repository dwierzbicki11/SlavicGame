using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class VeldridRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;
    private DeviceBuffer? _projectionBuffer;
    private DeviceBuffer? _viewBuffer;
    private ResourceLayout? _cameraLayout;
    private ResourceSet? _cameraSet;
    private Pipeline? _terrainPipeline;
    private Shader[]? _shaders;

    private readonly Camera3D _camera = new();
    private uint _indexCount;

    public GraphicsDevice GraphicsDevice =>
        _graphicsDevice ?? throw new InvalidOperationException("Renderer has not been initialized.");

    public void Initialize(GameWindow window, WorldState world, bool vsync)
    {
        if (_graphicsDevice is not null)
        {
            return;
        }

        var options = new GraphicsDeviceOptions
        {
            Debug = false,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(
            window.NativeWindow,
            options,
            GraphicsBackend.Vulkan);

        _graphicsDevice.SyncToVerticalBlank = vsync;

        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();

        TerrainMesh.Build(world.Terrain, out var vertices, out var indices);

        _vertexBuffer = factory.CreateBuffer(new BufferDescription(
            TerrainVertex.SizeInBytes * (uint)vertices.Length,
            BufferUsage.VertexBuffer));

        _indexBuffer = factory.CreateBuffer(new BufferDescription(
            sizeof(ushort) * (uint)indices.Length,
            BufferUsage.IndexBuffer));

        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);
        _indexCount = (uint)indices.Length;

        _cameraLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription(
                "View", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        _cameraSet = factory.CreateResourceSet(new ResourceSetDescription(
            _cameraLayout,
            _projectionBuffer,
            _viewBuffer));

        _shaders = factory.CreateFromSpirv(
            new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexShader),
                "main"),
            new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentShader),
                "main"));

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementSemantic.Position,
                VertexElementFormat.Float3),
            new VertexElementDescription(
                "Color",
                VertexElementSemantic.Color,
                VertexElementFormat.Float3));

        _terrainPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                new[] { vertexLayout },
                _shaders),
            new[] { _cameraLayout },
            _graphicsDevice.SwapchainFramebuffer.OutputDescription));

        EngineLog.Info($"Veldrid renderer initialized with {_graphicsDevice.BackendType}.");
        EngineLog.Info($"Graphics device: {_graphicsDevice.DeviceName}.");
        EngineLog.Info("Terrain mesh uploaded to GPU.");
    }

    public void Render(WorldTime worldTime)
    {
        if (_graphicsDevice is null ||
            _commandList is null ||
            _vertexBuffer is null ||
            _indexBuffer is null ||
            _projectionBuffer is null ||
            _viewBuffer is null ||
            _cameraSet is null ||
            _terrainPipeline is null)
        {
            throw new InvalidOperationException("Renderer has not been initialized.");
        }

        var framebuffer = _graphicsDevice.SwapchainFramebuffer;
        if (framebuffer is null)
        {
            return;
        }

        var aspect = MathF.Max(0.1f, (float)framebuffer.Width / framebuffer.Height);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(_camera.FieldOfView, aspect, _camera.NearPlane, _camera.FarPlane);
        var view = Matrix4x4.CreateLookAt(_camera.Position, _camera.Target, Vector3.UnitY);

        _commandList.Begin();
        _commandList.UpdateBuffer(_projectionBuffer, 0, projection);
        _commandList.UpdateBuffer(_viewBuffer, 0, view);
        _commandList.SetFramebuffer(framebuffer);
        _commandList.ClearColorTarget(0, GetAtmosphereColor(worldTime));
        _commandList.SetPipeline(_terrainPipeline);
        _commandList.SetGraphicsResourceSet(0, _cameraSet);
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        _commandList.DrawIndexed(_indexCount);

        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    private static RgbaFloat GetAtmosphereColor(WorldTime time)
    {
        if (time.IsNight)
        {
            return new RgbaFloat(0.012f, 0.018f, 0.035f, 1f);
        }

        var daylight = (float)Math.Clamp(
            Math.Sin((time.TimeOfDayHours - 6.0) / 14.0 * Math.PI),
            0.0,
            1.0);

        return new RgbaFloat(
            0.025f + daylight * 0.055f,
            0.045f + daylight * 0.075f,
            0.065f + daylight * 0.095f,
            1f);
    }

    public void Resize(uint width, uint height)
    {
        if (_graphicsDevice is null || width == 0 || height == 0)
        {
            return;
        }

        _graphicsDevice.ResizeMainWindow(width, height);
    }

    public void Dispose()
    {
        if (_graphicsDevice is null)
        {
            return;
        }

        _graphicsDevice.WaitForIdle();

        _terrainPipeline?.Dispose();
        _cameraSet?.Dispose();
        _cameraLayout?.Dispose();
        _projectionBuffer?.Dispose();
        _viewBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        if (_shaders is not null)
        {
            foreach (var shader in _shaders)
            {
                shader.Dispose();
            }
        }

        _commandList?.Dispose();
        _graphicsDevice.Dispose();

        _shaders = null;
        _terrainPipeline = null;
        _cameraSet = null;
        _cameraLayout = null;
        _projectionBuffer = null;
        _viewBuffer = null;
        _vertexBuffer = null;
        _indexBuffer = null;
        _commandList = null;
        _graphicsDevice = null;
    }

    private const string VertexShader = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Color;

layout(location = 0) out vec3 fsin_Color;

void main()
{
    gl_Position = Projection * View * vec4(Position, 1.0);
    fsin_Color = Color;
}";

    private const string FragmentShader = @"
#version 450

layout(location = 0) in vec3 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = vec4(fsin_Color, 1.0);
}";
}
