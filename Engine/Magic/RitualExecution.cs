using System.Numerics;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Magic;

public enum RitualStartFailure
{
    None,
    PlayerUnavailable,
    AlreadyResolved,
    NotLearned,
    MissingSign,
    WrongLocation,
    WrongTime,
    MissingItem,
    MissingEvidence,
    MissingKnowledge,
    ThreatNearby
}

public enum RitualStep
{
    None,
    DefineArea,
    PlaceAnchor,
    RecognitionSign,
    AwaitReaction,
    ConfirmIdentity,
    CloseBond,
    ObserveResult,
    Complete
}

public sealed record RitualStartResult(bool Started, RitualStartFailure Failure, string Message)
{
    public static RitualStartResult Success(string message) => new(true, RitualStartFailure.None, message);
    public static RitualStartResult Reject(RitualStartFailure failure, string message) => new(false, failure, message);
}

public static class VerticalSliceRituals
{
    public const string ReleaseBoundEchoId = "ritual.release-bound-echo";
    public const string LearnedFlag = "magic.ritual.release-bound-echo.learned";
    public const string IdentitySignFlag = "magic.sign.identity.learned";
    public const string BoundarySignFlag = "magic.sign.boundary.learned";
    public const string AnchorKnowledgeFlag = "light-over-swamp.anchor-link-known";
    public const string ReleasedFlag = "swamp.apparition-released";
    public const string BoundFlag = "swamp.apparition-bound";
    public const string DestroyedFlag = "swamp.anchor-destroyed";

    public static RitualDefinition ReleaseBoundEcho { get; } = new(
        ReleaseBoundEchoId,
        "DOMKNIECIE WIEZI",
        MagicSource.SoulsAndNawia,
        "old-shrine",
        20.0,
        6.0,
        [
            new ItemCost("missing-person-keepsake", 1),
            new ItemCost("ritual-thread", 1)
        ],
        ["light-over-swamp.keepsake-owner"],
        new MagicCost());
}

public sealed class RitualExecution
{
    private static readonly RitualStep[] Sequence =
    [
        RitualStep.DefineArea,
        RitualStep.PlaceAnchor,
        RitualStep.RecognitionSign,
        RitualStep.AwaitReaction,
        RitualStep.ConfirmIdentity,
        RitualStep.CloseBond,
        RitualStep.ObserveResult
    ];

    private static readonly IReadOnlyDictionary<RitualStep, string> StepMessages =
        new Dictionary<RitualStep, string>
        {
            [RitualStep.DefineArea] = "RYTUAL: WYZNACZANIE GRANICY",
            [RitualStep.PlaceAnchor] = "RYTUAL: UMIESZCZANIE KOTWICY",
            [RitualStep.RecognitionSign] = "RYTUAL: ZNAK ROZPOZNANIA",
            [RitualStep.AwaitReaction] = "RYTUAL: OCZEKIWANIE NA REAKCJE",
            [RitualStep.ConfirmIdentity] = "RYTUAL: POTWIERDZENIE TOZSAMOSCI",
            [RitualStep.CloseBond] = "RYTUAL: DOMYKANIE WIEZI",
            [RitualStep.ObserveResult] = "RYTUAL: OBSERWACJA SKUTKU"
        };

    private const double StepDurationSeconds = 1.0;
    private const float ThreatRadius = 25f;

    private int _stepIndex = -1;
    private double _stepRemaining;
    private float _healthAtStart;
    private bool _completionSignal;

    public bool IsPerforming => _stepIndex >= 0 && _stepIndex < Sequence.Length;
    public RitualStep CurrentStep => IsPerforming ? Sequence[_stepIndex] : RitualStep.None;
    public Vector3 VisualOrigin { get; private set; }
    public double CompletionGlowRemaining { get; private set; }
    public double StepProgress => !IsPerforming
        ? 0
        : Math.Clamp(1.0 - _stepRemaining / StepDurationSeconds, 0.0, 1.0);
    public double OverallProgress => IsPerforming
        ? Math.Clamp((_stepIndex + StepProgress) / Sequence.Length, 0.0, 1.0)
        : CompletionGlowRemaining > 0 ? 1.0 : 0.0;
    public string Message { get; private set; } = "R RYTUAL";
    public RitualStartFailure LastFailure { get; private set; }

    public RitualStartResult TryStart(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var validation = Validate(world);
        LastFailure = validation.Failure;
        Message = validation.Message;
        if (!validation.Started)
            return validation;

        _stepIndex = 0;
        _stepRemaining = StepDurationSeconds;
        _healthAtStart = world.Player.Health;
        VisualOrigin = world.PlayerPosition;
        CompletionGlowRemaining = 0;
        _completionSignal = false;
        Message = StepMessages[CurrentStep];
        return validation with { Message = Message };
    }

