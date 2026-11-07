using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum TarBurnerOutcome
{
    None,
    TrailSecured,
    BurnerHelped,
    Completed
}

public static class TarBurnerSideQuest
{
    public const string QuestId = "side-tar-trail";
    public const string GiverId = "stronghold-chieftain";
    public const string BurnerId = "smolnik-burner";
    public const string TarBarrelItemId = "tar-barrel";

    public const string AcceptedFlag = "side-tar-trail.accepted";
    public const string BarrelFoundFlag = "side-tar-trail.barrel-found";
    public const string CartSecuredFlag = "side-tar-trail.cart-secured";
    public const string BurnerHelpedFlag = "side-tar-trail.burner-helped";
    public const string CompletedFlag = "side-tar-trail.completed";

    public const string TrailEvidenceId = "side-tar-trail.cart-found";
    public const string WolfEvidenceId = "side-tar-trail.wolf-tracks";

    public static readonly System.Numerics.Vector3 AmbushedCartSpot = new(-192f, 0f, -185f);

    public static bool IsStarted(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        return world.Progress.Quests.Get(QuestId).Phase is not QuestPhase.Unavailable;
    }

    public static bool CanInvestigate(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var phase = world.Progress.Quests.Get(QuestId).Phase;
        return phase is QuestPhase.Active or QuestPhase.Investigation;
    }

    public static bool RecordCartFound(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInvestigate(world) || world.Progress.HasFlag(BarrelFoundFlag))
            return false;

        var quest = world.Progress.Quests.Get(QuestId);
        world.Progress.Inventory.Add(TarBarrelItemId);
        world.Progress.SetFlag(BarrelFoundFlag);

        quest.AddEvidence(new EvidenceEntry(
            TrailEvidenceId,
            QuestId,
            KnowledgeKind.Observation,
            "Na leśnym trakcie odnaleziono wywrócony wóz smolników i ocalałą beczkę smoły.",
            "forest-trail"));

        quest.AddEvidence(new EvidenceEntry(
            WolfEvidenceId,
            QuestId,
            KnowledgeKind.ConfirmedFact,
            "Głębokie ślady pazurów wokół wozu świadczą o ataku watahy leśnych wilków.",
            "forest-trail"));

        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public static bool RecordCartSecured(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInvestigate(world) || world.Progress.HasFlag(CartSecuredFlag))
            return false;

        world.Progress.SetFlag(CartSecuredFlag);
        var quest = world.Progress.Quests.Get(QuestId);
        if (world.Progress.HasFlag(BurnerHelpedFlag))
            quest.SetPhase(QuestPhase.Resolved);

        return true;
    }

    public static bool HelpBurner(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (world.Progress.HasFlag(BurnerHelpedFlag))
            return false;

        world.Progress.SetFlag(BurnerHelpedFlag);
        world.Progress.Inventory.Remove(TarBarrelItemId, 1);
        world.Progress.Profile.ChangeMoney(25);
        world.Progress.Inventory.Add("forest-resin", 2);

        var quest = world.Progress.Quests.Get(QuestId);
        if (world.Progress.HasFlag(CartSecuredFlag))
            quest.SetPhase(QuestPhase.Resolved);

        return true;
    }

    public static bool CompleteQuest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (world.Progress.HasFlag(CompletedFlag))
            return false;

        world.Progress.SetFlag(CompletedFlag);
        world.Progress.Profile.ChangeMoney(60);
        world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 20);

        var quest = world.Progress.Quests.Get(QuestId);
        quest.SetPhase(QuestPhase.TurnedIn);
        quest.ClaimReward();

        return true;
    }

    public static TarBurnerOutcome Outcome(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (world.Progress.HasFlag(CompletedFlag))
            return TarBurnerOutcome.Completed;
        if (world.Progress.HasFlag(BurnerHelpedFlag))
            return TarBurnerOutcome.BurnerHelped;
        if (world.Progress.HasFlag(CartSecuredFlag))
            return TarBurnerOutcome.TrailSecured;
        return TarBurnerOutcome.None;
    }
}
