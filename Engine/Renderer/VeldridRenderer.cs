using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.Settings;
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
    private readonly BloomRenderer _bloom = new();
    private readonly PostProcessRenderer _postProcess = new();
    private readonly PbrModelRenderer _pbrModels = new();
    private readonly FarVegetationRenderer _farVegetation = new();

    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
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
    private uint _actorIndexCount;
    private uint _actorVertexCapacity;
    private uint _actorIndexCapacity;
    private uint _hudVertexCapacity;

    public GraphicsDevice GraphicsDevice =>
        _graphicsDevice ?? throw new InvalidOperationException("Renderer has not been initialized.");

    public void Initialize(
        GameWindow window,
        WorldState world,
        bool vsync,
        TextureQuality textureQuality,
        MsaaQuality msaaQuality)
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
                msaaQuality);
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
        MsaaQuality msaaQuality)
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
                "Atmosphere", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

        _cameraSet = factory.CreateResourceSet(new ResourceSetDescription(
            _cameraLayout,
            _projectionBuffer,
            _viewBuffer,
            _atmosphereBuffer));

        _shadows.Initialize(_graphicsDevice);

        _resolutionScaler.Initialize(
            _graphicsDevice,
            _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            _graphicsDevice.SwapchainFramebuffer.Width,
            _graphicsDevice.SwapchainFramebuffer.Height,
            msaaQuality);

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
            _graphicsDevice.SwapchainFramebuffer.OutputDescription);

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

        var swapchainFramebuffer = _graphicsDevice.SwapchainFramebuffer;
        if (swapchainFramebuffer is null)
        {
            return;
        }

        var framebuffer = _resolutionScaler.SceneFramebuffer;
        var width = Math.Max(1u, framebuffer.Width);
        var height = Math.Max(1u, framebuffer.Height);
        var displayWidth = Math.Max(1u, swapchainFramebuffer.Width);
        var displayHeight = Math.Max(1u, swapchainFramebuffer.Height);
        var aspect = MathF.Max(0.1f, (float)width / height);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FieldOfView, aspect, camera.NearPlane, camera.FarPlane);
        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);

        ActorModelMesh.Build(
            world,
            _playerModel,
            _npcModels,
            _enemyModel,
            camera.Position,
            animationSeconds,
            camera.Yaw,
            camera.Mode != CameraMode.FirstPerson,
            out var actorVertices,
            out var actorIndices);
        WildlifeModelMesh.Append(
            world,
            _wildlifeModels,
            animationSeconds,
            camera.Position,
            MathF.Min(
                GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
                220f),
            ref actorVertices,
            ref actorIndices);
        BowPresentationMesh.Append(
            world,
            _bowModel!,
            _arrowModel!,
            camera.Position,
            camera.GetLookDirection(),
            camera.GetMoveRight(),
            camera.Mode == CameraMode.FirstPerson,
            ref actorVertices,
            ref actorIndices);
        WorldItemModelMesh.Append(
            world,
            _worldItemModels,
            ref actorVertices,
            ref actorIndices);
        WaterLandscape.AppendSurface(world.Terrain, (float)animationSeconds, camera.Position,
            GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
            ref actorVertices, ref actorIndices);
        WaterInteractionMesh.Append(
            world,
            (float)animationSeconds,
            ref actorVertices,
            ref actorIndices);
        CampfireEffectMesh.Append(
            world,
            (float)animationSeconds,
            ref actorVertices,
            ref actorIndices);
        RainEffectMesh.Append(
            world,
            camera.Position,
            (float)animationSeconds,
            settings.CloudQuality,
            ref actorVertices,
            ref actorIndices);
        FootprintEffectMesh.Append(
            world,
            ref actorVertices,
            ref actorIndices);
        WildlifeTrackEffectMesh.Append(
            world,
            ref actorVertices,
            ref actorIndices);
        ApparitionEffectMesh.Append(
            world,
            (float)animationSeconds,
            ref actorVertices,
            ref actorIndices);
        MagicEffectMesh.Append(world, ref actorVertices, ref actorIndices);
        EnsureActorCapacity(actorVertices.Length, actorIndices.Length);
        _actorIndexCount = (uint)actorIndices.Length;

        BuildHud(
            (float)Math.Max(0, fps),
            world.Player.Health / world.Player.MaxHealth,
            world.Player.Stamina / world.Player.MaxStamina,
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
                var currentSpellLearned = SlavicGame.Engine.Magic.SpellLessons.IsLearned(world, world.Magic.Current.Id);
                AddGameplayText(
                    currentSpellLearned
                        ? $"Q CZAR: {world.Magic.Current.Name} / F RZUC / L NAUKA / R RYTUAL"
                        : "Q CZAR: NIEPOZNANY / L NAUKA / R RYTUAL",
                    18, 122, displayWidth - 36);
                AddGameplayText(
                    currentSpellLearned
                        ? $"{world.Magic.Current.Incantation} / KOSZT {world.Magic.Current.Cost:0} / ODNOWIENIE {world.Magic.Cooldown:0.0}"
                        : "INKANTACJA I KOSZT POZOSTAJA NIEZNANE",
                    18, 146, displayWidth - 36);
                AddGameplayText(world.Magic.Message, 18, 170, displayWidth - 36);
                AddGameplayText(world.SpellLearning.Message, 18, 194, displayWidth - 36);
                AddGameplayText(world.Rituals.Message, 18, 218, displayWidth - 36);
                var interactionText =
                    world.QuestInteractions.Current?.Prompt ??
                    world.EnvironmentInteractions.Current?.Prompt;

                if (string.IsNullOrWhiteSpace(interactionText))
                    interactionText = world.Bow.RetrievalPrompt(world);

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

                if (!string.IsNullOrWhiteSpace(interactionText))
                    AddGameplayText(interactionText, 18, 242, displayWidth - 36);

                AddGameplayText(
                    world.Bow.IsAiming
                        ? world.Bow.HudText(world)
                        : world.Melee.Message,
                    18,
                    266,
                    displayWidth - 36);

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
                        290,
                        displayWidth - 36);
                }

                if (!string.IsNullOrWhiteSpace(world.WaterInteraction.HudStatus))
                {
                    AddGameplayText(
                        world.WaterInteraction.HudStatus,
                        18,
                        314,
                        displayWidth - 36);
                }

                var wildlifeTrackStatus =
                    world.WildlifeTracks.HudStatus(world);
                if (!string.IsNullOrWhiteSpace(wildlifeTrackStatus))
                {
                    AddGameplayText(
                        wildlifeTrackStatus,
                        18,
                        338,
                        displayWidth - 36);
                }

                if (world.Bow.IsAiming)
                {
                    var reticleX = displayWidth * 0.5f;
                    var reticleY = displayHeight * 0.5f;
                    var reticle = new Vector4(0.92f, 0.92f, 0.86f, 0.92f);

                    AddHudQuad(reticleX - 13f, reticleY - 1f, 9f, 2f, reticle);
                    AddHudQuad(reticleX + 4f, reticleY - 1f, 9f, 2f, reticle);
                    AddHudQuad(reticleX - 1f, reticleY - 13f, 2f, 9f, reticle);
                    AddHudQuad(reticleX - 1f, reticleY + 4f, 2f, 9f, reticle);

                    AddGameplayText(
                        world.Bow.Message,
                        18,
                        362,
                        displayWidth - 36);
                }
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
            settings.SunShadows ? 1f : 0f,
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

        _commandList.Begin();
        _commandList.UpdateBuffer(_projectionBuffer, 0, projection);
        _commandList.UpdateBuffer(_viewBuffer, 0, view);
        _commandList.UpdateBuffer(_atmosphereBuffer, 0, fogParameters);
        _commandList.UpdateBuffer(_atmosphereBuffer, 16, lightingParameters);
        _commandList.UpdateBuffer(_atmosphereBuffer, 32, sunColorTime);
        _commandList.UpdateBuffer(_atmosphereBuffer, 48, skyWeather);
        _commandList.UpdateBuffer(_atmosphereBuffer, 64, moonParameters);
        _commandList.UpdateBuffer(_atmosphereBuffer, 80, celestialParameters);
        _commandList.UpdateBuffer(_atmosphereBuffer, 96, graphicsFeatures0);
        _commandList.UpdateBuffer(_atmosphereBuffer, 112, graphicsFeatures1);
        _commandList.UpdateBuffer(_atmosphereBuffer, 128, graphicsFeatures2);
        _commandList.UpdateBuffer(_atmosphereBuffer, 144, graphicsFeatures3);
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

        _shadows.UpdateLight(
            _commandList,
            camera.Position,
            celestial.SunDirection);
        _shadows.BeginDepthPass(_commandList);

        var shadowEnabled =
            settings.SunShadows &&
            celestial.SunIntensity > 0.02f &&
            celestial.SunDirection.Y > 0.02f;

        if (shadowEnabled)
        {
            _terrain.RenderShadow(
                _commandList,
                _shadows.TerrainPipeline,
                _shadows.DepthSet,
                settings.TerrainDetail);
            _pbrModels.RenderShadow(
                _commandList,
                _shadows.PbrPipeline,
                _shadows.DepthSet,
                camera.Position,
                GraphicsQualityCatalog.ShadowDistance(settings.ShadowDistance),
                GraphicsQualityCatalog.VegetationDistance(settings.VegetationDistance),
                GraphicsQualityCatalog.GroundClutterDistance(settings.GroundClutter),
                settings.ModelLod,
                settings.FarVegetation);
            _shadows.RenderActors(
                _commandList,
                _actorVertexBuffer,
                _actorIndexBuffer,
                _actorIndexCount);
        }

        _commandList.SetFramebuffer(framebuffer);
        _commandList.SetFullViewports();
        _commandList.SetFullScissorRects();
        _commandList.ClearColorTarget(0, atmosphereColor);
        _commandList.ClearDepthStencil(1f);

        _sky.Render(_commandList, _cameraSet);

        _terrain.Render(
            _commandList,
            _cameraSet,
            _shadows.SampleSet,
            settings.TerrainDetail);

        _pbrModels.Render(
            _commandList,
            _cameraSet,
            _shadows.SampleSet,
            camera.Position,
            cameraFrustum,
            GraphicsQualityCatalog.RenderDistance(settings.RenderDistance),
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

        _commandList.SetPipeline(_actorPipeline);
        _commandList.SetGraphicsResourceSet(0, _cameraSet);
        _commandList.SetGraphicsResourceSet(1, _shadows.SampleSet);

        if (_actorIndexCount > 0)
        {
            _commandList.SetVertexBuffer(0, _actorVertexBuffer);
            _commandList.SetIndexBuffer(_actorIndexBuffer, IndexFormat.UInt32);
            _commandList.DrawIndexed(_actorIndexCount);
        }

        _resolutionScaler.ResolveScene(_commandList);

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
        if (_postProcess.IsNeeded(settings))
        {
            presentationSource = _postProcess.Render(
                _commandList,
                settings);
        }

        _resolutionScaler.Present(
            _commandList,
            swapchainFramebuffer,
            settings.Upscaler,
            settings.FsrSharpness,
            presentationSource);

        if (_hudVertices.Count > 0)
        {
            _commandList.SetPipeline(_hudPipeline);
            _commandList.SetGraphicsResourceSet(0, _hudSet);
            _commandList.SetVertexBuffer(0, _hudVertexBuffer);
            _commandList.Draw((uint)_hudVertices.Count);
        }

        _menu.Render(
            _commandList,
            displayWidth,
            displayHeight,
            menuView);

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

    private void BuildHud(
        float fps,
        float healthRatio,
        float staminaRatio,
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
        const float barX = 18f;
        const float barWidth = 160f;
        const float barHeight = 9f;
        AddHudQuad(barX, 78f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(barX, 78f, barWidth * healthRatio, barHeight, new Vector4(0.62f, 0.16f, 0.12f, 0.95f));
        AddHudQuad(barX, 96f, barWidth, barHeight, new Vector4(0.08f, 0.07f, 0.06f, 0.82f));
        AddHudQuad(barX, 96f, barWidth * staminaRatio, barHeight, new Vector4(0.72f, 0.58f, 0.18f, 0.95f));
    }

    private void AddGameplayText(string text, float x, float y, float availableWidth)
    {
        var scale = Math.Clamp(availableWidth / Math.Max(1, text.Length * 6f), 0.8f, 2f);
        AddHudQuad(x - 4, y - 4, text.Length * 6f * scale + 8, 7 * scale + 8, new Vector4(0, 0, 0, 0.7f));
        foreach (var character in text.ToUpperInvariant())
        {
            if (MenuRenderer.Font.TryGetValue(character, out var glyph))
                for (var row = 0; row < 7; row++)
                for (var col = 0; col < 5; col++)
                    if ((glyph[row] & (1 << (4 - col))) != 0)
                    {
                        // HUD quads have one-pixel padding, compensate for text spacing.
                        var color = new Vector4(0.9f, 0.93f, 1f, 1f);
                        var px = x + col * scale; var py = y + row * scale;
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px + scale, py + scale), color));
                        _hudVertices.Add(new HudVertex(new Vector2(px, py + scale), color));
                    }
            x += 6 * scale;
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
                0.025f + daylight * 0.055f,
                0.045f + daylight * 0.075f,
                0.065f + daylight * 0.095f);

        var cloudColor = time.IsNight
            ? new Vector3(0.007f, 0.009f, 0.014f)
            : new Vector3(0.09f, 0.095f, 0.10f);
        var fogColor = time.IsNight
            ? new Vector3(0.010f, 0.012f, 0.016f)
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

    public void SetTextureQuality(TextureQuality quality)
    {
        if (_graphicsDevice is null)
            return;

        _terrain.SetTextureQuality(quality);
        _pbrModels.SetTextureQuality(quality);
    }

    public void SetRenderResolution(int width, int height)
    {
        if (_graphicsDevice is null)
            return;

        _resolutionScaler.SetResolution(
            checked((uint)Math.Max(1, width)),
            checked((uint)Math.Max(1, height)));

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

        PresentationPolicy.Apply(enabled);
        _graphicsDevice.SyncToVerticalBlank = enabled;
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
        _menu.Dispose();
        _postProcess.Dispose();
        _bloom.Dispose();
        _resolutionScaler.Dispose();
        _shadows.Dispose();
        _terrain.Dispose();
        _pbrModels.Dispose();
        _farVegetation.Dispose();

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