    public RitualStartResult Validate(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var definition = VerticalSliceRituals.ReleaseBoundEcho;
        var progress = world.Progress;
        var inventory = progress.Inventory;
        var quest = progress.Quests.Get(SlavicGame.Engine.Gameplay.VerticalSliceBootstrap.ContractQuestId);

        if (!world.Player.IsAlive || world.Magic.IsCasting || world.Cinematics.IsPlaying || IsPerforming)
            return RitualStartResult.Reject(RitualStartFailure.PlayerUnavailable, "RYTUAL TERAZ NIEDOSTEPNY");

        if (progress.HasFlag(VerticalSliceRituals.ReleasedFlag) ||
            progress.HasFlag(VerticalSliceRituals.BoundFlag) ||
            progress.HasFlag(VerticalSliceRituals.DestroyedFlag) ||
            quest.Resolution != QuestResolution.None)
            return RitualStartResult.Reject(RitualStartFailure.AlreadyResolved, "LOS ZJAWY JUZ ROZSTRZYGNIETY");

        if (!progress.HasFlag(VerticalSliceRituals.LearnedFlag))
            return RitualStartResult.Reject(RitualStartFailure.NotLearned, "NIE ZNASZ TEGO RYTUALU");

        if (!progress.HasFlag(VerticalSliceRituals.IdentitySignFlag) ||
            !progress.HasFlag(VerticalSliceRituals.BoundarySignFlag))
            return RitualStartResult.Reject(RitualStartFailure.MissingSign, "BRAK POZNANYCH ZNAKOW RYTUALNYCH");

        if (!string.Equals(world.CurrentRegion, definition.RequiredLocationId, StringComparison.Ordinal))
            return RitualStartResult.Reject(RitualStartFailure.WrongLocation, "TO NIE JEST WLASCIWE MIEJSCE");

        if (!RitualRules.IsActiveAt(definition, world.Time.TimeOfDayHours))
            return RitualStartResult.Reject(RitualStartFailure.WrongTime, "MIEJSCE NIE REAGUJE O TEJ PORZE");

        foreach (var item in definition.RequiredItems)
            if (!inventory.Contains(item.ItemId, item.Quantity))
                return RitualStartResult.Reject(
                    RitualStartFailure.MissingItem,
                    $"BRAK SKLADNIKA: {item.ItemId.ToUpperInvariant()}");

        var evidence = quest.Evidence.ToDictionary(entry => entry.Id, StringComparer.Ordinal);
        foreach (var evidenceId in definition.RequiredEvidenceIds)
            if (!evidence.TryGetValue(evidenceId, out var entry) || entry.Kind != KnowledgeKind.ConfirmedFact)
                return RitualStartResult.Reject(RitualStartFailure.MissingEvidence, "BRAK POTWIERDZONEJ TOZSAMOSCI");

        if (!progress.HasFlag(VerticalSliceRituals.AnchorKnowledgeFlag))
            return RitualStartResult.Reject(RitualStartFailure.MissingKnowledge, "NIE ZNASZ ZWIAZKU KOTWICY ZE ZJAWA");

        if (HasNearbyThreat(world))
            return RitualStartResult.Reject(RitualStartFailure.ThreatNearby, "NAJPIERW ZABEZPIECZ MIEJSCE");

        return RitualStartResult.Success("RYTUAL GOTOWY");
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
            return;

        CompletionGlowRemaining = Math.Max(0, CompletionGlowRemaining - deltaSeconds);
        if (!IsPerforming)
            return;

        if (!world.Player.IsAlive || world.Player.Health < _healthAtStart)
        {
            Interrupt("RYTUAL PRZERWANY: OTRZYMANO OBRAZENIA");
            return;
        }

        if (HasNearbyThreat(world))
        {
            Interrupt("RYTUAL PRZERWANY: ZAGROZENIE");
            return;
        }

        if (!string.Equals(world.CurrentRegion, VerticalSliceRituals.ReleaseBoundEcho.RequiredLocationId, StringComparison.Ordinal))
        {
            Interrupt("RYTUAL PRZERWANY: OPUSZCZONO MIEJSCE");
            return;
        }

        _stepRemaining -= deltaSeconds;
        while (IsPerforming && _stepRemaining <= 0)
        {
            _stepIndex++;
            if (_stepIndex >= Sequence.Length)
            {
                Complete(world);
                return;
            }

            _stepRemaining += StepDurationSeconds;
            Message = StepMessages[CurrentStep];
        }
    }

    public void Interrupt(string message = "RYTUAL PRZERWANY")
    {
        _stepIndex = -1;
        _stepRemaining = 0;
        Message = message;
    }

    public bool ConsumeCompletionSignal()
    {
        if (!_completionSignal) return false;
        _completionSignal = false;
        return true;
    }

    private void Complete(WorldState world)
    {
        var definition = VerticalSliceRituals.ReleaseBoundEcho;
        var inventory = world.Progress.Inventory;

        // Critical items are committed only here. Interrupted or invalid attempts
        // never consume the anchor or ritual thread.
        foreach (var item in definition.RequiredItems)
        {
            if (!inventory.Contains(item.ItemId, item.Quantity))
            {
                Interrupt("RYTUAL PRZERWANY: BRAK SKLADNIKA");
                return;
            }
        }

        foreach (var item in definition.RequiredItems)
            inventory.Remove(item.ItemId, item.Quantity);

        world.Progress.SetFlag(VerticalSliceRituals.ReleasedFlag);
        world.Progress.SetFlag("magic.ritual.release-bound-echo.completed");

        var quest = world.Progress.Quests.Get(SlavicGame.Engine.Gameplay.VerticalSliceBootstrap.ContractQuestId);
        if (quest.Resolution == QuestResolution.None)
            quest.Resolve(QuestResolution.RitualClosure);

        _stepIndex = -1;
        _stepRemaining = 0;
        _completionSignal = true;
        CompletionGlowRemaining = 2.0;
        LastFailure = RitualStartFailure.None;
        Message = "RYTUAL ZAKONCZONY: ECHO UWOLNIONE";
    }

    private static bool HasNearbyThreat(WorldState world) =>
        world.Enemies.Any(enemy =>
            enemy.IsAlive &&
            System.Numerics.Vector3.DistanceSquared(enemy.Position, world.PlayerPosition) <= ThreatRadius * ThreatRadius);
}
