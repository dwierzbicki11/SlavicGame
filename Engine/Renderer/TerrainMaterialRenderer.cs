using Veldrid;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public readonly struct TerrainSurfaceVertex
{
    public const uint SizeInBytes = 48;

    public readonly System.Numerics.Vector3 Position;
    public readonly System.Numerics.Vector3 Normal;
    public readonly System.Numerics.Vector3 PrimaryWeights;
    public readonly System.Numerics.Vector3 SecondaryWeights;

    public TerrainSurfaceVertex(
        System.Numerics.Vector3 position,
        System.Numerics.Vector3 normal,
        System.Numerics.Vector3 primaryWeights,
        System.Numerics.Vector3 secondaryWeights)
    {
        Position = position;
        Normal = normal;
        PrimaryWeights = primaryWeights;
        SecondaryWeights = secondaryWeights;
    }
}

public sealed class TerrainMaterialRenderer : IDisposable
{
    private readonly List<Texture> _textures = [];
    private readonly List<TextureView> _views = [];

    private readonly TerrainGeometry?[] _geometries = new TerrainGeometry?[3];
    private ResourceLayout? _materialLayout;
    private readonly ResourceSet?[] _materialSets = new ResourceSet?[4];
    private readonly Sampler?[] _qualitySamplers = new Sampler?[4];
    private TextureQuality _textureQuality = TextureQuality.High;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private bool _disposed;

