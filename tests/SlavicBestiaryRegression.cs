using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.World;

internal static class SlavicBestiaryRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(12);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.Initialize();

        // 1. Initial bestiary population
        var actors = world.Bestiary.Actors;
        check(actors.Count == 5, $"Initial bestiary world contains 5 mythical Slavic monsters (found {actors.Count})");

        var topielec = actors.FirstOrDefault(a => a.Species == BestiarySpecies.Topielec);
        var poludnica = actors.FirstOrDefault(a => a.Species == BestiarySpecies.Poludnica);
        var upior = actors.FirstOrDefault(a => a.Species == BestiarySpecies.Upior);
        var boginka = actors.FirstOrDefault(a => a.Species == BestiarySpecies.Boginka);
        var leszy = actors.FirstOrDefault(a => a.Species == BestiarySpecies.ForestGuardian);

        check(topielec is not null && topielec.TrophyItemId == "pazur-topielca", "Topielec loaded with claw trophy");
        check(poludnica is not null && poludnica.TrophyItemId == "wieniec-poludnicy", "Poludnica loaded with wreath trophy");
        check(upior is not null && upior.TrophyItemId == "pyl-upiora", "Upior loaded with dust trophy");
        check(boginka is not null && boginka.TrophyItemId == "lza-boginki", "Boginka loaded with tear trophy");
        check(leszy is not null && leszy.TrophyItemId == "kora-leszego", "Leszy loaded with bark trophy");

        check(topielec!.IsAlive && topielec.Health > 0f && topielec.MaxHealth == 55f, "Topielec starts alive with 55 HP");
        check(leszy!.IsAlive && leszy.MaxHealth == 120f, "Leszy starts alive as legendary forest guardian with 120 HP");

        // 2. AI behavior & movement update
        world.SetPlayerPosition(new Vector3(115f, 0f, 15f)); // Right near Topielec
        world.Update(0.1);

        topielec = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Topielec);
        check(topielec.Position.Y == world.Terrain.SampleHeight(topielec.Position), "Topielec position tracks ground elevation");

        // 3. Melee combat: hitting Topielec with sword
        world.Player.SetState(100f, 100f);
        var initialHealth = topielec.Health;
        var hitSuccess = world.Bestiary.TryDamage(world, topielec.Id, 25f);
        check(hitSuccess, "Melee damage successfully registered against Topielec");

        world.Update(0.01);
        topielec = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Topielec);
        check(topielec.Health == initialHealth - 25f, "Topielec health decreased by 25");
        check(topielec.AnimationClip == "Hit", "Topielec plays Hit reaction animation");

        // Finish off Topielec
        world.Bestiary.TryDamage(world, topielec.Id, 50f);
        world.Update(0.01);
        topielec = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Topielec);
        check(!topielec.IsAlive, "Topielec is marked dead after lethal damage");
        check(topielec.AnimationClip == "Death", "Topielec plays Death animation");

        // 4. Harvesting trophy
        world.SetPlayerPosition(topielec.Position);
        world.EnvironmentInteractions.Update(world);
        var target = world.EnvironmentInteractions.Current;
        check(target is not null && target.Id == $"bestiary.harvest.{topielec.Id}", "Harvest interaction prompt surfaces for fallen Topielec");
        check(target!.Prompt.Contains("ZBIERZ TROFEUM", StringComparison.Ordinal), "Harvest prompt contains Slavic trophy text");

        var clawBefore = world.Progress.Inventory.Count("pazur-topielca");
        var harvestSuccess = world.EnvironmentInteractions.TryInteract(world);
        check(harvestSuccess, "Harvesting Topielec carcass succeeds");
        check(world.Progress.Inventory.Count("pazur-topielca") == clawBefore + 1, "Player inventory receives 1x pazur-topielca");
        check(world.Progress.HasFlag("recipe.drowned-tallow.learned"), "Harvesting Topielec unlocks SMALEC TOPIELCA alchemy recipe");

        topielec = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Topielec);
        check(topielec.Looted, "Topielec marked as looted");

        // Cannot harvest twice
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id != $"bestiary.harvest.{topielec.Id}", "Looted Topielec is no longer harvestable");

        // 5. Magic combat: casting Spark at Poludnica
        world.Progress.SetFlag(SpellLessons.LearnedFlag("spell.spark"));
        world.Player.Restore();
        world.Magic.SelectSpell("spell.spark");

        poludnica = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Poludnica);
        world.SetPlayerPosition(poludnica.Position - new Vector3(0f, 0f, 5f)); // 5m south of Poludnica
        var poludnicaHealthBefore = poludnica.Health;

        var origin = world.PlayerPosition + Vector3.UnitY * 1.1f;
        var targetPoint = poludnica.Position + Vector3.UnitY * 0.85f;
        var magicDir = Vector3.Normalize(targetPoint - origin);

        var startSpark = world.Magic.TryStart(world, magicDir);
        check(startSpark, "Player starts casting Spark towards Poludnica");

        // Complete cast
        world.Magic.Update(world, 5.0);
        poludnica = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Poludnica);
        check(poludnica.Health < poludnicaHealthBefore, "Magic Spark spell successfully damaged Poludnica");

        // 6. Bow combat: shooting arrow at Upiór
        upior = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Upior);
        world.Progress.Inventory.Add(BowCombatRuntime.BowItemId, 1);
        world.Progress.Inventory.Add(BowCombatRuntime.ArrowItemId, 10);
        world.SetPlayerPosition(upior.Position - new Vector3(0f, 0f, 4f)); // 4m south of Upiór
        world.Update(0.01);
        upior = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Upior);
        var upiorHealthBefore = upior.Health;

        world.Bow.SetAiming(world, true);
        world.Bow.TryStartDraw(world);
        world.Bow.Update(world, 1.5); // Full draw
        var arrowOrigin = world.PlayerPosition + Vector3.UnitY * 1.5f;
        var arrowDir = Vector3.Normalize((upior.Position + Vector3.UnitY * 0.9f) - arrowOrigin);
        var shot = world.Bow.TryRelease(world, arrowOrigin, arrowDir);
        check(shot, "Bow released arrow towards Upior");

        // Step arrow simulation until hit
        for (var i = 0; i < 60; i++)
        {
            world.Bow.Update(world, 1f / 60f);
        }

        upior = world.Bestiary.Actors.First(a => a.Species == BestiarySpecies.Upior);
        check(upior.Health < upiorHealthBefore, $"Bow arrow hit Upior and inflicted damage (HP: {upior.Health}/{upiorHealthBefore}, BowMsg: '{world.Bow.Message}')");

        // 7. Crafting Slavic monster alchemy
        world.Time.SetTimeOfDay(11);
        var herbalist = world.NpcWorld.Find("herbalist") ?? throw new Exception("Herbalist missing");
        world.SetPlayerPosition(herbalist.Position);
        world.NpcWorld.Update(world);

        check(world.Crafting.TryOpenNearest(world), "Alchemy station opens near herbalist");
        var lines = world.Crafting.BuildLines(world);
        var tallowLine = lines.FirstOrDefault(l => l.RecipeId == "recipe.drowned-tallow");
        check(tallowLine is not null && tallowLine.Unlocked, "SMALEC TOPIELCA is unlocked in herbalist recipe list");
        check(!tallowLine!.CanCraft, "Cannot craft tallow yet without resin");

        world.Progress.Inventory.Add("forest-resin", 1);
        lines = world.Crafting.BuildLines(world);
        tallowLine = lines.First(l => l.RecipeId == "recipe.drowned-tallow");
        check(tallowLine.CanCraft, "Can craft SMALEC TOPIELCA with claw and resin");

        // Select SMALEC TOPIELCA and craft
        var tallowIdx = CraftingRuntime.Catalog.ToList().FindIndex(r => r.Recipe.Id == "recipe.drowned-tallow");
        for (var step = 0; step < tallowIdx; step++)
            world.Crafting.MoveSelection(world, 1);

        var craftResult = world.Crafting.Confirm(world);
        check(craftResult == CraftingResult.Completed, "Crafting SMALEC TOPIELCA succeeded");
        check(world.Progress.Inventory.Count("drowned-tallow") == 1, "Player holds 1x SMALEC TOPIELCA");
        check(world.Progress.Inventory.Count("pazur-topielca") == 0, "Topielec claw consumed in alchemy");
        check(world.Progress.Inventory.Count("forest-resin") == 0, "Resin consumed in alchemy");

        world.Crafting.Close();
    }
}
