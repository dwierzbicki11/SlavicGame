using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class WildlifeHuntingRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var deer =
            world.Wildlife.Actors.Single(actor =>
                actor.Id == "deer-oak-01");

        check(
            deer.Behavior != WildlifeBehavior.Dead &&
            MathF.Abs(deer.Health - 45f) < 0.001f &&
            MathF.Abs(deer.MaxHealth - 45f) < 0.001f,
            "Deer starts alive with its hunting health budget");

        var playerPosition =
            new Vector3(
                deer.Position.X,
                0f,
                deer.Position.Z - 1.45f);
        world.SetPlayerPosition(playerPosition);

        var staminaBefore = world.Player.Stamina;

        for (var swing = 0; swing < 3; swing++)
        {
            check(
                world.Melee.TryStart(world),
                $"Wildlife melee swing {swing + 1} starts");

            world.Melee.Update(
                world,
                Vector3.UnitZ,
                0.25);

            var afterHit =
                world.Wildlife.Actors.Single(actor =>
                    actor.Id == deer.Id);

            if (swing == 0)
            {
                check(
                    MathF.Abs(afterHit.Health - 25f) < 0.001f &&
                    afterHit.Behavior == WildlifeBehavior.Flee,
                    "First melee wound makes the deer flee instead of ignoring damage");
            }

            world.Melee.Update(
                world,
                Vector3.UnitZ,
                0.40);
        }

        var carcass =
            world.Wildlife.Actors.Single(actor =>
                actor.Id == deer.Id);

        check(
            carcass.Behavior == WildlifeBehavior.Dead &&
            !carcass.IsMoving &&
            carcass.Health == 0f &&
            !carcass.Looted,
            "Three light attacks create a stationary unlooted deer carcass");

        check(
            MathF.Abs(
                world.Player.Stamina -
                (staminaBefore -
                 PlayerMeleeCombat.LightAttack.StaminaCost * 3f)) <
            0.001f,
            "Hunting melee spends stamina exactly once per swing");

        world.SetPlayerPosition(carcass.Position);
        world.EnvironmentInteractions.Update(world);

        check(
            world.EnvironmentInteractions.Current?.Id ==
                $"wildlife.harvest.{deer.Id}" &&
            world.EnvironmentInteractions.Current.Prompt.Contains(
                "JELEN",
                StringComparison.Ordinal),
            "Dead deer exposes a contextual E harvest interaction");

        check(
            world.EnvironmentInteractions.TryInteract(world) &&
            world.Progress.Inventory.Count("venison") == 3 &&
            world.Progress.Inventory.Count("deer-hide") == 1,
            "Harvest transfers the prototype deer meat and hide exactly once");

        var harvested =
            world.Wildlife.Actors.Single(actor =>
                actor.Id == deer.Id);
        check(
            harvested.Looted &&
            world.Wildlife.FindNearestHarvestable(
                harvested.Position) is null,
            "Harvested carcass is no longer harvestable");

        world.EnvironmentInteractions.Update(world);
        check(
            world.EnvironmentInteractions.Current?.Id !=
                $"wildlife.harvest.{deer.Id}",
            "Harvest interaction disappears immediately after looting");

        check(
            WildlifeHarvestCatalog.For(
                WildlifeSpecies.Boar).Any(item =>
                    item.ItemId == "boar-meat" &&
                    item.Quantity == 3) &&
            WildlifeHarvestCatalog.For(
                WildlifeSpecies.Wolf).Any(item =>
                    item.ItemId == "wolf-pelt" &&
                    item.Quantity == 1),
            "Boar and wolf expose distinct prototype hunting loot");

        var raven =
            world.Wildlife.Actors.Single(actor =>
                actor.Id == "raven-start-01");
        check(
            raven.Behavior != WildlifeBehavior.Dead &&
            raven.Health == raven.MaxHealth,
            "Ambient raven remains alive during ground melee hunting pass");

        var json =
            SaveGameService.Serialize(world);
        var restored =
            WorldGenerator.Generate();
        SaveGameService.Restore(
            restored,
            json);

        var restoredDeer =
            restored.Wildlife.Actors.Single(actor =>
                actor.Id == deer.Id);

        check(
            restoredDeer.Behavior == WildlifeBehavior.Dead &&
            restoredDeer.Health == 0f &&
            restoredDeer.Looted,
            "Dead harvested wildlife survives save/load");

        check(
            restored.Progress.Inventory.Count("venison") == 3 &&
            restored.Progress.Inventory.Count("deer-hide") == 1 &&
            restored.Wildlife.FindNearestHarvestable(
                restoredDeer.Position) is null,
            "Save/load preserves hunting loot without reopening the carcass");
    }
}
