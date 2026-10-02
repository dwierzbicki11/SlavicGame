using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.World;

internal static class AssetIntegrationRegression
{
    public static void Run(Action<bool, string> check)
    {
        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        var housePath = Path.Combine(assetsRoot, "models", "static", "chata_r0_variant_01.glb");
        var playerPath = Path.Combine(assetsRoot, "models", "animated", "player_hunter_animated.glb");
        var enemyPath = Path.Combine(assetsRoot, "models", "animated", "swamp_predator_animated.glb");

        check(File.Exists(housePath), "R0 house GLB is copied into test runtime");
        check(File.Exists(playerPath), "Animated player GLB is copied into test runtime");
        check(File.Exists(enemyPath), "Animated predator GLB is copied into test runtime");

        var house = GlbModel.Load(housePath);
        var houseMesh = house.BuildMesh(Matrix4x4.Identity, sourceIsZUp: true);
        check(houseMesh.Positions.Length > 0 && houseMesh.Indices.Length > 0,
            "Static GLB loader returns renderable house geometry");

        var housePbr = house.BuildPbrMesh(Matrix4x4.Identity, sourceIsZUp: true);
        check(housePbr.Vertices.Length == houseMesh.Positions.Length && housePbr.DrawRanges.Length > 0,
            "PBR GLB path preserves geometry and material draw ranges");
        check(housePbr.Vertices.All(vertex =>
                float.IsFinite(vertex.Normal.X) &&
                float.IsFinite(vertex.Normal.Y) &&
                float.IsFinite(vertex.Normal.Z) &&
                vertex.Normal.LengthSquared() > 0.5f),
            "PBR GLB vertices contain valid normals");
        check(housePbr.Materials.Any(material =>
                material.BaseColorImage is { Length: > 0 } ||
                material.NormalImage is { Length: > 0 } ||
                material.MetallicRoughnessImage is { Length: > 0 }),
            "Tracked R0 house exposes embedded PBR texture data");

        var player = GlbModel.Load(playerPath);
        check(player.AnimationNames.Contains("Idle") &&
              player.AnimationNames.Contains("Walk") &&
              player.AnimationNames.Contains("Run") &&
              player.AnimationNames.Contains("Attack") &&
              player.AnimationNames.Contains("Hit") &&
              player.AnimationNames.Contains("Death"),
            "Animated player GLB exposes gameplay clips");

        var idleStart = player.BuildMesh(Matrix4x4.Identity, "Idle", 0f, sourceIsZUp: true);
        var idleMid = player.BuildMesh(Matrix4x4.Identity, "Idle", 0.5f, sourceIsZUp: true);
        check(idleStart.Positions.Length == idleMid.Positions.Length && idleStart.Positions.Length > 0,
            "Animated GLB evaluates stable topology");
        check(idleStart.Positions.Zip(idleMid.Positions).Any(pair => Vector3.DistanceSquared(pair.First, pair.Second) > 0.0000001f),
            "Animation evaluation changes player bind-pose geometry over time");

        var world = WorldGenerator.Generate();
        check(world.Models.Count == 189,
            "R0 world registers curated models plus deterministic environment decoration");
        check(world.Models.Select(model => model.AssetPath).Distinct(StringComparer.Ordinal).Count() < world.Models.Count,
            "R0 decoration intentionally reuses source assets for batching");

        var decorationsA = WorldDecorationGenerator.Generate(world.Terrain);
        var decorationsB = WorldDecorationGenerator.Generate(world.Terrain);
        check(decorationsA.Count == 168 && decorationsB.Count == 168,
            "R0 decoration pass has a bounded deterministic instance budget");
        check(decorationsA.Zip(decorationsB).All(pair =>
                pair.First.AssetPath == pair.Second.AssetPath &&
                Vector3.DistanceSquared(pair.First.Position, pair.Second.Position) < 0.000001f &&
                Vector3.DistanceSquared(pair.First.Scale, pair.Second.Scale) < 0.000001f),
            "R0 decoration layout is deterministic across runs");
        check(world.Models.All(model => File.Exists(Path.Combine(
                assetsRoot,
                model.AssetPath.Replace('/', Path.DirectorySeparatorChar)))),
            "Every R0 world instance resolves to a tracked GLB file");

        TerrainMesh.Build(world.Terrain, out var terrainVertices, out _);
        check(terrainVertices.All(vertex =>
                float.IsFinite(vertex.Normal.X) &&
                float.IsFinite(vertex.Normal.Y) &&
                float.IsFinite(vertex.Normal.Z) &&
                MathF.Abs(vertex.Normal.Length() - 1f) < 0.01f),
            "Terrain mesh contains normalized finite surface normals");
        check(terrainVertices.Any(vertex => Vector3.Dot(vertex.Normal, Vector3.UnitY) < 0.995f),
            "Generated terrain contains non-flat lighting normals");

        StaticWorldMesh.BuildWithAssets(world, assetsRoot, out var worldVertices, out var worldIndices);
        var terrainVertexCount = world.Terrain.Width * world.Terrain.Depth;
        check(worldVertices.Length > terrainVertexCount,
            "Static R0 mesh contains GLB world geometry beyond terrain");
        check(worldIndices.Length > (world.Terrain.Width - 1) * (world.Terrain.Depth - 1) * 6,
            "Static R0 mesh contains GLB world indices beyond terrain");

        var enemy = GlbModel.Load(enemyPath);
        ActorModelMesh.Build(world, player, enemy, 0.35, 0f, true, out var actorVertices, out var actorIndices);
        check(actorVertices.Length > 0 && actorIndices.Length > 0,
            "Animated player and enemy models produce dynamic actor geometry");
    }
}
