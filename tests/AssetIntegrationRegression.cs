using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.NPC;
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

        var realismSamples = new[]
        {
            housePath,
            palisadePath,
            grassClutterPath
        };
        var realismReports = realismSamples
            .Select(AssetRealismAudit.InspectGlb)
            .ToArray();

        check(realismReports.All(report => report.MaterialCount > 0),
            "Realism audit reads material data from visible GLB assets");
        check(realismReports.All(report =>
                report.Issues.All(issue => !string.IsNullOrWhiteSpace(issue.Code))),
            "Realism audit returns structured issue codes");
        Console.WriteLine(
            "[REALISM] " +
            string.Join(" | ", realismReports.Select(report =>
                $"{Path.GetFileName(report.Path)}={report.Tier},minTex={report.MinimumTextureEdge}px," +
                $"issues={report.Issues.Count}")));

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
        check(decorationsA.Count == 1065 && decorationsB.Count == 1065,
            "Forest and riverbank pass has a bounded deterministic instance budget");
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

        var riverbankA = RiverbankPropGenerator.Generate(world.Terrain);
        var riverbankB = RiverbankPropGenerator.Generate(world.Terrain);
        check(riverbankA.Count == 116 && riverbankB.Count == 116,
            "Rocky river channel dressing has a bounded deterministic population");
        check(riverbankA.Zip(riverbankB).All(pair =>
                pair.First.AssetPath == pair.Second.AssetPath &&
                Vector3.DistanceSquared(pair.First.Position, pair.Second.Position) < 0.000001f &&
                MathF.Abs(pair.First.YawRadians - pair.Second.YawRadians) < 0.000001f),
            "Rocky riverbank placement is deterministic");
        check(riverbankA.All(model =>
                model.AssetPath.EndsWith("riverbank_rocky_r0_01.glb", StringComparison.Ordinal) &&
                MathF.Abs(
                    model.Position.X - WaterLandscape.CenterX(model.Position.Z)) >=
                    WaterLandscape.SurfaceHalfWidth(model.Position.Z)),
            "Rocky riverbank models sit outside the animated water ribbon");

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

        check(RegionalPropLayout.Placements.Count >= 50,
            "R0 regional prop pass adds a substantial set of curated world models");
        check(RegionalPropLayout.Placements.Count(item =>
                item.Id.StartsWith("village-", StringComparison.Ordinal)) >= 20,
            "Village receives buildings plus lived-in market and workshop props");
        check(RegionalPropLayout.Placements.Count(item =>
                item.Id.StartsWith("swamp-", StringComparison.Ordinal)) >= 10,
            "Black Swamp receives traversal and investigation set dressing");
        check(RegionalPropLayout.Placements.Count(item =>
                item.Id.StartsWith("shrine-", StringComparison.Ordinal)) >= 10,
            "Stone Circle receives ruins graves and ritual dressing");
        check(RegionalPropLayout.Placements.Any(item =>
                item.AssetPath.EndsWith("chata_r0_variant_04.glb", StringComparison.Ordinal)) &&
              RegionalPropLayout.Placements.Any(item =>
                item.AssetPath.EndsWith("stajnia_r0_01.glb", StringComparison.Ordinal)) &&
              RegionalPropLayout.Placements.Any(item =>
                item.AssetPath.EndsWith("stodola_r0_01.glb", StringComparison.Ordinal)) &&
              RegionalPropLayout.Placements.Any(item =>
                item.AssetPath.EndsWith("wieza_straznicza_r0_01.glb", StringComparison.Ordinal)),
            "Village skyline uses the previously unused fourth hut stable barn and watchtower");
        check(RegionalPropLayout.Placements.All(item =>
                File.Exists(Path.Combine(
                    assetsRoot,
                    item.AssetPath.Replace('/', Path.DirectorySeparatorChar)))),
            "Every new regional prop resolves to a tracked GLB");
        check(SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/basket_r0_01.glb") &&
              SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/slady_pazurow_r0_01.glb") &&
              SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/riverbank_rocky_r0_01.glb") &&
              SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/stol_warsztatowy_r0_01.glb") &&
              SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/krosno_r0_01.glb") &&
              !SlavicGame.Engine.Renderer.WorldModelRenderPolicy.IsShortRangeProp(
                "models/static/stodola_r0_01.glb"),
            "Small props, workstation furniture and rocky banks use short-range culling while major buildings keep world render distance");

        var workstationModels =
            NpcWorkstationCatalog.BuildModels(world.Terrain);
        check(workstationModels.Count >= 20 &&
              workstationModels.All(model =>
                  File.Exists(Path.Combine(
                      assetsRoot,
                      model.AssetPath.Replace('/', Path.DirectorySeparatorChar)))),
            "Visible NPC workstation props resolve to tracked GLB assets");

        var expectedWorldModels =
            21 +
            RegionalPropLayout.Placements.Count +
            decorationsA.Count +
            riverbankA.Count +
            workstationModels.Count +
            clutterA.Count +
            VillageBoundaryLayout.Placements.Count;
        check(world.Models.Count == expectedWorldModels,
            "World registers curated content, NPC workstations, forest decoration, ground clutter and village walls");

        check(world.Models.All(model => File.Exists(Path.Combine(
                assetsRoot,
                model.AssetPath.Replace('/', Path.DirectorySeparatorChar)))),
            "Every R0 world instance resolves to a tracked GLB file");

        CheckFacing("village-hut-d", new Vector2(0f, -85f));
        CheckFacing("village-forge-tools", new Vector2(21f, -91f));
        CheckFacing("village-road-sign", new Vector2(0f, -85f));
        CheckFacing("forest-cave-entrance", new Vector2(-45f, 20f));
        CheckFacing("swamp-damaged-boardwalk", new Vector2(95f, 35f));
        CheckFacing("swamp-claw-tracks-a", new Vector2(92f, 35f));
        CheckFacing("shrine-grave-a", new Vector2(-85f, 55f));

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
        var npcModelFiles = world.NpcWorld.Actors
            .Select(actor => NpcVisualCatalog.ModelFile(actor.Id, actor.Role))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(file => file, StringComparer.Ordinal)
            .ToArray();

        check(npcModelFiles.Length == 5,
            "NPC roster uses five distinct animated humanoid model families");

        var npcModels = npcModelFiles.ToDictionary(
            file => file,
            file =>
            {
                var path = Path.Combine(
                    assetsRoot,
                    "models",
                    "animated",
                    file);
                check(File.Exists(path),
                    $"NPC model asset exists: {file}");
                var model = GlbModel.Load(path);
                check(model.AnimationNames.Contains("Idle") &&
                      model.AnimationNames.Contains("Walk") &&
                      model.AnimationNames.Contains("Interact"),
                    $"NPC model {file} contains Idle, Walk and Interact clips");
                return model;
            },
            StringComparer.Ordinal);

        var populationViewer = new Vector3(45f, 0f, -20f);
        ActorModelMesh.Build(
            world,
            player,
            npcModels,
            enemy,
            populationViewer,
            0.35,
            0f,
            true,
            out var actorVertices,
            out var actorIndices);

        check(actorVertices.Length > 0 && actorIndices.Length > 0,
            "Animated player, settlers and enemy models produce dynamic actor geometry");

        var questNpcIds = new[]
        {
            "missing-family",
            "crossing-keeper",
            "herbalist",
            "community-guard",
            "shrine-keeper"
        };
        var questProfiles = questNpcIds
            .Select(id =>
            {
                var actor = world.NpcWorld.Find(id)
                    ?? throw new Exception($"NPC {id} missing from runtime");
                return NpcVisualCatalog.For(id, actor.Role);
            })
            .ToArray();

        check(questProfiles.Select(profile => profile.BodyScale).Distinct().Count() == 5,
            "Five authored NPCs have distinct body proportions");
        check(questProfiles.Select(profile => profile.PrimaryAccessory).Distinct().Count() >= 4,
            "Authored NPC silhouettes use multiple distinct accessory families");

        var ambientProfiles = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .Select(actor => NpcVisualCatalog.For(actor.Id, actor.Role))
            .ToArray();

        check(ambientProfiles.Length == 14 &&
              ambientProfiles.Select(profile => profile.BaseColor).Distinct().Count() >= 10 &&
              ambientProfiles.Select(profile => profile.PrimaryAccessory).Distinct().Count() >= 8,
            "Expanded ambient settlers have varied palettes and silhouette accessories");

        var ambientModelFiles = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .Select(actor => NpcVisualCatalog.ModelFile(actor.Id, actor.Role))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        check(ambientModelFiles.Length >= 5 &&
              ambientModelFiles.Contains("npc_villager_a_animated.glb") &&
              ambientModelFiles.Contains("npc_villager_b_animated.glb") &&
              ambientModelFiles.Contains("npc_hunter_animated.glb") &&
              ambientModelFiles.Contains("npc_merchant_animated.glb") &&
              ambientModelFiles.Contains("npc_elder_animated.glb"),
            "Ambient settlers span villager A/B, hunter, merchant and elder body models");

        check(
            NpcVisualCatalog.ModelFile("community-guard", NpcRole.CommunityGuard) ==
                "npc_hunter_animated.glb" &&
            NpcVisualCatalog.ModelFile("shrine-keeper", NpcRole.ShrineKeeper) ==
                "npc_elder_animated.glb" &&
            NpcVisualCatalog.ModelFile("settler-trader-01", NpcRole.Trader) ==
                "npc_merchant_animated.glb",
            "Authored NPC roles resolve to their intended animated model families");

        var playerGeometry = player.BuildMesh(
            Matrix4x4.Identity,
            "Idle",
            0.35f,
            sourceIsZUp: true);
        var enemyGeometry = enemy.BuildMesh(
            Matrix4x4.Identity,
            "Idle",
            0.35f,
            sourceIsZUp: true);
        var npcBaseVertexBudget = world.NpcWorld.Actors.Sum(actor =>
        {
            var file = NpcVisualCatalog.ModelFile(actor.Id, actor.Role);
            return npcModels[file]
                .BuildMesh(
                    Matrix4x4.Identity,
                    "Idle",
                    0.35f,
                    sourceIsZUp: true)
                .Positions.Length;
        });
        var baseAnimatedVertexBudget =
            playerGeometry.Positions.Length +
            npcBaseVertexBudget +
            enemyGeometry.Positions.Length;

        check(actorVertices.Length > baseAnimatedVertexBudget,
            "Settler accessories add visible geometry beyond their distinct humanoid GLBs");

        ActorModelMesh.Build(
            world,
            player,
            npcModels,
            enemy,
            new Vector3(-900f, 0f, -900f),
            0.35,
            0f,
            true,
            out var farActorVertices,
            out var farActorIndices);

        check(farActorVertices.Length < actorVertices.Length &&
              farActorIndices.Length < actorIndices.Length,
            "Ambient settler mesh generation is distance-culled away from the village");

        var npcGeometries = npcModels.Values
            .Select(model => model.BuildMesh(
                Matrix4x4.Identity,
                "Idle",
                0.35f,
                sourceIsZUp: true))
            .ToArray();

        check(npcGeometries.All(geometry =>
                geometry.Positions.Length > 0 &&
                geometry.Indices.Length > 0),
            "Every NPC model family produces renderable animated geometry");
        check(npcGeometries.Any(geometry =>
                geometry.Positions.Length != playerGeometry.Positions.Length ||
                !geometry.Positions.SequenceEqual(playerGeometry.Positions)),
            "NPC models are not all copies of the player hunter geometry");

        check(BestiaryVisualCatalog.Definitions.Count == 5 &&
              BestiaryVisualCatalog.RequiredModelFiles.Count == 5,
            "Animated bestiary catalog exposes five distinct monster families");

        foreach (var definition in BestiaryVisualCatalog.Definitions)
        {
            var bestiaryPath = Path.Combine(
                assetsRoot,
                "models",
                "animated",
                definition.ModelFile);
            check(File.Exists(bestiaryPath),
                $"Bestiary model asset exists: {definition.ModelFile}");

            var bestiaryModel = GlbModel.Load(bestiaryPath);
            check(definition.RequiredClips.All(clip =>
                    bestiaryModel.AnimationNames.Contains(clip)),
                $"Bestiary model {definition.ModelFile} exposes the full humanoid combat clip set");

            var bestiaryGeometry = bestiaryModel.BuildMesh(
                Matrix4x4.CreateScale(definition.Scale),
                "Idle",
                0.35f,
                sourceIsZUp: true);
            check(bestiaryGeometry.Positions.Length > 0 &&
                  bestiaryGeometry.Indices.Length > 0,
                $"Bestiary model {definition.Id} produces renderable animated geometry");
        }

        void CheckFacing(string id, Vector2 target)
        {
            var model = world.Models.Single(item =>
                string.Equals(item.Id, id, StringComparison.Ordinal));
            var from = new Vector2(model.Position.X, model.Position.Z);
            var desired = Vector2.Normalize(target - from);
            var actual = WorldPlacementOrientation.ForwardFromYaw(model.YawRadians);
            check(Vector2.Dot(actual, desired) > 0.985f,
                $"{id} faces its intended local anchor");
        }
    }
}
