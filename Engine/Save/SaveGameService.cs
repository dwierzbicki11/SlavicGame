using Newtonsoft.Json;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Gods;
using SlavicGame.Engine.Inventory;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Save;

public sealed record SavedVector3(float X, float Y, float Z);

public sealed record EnemySaveEntry(string Id, SavedVector3 Position, float Health, EnemyState State);

public sealed record GameSaveSnapshot(
    int Version, SavedVector3 PlayerPosition, double TimeOfDayHours, WeatherKind Weather,
    float PlayerHealth, float PlayerStamina, string PlayerName, int Money, string[] Titles,
    InventoryEntry[] Inventory, QuestSnapshot[] Quests, ReputationEntry[] Reputation,
    DivineRelationshipSnapshot[] DivineRelationships, RelationshipSnapshot[] Relationships,
    string[] WorldFlags, TrackSnapshot[] Tracks, NavigationSnapshot Navigation,
    MapOverlaySnapshot MapOverlay, BoundaryPhenomenonSnapshot[] BoundaryPhenomena,
    EnemySaveEntry[] Enemies)
{
    public SlavicGame.Engine.Magic.MagicSnapshot? Magic { get; init; }
    public SlavicGame.Engine.Gameplay.EncounterSnapshot[]? Encounters { get; init; }
    public SlavicGame.Engine.Gameplay.VendorStockSnapshot[]? Vendors { get; init; }
    public WildlifeSnapshot[]? Wildlife { get; init; }
    public SlavicGame.Engine.Interaction.LootContainerSnapshot[]? LootContainers { get; init; }
}

public static class SaveGameService
{
    public const int CurrentVersion = 3;

    public static GameSaveSnapshot Capture(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var position = world.PlayerPosition;
        return new GameSaveSnapshot(
            CurrentVersion,
            new SavedVector3(position.X, position.Y, position.Z),
            world.Time.TimeOfDayHours, world.Weather.Condition,
            world.Player.Health, world.Player.Stamina,
            world.Progress.Profile.Name, world.Progress.Profile.Money,
            world.Progress.Profile.Titles.OrderBy(title => title).ToArray(),
            world.Progress.Inventory.Capture(), world.Progress.Quests.Capture(),
            world.Progress.Reputation.Capture(), world.Progress.DivineRelationships.Capture(),
            world.Progress.Relationships.Capture(), world.Progress.CaptureFlags(),
            world.Progress.Tracking.Capture(), world.Progress.Navigation.Capture(),
            world.Progress.MapOverlay.Capture(), world.Cosmology.Capture(),
            world.Enemies.Select(enemy =>
            {
                var snapshot = enemy.Capture();
                return new EnemySaveEntry(snapshot.Id,
                    new SavedVector3(snapshot.Position.X, snapshot.Position.Y, snapshot.Position.Z),
                    snapshot.Health, snapshot.State);
            }).ToArray())
        {
            Magic = world.Magic.Capture(),
            Encounters = world.Progress.Encounters.Capture(),
            Vendors = world.Vendors.Capture(),
            Wildlife = world.Wildlife.Capture(),
            LootContainers = world.Progress.LootContainers.Capture()
        };
    }

    public static string Serialize(WorldState world, bool indented = false) =>
        JsonConvert.SerializeObject(Capture(world), indented ? Formatting.Indented : Formatting.None);

    public static GameSaveSnapshot Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var snapshot = JsonConvert.DeserializeObject<GameSaveSnapshot>(json)
            ?? throw new InvalidOperationException("Save data could not be deserialized.");
        if (snapshot.Version != CurrentVersion)
            throw new NotSupportedException($"Save version {snapshot.Version} is not supported; expected {CurrentVersion}.");
        return snapshot;
    }

    public static void Restore(WorldState world, string json) => Restore(world, Deserialize(json));

    public static void Restore(WorldState world, GameSaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Version != CurrentVersion)
            throw new NotSupportedException($"Unsupported save version {snapshot.Version}.");

        world.Magic.Restore(snapshot.Magic);
        world.Cinematics.Reset();
        world.SetPlayerPosition(new System.Numerics.Vector3(snapshot.PlayerPosition.X, snapshot.PlayerPosition.Y, snapshot.PlayerPosition.Z));
        world.Time.SetTimeOfDay(snapshot.TimeOfDayHours);
        world.Weather.SetCondition(snapshot.Weather, immediate: true);
        world.Player.SetState(snapshot.PlayerHealth, snapshot.PlayerStamina);
        world.Progress.Profile.Restore(snapshot.PlayerName, snapshot.Money, snapshot.Titles ?? []);
        world.Progress.Inventory.Restore(snapshot.Inventory ?? []);
        world.Progress.Quests.Restore(snapshot.Quests ?? []);
        world.Progress.Reputation.Restore(snapshot.Reputation ?? []);
        world.Progress.DivineRelationships.Restore(snapshot.DivineRelationships ?? []);
        world.Progress.Relationships.Restore(snapshot.Relationships ?? []);
        world.Progress.RestoreFlags(snapshot.WorldFlags ?? []);
        world.Progress.Encounters.Restore(snapshot.Encounters ?? []);
        world.Vendors.Restore(snapshot.Vendors ?? []);
        world.Progress.LootContainers.Restore(snapshot.LootContainers ?? []);
        world.Loot.Close();
        world.Wildlife.Restore(
            world,
            snapshot.Wildlife ?? []);
        SlavicGame.Engine.Gameplay.VerticalSliceBootstrap.EnsureStarterBow(world);
        world.Bow.Reset();
        world.Magic.NormalizeSelection(world);
        world.Progress.Tracking.Restore(snapshot.Tracks ?? []);
        SlavicGame.Engine.Gameplay.VerticalSliceBootstrap.EnsureMagicTraces(world);
        world.Progress.Navigation.Restore(snapshot.Navigation ?? new NavigationSnapshot(null, []));
        world.Progress.MapOverlay.Restore(snapshot.MapOverlay ?? new MapOverlaySnapshot([], [], null, false));
        world.Cosmology.Restore(snapshot.BoundaryPhenomena ?? []);

        var byId = world.Enemies.ToDictionary(enemy => enemy.Id, StringComparer.Ordinal);
        foreach (var savedEnemy in snapshot.Enemies ?? [])
        {
            if (!byId.TryGetValue(savedEnemy.Id, out var enemy))
                throw new InvalidOperationException($"Save contains unknown enemy '{savedEnemy.Id}'.");
            enemy.Restore(new EnemySnapshot(savedEnemy.Id,
                new System.Numerics.Vector3(savedEnemy.Position.X, savedEnemy.Position.Y, savedEnemy.Position.Z),
                savedEnemy.Health, savedEnemy.State));
        }

        SlavicGame.Engine.Gameplay.SwampPredatorEncounter.Synchronize(world);
    }
}
