using SlavicGame.Engine.Gods;
using SlavicGame.Engine.Inventory;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public sealed class GameProgress
{
    private readonly HashSet<string> _worldFlags = new(StringComparer.Ordinal);

    public PlayerProfile Profile { get; } = new();
    public InventoryState Inventory { get; } = new();
    public QuestJournal Quests { get; } = new();
    public ReputationSystem Reputation { get; } = new();
    public DivineRelationshipSystem DivineRelationships { get; } = new();
    public RelationshipSystem Relationships { get; } = new();
    public TrackingState Tracking { get; } = new();
    public NavigationState Navigation { get; } = new();
    public IReadOnlyCollection<string> WorldFlags => _worldFlags;

    public bool HasFlag(string flag) => _worldFlags.Contains(flag);

    public void SetFlag(string flag, bool enabled = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flag);
        if (enabled) _worldFlags.Add(flag);
        else _worldFlags.Remove(flag);
    }

    public string[] CaptureFlags() => _worldFlags.OrderBy(flag => flag).ToArray();

    public void RestoreFlags(IEnumerable<string> flags)
    {
        ArgumentNullException.ThrowIfNull(flags);
        _worldFlags.Clear();
        foreach (var flag in flags)
        {
            if (!string.IsNullOrWhiteSpace(flag)) _worldFlags.Add(flag);
        }
    }
}
