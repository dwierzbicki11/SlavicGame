using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.World;

internal static class NpcReactionRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10.0);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var civilian =
            world.NpcWorld.Find("settler-youth-01")
            ?? throw new Exception("Youth settler missing");
        var guard =
            world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Community guard missing");

        world.SetPlayerPosition(
            civilian.Position +
            new Vector3(1.0f, 0f, 0f));

        check(world.Melee.TryStart(world),
            "Player can enter melee state near a civilian");

        world.NpcWorld.Update(world);

        var avoiding =
            world.NpcWorld.Find("settler-youth-01")
            ?? throw new Exception("Youth settler disappeared");
        check(
            avoiding.Reaction == NpcReactionKind.AvoidPlayer &&
            avoiding.IsMoving,
            "Civilian backs away from a nearby player attack");

        world.SetPlayerPosition(
            guard.Position +
            new Vector3(1.2f, 0f, 0f));

        var guardBase = new Vector2(
            guard.Position.X,
            guard.Position.Z);
        var guardReaction = NpcReactionSystem.Resolve(
            world,
            world.Npcs.Single(npc => npc.Id == "community-guard"),
            guardBase,
            Vector2.UnitY,
            true,
            protectedByDialogue: false);

        check(
            guardReaction.Kind == NpcReactionKind.WatchPlayer &&
            !guardReaction.IsMoving,
            "Community guard faces a threatening player instead of fleeing");

        world.Melee.Update(world, Vector3.UnitZ, 2.0);
        check(world.Melee.Controller.State == MeleeAttackState.Free,
            "Melee threat state returns to free before enemy reaction test");

        world.NpcWorld.Update(world);
        civilian =
            world.NpcWorld.Find("settler-farmer-02")
            ?? throw new Exception("Farmer settler missing");
        guard =
            world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Community guard missing");

        var threat = world.Enemies.Single(enemy =>
            string.Equals(
                enemy.Id,
                "swamp-predator",
                StringComparison.Ordinal));

        var civilianThreatPosition =
            civilian.Position +
            new Vector3(2.2f, 0f, 0f);
        civilianThreatPosition.Y =
            world.Terrain.SampleHeight(civilianThreatPosition);

        threat.Restore(new EnemySnapshot(
            threat.Id,
            civilianThreatPosition,
            threat.MaxHealth,
            EnemyState.Attack));

        world.NpcWorld.Update(world);

        var fleeing =
            world.NpcWorld.Find("settler-farmer-02")
            ?? throw new Exception("Farmer disappeared during threat");
        check(
            fleeing.Reaction == NpcReactionKind.FleeThreat &&
            fleeing.IsMoving &&
            HorizontalDistance(
                fleeing.Position,
                threat.Position) >
            HorizontalDistance(
                civilian.Position,
                threat.Position),
            "Civilian moves farther away from an engaged predator");

        var guardThreatPosition =
            guard.Position +
            new Vector3(3.2f, 0f, 0f);
        guardThreatPosition.Y =
            world.Terrain.SampleHeight(guardThreatPosition);

        threat.Restore(new EnemySnapshot(
            threat.Id,
            guardThreatPosition,
            threat.MaxHealth,
            EnemyState.Attack));

        world.NpcWorld.Update(world);

        var guarding =
            world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Guard disappeared during threat");
        check(
            guarding.Reaction == NpcReactionKind.GuardThreat &&
            guarding.IsMoving,
            "Community guard takes a guarding stance toward an engaged predator");

        var protectedReaction = NpcReactionSystem.Resolve(
            world,
            world.Npcs.Single(npc => npc.Id == "settler-farmer-02"),
            new Vector2(
                fleeing.Position.X,
                fleeing.Position.Z),
            Vector2.UnitX,
            true,
            protectedByDialogue: true);

        check(
            protectedReaction.Kind == NpcReactionKind.None &&
            !protectedReaction.IsMoving,
            "Dialogue protection freezes NPC and suppresses situational reactions");
    }

    private static float HorizontalDistance(
        Vector3 a,
        Vector3 b) =>
        Vector2.Distance(
            new Vector2(a.X, a.Z),
            new Vector2(b.X, b.Z));
}
