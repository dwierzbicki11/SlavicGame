using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class BowCombatRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();

        check(
            world.Progress.Inventory.Contains(BowCombatRuntime.BowItemId) &&
            world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == 16,
            "Vertical slice grants one bow and sixteen basic arrows");

        var ammoAtStart =
            world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);

        world.Bow.SetAiming(world, true);
        check(world.Bow.IsAiming && world.Bow.TryStartDraw(world),
            "RMB aim allows starting bow draw");

        world.Bow.Update(world, 0.45);
        check(world.Bow.DrawFraction > 0.3f,
            "Bow draw fraction advances over time");

        world.Bow.SetAiming(world, false);
        check(!world.Bow.IsDrawing &&
              world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammoAtStart,
            "Cancelling aim cancels draw without consuming ammo");

        var predator = world.Enemies.Single(enemy =>
            enemy.Id == SlavicGame.Engine.Gameplay.SwampPredatorEncounter.Id);

        var predatorTarget =
            predator.Position + Vector3.UnitY * 0.78f;
        var predatorOrigin =
            predatorTarget - Vector3.UnitZ * 5f;
        var predatorDirection =
            Vector3.Normalize(predatorTarget - predatorOrigin);

        world.Bow.SetAiming(world, true);
        check(world.Bow.TryStartDraw(world),
            "Bow can start a second draw after cancellation");
        world.Bow.Update(world, 1.3);
        check(world.Bow.DrawFraction >= 0.99f,
            "Bow reaches full draw deterministically");
        check(world.Bow.TryRelease(
                world,
                predatorOrigin,
                predatorDirection),
            "Releasing full draw spawns a physical arrow");
        check(world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammoAtStart - 1,
            "Arrow ammo is consumed exactly once on release");
        check(world.Bow.Projectiles.Count == 1,
            "Released arrow exists as an active projectile");

        world.Bow.Update(world, 0.25);
        check(predator.Health < predator.MaxHealth &&
              predator.State != SlavicGame.Engine.AI.EnemyState.Patrol,
            "Projectile collision damages and alerts the swamp predator");
        check(world.Bow.Projectiles.Count == 0,
            "Arrow stops simulating after target collision");

        world.Bow.SetAiming(world, true);
        check(world.Bow.TryStartDraw(world),
            "Terrain-shot draw starts");
        world.Bow.Update(world, 0.5);

        var groundOrigin =
            world.PlayerPosition + Vector3.UnitY * 4f;
        check(world.Bow.TryRelease(
                world,
                groundOrigin,
                Vector3.Normalize(new Vector3(0.15f, -1f, 0.1f))),
            "Bow can fire toward terrain");

        world.Bow.Update(world, 0.6);
        check(world.Bow.Projectiles.Count == 0 &&
              world.Bow.RecoverableArrows.Count == 1,
            "Terrain collision creates one recoverable stuck arrow");

        var ammoBeforeRetrieve =
            world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);
        var stuck =
            world.Bow.RecoverableArrows.Single();
        world.SetPlayerPosition(stuck.Position);

        check(world.Bow.RetrievalPrompt(world).Contains("PODNIES", StringComparison.Ordinal) &&
              world.Bow.TryRetrieveNearest(world),
            "Nearby terrain arrow exposes E retrieval and can be recovered");
        check(world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammoBeforeRetrieve + 1 &&
              world.Bow.RecoverableArrows.Count == 0,
            "Recovering a stuck arrow returns exactly one ammo");

        var raven = world.Wildlife.Actors.Single(actor =>
            actor.Id == "raven-start-01");
        var ravenOrigin =
            raven.Position - Vector3.UnitZ * 4f;
        var ravenDirection =
            Vector3.Normalize(raven.Position - ravenOrigin);

        world.Bow.SetAiming(world, true);
        check(world.Bow.TryStartDraw(world),
            "Hunting draw starts with remaining ammo");
        world.Bow.Update(world, 1.3);
        check(world.Bow.TryRelease(
                world,
                ravenOrigin,
                ravenDirection),
            "Full-draw hunting arrow releases");
        world.Bow.Update(world, 0.25);

        check(world.Wildlife.Actors.Single(actor => actor.Id == raven.Id).Health == 0f,
            "Full-draw arrow kills wildlife through shared hunting health");

        var assetsRoot =
            Path.Combine(AppContext.BaseDirectory, "assets");
        var bowModel =
            GlbModel.Load(
                Path.Combine(assetsRoot, "models", "static", "luk_r0_01.glb"));
        var arrowModel =
            GlbModel.Load(
                Path.Combine(assetsRoot, "models", "static", "strzala_r0_01.glb"));

        TerrainVertex[] bowVertices = [];
        uint[] bowIndices = [];
        BowPresentationMesh.Append(
            world,
            bowModel,
            arrowModel,
            world.PlayerPosition + Vector3.UnitY * 1.7f,
            Vector3.UnitZ,
            Vector3.UnitX,
            true,
            ref bowVertices,
            ref bowIndices);

        check(bowVertices.Length > 0 &&
              bowIndices.Length > 0 &&
              bowIndices.All(index => index < bowVertices.Length),
            "Aiming bow and arrow assets produce valid presentation geometry");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        check(restored.Wildlife.Actors.Single(actor => actor.Id == raven.Id).Health == 0f,
            "Hunted wildlife health and carcass survive save/load");
        check(restored.Wildlife.TryHarvest(restored, raven.Id) &&
              !restored.Wildlife.TryHarvest(restored, raven.Id),
            "Bow-killed wildlife yields harvest once through the shared hunting system");

        var wildlifeModels =
            WildlifeCatalog.RequiredModelFiles.ToDictionary(
                file => file,
                file => GlbModel.Load(
                    Path.Combine(
                        assetsRoot,
                        "models",
                        "animated",
                        file)),
                StringComparer.Ordinal);

        TerrainVertex[] deadRavenVertices = [];
        uint[] deadRavenIndices = [];
        var restoredRaven =
            restored.Wildlife.Actors.Single(actor =>
                actor.Id == "raven-start-01");

        WildlifeModelMesh.Append(
            restored,
            wildlifeModels,
            0.5,
            restoredRaven.Position,
            3f,
            ref deadRavenVertices,
            ref deadRavenIndices);

        check(deadRavenVertices.Length == 0 &&
              deadRavenIndices.Length == 0,
            "Harvested wildlife is culled from rendering");

        var harvestedRestore = WorldGenerator.Generate();
        SaveGameService.Restore(harvestedRestore, SaveGameService.Serialize(restored));
        check(harvestedRestore.Wildlife.Actors.Single(actor => actor.Id == raven.Id).Looted &&
              !harvestedRestore.Wildlife.TryHarvest(harvestedRestore, raven.Id),
            "Bow harvest stays consumed after save/load");

        var slow = Flight(0.1);
        var fast = Flight(1.0 / 120.0);
        check(Vector3.Distance(slow, fast) < 0.005f,
            "Arrow trajectory is independent of 10 versus 120 FPS");

        var wounded = WorldGenerator.Generate();
        var deer = wounded.Wildlife.Actors.First(actor => actor.Species == WildlifeSpecies.Deer);
        wounded.Wildlife.TryDamage(wounded, deer.Id, 20f);
        var origin = deer.Position + Vector3.UnitY * WildlifeCatalog.For(deer.Species).BodyRadius * 0.72f - Vector3.UnitZ * 2f;
        wounded.Bow.SetAiming(wounded, true);
        wounded.Bow.TryStartDraw(wounded);
        wounded.Bow.Update(wounded, 1.3);
        wounded.Bow.TryRelease(wounded, origin, Vector3.UnitZ);
        wounded.Bow.Update(wounded, 0.15);
        check(wounded.Wildlife.Actors.Single(actor => actor.Id == deer.Id).Health == 0f,
            "Melee wounds and bow hits use one shared wildlife health pool");

        var blocked = WorldGenerator.Generate();
        blocked.Bow.SetAiming(blocked, true);
        blocked.Bow.TryStartDraw(blocked);
        var ammo = blocked.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);
        blocked.Bow.SetAiming(blocked, false);
        check(!blocked.Bow.TryRelease(blocked, blocked.PlayerPosition, Vector3.UnitZ) &&
              blocked.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammo,
            "Cancelled draw cannot fire or spend ammo on a later release");
        blocked.Bow.SetAiming(blocked, true);
        blocked.Bow.TryStartDraw(blocked);
        blocked.Bow.TryRelease(blocked, blocked.PlayerPosition + Vector3.UnitY * 100f, Vector3.UnitZ);
        SaveGameService.Restore(blocked, SaveGameService.Serialize(blocked));
        check(!blocked.Bow.IsAiming && !blocked.Bow.IsDrawing && blocked.Bow.Projectiles.Count == 0,
            "Restore clears transient bow input and projectiles");

        Vector3 Flight(double step)
        {
            var flight = WorldGenerator.Generate();
            flight.Bow.SetAiming(flight, true);
            flight.Bow.TryStartDraw(flight);
            flight.Bow.Update(flight, 1.3);
            flight.Bow.TryRelease(flight, flight.PlayerPosition + Vector3.UnitY * 100f, Vector3.UnitZ);
            var remaining = 0.6;
            while (remaining > 0.000001)
            {
                var dt = Math.Min(step, remaining);
                flight.Bow.Update(flight, dt);
                remaining -= dt;
            }
            return flight.Bow.Projectiles.Single().Position;
        }
    }
}
