using System.Numerics;
using System.Runtime.InteropServices;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.World;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class FarVegetationRenderer : IDisposable
{
    private const float ChunkSize = 192f;

    private readonly List<ProxyBatch> _batches = [];
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private bool _disposed;

    public int TreeCount { get; private set; }
    public int BatchCount => _batches.Count;

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

        var factory = graphicsDevice.ResourceFactory;
        _shaders = ShaderLibrary.LoadPair(factory, "vegetation_impostor");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementSemantic.Position,
                VertexElementFormat.Float3),
            new VertexElementDescription(
                "TexCoord",
                VertexElementSemantic.TextureCoordinate,
                VertexElementFormat.Float2),
            new VertexElementDescription(
                "ColorType",
                VertexElementSemantic.Color,
                VertexElementFormat.Float4),
            new VertexElementDescription(
                "WindPhase",
                VertexElementSemantic.TextureCoordinate,
                VertexElementFormat.Float1));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription([vertexLayout], _shaders),
            [cameraLayout],
            outputDescription));

        var treeGroups = world.Models
            .Where(instance => IsTreeAsset(instance.AssetPath))
            .GroupBy(instance => instance.AssetPath, StringComparer.Ordinal)
            .ToArray();

        var dimensionsByAsset = new Dictionary<string, ProxyDimensions>(
            StringComparer.Ordinal);

        foreach (var group in treeGroups)
        {
            var relative = group.Key.Replace('/', Path.DirectorySeparatorChar);
            var path = Path.Combine(assetsRoot, relative);
            if (!File.Exists(path))
                continue;

            var model = GlbModel.Load(path);
            var reference = group.First();
            var mesh = model.BuildPbrMesh(
                Matrix4x4.Identity,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: reference.SourceIsZUp);

            if (mesh.Vertices.Length == 0)
                continue;

            var min = mesh.Vertices[0].Position;
            var max = min;
            foreach (var vertex in mesh.Vertices)
            {
                min = Vector3.Min(min, vertex.Position);
                max = Vector3.Max(max, vertex.Position);
            }

            var width = MathF.Max(max.X - min.X, max.Z - min.Z);
            var height = max.Y - min.Y;

            dimensionsByAsset[group.Key] = new ProxyDimensions(
                Math.Clamp(width, 1.5f, 24f),
                Math.Clamp(height, 2.5f, 40f));
        }

        var trees = world.Models
            .Where(instance =>
                IsTreeAsset(instance.AssetPath) &&
                dimensionsByAsset.ContainsKey(instance.AssetPath))
            .ToArray();

        TreeCount = trees.Length;

        foreach (var chunk in trees.GroupBy(instance =>
                     GetChunkKey(instance.Position)))
        {
            var vertices = new List<ProxyVertex>();
            var indices = new List<uint>();
            var instances = chunk.ToArray();

            foreach (var instance in instances)
            {
                var dimensions = dimensionsByAsset[instance.AssetPath];
                var width =
                    dimensions.Width *
                    MathF.Max(instance.Scale.X, instance.Scale.Z);
                var height = dimensions.Height * instance.Scale.Y;

                width = Math.Clamp(width, 1.5f, 28f);
                height = Math.Clamp(height, 3f, 42f);

                var species = SpeciesType(instance.AssetPath);
                var leafColor = SpeciesColor(instance.AssetPath);
                AddCrossProxy(
                    vertices,
                    indices,
                    instance.Position,
                    width,
                    height,
                    leafColor,
                    species,
                    instance.YawRadians);
            }

            if (vertices.Count == 0 || indices.Count == 0)
                continue;

            var vertexArray = vertices.ToArray();
            var indexArray = indices.ToArray();

            var vertexBuffer = factory.CreateBuffer(new BufferDescription(
                ProxyVertex.SizeInBytes * checked((uint)vertexArray.Length),
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
                    new Vector2(center.X, center.Z))) + 34f;

            _batches.Add(new ProxyBatch(
                center,
                radius,
                vertexBuffer,
                indexBuffer,
                checked((uint)indexArray.Length)));
        }

        EngineLog.Info(
            $"Far vegetation impostors initialized: {TreeCount} trees, " +
            $"{_batches.Count} spatial batches, 4 crossed cards/tree with wind.");
    }

    public void Render(
        CommandList commandList,
        ResourceSet cameraSet,
        Vector3 cameraPosition,
        CameraFrustum frustum,
        float startDistance,
        float maximumDistance)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_pipeline is null)
            throw new InvalidOperationException(
                "Far vegetation renderer is not initialized.");

        startDistance = Math.Max(0f, startDistance);
        maximumDistance = Math.Max(startDistance, maximumDistance);

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, cameraSet);

        foreach (var batch in _batches)
        {
            var delta = new Vector2(
                batch.Center.X - cameraPosition.X,
                batch.Center.Z - cameraPosition.Z);
            var distance = delta.Length();

            if (distance + batch.Radius < startDistance)
                continue;
            if (distance - batch.Radius > maximumDistance)
                continue;
            if (!frustum.IntersectsSphere(batch.Center, batch.Radius))
                continue;

            commandList.SetVertexBuffer(0, batch.VertexBuffer);
            commandList.SetIndexBuffer(batch.IndexBuffer, IndexFormat.UInt32);
            commandList.DrawIndexed(batch.IndexCount);
        }
    }

    public static bool IsTreeAsset(string assetPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(assetPath);

        return fileName.StartsWith("dab_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("dab_stary_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddCrossProxy(
        List<ProxyVertex> vertices,
        List<uint> indices,
        Vector3 basePosition,
        float width,
        float height,
        Vector3 color,
        float species,
        float yaw)
    {
        var windPhase =
            basePosition.X * 0.137f +
            basePosition.Z * 0.173f +
            yaw * 1.91f;

        for (var plane = 0; plane < 4; plane++)
        {
            var angle = yaw + plane * MathF.PI / 4f;
            var axis = new Vector3(
                MathF.Cos(angle),
                0f,
                MathF.Sin(angle));
            var silhouetteScale =
                0.94f + 0.06f * MathF.Sin(windPhase + plane * 1.73f);
            var half = axis * (width * 0.5f * silhouetteScale);

            var bottomLeft = basePosition - half;
            var bottomRight = basePosition + half;
            var topLeft = bottomLeft + Vector3.UnitY * height;
            var topRight = bottomRight + Vector3.UnitY * height;

            var start = checked((uint)vertices.Count);
            var colorType = new Vector4(color, species);

            vertices.Add(new ProxyVertex(
                bottomLeft,
                new Vector2(0f, 0f),
                colorType,
                windPhase));
            vertices.Add(new ProxyVertex(
                bottomRight,
                new Vector2(1f, 0f),
                colorType,
                windPhase));
            vertices.Add(new ProxyVertex(
                topRight,
                new Vector2(1f, 1f),
                colorType,
                windPhase));
            vertices.Add(new ProxyVertex(
                topLeft,
                new Vector2(0f, 1f),
                colorType,
                windPhase));

            indices.Add(start + 0);
            indices.Add(start + 1);
            indices.Add(start + 2);
            indices.Add(start + 0);
            indices.Add(start + 2);
            indices.Add(start + 3);
        }
    }

    private static float SpeciesType(string assetPath)
    {
        var name = Path.GetFileNameWithoutExtension(assetPath);
        if (name.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase))
            return 1f;
        if (name.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase))
            return 2f;
        if (name.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase))
            return 3f;
        return 0f;
    }

    private static Vector3 SpeciesColor(string assetPath)
    {
        var name = Path.GetFileNameWithoutExtension(assetPath);

        if (name.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(0.17f, 0.29f, 0.11f);
        if (name.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(0.34f, 0.46f, 0.17f);
        if (name.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(0.22f, 0.37f, 0.14f);

        return new Vector3(0.27f, 0.40f, 0.13f);
    }

    private static (int X, int Z) GetChunkKey(Vector3 position) =>
        ((int)MathF.Floor(position.X / ChunkSize),
         (int)MathF.Floor(position.Z / ChunkSize));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var batch in _batches)
        {
            batch.VertexBuffer.Dispose();
            batch.IndexBuffer.Dispose();
        }

        _batches.Clear();
        _pipeline?.Dispose();

        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _shaders = null;
    }

    private readonly record struct ProxyDimensions(
        float Width,
        float Height);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct ProxyVertex
    {
        public const uint SizeInBytes = 40;

        public readonly Vector3 Position;
        public readonly Vector2 TexCoord;
        public readonly Vector4 ColorType;
        public readonly float WindPhase;

        public ProxyVertex(
            Vector3 position,
            Vector2 texCoord,
            Vector4 colorType,
            float windPhase)
        {
            Position = position;
            TexCoord = texCoord;
            ColorType = colorType;
            WindPhase = windPhase;
        }
    }

    private sealed record ProxyBatch(
        Vector3 Center,
        float Radius,
        DeviceBuffer VertexBuffer,
        DeviceBuffer IndexBuffer,
        uint IndexCount);
}
