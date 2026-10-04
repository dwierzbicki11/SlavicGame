using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq33CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(false);
        Check(!new Mq33Campaign(gated.Progress).Begin(), "MQ33 is gated by MQ32 completion");

        var peaceful = NewWorld(true); var mq33 = new Mq33Campaign(peaceful.Progress);
        Check(mq33.Begin(), "MQ33 begins after MQ32");
        Check(mq33.RecordWszeborPosition(Mq33WszeborStance.Guarded), "Wszebor position recorded");
        Check(mq33.ResolveConfrontation(Mq33ConfrontationOutcome.TemporaryAccord, Mq33WszeborStance.Cooperative, true), "peaceful outcome completes MQ33");
        Check(mq33.HasOutcome(Mq33ConfrontationOutcome.TemporaryAccord), "peaceful outcome persists");
        Check(mq33.HasStance(Mq33WszeborStance.Cooperative), "final stance persists");
        Check(mq33.HasDataForMq34, "direct data is available to MQ34");
        Check(peaceful.Progress.Quests.Get(Mq33Campaign.NextQuestId).Phase == QuestPhase.Offered, "peaceful outcome unlocks MQ34");
        Check(!mq33.ResolveConfrontation(Mq33ConfrontationOutcome.Break, Mq33WszeborStance.Hostile, false), "completion is idempotent");

        var combat = NewWorld(true); var combatQuest = new Mq33Campaign(combat.Progress); combatQuest.Begin();
        Check(combatQuest.ResolveConfrontation(Mq33ConfrontationOutcome.Combat, Mq33WszeborStance.Hostile, false), "combat outcome completes MQ33");
        Check(combatQuest.HasOutcome(Mq33ConfrontationOutcome.Combat), "combat outcome persists");
        Check(combat.Progress.HasFlag(Mq33Campaign.RecoveryDataAvailable), "secured recovery supplies data after combat");
        Check(combatQuest.HasDataForMq34, "recovery data is sufficient for MQ34");
        Check(combat.Progress.Quests.Get(Mq33Campaign.NextQuestId).Phase == QuestPhase.Offered, "combat cannot block MQ34");

        var escape = NewWorld(true); var escapeQuest = new Mq33Campaign(escape.Progress); escapeQuest.Begin();
        Check(escapeQuest.ResolveConfrontation(Mq33ConfrontationOutcome.Escape, Mq33WszeborStance.Hostile, false), "escape outcome completes MQ33");
        Check(escapeQuest.HasDataForMq34, "escape recovery cannot block MQ34");

        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, SaveGameService.Serialize(combat));
        var resumed = new Mq33Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq33Campaign.Complete), "MQ33 completion survives save/load");
        Check(resumed.HasOutcome(Mq33ConfrontationOutcome.Combat), "MQ52-facing outcome survives save/load");
        Check(resumed.HasStance(Mq33WszeborStance.Hostile), "stance survives save/load");
        Check(resumed.HasDataForMq34, "recovery package survives save/load");
        Check(restored.Progress.Quests.Get(Mq33Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ34 handoff survives save/load");
        restored.Progress.Quests.Get(Mq33Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!resumed.ResolveConfrontation(Mq33ConfrontationOutcome.Conversation, Mq33WszeborStance.Cooperative, true), "restored completion remains idempotent");
        Check(restored.Progress.Quests.Get(Mq33Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeat cannot reset MQ34 progress");

        Console.WriteLine($"MQ33 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState(); world.Initialize();
        if (ready) world.Progress.SetFlag(Mq32Campaign.Complete);
        world.Progress.Quests.Get(Mq33Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
