using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class VeldridRenderer : IDisposable
{
    private readonly List<HudVertex> _hudVertices = [];
    private readonly SkyRenderer _sky = new();
    private readonly PbrModelRenderer _pbrModels = new();

    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;
    private DeviceBuffer? _projectionBuffer;
    private DeviceBuffer? _viewBuffer;
    private DeviceBuffer? _atmosphereBuffer;
    private DeviceBuffer? _actorVertexBuffer;
    private DeviceBuffer? _actorIndexBuffer;
    private DeviceBuffer? _hudVertexBuffer;
    private DeviceBuffer? _hudScreenBuffer;
    private ResourceLayout? _cameraLayout;
    private ResourceSet? _cameraSet;
    private ResourceLayout? _hudLayout;
    private ResourceSet? _hudSet;
    private Pipeline? _terrainPipeline;
    private Pipeline? _hudPipeline;
    private Shader[]? _shaders;
    private Shader[]? _hudShaders;
    private GlbModel? _playerModel;
    private GlbModel? _enemyModel;

    private bool _initialized;
    private bool _disposed;
    private uint _indexCount;
    private uint _actorIndexCount;
    private uint _actorVertexCapacity;
    private uint _actorIndexCapacity;
    private uint _hudVertexCapacity;

    public GraphicsDevice GraphicsDevice =>
        _graphicsDevice ?? throw new InvalidOperationException("Renderer has not been initialized.");

    public void Initialize(GameWindow window, WorldState world, bool vsync)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) return;
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(world);
        try
        {
            InitializeResources(window, world, vsync);
            _initialized = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void InitializeResources(GameWindow window, WorldState world, bool vsync)
    {
        PresentationPolicy.Apply(vsync);

        var options = new GraphicsDeviceOptions
        {
            Debug = false,
            SwapchainDepthFormat = PixelFormat.R32_Float,
            SyncToVerticalBlank = vsync,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(
            window.NativeWindow,
            options,
            GraphicsBackend.Vulkan);

        EngineLog.Info(
            $"Vulkan presentation: requested VSync={vsync}, " +
            $"Veldrid SyncToVerticalBlank={_graphicsDevice.SyncToVerticalBlank}, " +
            $"Mesa override={Environment.GetEnvironmentVariable(PresentationPolicy.MesaPresentModeVariable) ?? "<none>"}.");

        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();

        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        TerrainMesh.Build(world.Terrain, out var vertices, out var indices);
        _playerModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "animated", "player_hunter_animated.glb"));
        _enemyModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "animated", "swamp_predator_animated.glb"));

        _vertexBuffer = factory.CreateBuffer(new BufferDescription(
            TerrainVertex.SizeInBytes * (uint)vertices.Length,
            BufferUsage.VertexBuffer));

        _indexBuffer = factory.CreateBuffer(new BufferDescription(
            sizeof(uint) * (uint)indices.Length,
            BufferUsage.IndexBuffer));

        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _atmosphereBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);
        _indexCount = (uint)indices.Length;

        _actorVertexCapacity = 64;
        _actorIndexCapacity = 128;
        _actorVertexBuffer = factory.CreateBuffer(new BufferDescription(
            TerrainVertex.SizeInBytes * _actorVertexCapacity,
            BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _actorIndexBuffer = factory.CreateBuffer(new BufferDescription(
            sizeof(uint) * _actorIndexCapacity,
            BufferUsage.IndexBuffer | BufferUsage.Dynamic));

        _cameraLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription(
                "View", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription(
                "Atmosphere", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

        _cameraSet = factory.CreateResourceSet(new ResourceSetDescription(
            _cameraLayout,
            _projectionBuffer,
            _viewBuffer,
            _atmosphereBuffer));

        _sky.Initialize(
            factory,
            _cameraLayout,
            _graphicsDevice.SwapchainFramebuffer.OutputDescription);

        _pbrModels.Initialize(
            _graphicsDevice,
            _cameraLayout,
            _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            world,
            assetsRoot);

        _shaders = ShaderLibrary.LoadPair(factory, "terrain");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3));

        _terrainPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
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
            new[] { _cameraLayout },
            _graphicsDevice.SwapchainFramebuffer.OutputDescription));

        _hudScreenBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _hudVertexCapacity = 4096;
        _hudVertexBuffer = factory.CreateBuffer(new BufferDescription(
            HudVertex.SizeInBytes * _hudVertexCapacity,
            BufferUsage.VertexBuffer));

        _hudLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "ScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        _hudSet = factory.CreateResourceSet(new ResourceSetDescription(
            _hudLayout,
            _hudScreenBuffer));

        _hudShaders = ShaderLibrary.LoadPair(factory, "hud");

        var hudVertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

        _hudPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(new[] { hudVertexLayout }, _hudShaders),
            new[] { _hudLayout },
            _graphicsDevice.SwapchainFramebuffer.OutputDescription));

        EngineLog.Info($"Veldrid renderer initialized with {_graphicsDevice.BackendType}.");
        EngineLog.Info($"Graphics device: {_graphicsDevice.DeviceName}.");
        EngineLog.Info($"Terrain uploaded to GPU; PBR world instances={_pbrModels.InstanceCount}, " +
            $"unique assets={_pbrModels.RenderableCount}, collision obstacles={world.Obstacles.Count}.");
        EngineLog.Info($"Animated actor models loaded: player clips={_playerModel.AnimationNames.Count}, enemy clips={_enemyModel.AnimationNames.Count}.");
        EngineLog.Info("HUD renderer initialized.");
    }

    public void Render(WorldState world, Camera3D camera, double fps, double animationSeconds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized || _graphicsDevice is null ||
            _commandList is null ||
            _vertexBuffer is null ||
            _indexBuffer is null ||
            _projectionBuffer is null ||
            _viewBuffer is null ||
            _atmosphereBuffer is null ||
            _actorVertexBuffer is null ||
            _actorIndexBuffer is null ||
            _playerModel is null ||
            _enemyModel is null ||
            _cameraSet is null ||
            _terrainPipeline is null ||
            _hudVertexBuffer is null ||
            _hudScreenBuffer is null ||
            _hudSet is null ||
            _hudPipeline is null)
        {
            throw new InvalidOperationException("Renderer has not been initialized.");
        }

        var framebuffer = _graphicsDevice.SwapchainFramebuffer;
        if (framebuffer is null)
        {
            return;
        }

        var width = Math.Max(1u, framebuffer.Width);
        var height = Math.Max(1u, framebuffer.Height);
        var aspect = MathF.Max(0.1f, (float)width / height);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FieldOfView, aspect, camera.NearPlane, camera.FarPlane);
        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);

        ActorModelMesh.Build(
            world,
            _playerModel,
            _enemyModel,
            animationSeconds,
            camera.Yaw,
            out var actorVertices,
            out var actorIndices);
        EnsureActorCapacity(actorVertices.Length, actorIndices.Length);
        _actorIndexCount = (uint)actorIndices.Length;

        BuildHud(
            (float)Math.Max(0, fps),
            world.Player.Health / world.Player.MaxHealth,
            world.Player.Stamina / world.Player.MaxStamina);
        if (_hudVertices.Count > _hudVertexCapacity)
        {
            _hudVertexCapacity = (uint)Math.Max(_hudVertices.Count, _hudVertexCapacity * 2);
            _graphicsDevice.WaitForIdle();
            _hudVertexBuffer.Dispose();
            _hudVertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                HudVertex.SizeInBytes * _hudVertexCapacity,
                BufferUsage.VertexBuffer));
        }

        var screenSize = new Vector4(width, height, 0, 0);
        var celestial = CelestialLighting.Evaluate(world.Time, world.Weather);
        var atmosphereColor = GetAtmosphereColor(world.Time, world.Weather, celestial);
        var fogParameters = new Vector4(
            atmosphereColor.R,
            atmosphereColor.G,
            atmosphereColor.B,
            world.Weather.FogDensity);
        var lightingParameters = new Vector4(
            celestial.SunIntensity,
            celestial.SunDirection.X,
            celestial.SunDirection.Y,
            celestial.SunDirection.Z);

        _commandList.Begin();
        _commandList.UpdateBuffer(_projectionBuffer, 0, projection);
        _commandList.UpdateBuffer(_viewBuffer, 0, view);
        _commandList.UpdateBuffer(_atmosphereBuffer, 0, fogParameters);
        _commandList.UpdateBuffer(_atmosphereBuffer, 16, lightingParameters);
        _commandList.UpdateBuffer(_hudScreenBuffer, 0, screenSize);
        if (actorVertices.Length > 0)
        {
            _commandList.UpdateBuffer(_actorVertexBuffer, 0, actorVertices);
            _commandList.UpdateBuffer(_actorIndexBuffer, 0, actorIndices);
        }
        if (_hudVertices.Count > 0)
        {
            _commandList.UpdateBuffer(_hudVertexBuffer, 0, _hudVertices.ToArray());
        }

        _commandList.SetFramebuffer(framebuffer);
        _commandList.ClearColorTarget(0, atmosphereColor);
        _commandList.ClearDepthStencil(1f);

        _sky.Render(_commandList, _cameraSet);

        _commandList.SetPipeline(_terrainPipeline);
        _commandList.SetGraphicsResourceSet(0, _cameraSet);
        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        _commandList.DrawIndexed(_indexCount);

        _pbrModels.Render(_commandList, _cameraSet);

        _commandList.SetPipeline(_terrainPipeline);
        _commandList.SetGraphicsResourceSet(0, _cameraSet);

        if (_actorIndexCount > 0)
        {
            _commandList.SetVertexBuffer(0, _actorVertexBuffer);
            _commandList.SetIndexBuffer(_actorIndexBuffer, IndexFormat.UInt32);
            _commandList.DrawIndexed(_actorIndexCount);
        }

        if (_hudVertices.Count > 0)
        {
            _commandList.SetPipeline(_hudPipeline);
            _commandList.SetGraphicsResourceSet(0, _hudSet);
            _commandList.SetVertexBuffer(0, _hudVertexBuffer);
            _commandList.Draw((uint)_hudVertices.Count);
        }

        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    private void EnsureActorCapacity(int vertexCount, int indexCount)
    {
        if (_graphicsDevice is null)
        {
            return;
        }

        if ((uint)vertexCount > _actorVertexCapacity)
        {
            _actorVertexCapacity = Math.Max((uint)vertexCount, _actorVertexCapacity * 2);
            _graphicsDevice.WaitForIdle();
            _actorVertexBuffer?.Dispose();
            _actorVertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                TerrainVertex.SizeInBytes * _actorVertexCapacity,
                BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        if ((uint)indexCount > _actorIndexCapacity)
        {
            _actorIndexCapacity = Math.Max((uint)indexCount, _actorIndexCapacity * 2);
            _graphicsDevice.WaitForIdle();
            _actorIndexBuffer?.Dispose();
            _actorIndexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                sizeof(uint) * _actorIndexCapacity,
                BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        }
    }

    private void BuildHud(float fps, float healthRatio, float staminaRatio)
    {
        _hudVertices.Clear();

        var text = $"FPS {Math.Clamp((int)MathF.Round(fps), 0, 9999)}";
        const float x = 18f;
        const float y = 18f;
        const float pixel = 5f;
        const float gap = 2f;

        var cursor = x;
        foreach (var character in text)
        {
            if (!Glyphs.TryGetValue(character, out var glyph))
            {
                cursor += 7f * pixel;
                continue;
            }

            for (var row = 0; row < 7; row++)
            for (var col = 0; col < 5; col++)
            {
                if ((glyph[row] & (1 << (4 - col))) == 0)
                {
                    continue;
                }

                AddHudQuad(cursor + col * (pixel + gap), y + row * (pixel + gap), pixel, pixel);
            }

            cursor += 5f * (pixel + gap) + 6f;
        }

        healthRatio = Math.Clamp(healthRatio, 0f, 1f);
        staminaRatio = Math.Clamp(staminaRatio, 0f, 1f);
        const float barX = 18f;
        const float barWidth = 160f;
        const float barHeight = 9f;
        AddHudQuad(barX, 78f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(barX, 78f, barWidth * healthRatio, barHeight, new Vector4(0.62f, 0.16f, 0.12f, 0.95f));
        AddHudQuad(barX, 96f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(barX, 96f, barWidth * staminaRatio, barHeight, new Vector4(0.72f, 0.58f, 0.18f, 0.95f));
    }

    private void AddHudQuad(float x, float y, float width, float height)
    {
        AddHudQuad(x, y, width, height, new Vector4(0.92f, 0.88f, 0.68f, 0.95f));
    }

    private void AddHudQuad(float x, float y, float width, float height, Vector4 color)
    {
        if (width <= 0f || height <= 0f)
        {
            return;
        }

        const float padding = 1f;
        var x0 = x - padding;
        var y0 = y - padding;
        var x1 = x + width + padding;
        var y1 = y + height + padding;

        _hudVertices.Add(new HudVertex(new Vector2(x0, y0), color));
        _hudVertices.Add(new HudVertex(new Vector2(x1, y0), color));
        _hudVertices.Add(new HudVertex(new Vector2(x1, y1), color));
        _hudVertices.Add(new HudVertex(new Vector2(x0, y0), color));
        _hudVertices.Add(new HudVertex(new Vector2(x1, y1), color));
        _hudVertices.Add(new HudVertex(new Vector2(x0, y1), color));
    }

    private static RgbaFloat GetAtmosphereColor(
        WorldTime time,
        WeatherSystem weather,
        CelestialLightState celestial)
    {
        var daylight = time.IsNight
            ? 0f
            : (float)Math.Clamp(
                Math.Sin((time.TimeOfDayHours - 6.0) / 14.0 * Math.PI),
                0.0,
                1.0);

        var baseColor = time.IsNight
            ? new Vector3(0.012f, 0.018f, 0.035f)
            : new Vector3(
                0.025f + daylight * 0.055f,
                0.045f + daylight * 0.075f,
                0.065f + daylight * 0.095f);

        var cloudColor = time.IsNight
            ? new Vector3(0.025f, 0.028f, 0.038f)
            : new Vector3(0.09f, 0.095f, 0.10f);
        var fogColor = time.IsNight
            ? new Vector3(0.035f, 0.040f, 0.047f)
            : new Vector3(0.17f, 0.18f, 0.17f);

        var cloudy = Vector3.Lerp(baseColor, cloudColor, weather.Cloudiness * 0.72f);
        var twilightTint = Vector3.Lerp(
            cloudy,
            new Vector3(0.30f, 0.105f, 0.045f),
            celestial.TwilightFactor * 0.34f * (1f - weather.Cloudiness * 0.5f));
        var fogBlend = Math.Clamp(weather.FogDensity / 0.032f, 0f, 1f) * 0.58f;
        var final = Vector3.Lerp(twilightTint, fogColor, fogBlend);
        return new RgbaFloat(final.X, final.Y, final.Z, 1f);
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
        if (_disposed) return;
        _disposed = true;
        _initialized = false;
        if (_graphicsDevice is null)
        {
            return;
        }

        _graphicsDevice.WaitForIdle();

        _sky.Dispose();
        _pbrModels.Dispose();

        _hudPipeline?.Dispose();
        _hudSet?.Dispose();
        _hudLayout?.Dispose();
        _hudVertexBuffer?.Dispose();
        _hudScreenBuffer?.Dispose();

        _terrainPipeline?.Dispose();
        _cameraSet?.Dispose();
        _cameraLayout?.Dispose();
        _projectionBuffer?.Dispose();
        _viewBuffer?.Dispose();
        _atmosphereBuffer?.Dispose();
        _actorVertexBuffer?.Dispose();
        _actorIndexBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        if (_hudShaders is not null)
        {
            foreach (var shader in _hudShaders) shader.Dispose();
        }

        if (_shaders is not null)
        {
            foreach (var shader in _shaders) shader.Dispose();
        }

        _hudShaders = null;
        _shaders = null;
        _playerModel = null;
        _enemyModel = null;
        _hudPipeline = null;
        _hudSet = null;
        _hudLayout = null;
        _hudVertexBuffer = null;
        _hudScreenBuffer = null;
        _terrainPipeline = null;
        _cameraSet = null;
        _cameraLayout = null;
        _projectionBuffer = null;
        _viewBuffer = null;
        _atmosphereBuffer = null;
        _actorVertexBuffer = null;
        _actorIndexBuffer = null;
        _vertexBuffer = null;
        _indexBuffer = null;
        _commandList?.Dispose();
        _graphicsDevice.Dispose();

        _commandList = null;
        _graphicsDevice = null;
    }

    private readonly struct HudVertex
    {
        public const uint SizeInBytes = 24;
        public readonly Vector2 Position;
        public readonly Vector4 Color;

        public HudVertex(Vector2 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }

    private static readonly Dictionary<char, int[]> Glyphs = new()
    {
        ['F'] = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b10000],
        ['P'] = [0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000],
        ['S'] = [0b01111, 0b10000, 0b10000, 0b01110, 0b00001, 0b00001, 0b11110],
        ['0'] = [0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110],
        ['1'] = [0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110],
        ['2'] = [0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111],
        ['3'] = [0b11110, 0b00001, 0b00001, 0b01110, 0b00001, 0b00001, 0b11110],
        ['4'] = [0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010],
        ['5'] = [0b11111, 0b10000, 0b10000, 0b11110, 0b00001, 0b00001, 0b11110],
        ['6'] = [0b01110, 0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110],
        ['7'] = [0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b01000],
        ['8'] = [0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110],
        ['9'] = [0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00001, 0b01110],
        [' '] = [0, 0, 0, 0, 0, 0, 0]
    };

}
