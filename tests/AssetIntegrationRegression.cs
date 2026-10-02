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
        var palisadePath = Path.Combine(assetsRoot, "models", "static", "village_walls", "palisade_straight.glb");
        var grassClutterPath = Path.Combine(assetsRoot, "models", "static", "ground_clutter", "grass_tuft.glb");

        check(File.Exists(housePath), "R0 house GLB is copied into test runtime");
        check(File.Exists(playerPath), "Animated player GLB is copied into test runtime");
        check(File.Exists(enemyPath), "Animated predator GLB is copied into test runtime");
        check(File.Exists(palisadePath), "Village palisade GLB is copied into test runtime");
        check(File.Exists(grassClutterPath), "Ground clutter GLB is copied into test runtime");

        var terrainMaterials = new[]
        {
            "forest_grass",
            "dirt_path",
            "forest_litter",
            "wet_mud",
            "swamp_ground",
            "mossy_rock"
        };
        foreach (var material in terrainMaterials)
        {
            foreach (var map in new[] { "basecolor", "normal", "roughness", "height", "ao" })
            {
                var texturePath = Path.Combine(
                    assetsRoot,
                    "textures",
                    "terrain",
                    material,
                    $"{material}_{map}.png");
                check(File.Exists(texturePath),
                    $"Terrain material {material}/{map} is copied into test runtime");
            }
        }

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

        var palisade = GlbModel.Load(palisadePath);
        var palisadeMesh = palisade.BuildMesh(Matrix4x4.Identity, sourceIsZUp: true);
        check(palisadeMesh.Positions.Length > 0 && palisadeMesh.Indices.Length > 0,
            "Village palisade GLB returns renderable geometry");

        var grassClutter = GlbModel.Load(grassClutterPath);
        var grassClutterMesh = grassClutter.BuildMesh(Matrix4x4.Identity, sourceIsZUp: true);
        check(grassClutterMesh.Positions.Length > 0 && grassClutterMesh.Indices.Length > 0,
            "Ground clutter GLB returns renderable geometry");

        var world = WorldGenerator.Generate();
        check(world.Terrain.Width == 513 && world.Terrain.Depth == 513 && MathF.Abs(world.Terrain.CellSize - 4f) < 0.001f,
            "Expanded map spans roughly two kilometres per side");
        check(ForestLayout.Zones.Count == 4,
            "Expanded map defines four named forest biomes");
        check(ForestLayout.Zones.All(zone => world.Regions.Any(region =>
                region.Id == zone.Id &&
                region.Type == WorldRegionType.Forest)),
            "Every forest biome is registered as an explorable world region");
        check(world.Models.Select(model => model.AssetPath).Distinct(StringComparer.Ordinal).Count() < world.Models.Count,
            "World decoration intentionally reuses source assets for batching");

        var decorationsA = WorldDecorationGenerator.Generate(world.Terrain);
        var decorationsB = WorldDecorationGenerator.Generate(world.Terrain);
        check(decorationsA.Count == 965 && decorationsB.Count == 965,
            "Expanded forest pass has a bounded deterministic instance budget");
        check(decorationsA.Zip(decorationsB).All(pair =>
                pair.First.AssetPath == pair.Second.AssetPath &&
                Vector3.DistanceSquared(pair.First.Position, pair.Second.Position) < 0.000001f &&
                Vector3.DistanceSquared(pair.First.Scale, pair.Second.Scale) < 0.000001f),
            "Expanded forest layout is deterministic across runs");
        check(ForestLayout.Zones.All(zone =>
                decorationsA.Count(model => model.Id.StartsWith(zone.Id + "-", StringComparison.Ordinal))
                == zone.TreeCount + zone.UnderstoryCount),
            "Every named forest receives its configured canopy and understory population");
        check(decorationsA.Where(model =>
                ForestLayout.Zones.Any(zone => model.Id.StartsWith(zone.Id + "-", StringComparison.Ordinal)))
            .All(model => !ForestLayout.IsTrailCorridor(new Vector2(model.Position.X, model.Position.Z))),
            "Named forest generation keeps travel corridors open");

        var clutterA = GroundClutterGenerator.Generate(world.Terrain);
        var clutterB = GroundClutterGenerator.Generate(world.Terrain);
        check(clutterA.Count == clutterB.Count && clutterA.Count >= 300,
            "Ground clutter has a bounded deterministic population");
        check(clutterA.Zip(clutterB).All(pair =>
                pair.First.AssetPath == pair.Second.AssetPath &&
                Vector3.DistanceSquared(pair.First.Position, pair.Second.Position) < 0.000001f &&
                Vector3.DistanceSquared(pair.First.Scale, pair.Second.Scale) < 0.000001f),
            "Ground clutter layout is deterministic across runs");
        check(VillageBoundaryLayout.Placements.Count >= 25,
            "Village boundary contains a substantial modular wall layout");
        check(VillageBoundaryLayout.Placements.Any(wall =>
                wall.AssetPath.EndsWith("palisade_straight.glb", StringComparison.Ordinal)),
            "Village boundary uses the new palisade asset");
        check(VillageBoundaryLayout.Placements.Any(wall =>
                wall.AssetPath.EndsWith("wall_wattle_straight.glb", StringComparison.Ordinal)),
            "Village boundary uses the new wattle asset");

        var expectedWorldModels =
            21 +
            decorationsA.Count +
            clutterA.Count +
            VillageBoundaryLayout.Placements.Count;
        check(world.Models.Count == expectedWorldModels,
            "World registers curated content, forest decoration, ground clutter and village walls");

        check(world.Models.All(model => File.Exists(Path.Combine(
                assetsRoot,
                model.AssetPath.Replace('/', Path.DirectorySeparatorChar)))),
            "Every R0 world instance resolves to a tracked GLB file");

        var surfaceSamples = new[]
        {
            TerrainSurfaceClassifier.Classify(new Vector3(0f, 0f, -85f), Vector3.UnitY),
            TerrainSurfaceClassifier.Classify(new Vector3(95f, 0f, 35f), Vector3.UnitY),
            TerrainSurfaceClassifier.Classify(
                new Vector3(-400f, 0f, 390f),
                Vector3.Normalize(new Vector3(0.15f, 0.98f, 0.05f))),
            TerrainSurfaceClassifier.Classify(
                new Vector3(250f, 0f, 250f),
                Vector3.Normalize(new Vector3(0.85f, 0.24f, 0.10f)))
        };
        foreach (var weights in surfaceSamples)
        {
            var sum =
                weights.Grass +
                weights.ForestLitter +
                weights.Path +
                weights.Mud +
                weights.Swamp +
                weights.Rock;
            check(MathF.Abs(sum - 1f) < 0.001f,
                "Terrain surface weights remain normalized");
        }
        check(surfaceSamples[0].Path > surfaceSamples[0].Grass,
            "Village center resolves primarily to packed path ground");
        check(surfaceSamples[1].Mud + surfaceSamples[1].Swamp > 0.65f,
            "Swamp center resolves primarily to wet ground layers");
        check(surfaceSamples[3].Rock > 0.45f,
            "Steep terrain resolves primarily to mossy rock");

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
