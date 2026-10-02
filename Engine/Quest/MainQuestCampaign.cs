using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Reputation;

namespace SlavicGame.Engine.Quest;

public enum Mq11Position
{
    ConditionalCooperation,
    ProceduralOpposition,
    LocalArrangement
}

/// <summary>
/// Deterministic campaign progression for implementation-ready main-quest contracts.
/// Presentation/content systems decide how beats are presented; this class owns durable
/// quest, evidence and flag transitions so optional content cannot soft-lock progression.
/// </summary>
public sealed class MainQuestCampaign
{
    public const string Mq00Id = "MQ00";
    public const string Mq01Id = "MQ01";
    public const string Mq10Id = "MQ10";
    public const string Mq11Id = "MQ11";
    public const string Mq12Id = "MQ12";

    public const string Mq00Complete = "MQ00_COMPLETE";
    public const string Mq01ConsequenceSeen = "MQ01_CONSEQUENCE_SEEN";
    public const string Mq01NadborzeLead = "MQ01_NADBORZE_LEAD";
    public const string Mq01Complete = "MQ01_COMPLETE";
    public const string Mq10SiteFound = "MQ10_SITE_FOUND";
    public const string Mq10OldLayerConfirmed = "MQ10_OLD_LAYER_CONFIRMED";
    public const string Mq10Pattern02 = "MQ10_PATTERN_02";
    public const string Mq10SiteSecured = "MQ10_SITE_SECURED";
    public const string Mq10Complete = "MQ10_COMPLETE";
    public const string Mq11PolicySeen = "MQ11_POLICY_SEEN";
    public const string Mq11AdministrationPerspective = "MQ11_PERSPECTIVE_ADMINISTRATION";
    public const string Mq11CommunityPerspective = "MQ11_PERSPECTIVE_COMMUNITY";
    public const string Mq11ProcedureCostSeen = "MQ11_PROCEDURE_COST_SEEN";
    public const string Mq11DecisionConditional = "MQ11-D01_CONDITIONAL_COOPERATION";
    public const string Mq11DecisionOpposition = "MQ11-D01_PROCEDURAL_OPPOSITION";
    public const string Mq11DecisionLocal = "MQ11-D01_LOCAL_ARRANGEMENT";
    public const string Mq11Complete = "MQ11_COMPLETE";

    private readonly GameProgress _progress;

    public MainQuestCampaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public void SynchronizeAct0()
    {
        var mq00 = _progress.Quests.Get(Mq00Id);
        if (mq00.Phase == QuestPhase.TurnedIn) _progress.SetFlag(Mq00Complete);

        if (_progress.HasFlag(Mq00Complete))
        {
            var mq01 = _progress.Quests.Get(Mq01Id);
            if (mq01.Phase == QuestPhase.Unavailable) mq01.SetPhase(QuestPhase.Offered);
        }

        if (_progress.HasFlag(Mq01Complete))
        {
            var mq10 = _progress.Quests.Get(Mq10Id);
            if (mq10.Phase == QuestPhase.Unavailable) mq10.SetPhase(QuestPhase.Offered);
        }
    }

    public bool MarkMq01ConsequenceSeen()
    {
        if (!_progress.HasFlag(Mq00Complete)) return false;
        _progress.SetFlag(Mq01ConsequenceSeen);
        return true;
    }

    public bool GrantNadborzeLead()
    {
        if (!_progress.HasFlag(Mq00Complete)) return false;
        _progress.SetFlag(Mq01NadborzeLead);
        var mq01 = _progress.Quests.Get(Mq01Id);
        if (mq01.Phase is QuestPhase.Unavailable or QuestPhase.Offered) mq01.SetPhase(QuestPhase.Active);
        return true;
    }

    public bool EnsureNadborzeLead() => GrantNadborzeLead();

    public bool DepartForNadborze()
    {
        if (!_progress.HasFlag(Mq01NadborzeLead) || _progress.HasFlag(Mq01Complete)) return false;
        _progress.Quests.Get(Mq01Id).Resolve(QuestResolution.Other);
        _progress.SetFlag(Mq01Complete);
        SynchronizeAct0();
        return true;
    }