    public void Initialize(
        GraphicsDevice graphicsDevice,
        ResourceLayout cameraLayout,
        ResourceLayout shadowLayout,
        OutputDescription outputDescription,
        Terrain terrain,
        string assetsRoot,
        TextureQuality textureQuality)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(cameraLayout);
        ArgumentNullException.ThrowIfNull(shadowLayout);
        ArgumentNullException.ThrowIfNull(terrain);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetsRoot);
        _textureQuality = textureQuality;

        var factory = graphicsDevice.ResourceFactory;
        _geometries[0] = CreateGeometry(
            graphicsDevice,
            factory,
            terrain,
            GraphicsQualityCatalog.TerrainMeshStep(TerrainDetailQuality.Low));
        _geometries[1] = CreateGeometry(
            graphicsDevice,
            factory,
            terrain,
            GraphicsQualityCatalog.TerrainMeshStep(TerrainDetailQuality.Medium));
        _geometries[2] = CreateGeometry(
            graphicsDevice,
            factory,
            terrain,
            GraphicsQualityCatalog.TerrainMeshStep(TerrainDetailQuality.High));

        _materialLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            Texture("GrassBase"),
            Texture("GrassNormal"),
            Texture("GrassRoughness"),
            Texture("GrassAo"),
            Texture("GrassHeight"),
            Texture("PathBase"),
            Texture("PathNormal"),
            Texture("PathRoughness"),
            Texture("PathAo"),
            Texture("PathHeight"),
            Texture("LitterBase"),
            Texture("LitterNormal"),
            Texture("LitterRoughness"),
            Texture("LitterAo"),
            Texture("LitterHeight"),
            Texture("MudBase"),
            Texture("MudNormal"),
            Texture("MudRoughness"),
            Texture("MudAo"),
            Texture("MudHeight"),
            Texture("SwampBase"),
            Texture("SwampNormal"),
            Texture("SwampRoughness"),
            Texture("SwampAo"),
            Texture("SwampHeight"),
            Texture("RockBase"),
            Texture("RockNormal"),
            Texture("RockRoughness"),
            Texture("RockAo"),
            Texture("RockHeight"),
            new ResourceLayoutElementDescription(
                "TerrainSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        var resources = new List<BindableResource>
        {
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_grass", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_grass", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_grass", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_grass", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_grass", "height", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "dirt_path", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "dirt_path", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "dirt_path", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "dirt_path", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "dirt_path", "height", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_litter", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_litter", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_litter", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_litter", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "forest_litter", "height", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "wet_mud", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "wet_mud", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "wet_mud", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "wet_mud", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "wet_mud", "height", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "swamp_ground", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "swamp_ground", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "swamp_ground", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "swamp_ground", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "swamp_ground", "height", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "mossy_rock", "basecolor", srgb: true, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "mossy_rock", "normal", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "mossy_rock", "roughness", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "mossy_rock", "ao", srgb: false, textureQuality),
            LoadTexture(graphicsDevice, factory, assetsRoot, "mossy_rock", "height", srgb: false, textureQuality)
        };

        for (var qualityIndex = 0; qualityIndex < _materialSets.Length; qualityIndex++)
        {
            var quality = (TextureQuality)qualityIndex;
            var sampler = TextureQualityResources.CreateSampler(
                graphicsDevice,
                factory,
                quality);
            _qualitySamplers[qualityIndex] = sampler;

            var bindings = resources
                .Concat<BindableResource>([sampler])
                .ToArray();
            _materialSets[qualityIndex] = factory.CreateResourceSet(
                new ResourceSetDescription(_materialLayout, bindings));
        }

        _shaders = ShaderLibrary.LoadPair(factory, "terrain");
        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription(
                "Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
            new VertexElementDescription(
                "PrimaryWeights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription(
                "SecondaryWeights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

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
            [cameraLayout, _materialLayout, shadowLayout],
            outputDescription));

        var low = _geometries[0]!;
        var medium = _geometries[1]!;
        var high = _geometries[2]!;
        EngineLog.Info(
            $"Terrain geometry LOD initialized: " +
            $"LOW={low.VertexCount} vertices/{low.IndexCount / 3} tris, " +
            $"MEDIUM={medium.VertexCount} vertices/{medium.IndexCount / 3} tris, " +
            $"HIGH={high.VertexCount} vertices/{high.IndexCount / 3} tris; " +
            $"6 PBR surface materials, 30 textures, texture quality={textureQuality}.");
    }

    public void Render(
        CommandList commandList,
        ResourceSet cameraSet,
        ResourceSet shadowSet,
        TerrainDetailQuality terrainDetail)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var materialSet = _materialSets[(int)_textureQuality];
        var geometry = GetGeometry(terrainDetail);
        if (_pipeline is null || materialSet is null)
            throw new InvalidOperationException("Terrain material renderer is not initialized.");

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);
        commandList.SetGraphicsResourceSet(1, materialSet);
        commandList.SetGraphicsResourceSet(2, shadowSet);
        commandList.SetVertexBuffer(0, geometry.VertexBuffer);
        commandList.SetIndexBuffer(geometry.IndexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed(geometry.IndexCount);
    }

    public void RenderShadow(
        CommandList commandList,
        Pipeline shadowPipeline,
        ResourceSet shadowDepthSet,
        TerrainDetailQuality terrainDetail)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var geometry = GetGeometry(terrainDetail);

        commandList.SetPipeline(shadowPipeline);
        commandList.SetGraphicsResourceSet(0, shadowDepthSet);
        commandList.SetVertexBuffer(0, geometry.VertexBuffer);
        commandList.SetIndexBuffer(geometry.IndexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed(geometry.IndexCount);
    }

    private static TerrainGeometry CreateGeometry(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        Terrain terrain,
        int gridStep)
    {
        TerrainMesh.Build(terrain, gridStep, out var baseVertices, out var indices);
        var vertices = new TerrainSurfaceVertex[baseVertices.Length];

        for (var i = 0; i < baseVertices.Length; i++)
        {
            var source = baseVertices[i];
            var weights = TerrainSurfaceClassifier.Classify(
                source.Position,
                source.Normal);
            vertices[i] = new TerrainSurfaceVertex(
                source.Position,
                source.Normal,
                weights.Primary,
                weights.Secondary);
        }

        var vertexBuffer = factory.CreateBuffer(new BufferDescription(
            TerrainSurfaceVertex.SizeInBytes * checked((uint)vertices.Length),
            BufferUsage.VertexBuffer));
        var indexBuffer = factory.CreateBuffer(new BufferDescription(
            sizeof(uint) * checked((uint)indices.Length),
            BufferUsage.IndexBuffer));

        graphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices);
        graphicsDevice.UpdateBuffer(indexBuffer, 0, indices);

        return new TerrainGeometry(
            vertexBuffer,
            indexBuffer,
            checked((uint)indices.Length),
            vertices.Length);
    }

    private TerrainGeometry GetGeometry(TerrainDetailQuality quality)
    {
        var index = quality switch
        {
            TerrainDetailQuality.Low => 0,
            TerrainDetailQuality.Medium => 1,
            _ => 2
        };

        return _geometries[index] ??
            throw new InvalidOperationException("Terrain geometry LOD is not initialized.");
    }

    private TextureView LoadTexture(
        GraphicsDevice graphicsDevice,
        ResourceFactory factory,
        string assetsRoot,
        string material,
        string map,
        bool srgb,
        TextureQuality textureQuality)
    {
        var path = Path.Combine(
            assetsRoot,
            "textures",
            "terrain",
            material,
            $"{material}_{map}.png");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Required terrain texture '{material}/{map}' is missing.",
                path);

        using var stream = File.OpenRead(path);
        var texture = TextureQualityResources.CreateTexture(
            graphicsDevice,
            factory,
            stream,
            srgb,
            textureQuality,
            out _,
            out _);
        var view = factory.CreateTextureView(texture);
        _textures.Add(texture);
        _views.Add(view);
        return view;
    }

    public void SetTextureQuality(TextureQuality quality) =>
        _textureQuality = quality;

    private static ResourceLayoutElementDescription Texture(string name) =>
        new(name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var set in _materialSets)
            set?.Dispose();
        foreach (var sampler in _qualitySamplers)
            sampler?.Dispose();
        _materialLayout?.Dispose();
        _pipeline?.Dispose();
        foreach (var geometry in _geometries)
            geometry?.Dispose();

        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        foreach (var view in _views)
            view.Dispose();
        foreach (var texture in _textures)
            texture.Dispose();

        _views.Clear();
        _textures.Clear();
        Array.Clear(_materialSets);
        Array.Clear(_qualitySamplers);
        _materialLayout = null;
        _pipeline = null;
        Array.Clear(_geometries);
        _shaders = null;
    }

    private sealed record TerrainGeometry(
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        uint IndexCount,
        int VertexCount) : IDisposable
    {
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
