using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq51ApproachPressure
{
    Standard,
    Elevated,
    High
}

/// <summary>
/// Durable MQ51 progression ("Pierwszy Próg").
/// Guards entry into the final site until MQ50 is complete and translates lower support
/// tiers into increased encounter pressure rather than a campaign softlock.
/// </summary>
public sealed class Mq51Campaign
{
    public const string QuestId = "MQ51";
    public const string NextQuestId = "MQ52";
    public const string ApproachStarted = "MQ51_APPROACH_STARTED";
    public const string ThresholdReached = "MQ51_THRESHOLD_REACHED";
    public const string FinalSiteUnlocked = "MQ51_FINAL_SITE_UNLOCKED";
    public const string Complete = "MQ51_COMPLETE";
    public const string PressurePrefix = "MQ51_APPROACH_PRESSURE_";

    private readonly GameProgress _progress;

    public Mq51Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool BeginApproach()
    {
        if (!_progress.HasFlag(Mq50Campaign.Complete))
            return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        if (!_progress.HasFlag(ApproachStarted))
        {
            LockApproachPressure(ComputeApproachPressure());
            _progress.SetFlag(ApproachStarted);
        }

        return true;
    }

    public Mq51ApproachPressure? ApproachPressure
    {
        get
        {
            foreach (var pressure in Enum.GetValues<Mq51ApproachPressure>())
            {
                if (_progress.HasFlag(PressureFlag(pressure)))
                    return pressure;
            }

            return null;
        }
    }

    public bool ReachThreshold()
    {
        if (!IsActive() || !_progress.HasFlag(ApproachStarted))
            return false;

        _progress.SetFlag(ThresholdReached);
        return true;
    }

    public bool UnlockFinalSite()
    {
        if (!IsActive() || !_progress.HasFlag(ThresholdReached))
            return false;

        _progress.SetFlag(FinalSiteUnlocked);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(FinalSiteUnlocked))
            return false;

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);

        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);

        return true;
    }

    private Mq51ApproachPressure ComputeApproachPressure()
    {
        var mq50 = new Mq50Campaign(_progress);
        var reinforcedCount = Enum.GetValues<Mq50PreparationCategory>()
            .Count(category => mq50.GetTier(category) >= Mq50PreparationTier.Reinforced);

        return reinforcedCount switch
        {
            >= 3 => Mq51ApproachPressure.Standard,
            >= 1 => Mq51ApproachPressure.Elevated,
            _ => Mq51ApproachPressure.High
        };
    }

    private void LockApproachPressure(Mq51ApproachPressure selected)
    {
        foreach (var pressure in Enum.GetValues<Mq51ApproachPressure>())
            _progress.SetFlag(PressureFlag(pressure), pressure == selected);
    }

    private static string PressureFlag(Mq51ApproachPressure pressure) =>
        PressurePrefix + pressure.ToString().ToUpperInvariant();

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