    public bool DiscoverMq10Site()
    {
        if (!_progress.HasFlag(Mq01Complete)) return false;
        _progress.SetFlag(Mq10SiteFound);
        var quest = _progress.Quests.Get(Mq10Id);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered) quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool RecordMq10OldLayerEvidence(string evidenceId, string text, string? sourceId = null)
    {
        if (!_progress.HasFlag(Mq10SiteFound)) return false;
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var quest = _progress.Quests.Get(Mq10Id);
        quest.AddEvidence(new EvidenceEntry(evidenceId, Mq10Id, KnowledgeKind.ConfirmedFact, text, sourceId));
        if (quest.Evidence.Count(e => e.Kind == KnowledgeKind.ConfirmedFact) >= 2)
        {
            _progress.SetFlag(Mq10OldLayerConfirmed);
        }
        return true;
    }

    public bool CompareMq10Pattern()
    {
        if (!_progress.HasFlag(Mq10OldLayerConfirmed)) return false;
        _progress.SetFlag(Mq10Pattern02);
        return true;
    }

    public bool SecureMq10Site()
    {
        if (!_progress.HasFlag(Mq10Pattern02)) return false;
        _progress.SetFlag(Mq10SiteSecured);
        return true;
    }

    public bool CompleteMq10()
    {
        if (!_progress.HasFlag(Mq10Pattern02) || !_progress.HasFlag(Mq10SiteSecured) || _progress.HasFlag(Mq10Complete)) return false;
        _progress.Quests.Get(Mq10Id).Resolve(QuestResolution.Other);
        _progress.SetFlag(Mq10Complete);
        var mq11 = _progress.Quests.Get(Mq11Id);
        if (mq11.Phase == QuestPhase.Unavailable) mq11.SetPhase(QuestPhase.Offered);
        return true;
    }

    public bool SeeMq11Policy()
    {
        if (!_progress.HasFlag(Mq10Complete)) return false;
        _progress.SetFlag(Mq11PolicySeen);
        var quest = _progress.Quests.Get(Mq11Id);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered) quest.SetPhase(QuestPhase.Active);
        return true;
    }

    public bool RecordMq11Perspective(bool administration)
    {
        if (!_progress.HasFlag(Mq11PolicySeen)) return false;
        _progress.SetFlag(administration ? Mq11AdministrationPerspective : Mq11CommunityPerspective);
        return true;
    }

    public bool RecordMq11ProcedureCost()
    {
        if (!_progress.HasFlag(Mq11PolicySeen)) return false;
        _progress.SetFlag(Mq11ProcedureCostSeen);
        return true;
    }

    public bool ChooseMq11Position(Mq11Position position, string factionId, int reputationDelta)
    {
        if (!_progress.HasFlag(Mq11PolicySeen) ||
            !_progress.HasFlag(Mq11AdministrationPerspective) ||
            !_progress.HasFlag(Mq11CommunityPerspective) ||
            !_progress.HasFlag(Mq11ProcedureCostSeen) ||
            HasMq11Decision()) return false;

        ArgumentException.ThrowIfNullOrWhiteSpace(factionId);
        var decisionFlag = position switch
        {
            Mq11Position.ConditionalCooperation => Mq11DecisionConditional,
            Mq11Position.ProceduralOpposition => Mq11DecisionOpposition,
            Mq11Position.LocalArrangement => Mq11DecisionLocal,
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };

        // Tuning owns the numeric delta and faction ID; campaign code owns applying it once
        // and persisting the selected MQ11-D01 branch before downstream world reactions.
        _progress.SetFlag(decisionFlag);
        _progress.Reputation.Change(ReputationScope.Faction, factionId, reputationDelta);
        return true;
    }

    public bool CompleteMq11()
    {
        if (!HasMq11Decision() || _progress.HasFlag(Mq11Complete)) return false;
        _progress.Quests.Get(Mq11Id).Resolve(QuestResolution.Other);
        _progress.SetFlag(Mq11Complete);
        var mq12 = _progress.Quests.Get(Mq12Id);
        if (mq12.Phase == QuestPhase.Unavailable) mq12.SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool HasMq11Decision() =>
        _progress.HasFlag(Mq11DecisionConditional) ||
        _progress.HasFlag(Mq11DecisionOpposition) ||
        _progress.HasFlag(Mq11DecisionLocal);
}
