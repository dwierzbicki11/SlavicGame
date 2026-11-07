using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.Renderer.FidelityFx;
using SlavicGame.Engine.UI;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class VeldridRenderer : IDisposable
{
    private readonly List<HudVertex> _hudVertices = [];
    private readonly SkyRenderer _sky = new();
    private readonly ShadowMapRenderer _shadows = new();
    private readonly TerrainMaterialRenderer _terrain = new();
    private readonly MenuRenderer _menu = new();
    private readonly ResolutionScalerRenderer _resolutionScaler = new();
    private readonly TemporalFrameState _temporalFrame = new();
    private readonly DynamicMotionHistory _dynamicMotionHistory = new();
    private readonly MotionVectorRenderer _motionVectors = new();
    private readonly ReactiveMaskRenderer _reactiveMask = new();
    private readonly BloomRenderer _bloom = new();
    private readonly PostProcessRenderer _postProcess = new();
    private readonly PbrModelRenderer _pbrModels = new();
    private readonly FarVegetationRenderer _farVegetation = new();
    private readonly CharacterRenderer _characters = new();

    private FidelityFxUpscaler? _fsr3Upscaler;
    private FidelityFxSceneFrameGeneration? _frameGeneration;
    private FrameGenerationPresenter? _framePresenter;
    private Framebuffer? _offscreenTarget;
    private bool _ownsDevice = true;
    private ulong _sceneFrameId;
    private UpscalerMode? _previousUpscaler;
    private bool? _previousMenuVisible;
    private bool _hasPreviousCameraState;
    private Vector3 _previousCameraPosition;
    private Vector3 _previousCameraTarget;
    private float _previousCameraFov;
    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
    private DeviceBuffer? _projectionBuffer;
    private DeviceBuffer? _viewBuffer;
    private DeviceBuffer? _atmosphereBuffer;
    private DeviceBuffer? _actorVertexBuffer;
    private DeviceBuffer? _actorIndexBuffer;
    private DeviceBuffer? _dynamicMotionVertexBuffer;
    private DeviceBuffer? _hudVertexBuffer;
    private DeviceBuffer? _hudScreenBuffer;
    private ResourceLayout? _cameraLayout;
    private ResourceSet? _cameraSet;
    private ResourceLayout? _hudLayout;
    private ResourceSet? _hudSet;
    private Pipeline? _actorPipeline;
    private Pipeline? _hudPipeline;
    private Shader[]? _actorShaders;
    private Shader[]? _hudShaders;
    private GlbModel? _playerModel;
    private GlbModel? _enemyModel;
    private GlbModel? _bowModel;
    private GlbModel? _arrowModel;
    private readonly Dictionary<string, GlbModel> _npcModels =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, GlbModel> _wildlifeModels =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, GlbModel> _bestiaryModels =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, GlbModel> _worldItemModels =
        new(StringComparer.Ordinal);

    private bool _initialized;
    private bool _disposed;
    private bool _temporalInputsEnabled;
    private bool _fsr3Requested;
    private bool _fsr3ForcedByEnvironment;
    private bool _fsr3DisabledAfterError;
    private uint _actorIndexCount;
    private uint _actorVertexCapacity;
    private uint _actorIndexCapacity;
    private readonly List<TerrainVertex> _actorVertices = new(8192);
    private readonly List<uint> _actorIndices = new(24576);
    private uint _dynamicMotionVertexCapacity;
    private DynamicMotionVertex[] _dynamicMotionUpload = [];
    private uint _hudVertexCapacity;

    public GraphicsDevice GraphicsDevice =>
        _graphicsDevice ?? throw new InvalidOperationException("Renderer has not been initialized.");

    private Framebuffer OutputFramebuffer => _offscreenTarget ?? GraphicsDevice.SwapchainFramebuffer;
    internal FidelityFxSceneFrameGeneration? FrameGenerationScene => _frameGeneration;
    internal FrameGenerationPresenter? FramePresenter => _framePresenter;
    internal ulong MenuPrepareCount => _menu.PrepareCount;
    internal ulong MenuDrawCount => _menu.DrawCount;
    // Runtime proof copies the very swapchain image submitted to WSI. Ordinary
    // gameplay leaves this hook null and performs no staging copy/readback.
    internal Action<CommandList, Framebuffer, PresentedFrameKind, ulong>? BeforeFramePresentation { get; set; }
    internal Texture? SceneDepth => _resolutionScaler.SampleableDepthTexture;
    internal Texture? SceneMotion => _motionVectors.IsInitialized ? _motionVectors.MotionVectorTexture : null;
    internal bool NativeUpscalerReady => _fsr3Upscaler is { IsReady: true } && !_fsr3DisabledAfterError;
    internal bool FrameGenerationActive =>
        _frameGeneration is not null && _framePresenter is not null;
    internal string FrameGenerationDiagnostic { get; private set; } = "Scene FG validation was not requested.";
    public int LastPresentedFrameCount { get; private set; } = 1;

    // An externally owned framebuffer drives the complete production renderer
    // without an OS window. Only the final SwapBuffers operation is omitted.
    internal void InitializeOffscreen(GraphicsDevice device, Framebuffer target, WorldState world,
        TextureQuality textureQuality, MsaaQuality msaa, UpscalerMode upscaler,
        bool frameGenerationEnabled = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) throw new InvalidOperationException("Renderer is already initialized.");
        _graphicsDevice = device;
        _offscreenTarget = target;
        _ownsDevice = false;
        try
        {
            InitializeSceneResources(
                world,
                textureQuality,
                msaa,
                upscaler,
                frameGenerationEnabled);
            _initialized = true;
        }
        catch { Dispose(); throw; }
    }

    internal void SetOffscreenTarget(Framebuffer target)
    {
        if (_offscreenTarget is null) throw new InvalidOperationException("Renderer owns a window swapchain.");
        if (!target.OutputDescription.Equals(_offscreenTarget.OutputDescription))
            throw new ArgumentException("Replacement output must retain the framebuffer formats.", nameof(target));
        _offscreenTarget = target;
        ResetTemporalHistory();
    }

    public void Initialize(
        GameWindow window,
        WorldState world,
        bool vsync,
        TextureQuality textureQuality,
        MsaaQuality msaaQuality,
        UpscalerMode upscalerMode,
        bool frameGenerationEnabled = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized) return;
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(world);
        try
        {
            InitializeResources(
                window,
                world,
                vsync,
                textureQuality,
                msaaQuality,
                upscalerMode,
                frameGenerationEnabled);
            _initialized = true;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void InitializeResources(
        GameWindow window,
        WorldState world,
        bool vsync,
        TextureQuality textureQuality,
        MsaaQuality msaaQuality,
        UpscalerMode upscalerMode,
        bool frameGenerationEnabled)
    {
        PresentationPolicy.Apply(vsync);
        VulkanRuntimeCompatibility.EnsureInitialized();

        var options = new GraphicsDeviceOptions
        {
            Debug = false,
            SwapchainDepthFormat = PixelFormat.R32_Float,
            SyncToVerticalBlank = vsync,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        };

        _graphicsDevice = VulkanDeviceFactory.Create(options, new SwapchainDescription(
            VeldridStartup.GetSwapchainSource(window.NativeWindow),
            (uint)window.NativeWindow.Width, (uint)window.NativeWindow.Height,
            options.SwapchainDepthFormat, options.SyncToVerticalBlank, options.SwapchainSrgbFormat));

        InitializeSceneResources(
            world,
            textureQuality,
            msaaQuality,
            upscalerMode,
            frameGenerationEnabled);
    }

    private void InitializeSceneResources(
        WorldState world,
        TextureQuality textureQuality,
        MsaaQuality msaaQuality,
        UpscalerMode upscalerMode,
        bool frameGenerationEnabled)
    {
        var device = GraphicsDevice;
        _graphicsDevice = device;

        var vkInfo = _graphicsDevice.GetVulkanInfo();
        EngineLog.Info($"Vulkan creation: instance=0x{vkInfo.InstanceApiVersion:X}, " +
            $"physicalDevice=0x{vkInfo.PhysicalDeviceApiVersion:X}, " +
            $"shaderStorageImageExtendedFormats={vkInfo.ShaderStorageImageExtendedFormatsEnabled}; " +
            (VulkanDeviceFactory.FrameGenerationUnavailableReason(vkInfo) ?? "FG creation prerequisites enabled."));

        EngineLog.Info(
            "Vulkan presentation: " +
            $"Veldrid SyncToVerticalBlank={(_offscreenTarget is null && device.SyncToVerticalBlank)}, " +
            $"Mesa override={Environment.GetEnvironmentVariable(PresentationPolicy.MesaPresentModeVariable) ?? "<none>"}.");

        var fidelityFxProbe = FidelityFxRuntimePolicy.Probe();
        _fsr3ForcedByEnvironment = FidelityFxUpscalerPolicy.IsRequested();
        _fsr3Requested =
            upscalerMode == UpscalerMode.Fsr3 ||
            _fsr3ForcedByEnvironment;
        var frameGenerationRequested =
            frameGenerationEnabled ||
            FidelityFxSceneFrameGeneration.IsSceneValidationRequested();
        if (frameGenerationRequested)
        {
            FidelityFxSceneFrameGeneration.TryCreate(
                device,
                out _frameGeneration,
                out var fgDiagnostic);
            FrameGenerationDiagnostic = fgDiagnostic;
            EngineLog.Info(
                (frameGenerationEnabled
                    ? "Frame Generation setting: "
                    : "FG scene validation: ") +
                fgDiagnostic);
            if (_frameGeneration is not null && _offscreenTarget is null)
                _framePresenter = new FrameGenerationPresenter(device);
        }
        else
        {
            FrameGenerationDiagnostic =
                "Frame Generation is disabled in settings.";
        }
        var effectiveMsaa = FidelityFxStartupPolicy.EffectiveMsaa(
            msaaQuality,
            _fsr3Requested || _frameGeneration is not null,
            OperatingSystem.IsWindows() || OperatingSystem.IsLinux(),
            fidelityFxProbe.IsAvailable);
        if (effectiveMsaa != msaaQuality)
            EngineLog.Info("Native temporal inputs require single-sample scene depth; MSAA disabled for this run.");
        var fidelityFxHandles =
            FidelityFxVulkanInterop.GetDeviceHandles(_graphicsDevice);
        EngineLog.Info(
            $"FidelityFX Vulkan interop: {fidelityFxProbe.Status}; " +
            $"graphicsQueueFamily={fidelityFxHandles.GraphicsQueueFamilyIndex}. " +
            fidelityFxProbe.Diagnostic);

        var factory = _graphicsDevice.ResourceFactory;
        _commandList = factory.CreateCommandList();

        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        _playerModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "animated", "player_hunter_animated.glb"));
        _enemyModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "animated", "swamp_predator_animated.glb"));
        _bowModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "static", "luk_r0_01.glb"));
        _arrowModel = GlbModel.Load(Path.Combine(assetsRoot, "models", "static", "strzala_r0_01.glb"));

        _npcModels.Clear();
        foreach (var modelFile in world.Npcs
                     .Select(npc => NpcVisualCatalog.ModelFile(npc.Id, npc.Role))
                     .Distinct(StringComparer.Ordinal))
        {
            var npcPath = Path.Combine(
                assetsRoot,
                "models",
                "animated",
                modelFile);
            if (!File.Exists(npcPath))
            {
                throw new FileNotFoundException(
                    $"Required NPC model '{modelFile}' was not found.",
                    npcPath);
            }

            var model = GlbModel.Load(npcPath);
            if (!model.AnimationNames.Contains("Idle") ||
                !model.AnimationNames.Contains("Walk"))
            {
                throw new InvalidDataException(
                    $"NPC model '{modelFile}' must contain Idle and Walk clips.");
            }

            _npcModels[modelFile] = model;
        }

        _wildlifeModels.Clear();
        foreach (var modelFile in WildlifeCatalog.RequiredModelFiles)
        {
            var wildlifePath = Path.Combine(
                assetsRoot,
                "models",
                "animated",
                modelFile);
            if (!File.Exists(wildlifePath))
            {
                throw new FileNotFoundException(
                    $"Required wildlife model '{modelFile}' was not found.",
                    wildlifePath);
            }

            var model = GlbModel.Load(wildlifePath);
            if (model.AnimationNames.Count == 0)
            {
                throw new InvalidDataException(
                    $"Wildlife model '{modelFile}' does not expose animation clips.");
            }

            _wildlifeModels[modelFile] = model;
        }

        _bestiaryModels.Clear();
        foreach (var definition in BestiaryVisualCatalog.Definitions)
        {
            var monsterPath = Path.Combine(
                assetsRoot,
                "models",
                "animated",
                definition.ModelFile);
            if (!File.Exists(monsterPath))
            {
                throw new FileNotFoundException(
                    $"Required bestiary model '{definition.ModelFile}' was not found.",
                    monsterPath);
            }

            var model = GlbModel.Load(monsterPath);
            var missingClips = definition.RequiredClips
                .Where(clip => !model.AnimationNames.Contains(clip))
                .ToArray();
            if (missingClips.Length > 0)
            {
                throw new InvalidDataException(
                    $"Bestiary model '{definition.ModelFile}' is missing clips: {string.Join(", ", missingClips)}.");
            }

            _bestiaryModels[definition.ModelFile] = model;
        }

        _worldItemModels.Clear();
        foreach (var assetPath in WorldItemVisualCatalog.Definitions
                     .Select(definition => definition.AssetPath)
                     .Distinct(StringComparer.Ordinal))
        {
            var itemPath = Path.Combine(
                assetsRoot,
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(itemPath))
            {
                throw new FileNotFoundException(
                    $"Required dynamic world item asset '{assetPath}' was not found.",
                    itemPath);
            }

            _worldItemModels[assetPath] = GlbModel.Load(itemPath);
        }

        _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _atmosphereBuffer = factory.CreateBuffer(new BufferDescription(160, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

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
                "Atmosphere", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)));

        _cameraSet = factory.CreateResourceSet(new ResourceSetDescription(
            _cameraLayout,
            _projectionBuffer,
            _viewBuffer,
            _atmosphereBuffer));

        _shadows.Initialize(_graphicsDevice);

        _resolutionScaler.Initialize(
            _graphicsDevice,
            OutputFramebuffer.OutputDescription,
            OutputFramebuffer.Width,
            OutputFramebuffer.Height,
            effectiveMsaa);

        _temporalInputsEnabled =
            TemporalInputPolicy.IsEnabled() ||
            _fsr3Requested || _frameGeneration is not null;
        if (_temporalInputsEnabled &&
            _resolutionScaler.SampleableDepthView is { } temporalDepth &&
            _resolutionScaler.SampleableDepthTexture is { } temporalDepthTexture)
        {
            _motionVectors.Initialize(
                _graphicsDevice,
                temporalDepth,
                temporalDepthTexture,
                _resolutionScaler.Width,
                _resolutionScaler.Height);
            _dynamicMotionVertexCapacity = 64;
            _dynamicMotionVertexBuffer = factory.CreateBuffer(
                new BufferDescription(
                    DynamicMotionVertex.SizeInBytes *
                    _dynamicMotionVertexCapacity,
                    BufferUsage.VertexBuffer | BufferUsage.Dynamic));

            // Frame Generation consumes depth + motion but not FSR's reactive
            // mask. Do not allocate/render that extra target for FG-only runs.
            var needsReactiveMask =
                _fsr3Requested || TemporalInputPolicy.IsEnabled();
            if (needsReactiveMask)
            {
                _reactiveMask.Initialize(
                    _graphicsDevice,
                    _cameraLayout,
                    temporalDepthTexture,
                    _resolutionScaler.Width,
                    _resolutionScaler.Height);
            }

            EngineLog.Info(
                needsReactiveMask
                    ? "Temporal FSR inputs enabled: camera + dynamic object motion + reactive mask."
                    : "Temporal FG inputs enabled: camera + dynamic object motion; reactive mask skipped.");
        }
        else if (_temporalInputsEnabled)
        {
            EngineLog.Info(
                "Temporal FSR inputs deferred while scene MSAA is active; " +
                "temporal upscaling requires single-sample depth.");
        }

        if (_fsr3Requested &&
            _motionVectors.IsInitialized &&
            _reactiveMask.IsInitialized)
        {
            var outputWidth =
                Math.Max(1u, OutputFramebuffer.Width);
            var outputHeight =
                Math.Max(1u, OutputFramebuffer.Height);

            if (FidelityFxUpscaler.TryCreate(
                    _graphicsDevice,
                    outputWidth,
                    outputHeight,
                    out _fsr3Upscaler,
                    out var fsr3Diagnostic))
            {
                _resolutionScaler.EnsureFsr3Output(
                    outputWidth,
                    outputHeight);
                EngineLog.Info(fsr3Diagnostic);
            }
            else
            {
                _fsr3DisabledAfterError = true;
                EngineLog.Warn(
                    "FSR3 request fell back to the normal presentation path: " +
                    fsr3Diagnostic);
                _temporalInputsEnabled = FidelityFxStartupPolicy.NeedsTemporalInputs(
                    TemporalInputPolicy.IsEnabled(), _fsr3Requested, fsrFailed: true) || _frameGeneration is not null;
            }
        }

        var sceneOutput =
            _resolutionScaler.SceneFramebuffer.OutputDescription;

        _bloom.Initialize(
            _graphicsDevice,
            sceneOutput,
            _resolutionScaler.ResolvedSceneView,
            _resolutionScaler.Width,
            _resolutionScaler.Height);

        _postProcess.Initialize(
            _graphicsDevice,
            sceneOutput,
            _resolutionScaler.ResolvedSceneView,
            _resolutionScaler.Width,
            _resolutionScaler.Height);
        _postProcess.SetSources(
            _resolutionScaler.ResolvedSceneView,
            _bloom.OutputView,
            _resolutionScaler.Width,
            _resolutionScaler.Height,
            sceneOutput.ColorAttachments[0].Format);

        _sky.Initialize(
            factory,
            _cameraLayout,
            sceneOutput);

        _terrain.Initialize(
            _graphicsDevice,
            _cameraLayout,
            _shadows.SampleLayout,
            sceneOutput,
            world.Terrain,
            assetsRoot,
            textureQuality);

        _pbrModels.Initialize(
            _graphicsDevice,
            _cameraLayout,
            _shadows.SampleLayout,
            sceneOutput,
            world,
            assetsRoot,
            textureQuality);

        _farVegetation.Initialize(
            _graphicsDevice,
            _cameraLayout,
            sceneOutput,
            world,
            assetsRoot);

        _actorShaders = ShaderLibrary.LoadPair(factory, "actor");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3));

        _actorPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(new[] { vertexLayout }, _actorShaders),
            new[] { _cameraLayout, _shadows.SampleLayout },
            sceneOutput));

        _characters.Initialize(
            factory,
            vertexLayout,
            _cameraLayout,
            _shadows.SampleLayout,
            sceneOutput);

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

        _menu.Initialize(
            _graphicsDevice,
            OutputFramebuffer.OutputDescription);

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
            OutputFramebuffer.OutputDescription));

        EngineLog.Info($"Veldrid renderer initialized with {_graphicsDevice.BackendType}.");
        EngineLog.Info($"Graphics device: {_graphicsDevice.DeviceName}.");
        EngineLog.Info($"Terrain materials + PBR world initialized; PBR world instances={_pbrModels.InstanceCount}, " +
            $"unique assets={_pbrModels.UniqueAssetCount}, spatial batches={_pbrModels.RenderableCount}, " +
            $"far-tree proxies={_farVegetation.TreeCount}/{_farVegetation.BatchCount} batches, " +
            $"collision obstacles={world.Obstacles.Count}.");
        EngineLog.Info(
            $"Animated actor models loaded: player clips={_playerModel.AnimationNames.Count}, " +
            $"NPC models={_npcModels.Count}, wildlife models={_wildlifeModels.Count}, " +
            $"bestiary models={_bestiaryModels.Count}, enemy clips={_enemyModel.AnimationNames.Count}.");
        EngineLog.Info("HUD renderer initialized.");
    }

    public void Render(
        WorldState world,
        Camera3D camera,
        double fps,
        double animationSeconds,
        double frameDeltaSeconds,
        GameSettings settings,
        MenuView? menuView)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized || _graphicsDevice is null ||
            _commandList is null ||
            _projectionBuffer is null ||
            _viewBuffer is null ||
            _atmosphereBuffer is null ||
            _actorVertexBuffer is null ||
            _actorIndexBuffer is null ||
            _playerModel is null ||
            _enemyModel is null ||
            _cameraSet is null ||
            _actorPipeline is null ||
            _hudVertexBuffer is null ||
            _hudScreenBuffer is null ||
            _hudSet is null ||
            _hudPipeline is null)
        {
            throw new InvalidOperationException("Renderer has not been initialized.");
        }

        var swapchainFramebuffer = OutputFramebuffer;
        if (swapchainFramebuffer is null)
        {
            return;
        }

        LastPresentedFrameCount = 1;
        var sceneFrameId = _sceneFrameId++;

        var menuVisible = menuView is not null;
        if (_previousMenuVisible is { } previousMenuVisible &&
            previousMenuVisible != menuVisible)
        {
            // Pause/menu transitions can skip or stall simulation frames. Do
            // not interpolate across that discontinuity.
            ResetTemporalHistory();
        }
        _previousMenuVisible = menuVisible;

        if (_previousUpscaler is { } previous && previous != settings.Upscaler)
            ResetTemporalHistory();
        _previousUpscaler = settings.Upscaler;

        if (_hasPreviousCameraState &&
            (Vector3.DistanceSquared(
                 _previousCameraPosition,
                 camera.Position) > 64f ||
             Vector3.DistanceSquared(
                 _previousCameraTarget,
                 camera.Target) > 64f ||
             MathF.Abs(_previousCameraFov - camera.FieldOfView) > 0.20f))
        {
            // Teleports, hard cinematic cuts and large FOV jumps have no
            // meaningful motion-vector history.
            ResetTemporalHistory();
        }
        _previousCameraPosition = camera.Position;
        _previousCameraTarget = camera.Target;
        _previousCameraFov = camera.FieldOfView;
        _hasPreviousCameraState = true;

        _framePresenter?.BeginFrame(
            frameDeltaSeconds,
            GraphicsQualityCatalog.FrameRate(settings.FpsLimit));

        var framebuffer = _resolutionScaler.SceneFramebuffer;
        var width = Math.Max(1u, framebuffer.Width);
        var height = Math.Max(1u, framebuffer.Height);
        var displayWidth = Math.Max(1u, swapchainFramebuffer.Width);
        var displayHeight = Math.Max(1u, swapchainFramebuffer.Height);
        var aspect = MathF.Max(0.1f, (float)width / height);
        var spatialFallbackUpscaler =
            settings.Upscaler == UpscalerMode.Fsr3
                ? UpscalerMode.Fsr1
                : settings.Upscaler;

        // Native temporal upscaling and frame interpolation both require the
        // render extent to fit inside the display extent. A transient resize
        // must fall back for one frame, not poison the native contexts for the
        // rest of the process.
        var temporalExtentValid =
            width <= displayWidth &&
            height <= displayHeight;
        if (!temporalExtentValid &&
            (_fsr3Upscaler is { IsReady: true } || _frameGeneration is not null))
        {
            ResetTemporalHistory();
        }

        var fsr3Active =
            temporalExtentValid &&
            (_fsr3ForcedByEnvironment ||
             settings.Upscaler == UpscalerMode.Fsr3) &&
            _fsr3Upscaler is { IsReady: true } &&
            !_fsr3DisabledAfterError;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FieldOfView, aspect, camera.NearPlane, camera.FarPlane);
        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);
        var jitterPhaseCount = fsr3Active
            ? TemporalFrameState.FsrJitterPhaseCount(
                width,
                displayWidth)
            : 8;
        var temporalFrame = _temporalFrame.BeginFrame(
            projection,
            view,
            width,
            height,
            enableJitter: fsr3Active,
            jitterPhaseCount: jitterPhaseCount);
        projection = temporalFrame.Projection;

        ActorModelMesh.Build(
            world,
            _playerModel,
            _npcModels,
            _enemyModel,
            camera.Position,
            animationSeconds,
            camera.Mode == CameraMode.FirstPerson || world.Bow.IsAiming ? camera.Yaw : world.PlayerFacingYaw,
            camera.Mode != CameraMode.FirstPerson,
            _actorVertices,
            _actorIndices);
        WildlifeModelMesh.Append(
            world,
            _wildlifeModels,
            animationSeconds,
            camera.Position,
            MathF.Min(
                GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
                110f),
            _actorVertices,
            _actorIndices);
        BestiaryModelMesh.Append(
            world,
            _bestiaryModels,
            animationSeconds,
            camera.Position,
            MathF.Min(
                GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
                110f),
            _actorVertices,
            _actorIndices);
        BowPresentationMesh.Append(
            world,
            _bowModel!,
            _arrowModel!,
            camera.Position,
            camera.GetLookDirection(),
            camera.GetMoveRight(),
            camera.Mode == CameraMode.FirstPerson,
            _actorVertices,
            _actorIndices);
        WorldItemModelMesh.Append(
            world,
            _worldItemModels,
            _actorVertices,
            _actorIndices);
        SlavicFolkloreAtmosphereMesh.AppendCarvedTotems(
            world,
            camera.Position,
            (float)animationSeconds,
            _actorVertices,
            _actorIndices);
        var opaqueDynamicVertexCount = _actorVertices.Count;
        var opaqueDynamicEnd = checked((uint)_actorIndices.Count);

        var waterStart = opaqueDynamicEnd;
        WaterLandscape.AppendSurface(world.Terrain, (float)animationSeconds, camera.Position,
            GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
            _actorVertices, _actorIndices);
        WaterInteractionMesh.Append(
            world,
            (float)animationSeconds,
            _actorVertices,
            _actorIndices);
        var waterEnd = checked((uint)_actorIndices.Count);

        var transientStart = waterEnd;
        CampfireEffectMesh.Append(
            world,
            (float)animationSeconds,
            _actorVertices,
            _actorIndices);
        SlavicFolkloreAtmosphereMesh.AppendFolkloreGlows(
            world,
            camera.Position,
            (float)animationSeconds,
            _actorVertices,
            _actorIndices);
        RainEffectMesh.Append(
            world,
            camera.Position,
            (float)animationSeconds,
            settings.CloudQuality,
            _actorVertices,
            _actorIndices);
        var transientEnd = checked((uint)_actorIndices.Count);

        FootprintEffectMesh.Append(
            world,
            _actorVertices,
            _actorIndices);
        WildlifeTrackEffectMesh.Append(
            world,
            _actorVertices,
            _actorIndices);

        var spectralStart = checked((uint)_actorIndices.Count);
        ApparitionEffectMesh.Append(
            world,
            (float)animationSeconds,
            _actorVertices,
            _actorIndices);
        MagicEffectMesh.Append(world, _actorVertices, _actorIndices);
        var spectralEnd = checked((uint)_actorIndices.Count);

        // Opaque moving actors already have per-object motion vectors. Marking
        // them reactive wastes another geometry pass and weakens temporal reuse.
        // Reserve the mask for content FSR actually needs help reconstructing.
        var reactiveRanges = fsr3Active
            ? new ReactiveMaskRange[]
            {
                new(waterStart, waterEnd - waterStart, 0.88f),
                new(transientStart, transientEnd - transientStart, 1.0f),
                new(spectralStart, spectralEnd - spectralStart, 1.0f)
            }
            : [];

        EnsureActorCapacity(_actorVertices.Count, _actorIndices.Count);
        _actorIndexCount = (uint)_actorIndices.Count;

        DynamicMotionVertex[] dynamicMotionVertices = [];
        if (_temporalInputsEnabled && _motionVectors.IsInitialized)
        {
            var dynamicMotionSpan = CollectionsMarshal.AsSpan(_actorVertices).Slice(0, opaqueDynamicVertexCount);
            EnsureDynamicMotionCapacity(dynamicMotionSpan.Length);
            if (_dynamicMotionUpload.Length < dynamicMotionSpan.Length)
            {
                var nextCapacity = Math.Max(
                    dynamicMotionSpan.Length,
                    Math.Max(64, _dynamicMotionUpload.Length * 2));
                Array.Resize(ref _dynamicMotionUpload, nextCapacity);
            }

            _dynamicMotionHistory.BuildInto(
                dynamicMotionSpan,
                temporalFrame.ResetHistory,
                _dynamicMotionUpload.AsSpan(0, dynamicMotionSpan.Length));
            dynamicMotionVertices = _dynamicMotionUpload;
        }

        BuildHud(
            (float)Math.Max(0, fps),
            world.Player.Health,
            world.Player.MaxHealth,
            world.Player.Stamina,
            world.Player.MaxStamina,
            world.Player.Health / world.Player.MaxHealth,
            world.Player.Stamina / world.Player.MaxStamina,
            animationSeconds,
            settings.ShowFps,
            menuView is null);
        if (menuView is null)
        {
            if (world.Cinematics.IsPlaying)
            {
                AddHudQuad(0, 0, displayWidth, 55, new Vector4(0, 0, 0, 1));
                AddHudQuad(0, displayHeight - 85, displayWidth, 85, new Vector4(0, 0, 0, 1));
                AddGameplayText(world.Cinematics.Subtitle, 18, displayHeight - 66, displayWidth - 36);
                AddGameplayText("SPACJA / ESC - POMIN", 18, displayHeight - 32, displayWidth - 36);
            }
            else if (world.Loot.IsOpen)
            {
                var panelWidth = MathF.Min(960f, displayWidth - 32f);
                var panelHeight = MathF.Min(460f, displayHeight - 32f);
                var left = (displayWidth - panelWidth) * 0.5f;
                var top = (displayHeight - panelHeight) * 0.5f;
                var half = panelWidth * 0.5f;
                AddHudQuad(left, top, panelWidth, panelHeight, new Vector4(0.02f, 0.025f, 0.018f, 0.96f));
                AddGameplayText(world.Loot.CurrentContainerTitle, left + 18, top + 16, panelWidth - 36);
                var visibleRows = Math.Max(1, (int)((panelHeight - 165f) / 26f));
                foreach (var side in new[] { SlavicGame.Engine.Gameplay.LootPanel.Container, SlavicGame.Engine.Gameplay.LootPanel.Inventory })
                {
                    var columnLeft = left + (side == SlavicGame.Engine.Gameplay.LootPanel.Container ? 0f : half);
                    var active = world.Loot.Panel == side;
                    AddGameplayText((active ? "> " : "  ") + (side == SlavicGame.Engine.Gameplay.LootPanel.Container ? "KUFER [A]" : "EKWIPUNEK [D]"),
                        columnLeft + 18, top + 48, half - 36);
                    var lines = world.Loot.Lines(world, side);
                    var start = active ? Math.Max(0, world.Loot.SelectedIndex - visibleRows + 1) : 0;
                    if (lines.Count == 0) AddGameplayText("PUSTO", columnLeft + 18, top + 82, half - 36);
                    for (var i = start; i < Math.Min(lines.Count, start + visibleRows); i++)
                    {
                        var line = lines[i];
                        var text = (active && i == world.Loot.SelectedIndex ? "> " : "  ") +
                            SlavicGame.Engine.Gameplay.LootContainerRuntime.DisplayName(line.ItemId) + $" x{line.Quantity}";
                        var maxChars = Math.Max(4, (int)((half - 36f) / 4.8f));
                        if (text.Length > maxChars) text = text[..(maxChars - 3)] + "...";
                        AddGameplayText(text, columnLeft + 18, top + 82 + (i - start) * 26f, half - 36);
                    }
                }
                AddGameplayText(world.Loot.Message, left + 18, top + panelHeight - 72, panelWidth - 36);
                AddGameplayText("A/D PANEL  W/S WYBOR  E PRZENIES 1", left + 18, top + panelHeight - 46, panelWidth - 36);
                AddGameplayText("SHIFT+E CALY STOS  ESC ZAMKNIJ", left + 18, top + panelHeight - 22, panelWidth - 36);
            }
            else if (world.Crafting.IsOpen)
            {
                var lines = world.Crafting.BuildLines(world);
                var panelHeight =
                    MathF.Min(
                        350f,
                        MathF.Max(230f, 175f + lines.Count * 48f));
                var panelWidth =
                    MathF.Min(820f, displayWidth - 40f);
                var panelLeft =
                    (displayWidth - panelWidth) * 0.5f;
                var panelTop =
                    (displayHeight - panelHeight) * 0.5f;

                AddHudQuad(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight,
                    new Vector4(0.02f, 0.025f, 0.018f, 0.94f));

                AddGameplayText(
                    "ALCHEMIA - STOL ZIELARKI",
                    panelLeft + 24,
                    panelTop + 18,
                    panelWidth - 48);

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var prefix =
                        i == world.Crafting.SelectedIndex
                            ? "> "
                            : "  ";
                    var status =
                        !line.Unlocked
                            ? "NIEZNANA RECEPTURA"
                            : line.CanCraft
                                ? "GOTOWE"
                                : "BRAK SKLADNIKOW";

                    AddGameplayText(
                        $"{prefix}{line.DisplayName} / {status}",
                        panelLeft + 34,
                        panelTop + 62 + i * 48,
                        panelWidth - 68);
                    AddGameplayText(
                        $"  {line.RequirementText}",
                        panelLeft + 52,
                        panelTop + 84 + i * 48,
                        panelWidth - 90);
                }

                if (!string.IsNullOrWhiteSpace(world.Crafting.Message))
                {
                    AddGameplayText(
                        world.Crafting.Message,
                        panelLeft + 24,
                        panelTop + panelHeight - 68,
                        panelWidth - 48);
                }

                AddGameplayText(
                    "W/S WYBOR  E WYTWORZ  K/ESC ZAMKNIJ",
                    panelLeft + 24,
                    panelTop + panelHeight - 32,
                    panelWidth - 48);
            }
            else if (world.Blacksmith.IsOpen)
            {
                var lines = world.Blacksmith.BuildLines(world);
                var panelHeight = MathF.Min(380f, MathF.Max(240f, 175f + lines.Count * 44f));
                var panelWidth = MathF.Min(820f, displayWidth - 40f);
                var panelLeft = (displayWidth - panelWidth) * 0.5f;
                var panelTop = (displayHeight - panelHeight) * 0.5f;

                AddHudQuad(panelLeft, panelTop, panelWidth, panelHeight, new Vector4(0.025f, 0.02f, 0.018f, 0.95f));
                AddHudQuad(panelLeft + 2f, panelTop + 2f, panelWidth - 4f, 36f, new Vector4(0.20f, 0.14f, 0.08f, 0.85f));

                AddGameplayText("KUZNIA SLOWIANSKA - KOWALSTWO I ORĘŻ", panelLeft + 24, panelTop + 14, panelWidth - 48);

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var prefix = i == world.Blacksmith.SelectedIndex ? "> " : "  ";
                    var status = line.CanCraft ? "MOZNA WYKUC" : "BRAK SUROWCOW";

                    AddGameplayText($"{prefix}{line.DisplayName} [{line.Description}] / {status}", panelLeft + 34, panelTop + 54 + i * 44, panelWidth - 68);
                    AddGameplayText($"  {line.RequirementsText}", panelLeft + 52, panelTop + 72 + i * 44, panelWidth - 90);
                }

                if (!string.IsNullOrWhiteSpace(world.Blacksmith.Message))
                {
                    AddGameplayText(world.Blacksmith.Message, panelLeft + 24, panelTop + panelHeight - 68, panelWidth - 48);
                }

                AddGameplayText("W/S WYBOR  E WYKUJ  B/ESC ZAMKNIJ", panelLeft + 24, panelTop + panelHeight - 32, panelWidth - 48);
            }
            else if (world.Vendors.IsOpen)
            {
                var lines = world.Vendors.BuildLines(world);
                var panelHeight =
                    MathF.Min(
                        390f,
                        MathF.Max(240f, 165f + lines.Count * 32f));
                var panelWidth =
                    MathF.Min(760f, displayWidth - 40f);
                var panelLeft =
                    (displayWidth - panelWidth) * 0.5f;
                var panelTop =
                    (displayHeight - panelHeight) * 0.5f;

                AddHudQuad(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight,
                    new Vector4(0.02f, 0.02f, 0.018f, 0.94f));

                AddGameplayText(
                    world.Vendors.DisplayName,
                    panelLeft + 24,
                    panelTop + 18,
                    panelWidth - 48);

                AddGameplayText(
                    $"PIENIADZE {world.Progress.Profile.Money} / TRYB: " +
                    (world.Vendors.Mode ==
                        SlavicGame.Engine.Gameplay.VendorMode.Buy
                            ? "KUP"
                            : "SPRZEDAJ"),
                    panelLeft + 24,
                    panelTop + 50,
                    panelWidth - 48);

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var prefix =
                        i == world.Vendors.SelectedIndex
                            ? "> "
                            : "  ";
                    var quantityLabel =
                        world.Vendors.Mode ==
                        SlavicGame.Engine.Gameplay.VendorMode.Buy
                            ? $"STAN {line.Quantity}"
                            : $"MASZ {line.Quantity}";
                    var status =
                        line.Available
                            ? ""
                            : " / BRAK";

                    AddGameplayText(
                        $"{prefix}{line.DisplayName} / {line.Price} / {quantityLabel}{status}",
                        panelLeft + 34,
                        panelTop + 90 + i * 32,
                        panelWidth - 68);
                }

                if (!string.IsNullOrWhiteSpace(world.Vendors.Message))
                {
                    AddGameplayText(
                        world.Vendors.Message,
                        panelLeft + 24,
                        panelTop + panelHeight - 68,
                        panelWidth - 48);
                }

                AddGameplayText(
                    "W/S WYBOR  A/D KUP-SPRZEDAJ  E POTWIERDZ  T/ESC ZAMKNIJ",
                    panelLeft + 24,
                    panelTop + panelHeight - 32,
                    panelWidth - 48);
            }
            else if (world.Dialogue.IsOpen)
            {
                var node = world.Dialogue.CurrentNode;
                var choices = world.Dialogue.AvailableChoices(world);
                var panelHeight = MathF.Min(320f, MathF.Max(210f, 145f + choices.Count * 34f));
                var panelTop = displayHeight - panelHeight;

                AddHudQuad(
                    0,
                    panelTop,
                    displayWidth,
                    panelHeight,
                    new Vector4(0.02f, 0.02f, 0.018f, 0.92f));

                if (world.Dialogue.SpeakerId is not null)
                {
                    AddGameplayText(
                        NpcPresentation.DisplayName(world.Dialogue.SpeakerId),
                        24,
                        panelTop + 18,
                        displayWidth - 48);
                }

                if (node is not null)
                {
                    AddGameplayText(
                        node.Text,
                        24,
                        panelTop + 50,
                        displayWidth - 48);
                }

                for (var i = 0; i < choices.Count; i++)
                {
                    var prefix =
                        i == world.Dialogue.SelectedChoiceIndex
                            ? "> "
                            : "  ";
                    AddGameplayText(
                        prefix + choices[i].Text,
                        36,
                        panelTop + 92 + i * 32,
                        displayWidth - 72);
                }

                AddGameplayText(
                    "W/S WYBOR  E POTWIERDZ  ESC ZAKONCZ",
                    24,
                    displayHeight - 30,
                    displayWidth - 48);
            }
            else
            {
                if (world.Progress.HasFlag(SlavicGame.Engine.Gameplay.ActOneCampaignSystem.MilestoneAct1Flag))
                {
                    AddHudQuad(displayWidth * 0.5f - 240f, 16f, 480f, 40f, new Vector4(0.12f, 0.08f, 0.02f, 0.90f));
                    AddHudQuad(displayWidth * 0.5f - 238f, 18f, 476f, 36f, new Vector4(0.85f, 0.68f, 0.22f, 0.35f));
                    AddGameplayText("★ UKONCZONO AKT I: SPLOT PRZEZNACZENIA (25% GRY) ★", displayWidth * 0.5f - 220f, 26f, 440f);
                }

                if (world.Enemies.Any(enemy => enemy.IsAttackWindingUp &&
                    Vector3.DistanceSquared(enemy.Position, world.PlayerPosition) < 36f))
                {
                    AddGameplayText("PRZECIWNIK: ZAMACH - UNIKNIJ [SPACJA]", 18, 120, displayWidth - 36);
                }

                if (!string.IsNullOrWhiteSpace(world.Magic.Message))
                    AddGameplayText(world.Magic.Message, 18, 144, displayWidth - 36);

                if (!string.IsNullOrWhiteSpace(world.Rituals.Message))
                    AddGameplayText(world.Rituals.Message, 18, 168, displayWidth - 36);

                var interactionText =
                    world.QuestInteractions.Current?.Prompt ??
                    world.EnvironmentInteractions.Current?.Prompt;

                if (string.IsNullOrWhiteSpace(interactionText))
                    interactionText = world.Bow.RetrievalPrompt(world);

                if (string.IsNullOrWhiteSpace(interactionText) && world.Loot.CanOpenNearest(world))
                    interactionText = world.Loot.CurrentContainerPrompt;

                if (string.IsNullOrWhiteSpace(interactionText))
                    interactionText = world.NpcWorld.HudPrompt(world.PlayerPosition);

                if (string.IsNullOrWhiteSpace(interactionText))
                {
                    interactionText =
                        !string.IsNullOrWhiteSpace(world.EnvironmentInteractions.Message)
                            ? world.EnvironmentInteractions.Message
                            : world.QuestInteractions.Message;
                }

                if (world.Vendors.CanOpenNearest(world))
                {
                    interactionText =
                        string.IsNullOrWhiteSpace(interactionText)
                            ? "T HANDEL"
                            : interactionText + " / T HANDEL";
                }

                if (world.Crafting.CanOpenNearest(world))
                {
                    interactionText =
                        string.IsNullOrWhiteSpace(interactionText)
                            ? "K ALCHEMIA"
                            : interactionText + " / K ALCHEMIA";
                }

                if (world.Blacksmith.CanOpenNearest(world))
                {
                    interactionText =
                        string.IsNullOrWhiteSpace(interactionText)
                            ? "B KOWALSTWO"
                            : interactionText + " / B KOWALSTWO";
                }

                if (!string.IsNullOrWhiteSpace(interactionText))
                    AddGameplayText(interactionText, 18, displayHeight - 72f, displayWidth - 36);

                if (world.Bow.IsAiming)
                {
                    var reticleX = displayWidth * 0.5f;
                    var reticleY = displayHeight * 0.5f;
                    var drawFraction = world.Bow.DrawFraction;
                    var gap = MathF.Round(float.Lerp(12f, 4f, drawFraction));
                    var length = MathF.Round(float.Lerp(8f, 6f, drawFraction));
                    var reticle = drawFraction >= 0.95f
                        ? new Vector4(1.0f, 0.88f, 0.40f, 0.95f)
                        : new Vector4(0.92f, 0.92f, 0.86f, 0.92f);

                    AddHudQuad(reticleX - 1.5f, reticleY - 1.5f, 3f, 3f, reticle);
                    AddHudQuad(reticleX - gap - length, reticleY - 1f, length, 2f, reticle);
                    AddHudQuad(reticleX + gap, reticleY - 1f, length, 2f, reticle);
                    AddHudQuad(reticleX - 1f, reticleY - gap - length, 2f, length, reticle);
                    AddHudQuad(reticleX - 1f, reticleY + gap, 2f, length, reticle);

                    AddGameplayText(world.Bow.HudText(world), 18, 144, displayWidth - 36);
                    if (!string.IsNullOrWhiteSpace(world.Bow.Message))
                        AddGameplayText(world.Bow.Message, 18, 168, displayWidth - 36);
                }

                var predatorEncounter = world.Progress.Encounters.Get(
                    SlavicGame.Engine.Gameplay.SwampPredatorEncounter.Id);
                var predator = world.Enemies.FirstOrDefault(enemy =>
                    string.Equals(
                        enemy.Id,
                        SlavicGame.Engine.Gameplay.SwampPredatorEncounter.Id,
                        StringComparison.Ordinal));

                if (predator is not null &&
                    predatorEncounter.Awareness >= SlavicGame.Engine.Gameplay.EncounterAwareness.Identified)
                {
                    AddGameplayText(
                        predator.IsAlive
                            ? $"DRAPIEZNIK HP {predator.Health:0}/{predator.MaxHealth:0}"
                            : "DRAPIEZNIK POKONANY",
                        18,
                        192,
                        displayWidth - 36);
                }

                if (world.Springs.HasBlessedWater)
                {
                    AddGameplayText(
                        $"ZYWA WODA: +50% ENERGII ({(int)Math.Ceiling(world.Springs.BlessedWaterBuffRemainingSeconds)}s)",
                        18,
                        216,
                        displayWidth - 36);
                }
            }

            if (!world.Cinematics.IsPlaying && settings.ShowMinimap)
            {
                MinimapRenderer.Render(
                    world,
                    camera,
                    displayWidth,
                    displayHeight,
                    AddHudQuad,
                    AddHudTriangle,
                    AddHudText);
            }
        }
        if (_hudVertices.Count > _hudVertexCapacity)
        {
            _hudVertexCapacity = (uint)Math.Max(_hudVertices.Count, _hudVertexCapacity * 2);
            _graphicsDevice.WaitForIdle();
            _hudVertexBuffer.Dispose();
            _hudVertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                HudVertex.SizeInBytes * _hudVertexCapacity,
                BufferUsage.VertexBuffer));
        }

        var screenSize = new Vector4(displayWidth, displayHeight, 0, 0);
        var celestial = CelestialLighting.Evaluate(world.Time, world.Weather);
        var shadowEnabled =
            settings.SunShadows &&
            celestial.SunIntensity > 0.02f &&
            celestial.SunDirection.Y > 0.02f;
        var lightning = WeatherVisuals.LightningFlash(
            world.Weather,
            animationSeconds);
        var atmosphereColor = GetAtmosphereColor(world.Time, world.Weather, celestial);
        var fogParameters = new Vector4(
            atmosphereColor.R,
            atmosphereColor.G,
            atmosphereColor.B,
            world.Weather.FogDensity);
        var lightingParameters = new Vector4(
            celestial.SunIntensity + lightning * 1.55f,
            celestial.SunDirection.X,
            celestial.SunDirection.Y,
            celestial.SunDirection.Z);
        var stormLightColor = Vector3.Lerp(
            celestial.SunColor,
            new Vector3(0.72f, 0.82f, 1.00f),
            lightning);
        var sunColorTime = new Vector4(
            stormLightColor,
            (float)(world.Time.TimeOfDayHours / 24.0));
        var skyWeather = new Vector4(
            world.Weather.Cloudiness,
            world.Weather.RainIntensity,
            world.Weather.WindIntensity,
            (float)animationSeconds);
        var moonParameters = new Vector4(
            celestial.MoonDirection,
            celestial.MoonIntensity);
        var celestialParameters = new Vector4(
            celestial.NightFactor,
            celestial.LunarPhase,
            celestial.TwilightFactor,
            lightning);
        var graphicsFeatures0 = new Vector4(
            settings.VolumetricClouds ? 1f : 0f,
            settings.CloudShadows ? 1f : 0f,
            shadowEnabled ? 1f : 0f,
            settings.Fog ? 1f : 0f);
        var graphicsFeatures1 = new Vector4(
            settings.Sun ? 1f : 0f,
            settings.Moon ? 1f : 0f,
            settings.Stars ? 1f : 0f,
            settings.Sky ? 1f : 0f);
        var graphicsFeatures2 = new Vector4(
            settings.TerrainPbr ? 1f : 0f,
            settings.ModelPbr ? 1f : 0f,
            GraphicsQualityCatalog.CloudRaymarchSteps(settings.CloudQuality),
            0f);
        var graphicsFeatures3 = new Vector4(
            settings.NormalMapping ? 1f : 0f,
            settings.SpecularHighlights ? 1f : 0f,
            GraphicsQualityCatalog.TerrainDetailLevel(settings.TerrainDetail),
            0f);
        var cameraFrustum = CameraFrustum.Create(camera, aspect);
        var fgActive =
            temporalExtentValid &&
            _frameGeneration is not null &&
            _motionVectors.IsInitialized;
        if (fgActive)
        {
            try { _frameGeneration?.EnsureDisplaySize(displayWidth, displayHeight); }
            catch (Exception exception)
            {
                DisableSceneFrameGeneration(exception);
                fgActive = false;
            }
        }

        var atmosphereUniform = new AtmosphereUniform(
            fogParameters,
            lightingParameters,
            sunColorTime,
            skyWeather,
            moonParameters,
            celestialParameters,
            graphicsFeatures0,
            graphicsFeatures1,
            graphicsFeatures2,
            graphicsFeatures3);

        _commandList.Begin();
        _commandList.UpdateBuffer(_projectionBuffer, 0, projection);
        _commandList.UpdateBuffer(_viewBuffer, 0, view);
        _commandList.UpdateBuffer(_atmosphereBuffer, 0, ref atmosphereUniform);
        _commandList.UpdateBuffer(_hudScreenBuffer, 0, screenSize);
        if (_actorVertices.Count > 0)
        {
            _commandList.UpdateBuffer(
                _actorVertexBuffer,
                0,
                CollectionsMarshal.AsSpan(_actorVertices));
            _commandList.UpdateBuffer(
                _actorIndexBuffer,
                0,
                CollectionsMarshal.AsSpan(_actorIndices));
        }
        if (dynamicMotionVertices.Length > 0 &&
            _dynamicMotionVertexBuffer is not null)
        {
            _commandList.UpdateBuffer(
                _dynamicMotionVertexBuffer,
                0,
                _dynamicMotionUpload.AsSpan(0, opaqueDynamicVertexCount));
        }
        if (_hudVertices.Count > 0)
        {
            _commandList.UpdateBuffer(
                _hudVertexBuffer,
                0,
                CollectionsMarshal.AsSpan(_hudVertices));
        }

        if (shadowEnabled)
        {
            var shadowDist = GraphicsQualityCatalog.ShadowDistance(settings.ShadowDistance);
            var vegDist = GraphicsQualityCatalog.VegetationDistance(settings.VegetationDistance);
            var clutterDist = GraphicsQualityCatalog.GroundClutterDistance(settings.GroundClutter);

            if (_shadows.ShouldUpdateStaticShadow(
                    camera.Position,
                    celestial.SunDirection,
                    settings.TerrainDetail,
                    shadowDist,
                    vegDist,
                    clutterDist,
                    settings.ModelLod,
                    settings.FarVegetation))
            {
                _shadows.BeginStaticPass(
                    _commandList,
                    camera.Position,
                    celestial.SunDirection,
                    settings.TerrainDetail,
                    shadowDist,
                    vegDist,
                    clutterDist,
                    settings.ModelLod,
                    settings.FarVegetation);

                _terrain.RenderShadow(
                    _commandList,
                    _shadows.TerrainPipeline,
                    _shadows.DepthSet,
                    settings.TerrainDetail,
                    camera.Position,
                    shadowDist);

                _pbrModels.RenderShadow(
                    _commandList,
                    _shadows.PbrPipeline,
                    _shadows.DepthSet,
                    camera.Position,
                    shadowDist,
                    vegDist,
                    clutterDist,
                    settings.ModelLod,
                    settings.FarVegetation);

                _shadows.EndStaticPassAndPrepareActive(_commandList);
            }
            else
            {
                _shadows.BeginActivePassFromCache(_commandList);
            }

            _shadows.RenderActors(
                _commandList,
                _actorVertexBuffer,
                _actorIndexBuffer,
                opaqueDynamicEnd);
        }

        var renderDist = GraphicsQualityCatalog.RenderDistance(settings.RenderDistance);

        _commandList.SetFramebuffer(framebuffer);
        _commandList.SetFullViewports();
        _commandList.SetFullScissorRects();
        _commandList.ClearColorTarget(0, atmosphereColor);
        _commandList.ClearDepthStencil(1f);

        _terrain.Render(
            _commandList,
            _cameraSet,
            _shadows.SampleSet,
            settings.TerrainDetail,
            camera.Position,
            cameraFrustum,
            renderDist);

        _pbrModels.Render(
            _commandList,
            _cameraSet,
            _shadows.SampleSet,
            camera.Position,
            cameraFrustum,
            renderDist,
            GraphicsQualityCatalog.VegetationDistance(settings.VegetationDistance),
            GraphicsQualityCatalog.GroundClutterDistance(settings.GroundClutter),
            settings.ModelLod,
            settings.FarVegetation);

        if (settings.FarVegetation == FarVegetationMode.Impostors)
        {
            _farVegetation.Render(
                _commandList,
                _cameraSet,
                camera.Position,
                cameraFrustum,
                GraphicsQualityCatalog.VegetationImpostorStart(settings.ModelLod),
                Math.Min(
                    GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
                    GraphicsQualityCatalog.VegetationDistance(settings.VegetationDistance)));
        }

        if (opaqueDynamicEnd > 0)
        {
            _characters.Render(
                _commandList,
                _cameraSet,
                _shadows.SampleSet,
                _actorVertexBuffer,
                _actorIndexBuffer,
                opaqueDynamicEnd);
        }

        if (_actorIndexCount > opaqueDynamicEnd)
        {
            _commandList.SetPipeline(_actorPipeline);
            _commandList.SetGraphicsResourceSet(0, _cameraSet);
            _commandList.SetGraphicsResourceSet(1, _shadows.SampleSet);
            _commandList.SetVertexBuffer(0, _actorVertexBuffer);
            _commandList.SetIndexBuffer(_actorIndexBuffer, IndexFormat.UInt32);
            _commandList.DrawIndexed(
                _actorIndexCount - opaqueDynamicEnd,
                instanceCount: 1,
                indexStart: opaqueDynamicEnd,
                vertexOffset: 0,
                instanceStart: 0);
        }

        // Sky last among opaque passes: depth-tested at the far plane so its
        // raymarched clouds only run on genuinely visible sky pixels.
        _sky.Render(_commandList, _cameraSet);

        if (fsr3Active && _reactiveMask.IsInitialized)
        {
            _reactiveMask.Render(
                _commandList,
                _cameraSet,
                _actorVertexBuffer,
                _actorIndexBuffer,
                reactiveRanges);
        }

        _resolutionScaler.ResolveScene(_commandList);

        if (_temporalInputsEnabled && _motionVectors.IsInitialized)
        {
            _motionVectors.Render(_commandList, temporalFrame);
            if (dynamicMotionVertices.Length > 0 &&
                _dynamicMotionVertexBuffer is not null)
            {
                _motionVectors.RenderDynamic(
                    _commandList,
                    _dynamicMotionVertexBuffer,
                    _actorIndexBuffer,
                    opaqueDynamicEnd,
                    temporalFrame);
            }
        }

        var resolvedScene = _resolutionScaler.ResolvedSceneView;
        var bloomView = resolvedScene;

        if (settings.Bloom != BloomQuality.Off)
        {
            bloomView = _bloom.Render(
                _commandList,
                settings.Bloom,
                threshold: 0.72f);
        }

        var sceneOutputDescription =
            _resolutionScaler.SceneFramebuffer.OutputDescription;
        _postProcess.SetSources(
            resolvedScene,
            bloomView,
            _resolutionScaler.Width,
            _resolutionScaler.Height,
            sceneOutputDescription.ColorAttachments[0].Format);

        TextureView presentationSource = resolvedScene;
        Texture presentationTexture =
            _resolutionScaler.ResolvedSceneTexture;
        if (_postProcess.IsNeeded(settings))
        {
            presentationSource = _postProcess.Render(
                _commandList,
                settings,
                disableFxaa: fsr3Active);
            presentationTexture =
                _postProcess.OutputTexture;
        }

        var displaySource = presentationSource;
        var displayUpscaler = spatialFallbackUpscaler;
        var displaySharpness = settings.FsrSharpness;
        if ((fsr3Active || fgActive) &&
            _resolutionScaler.SampleableDepthTexture is { } fsrDepth)
        {
            var capturedDisplayBeforeSubmit = false;
            Exception? preSubmitFgFailure = null;
            if (!fsr3Active && fgActive && _frameGeneration is { } preSubmitFg)
            {
                try
                {
                    _resolutionScaler.CaptureDisplayColor(
                        _commandList,
                        preSubmitFg.HudlessFramebuffer,
                        displayUpscaler,
                        displaySharpness,
                        displaySource);
                    capturedDisplayBeforeSubmit = true;
                }
                catch (Exception exception)
                {
                    preSubmitFgFailure = exception;
                }
            }

            // Submit all scene, motion, reactive, bloom and post-process work
            // before recording AMD's native Vulkan compute dispatch.
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);

            var fsr3ProducedFrame = false;
            if (fsr3Active && _fsr3Upscaler is not null)
            {
                try
                {
                    _resolutionScaler.EnsureFsr3Output(displayWidth, displayHeight);
                    _fsr3Upscaler.EnsureOutputSize(displayWidth, displayHeight);
                    _fsr3Upscaler.Dispatch(
                        presentationTexture, fsrDepth, _motionVectors.MotionVectorTexture,
                        _reactiveMask.MaskTexture, _resolutionScaler.Fsr3OutputTexture,
                        temporalFrame, (float)Math.Max(0.00001, frameDeltaSeconds),
                        camera.NearPlane, camera.FarPlane, camera.FieldOfView, settings.FsrSharpness);
                    displaySource = _resolutionScaler.Fsr3OutputView;
                    displayUpscaler = UpscalerMode.Bilinear;
                    displaySharpness = 0f;
                    fsr3ProducedFrame = true;
                }
                catch (Exception exception)
                {
                    _fsr3DisabledAfterError = true;
                    _temporalInputsEnabled = FidelityFxStartupPolicy.NeedsTemporalInputs(
                        TemporalInputPolicy.IsEnabled(), _fsr3Requested, fsrFailed: true) || _frameGeneration is not null;
                    ResetTemporalHistory();
                    temporalFrame = temporalFrame with { ResetHistory = true };
                    EngineLog.Warn("Native FSR3 dispatch failed; disabling it for this run: " + exception.Message);
                }
            }

            try { _frameGeneration?.EnsureDisplaySize(displayWidth, displayHeight); }
            catch (Exception exception) { DisableSceneFrameGeneration(exception); }

            if (_frameGeneration is { } fgScene)
            {
                var fgFailure = preSubmitFgFailure;

                if (fsr3ProducedFrame)
                {
                    // Native FSR3 already produced display-size RGBA16F without
                    // HUD/UI. Feed that exact image into FG instead of spending
                    // another full-screen 1080p raster copy every rendered frame.
                    try
                    {
                        fgScene.Dispatch(
                            fsrDepth,
                            _motionVectors.MotionVectorTexture,
                            temporalFrame,
                            sceneFrameId,
                            (float)Math.Max(0.00001, frameDeltaSeconds),
                            camera.NearPlane,
                            camera.FarPlane,
                            camera.FieldOfView,
                            colorOverride: _resolutionScaler.Fsr3OutputTexture);
                        displaySource = _resolutionScaler.Fsr3OutputView;
                        displayUpscaler = UpscalerMode.Bilinear;
                        displaySharpness = 0f;
                    }
                    catch (Exception exception) { fgFailure = exception; }
                }
                else
                {
                    if (!capturedDisplayBeforeSubmit && fgFailure is null)
                    {
                        // Fallback when FSR3 failed mid-frame: capture a
                        // display-sized HUD-free image before FG dispatch.
                        _commandList.Begin();
                        try
                        {
                            _resolutionScaler.CaptureDisplayColor(
                                _commandList,
                                fgScene.HudlessFramebuffer,
                                displayUpscaler,
                                displaySharpness,
                                displaySource);
                        }
                        catch (Exception exception) { fgFailure = exception; }
                        _commandList.End();
                        _graphicsDevice.SubmitCommands(_commandList);
                    }

                    if (fgFailure is null)
                    {
                        try
                        {
                            fgScene.Dispatch(
                                fsrDepth,
                                _motionVectors.MotionVectorTexture,
                                temporalFrame,
                                sceneFrameId,
                                (float)Math.Max(0.00001, frameDeltaSeconds),
                                camera.NearPlane,
                                camera.FarPlane,
                                camera.FieldOfView);
                            // The real displayed frame uses exactly the image FG
                            // consumed. HUD/menu are drawn only after this pass.
                            displaySource = fgScene.HudlessView;
                            displayUpscaler = UpscalerMode.Bilinear;
                            displaySharpness = 0f;
                        }
                        catch (Exception exception) { fgFailure = exception; }
                    }
                }

                if (fgFailure is not null)
                    DisableSceneFrameGeneration(fgFailure);
            }

            // Continue normal generated/real presentation recording.
            _commandList.Begin();
        }

        // Build/upload menu geometry once. The same immutable GPU data is
        // then drawn on the generated and rendered presentations.
        _menu.Prepare(_commandList, displayWidth, displayHeight, menuView);

        var paired = _offscreenTarget is null && _framePresenter is not null &&
            _frameGeneration is { HasGeneratedFrame: true };
        var generatedPresented = false;
        if (paired)
        {
            // AMD has consumed the actual current scene and submitted the
            // interpolated image. Present it first, then use the freshly
            // acquired swapchain image for the real scene. World/input and UI
            // state are built once above; neither is advanced a second time.
            ComposeDisplay(_frameGeneration!.GeneratedView, UpscalerMode.Bilinear, 0f);
            BeforeFramePresentation?.Invoke(_commandList, swapchainFramebuffer,
                PresentedFrameKind.Generated, sceneFrameId);
            _commandList.End();
            generatedPresented = _framePresenter!.Submit(_commandList, PresentedFrameKind.Generated, sceneFrameId, paired: true);
            _commandList.Begin();
            swapchainFramebuffer = OutputFramebuffer;
        }
        ComposeDisplay(displaySource, displayUpscaler, displaySharpness);
        if (_framePresenter is not null)
            BeforeFramePresentation?.Invoke(_commandList, swapchainFramebuffer,
                PresentedFrameKind.Rendered, sceneFrameId);
        _commandList.End();
        if (_framePresenter is not null)
        {
            var renderedPresented = _framePresenter.Submit(_commandList, PresentedFrameKind.Rendered, sceneFrameId, paired);
            var presentedCount = (generatedPresented ? 1 : 0) + (renderedPresented ? 1 : 0);
            LastPresentedFrameCount = Math.Max(1, presentedCount);
        }
        else
        {
            _graphicsDevice.SubmitCommands(_commandList);
            if (_offscreenTarget is null) _graphicsDevice.SwapBuffers();
        }

        void ComposeDisplay(TextureView source, UpscalerMode upscaler, float sharpness)
        {
            _resolutionScaler.Present(_commandList, swapchainFramebuffer, upscaler, sharpness, source);
            if (_hudVertices.Count > 0)
            {
                _commandList.SetPipeline(_hudPipeline);
                _commandList.SetGraphicsResourceSet(0, _hudSet);
                _commandList.SetVertexBuffer(0, _hudVertexBuffer);
                _commandList.Draw((uint)_hudVertices.Count);
            }
            _menu.Draw(_commandList);
        }
    }

    internal void ResetTemporalHistory()
    {
        _temporalFrame.Reset();
        _dynamicMotionHistory.Reset();
        _frameGeneration?.RequestReset();
        _framePresenter?.ResetPacing();
        _hasPreviousCameraState = false;
    }

    private void DisableSceneFrameGeneration(Exception exception)
    {
        FrameGenerationDiagnostic = "Scene FG disabled: " + exception.Message;
        // Finish all native writes and renderer consumers before releasing
        // the output. The upscaler context and its setting stay independent.
        _frameGeneration?.Dispose();
        _frameGeneration = null;
        // Keep the already-created WSI presenter for the rest of this
        // renderer session. It now presents rendered-only frames through the
        // same semaphore-correct submit/present path. Switching back to plain
        // SwapBuffers mid-session would change synchronization models while
        // previous presentation work can still be owned by the present engine.
        // The presenter is disabled only at renderer shutdown/restart.
        _temporalInputsEnabled = FidelityFxStartupPolicy.NeedsTemporalInputs(
            TemporalInputPolicy.IsEnabled(), _fsr3Requested, _fsr3DisabledAfterError);
        EngineLog.Warn(FrameGenerationDiagnostic);
    }

    private void EnsureDynamicMotionCapacity(int vertexCount)
    {
        if (!_temporalInputsEnabled ||
            _graphicsDevice is null ||
            vertexCount <= 0)
        {
            return;
        }

        if (_dynamicMotionVertexBuffer is null)
        {
            _dynamicMotionVertexCapacity =
                Math.Max(64u, checked((uint)vertexCount));
            _dynamicMotionVertexBuffer =
                _graphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        DynamicMotionVertex.SizeInBytes *
                        _dynamicMotionVertexCapacity,
                        BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            return;
        }

        if ((uint)vertexCount <= _dynamicMotionVertexCapacity)
            return;

        _dynamicMotionVertexCapacity =
            Math.Max(
                checked((uint)vertexCount),
                _dynamicMotionVertexCapacity * 2);
        _graphicsDevice.WaitForIdle();
        _dynamicMotionVertexBuffer.Dispose();
        _dynamicMotionVertexBuffer =
            _graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    DynamicMotionVertex.SizeInBytes *
                    _dynamicMotionVertexCapacity,
                    BufferUsage.VertexBuffer | BufferUsage.Dynamic));
    }

    private void EnsureActorCapacity(int vertexCount, int indexCount)
    {
        if (_graphicsDevice is null)
        {
            return;
        }

        var growVertexBuffer = (uint)vertexCount > _actorVertexCapacity;
        var growIndexBuffer = (uint)indexCount > _actorIndexCapacity;
        if (!growVertexBuffer && !growIndexBuffer)
            return;

        // Both buffers can grow during the same scene build. Retire the
        // previous resources once, then recreate whichever capacities changed.
        _graphicsDevice.WaitForIdle();

        if (growVertexBuffer)
        {
            _actorVertexCapacity = Math.Max((uint)vertexCount, _actorVertexCapacity * 2);
            _actorVertexBuffer?.Dispose();
            _actorVertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                TerrainVertex.SizeInBytes * _actorVertexCapacity,
                BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        if (growIndexBuffer)
        {
            _actorIndexCapacity = Math.Max((uint)indexCount, _actorIndexCapacity * 2);
            _actorIndexBuffer?.Dispose();
            _actorIndexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(
                sizeof(uint) * _actorIndexCapacity,
                BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        }
    }

    private void BuildHud(
        float fps,
        float health,
        float maxHealth,
        float stamina,
        float maxStamina,
        float healthRatio,
        float staminaRatio,
        double animationSeconds,
        bool showFps,
        bool showGameplayHud)
    {
        _hudVertices.Clear();

        if (!showGameplayHud)
            return;

        var text = $"FPS {Math.Clamp((int)MathF.Round(fps), 0, 9999)}";
        const float x = 18f;
        const float y = 18f;
        const float pixel = 5f;
        const float gap = 2f;

        var cursor = x;
        if (showFps)
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
        maxHealth = MathF.Max(1f, maxHealth);
        maxStamina = MathF.Max(1f, maxStamina);
        health = Math.Clamp(health, 0f, maxHealth);
        stamina = Math.Clamp(stamina, 0f, maxStamina);
        const float barX = 18f;
        const float barWidth = 160f;
        const float barHeight = 9f;
        AddHudQuad(barX, 78f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(
            barX,
            78f,
            barWidth * healthRatio,
            barHeight,
            healthRatio <= 0.25f
                ? new Vector4(0.84f, 0.18f, 0.12f, 1f)
                : new Vector4(0.62f, 0.16f, 0.12f, 0.95f));
        AddHudQuad(barX, 96f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(
            barX,
            96f,
            barWidth * staminaRatio,
            barHeight,
            staminaRatio <= 0.18f
                ? new Vector4(0.88f, 0.76f, 0.20f, 1f)
                : new Vector4(0.72f, 0.58f, 0.18f, 0.95f));
        var warningPulse = ((int)Math.Floor(animationSeconds * 4.0)) % 2 == 0;
        if (healthRatio <= 0.25f && warningPulse)
            AddGameplayText("UWAGA: KRYTYCZNE ZYCIE", barX, 112f, 360f);
        if (staminaRatio <= 0.18f && warningPulse)
            AddGameplayText("UWAGA: BRAK STAMINY", barX, 134f, 360f);
    }

    private void AddGameplayText(string text, float x, float y, float availableWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var preparedText = BitmapFont.Prepare(text);
        if (string.IsNullOrWhiteSpace(preparedText))
            return;

        var width = preparedText.Length * BitmapFont.Advance;
        var scale = Math.Clamp(availableWidth / Math.Max(1, width), 0.8f, 2f);
        var (top, bottom) = BitmapFont.VerticalBounds(preparedText);
        AddHudQuad(x - 4, y + top * scale - 4, width * scale + 8,
            (bottom - top) * scale + 8, new Vector4(0, 0, 0, 0.7f));
        foreach (var character in preparedText)
        {
            if (BitmapFont.Glyphs.TryGetValue(character, out var glyph))
                for (var row = 0; row < glyph.Rows.Length; row++)
                for (var col = 0; col < BitmapFont.Width; col++)
                    if ((glyph.Rows[row] & (1 << (BitmapFont.Width - 1 - col))) != 0)
                    {
                        // HUD quads have one-pixel padding, compensate for text spacing.
                        var color = new Vector4(0.9f, 0.93f, 1f, 1f);
                        var px = x + col * scale; var py = y + (glyph.TopOffset + row) * scale;
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py + scale), color));
                    }
            x += BitmapFont.Advance * scale;
        }
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

    private void AddHudTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 color)
    {
        _hudVertices.Add(new HudVertex(a, color));
        _hudVertices.Add(new HudVertex(b, color));
        _hudVertices.Add(new HudVertex(c, color));
    }

    private void AddHudText(
        string text,
        float x,
        float y,
        float scale,
        Vector4 color,
        bool withBackground = false,
        Vector4? backgroundColor = null)
    {
        var preparedText = BitmapFont.Prepare(text);
        var width = preparedText.Length * BitmapFont.Advance;
        var (top, bottom) = BitmapFont.VerticalBounds(preparedText);
        if (withBackground)
        {
            var bg = backgroundColor ?? new Vector4(0f, 0f, 0f, 0.72f);
            AddHudQuad(x - 3f, y + top * scale - 2f, width * scale + 6f, (bottom - top) * scale + 4f, bg);
        }
        foreach (var character in preparedText)
        {
            if (BitmapFont.Glyphs.TryGetValue(character, out var glyph))
            {
                for (var row = 0; row < glyph.Rows.Length; row++)
                for (var col = 0; col < BitmapFont.Width; col++)
                {
                    if ((glyph.Rows[row] & (1 << (BitmapFont.Width - 1 - col))) != 0)
                    {
                        var px = x + col * scale;
                        var py = y + (glyph.TopOffset + row) * scale;
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py + scale), color));
                    }
                }
            }
            x += BitmapFont.Advance * scale;
        }
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
            ? new Vector3(0.0035f, 0.0060f, 0.0140f)
            : new Vector3(
                0.038f + daylight * 0.065f,
                0.060f + daylight * 0.085f,
                0.088f + daylight * 0.115f);

        var cloudColor = time.IsNight
            ? new Vector3(0.007f, 0.009f, 0.014f)
            : new Vector3(0.11f, 0.115f, 0.12f);
        var fogColor = time.IsNight
            ? new Vector3(0.010f, 0.012f, 0.016f)
            : new Vector3(0.19f, 0.20f, 0.19f);

        var cloudy = Vector3.Lerp(baseColor, cloudColor, weather.Cloudiness * 0.72f);
        var twilightTint = Vector3.Lerp(
            cloudy,
            new Vector3(0.35f, 0.135f, 0.055f),
            celestial.TwilightFactor * 0.36f * (1f - weather.Cloudiness * 0.5f));
        var fogBlend = Math.Clamp(weather.FogDensity / 0.032f, 0f, 1f) * 0.58f;
        var final = Vector3.Lerp(twilightTint, fogColor, fogBlend);
        return new RgbaFloat(final.X, final.Y, final.Z, 1f);
    }

    public void SetTextureQuality(TextureQuality quality)
    {
        if (_graphicsDevice is null)
            return;

        _terrain.SetTextureQuality(quality);
        _pbrModels.SetTextureQuality(quality);
    }

    public void SetUpscaler(UpscalerMode upscalerMode)
    {
        if (_graphicsDevice is null)
            return;

        _fsr3Requested =
            upscalerMode == UpscalerMode.Fsr3 ||
            _fsr3ForcedByEnvironment;

        if (_fsr3Requested &&
            _fsr3Upscaler is null &&
            !_fsr3DisabledAfterError &&
            _resolutionScaler.SampleableDepthView is { } temporalDepth &&
            _resolutionScaler.SampleableDepthTexture is { } temporalDepthTexture &&
            _cameraLayout is not null)
        {
            _graphicsDevice.WaitForIdle();
            EnsureTemporalMotionResources(temporalDepth, temporalDepthTexture);
            if (!_reactiveMask.IsInitialized)
            {
                _reactiveMask.Initialize(
                    _graphicsDevice,
                    _cameraLayout,
                    temporalDepthTexture,
                    _resolutionScaler.Width,
                    _resolutionScaler.Height);
            }

            var outputWidth = Math.Max(1u, OutputFramebuffer.Width);
            var outputHeight = Math.Max(1u, OutputFramebuffer.Height);
            if (FidelityFxUpscaler.TryCreate(
                    _graphicsDevice,
                    outputWidth,
                    outputHeight,
                    out _fsr3Upscaler,
                    out var fsr3Diagnostic))
            {
                _resolutionScaler.EnsureFsr3Output(outputWidth, outputHeight);
                EngineLog.Info(fsr3Diagnostic);
            }
            else
            {
                _fsr3DisabledAfterError = true;
                EngineLog.Warn(
                    "FSR3 request fell back to the normal presentation path: " +
                    fsr3Diagnostic);
            }
        }

        _temporalInputsEnabled =
            FidelityFxStartupPolicy.NeedsTemporalInputs(
                TemporalInputPolicy.IsEnabled(),
                _fsr3Requested,
                _fsr3DisabledAfterError) ||
            _frameGeneration is not null;
    }

    public bool TrySetFrameGeneration(bool enabled)
    {
        if (_graphicsDevice is null)
            return false;

        var requested =
            enabled ||
            FidelityFxSceneFrameGeneration.IsSceneValidationRequested();
        if (!requested)
        {
            if (_frameGeneration is not null)
            {
                _graphicsDevice.WaitForIdle();
                _frameGeneration.Dispose();
                _frameGeneration = null;
            }
            FrameGenerationDiagnostic = "Frame Generation is disabled in settings.";
            _temporalInputsEnabled =
                FidelityFxStartupPolicy.NeedsTemporalInputs(
                    TemporalInputPolicy.IsEnabled(),
                    _fsr3Requested,
                    _fsr3DisabledAfterError);
            ResetTemporalHistory();
            return true;
        }

        if (_frameGeneration is not null)
        {
            if (_offscreenTarget is null)
                _framePresenter ??= new FrameGenerationPresenter(_graphicsDevice);
            _temporalInputsEnabled = true;
            return true;
        }

        if (_resolutionScaler.SampleableDepthView is not { } temporalDepth ||
            _resolutionScaler.SampleableDepthTexture is not { } temporalDepthTexture)
        {
            return false;
        }

        _graphicsDevice.WaitForIdle();
        EnsureTemporalMotionResources(temporalDepth, temporalDepthTexture);

        if (FidelityFxSceneFrameGeneration.TryCreate(
                _graphicsDevice,
                out _frameGeneration,
                out var fgDiagnostic))
        {
            FrameGenerationDiagnostic = fgDiagnostic;
            if (_offscreenTarget is null)
                _framePresenter ??= new FrameGenerationPresenter(_graphicsDevice);
            _temporalInputsEnabled = true;
            ResetTemporalHistory();
            EngineLog.Info("Frame Generation setting: " + fgDiagnostic);
            return true;
        }

        FrameGenerationDiagnostic = fgDiagnostic;
        _temporalInputsEnabled =
            FidelityFxStartupPolicy.NeedsTemporalInputs(
                TemporalInputPolicy.IsEnabled(),
                _fsr3Requested,
                _fsr3DisabledAfterError);
        EngineLog.Warn("Frame Generation setting: " + fgDiagnostic);
        return true;
    }

    private void EnsureTemporalMotionResources(
        TextureView temporalDepth,
        Texture temporalDepthTexture)
    {
        if (_graphicsDevice is null)
            return;

        if (!_motionVectors.IsInitialized)
        {
            _motionVectors.Initialize(
                _graphicsDevice,
                temporalDepth,
                temporalDepthTexture,
                _resolutionScaler.Width,
                _resolutionScaler.Height);
        }

        if (_dynamicMotionVertexBuffer is null)
        {
            _dynamicMotionVertexCapacity = 64;
            _dynamicMotionVertexBuffer =
                _graphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        DynamicMotionVertex.SizeInBytes *
                        _dynamicMotionVertexCapacity,
                        BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }
    }

    public void SetRenderResolution(int width, int height)
    {
        if (_graphicsDevice is null)
            return;

        var renderWidth = checked((uint)Math.Max(1, width));
        var renderHeight = checked((uint)Math.Max(1, height));
        var requestedWidth = renderWidth;
        var requestedHeight = renderHeight;

        // Enforce the native FSR3/FG contract at the renderer boundary too,
        // not only in the settings catalog. This protects direct callers and
        // the one-frame window-resize interval before settings are reapplied.
        if (_fsr3Requested || _frameGeneration is not null || _framePresenter is not null)
        {
            var outputWidth = Math.Max(1u, OutputFramebuffer.Width);
            var outputHeight = Math.Max(1u, OutputFramebuffer.Height);
            if (renderWidth > outputWidth || renderHeight > outputHeight)
            {
                var scale = Math.Min(
                    outputWidth / (double)renderWidth,
                    outputHeight / (double)renderHeight);
                renderWidth = Math.Max(1u, (uint)Math.Floor(renderWidth * scale));
                renderHeight = Math.Max(1u, (uint)Math.Floor(renderHeight * scale));
                if (renderWidth > 1 && (renderWidth & 1u) != 0) renderWidth--;
                if (renderHeight > 1 && (renderHeight & 1u) != 0) renderHeight--;
            }
        }

        if (_resolutionScaler.Width == renderWidth &&
            _resolutionScaler.Height == renderHeight)
            return;

        if (renderWidth != requestedWidth || renderHeight != requestedHeight)
        {
            EngineLog.Warn(
                $"Temporal render size {requestedWidth}x{requestedHeight} exceeds " +
                $"the display; clamped to {renderWidth}x{renderHeight}.");
        }

        // Scene color/depth, motion and reactive targets all retire resources
        // from the same preceding frames. Synchronize once instead of forcing
        // up to three device-wide waits while cycling graphics resolutions.
        _graphicsDevice.WaitForIdle();
        _resolutionScaler.SetResolution(
            renderWidth,
            renderHeight,
            deviceAlreadyIdle: true);

        ResetTemporalHistory();
        if (_temporalInputsEnabled &&
            _motionVectors.IsInitialized &&
            _resolutionScaler.SampleableDepthView is { } temporalDepth &&
            _resolutionScaler.SampleableDepthTexture is { } temporalDepthTexture)
        {
            _motionVectors.SetSource(
                temporalDepth,
                temporalDepthTexture,
                _resolutionScaler.Width,
                _resolutionScaler.Height,
                deviceAlreadyIdle: true);
        }
        if (_temporalInputsEnabled &&
            _reactiveMask.IsInitialized &&
            _resolutionScaler.SampleableDepthTexture is { } reactiveDepthTexture)
        {
            _reactiveMask.SetDepthSource(
                reactiveDepthTexture,
                _resolutionScaler.Width,
                _resolutionScaler.Height,
                deviceAlreadyIdle: true);
        }

        var sceneOutput =
            _resolutionScaler.SceneFramebuffer.OutputDescription;
        _bloom.SetSource(
            _resolutionScaler.ResolvedSceneView,
            _resolutionScaler.Width,
            _resolutionScaler.Height,
            BloomQuality.Medium);
        _postProcess.SetSources(
            _resolutionScaler.ResolvedSceneView,
            _bloom.OutputView,
            _resolutionScaler.Width,
            _resolutionScaler.Height,
            sceneOutput.ColorAttachments[0].Format);
    }

    public void SetShadowResolution(uint mapSize)
    {
        if (_graphicsDevice is null)
            return;

        _shadows.SetMapSize(mapSize);
    }

    public void SetShadowDistance(float distance)
    {
        if (_graphicsDevice is null)
            return;

        _shadows.SetWorldSpan(Math.Max(40f, distance) * 2f);
    }

    public void SetVSync(bool enabled)
    {
        if (_graphicsDevice is null)
            return;

        var changed =
            _graphicsDevice.SyncToVerticalBlank != enabled;
        PresentationPolicy.Apply(enabled);
        _graphicsDevice.SyncToVerticalBlank = enabled;
        if (changed)
            ResetTemporalHistory();
    }

    public void Resize(uint width, uint height)
    {
        if (_graphicsDevice is null || width == 0 || height == 0)
        {
            return;
        }

        _graphicsDevice.ResizeMainWindow(width, height);
        ResetTemporalHistory();
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

        _frameGeneration?.Dispose();
        _frameGeneration = null;
        _framePresenter?.Dispose();
        _framePresenter = null;

        _sky.Dispose();
        _menu.Dispose();
        _postProcess.Dispose();
        _bloom.Dispose();
        _fsr3Upscaler?.Dispose();
        _fsr3Upscaler = null;
        _reactiveMask.Dispose();
        _motionVectors.Dispose();
        _resolutionScaler.Dispose();
        _shadows.Dispose();
        _terrain.Dispose();
        _pbrModels.Dispose();
        _farVegetation.Dispose();
        _characters.Dispose();

        _hudPipeline?.Dispose();
        _hudSet?.Dispose();
        _hudLayout?.Dispose();
        _hudVertexBuffer?.Dispose();
        _hudScreenBuffer?.Dispose();

        _actorPipeline?.Dispose();
        _cameraSet?.Dispose();
        _cameraLayout?.Dispose();
        _projectionBuffer?.Dispose();
        _viewBuffer?.Dispose();
        _atmosphereBuffer?.Dispose();
        _actorVertexBuffer?.Dispose();
        _actorIndexBuffer?.Dispose();
        _dynamicMotionVertexBuffer?.Dispose();

        if (_hudShaders is not null)
        {
            foreach (var shader in _hudShaders) shader.Dispose();
        }

        if (_actorShaders is not null)
        {
            foreach (var shader in _actorShaders) shader.Dispose();
        }

        _hudShaders = null;
        _actorShaders = null;
        _playerModel = null;
        _enemyModel = null;
        _bestiaryModels.Clear();
        _worldItemModels.Clear();
        _hudPipeline = null;
        _hudSet = null;
        _hudLayout = null;
        _hudVertexBuffer = null;
        _hudScreenBuffer = null;
        _actorPipeline = null;
        _cameraSet = null;
        _cameraLayout = null;
        _projectionBuffer = null;
        _viewBuffer = null;
        _atmosphereBuffer = null;
        _actorVertexBuffer = null;
        _actorIndexBuffer = null;
        _dynamicMotionVertexBuffer = null;
        _commandList?.Dispose();
        if (_ownsDevice) _graphicsDevice.Dispose();

        _commandList = null;
        _graphicsDevice = null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct AtmosphereUniform(
        Vector4 fogParameters,
        Vector4 lightingParameters,
        Vector4 sunColorTime,
        Vector4 skyWeather,
        Vector4 moonParameters,
        Vector4 celestialParameters,
        Vector4 graphicsFeatures0,
        Vector4 graphicsFeatures1,
        Vector4 graphicsFeatures2,
        Vector4 graphicsFeatures3)
    {
        public readonly Vector4 FogParameters = fogParameters;
        public readonly Vector4 LightingParameters = lightingParameters;
        public readonly Vector4 SunColorTime = sunColorTime;
        public readonly Vector4 SkyWeather = skyWeather;
        public readonly Vector4 MoonParameters = moonParameters;
        public readonly Vector4 CelestialParameters = celestialParameters;
        public readonly Vector4 GraphicsFeatures0 = graphicsFeatures0;
        public readonly Vector4 GraphicsFeatures1 = graphicsFeatures1;
        public readonly Vector4 GraphicsFeatures2 = graphicsFeatures2;
        public readonly Vector4 GraphicsFeatures3 = graphicsFeatures3;
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
