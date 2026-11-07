using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum LootPanel { Container, Inventory }
public enum LootCommand { ContainerPanel, InventoryPanel, Previous, Next, TransferOne, TransferStack, TakeAll, Close }

/// <summary>World access and selection for the authoritative multi-chest loot controller.</summary>
public sealed class LootContainerRuntime
{
    public const string TargetId = "workstation-trader-chest";
    private const float AccessDistance = 3.2f;

    public static readonly IReadOnlyDictionary<string, (string Title, Vector3 Position, LootStack[] InitialLoot)> WorldContainers =
        new Dictionary<string, (string Title, Vector3 Position, LootStack[] InitialLoot)>(StringComparer.Ordinal)
        {
            [TargetId] = ("KUFER NA RYNKU", new Vector3(2.0f, 0f, -76.5f), []),
            ["stronghold-chest-hall"] = ("KUFER W WIELKIEJ SALI GRODU", new Vector3(-317f, 0f, 231f), [
                new("iron-chunk", 5),
                new("arrow-iron", 20),
                new("simple-bandage", 3)
            ]),
            ["stronghold-chest-market"] = ("SKRZYNIA WOJSKOWA GRODU", new Vector3(-321f, 0f, 212f), [
                new("old-coin", 25),
                new("simple-bandage", 2)
            ]),
            ["caravan-chest-a"] = ("KUFER KARAWANY KUPIECKIEJ", new Vector3(-153f, 0f, 95f), [
                new("forest-resin", 5),
                new("iron-chunk", 3)
            ]),
            ["caravan-chest-b"] = ("SKRZYNIA TOWAROWA KUPCA", new Vector3(-156f, 0f, 96f), [
                new("old-coin", 30),
                new("marsh-herb", 4)
            ]),
            ["fishing-chest"] = ("SKRZYNIA RYBACKA PRZYSTANI", new Vector3(144f, 0f, -178f), [
                new("old-coin", 15),
                new("simple-bandage", 2),
                new("forest-resin", 3)
            ]),
            ["smolnik-chest"] = ("SCHOWEK SMOLNIKOW W BORZE", new Vector3(-176f, 0f, -272f), [
                new("forest-resin", 6),
                new("ziolo-piolun", 4),
                new("arrow-basic", 12)
            ]),
            ["mine-chest-gwarek"] = ("PRADAWNY KUFER GWARKOW W SZTOLNI", new Vector3(-413f, 0f, 314f), [
                new("iron-chunk", 8),
                new("ziolo-zywokost", 3),
                new("forest-resin", 4)
            ])
        };

    public string ActiveContainerId { get; private set; } = TargetId;
    public string CurrentContainerTitle =>
        WorldContainers.TryGetValue(ActiveContainerId, out var info) ? info.Title : "KUFER";
    public string CurrentContainerPrompt => $"E OTWORZ: {CurrentContainerTitle}";

    public bool IsOpen { get; private set; }
    public LootPanel Panel { get; private set; }
    public int SelectedIndex { get; private set; }
    public string Message { get; private set; } = "";

    public static Vector3? Position(WorldState world) =>
        world.Models.FirstOrDefault(model => model.Id == TargetId)?.Position;

    public static Vector3? Position(WorldState world, string containerId)
    {
        var model = world.Models.FirstOrDefault(m => string.Equals(m.Id, containerId, StringComparison.Ordinal));
        if (model is not null) return model.Position;
        return WorldContainers.TryGetValue(containerId, out var info) ? info.Position : null;
    }

    public (string Id, Vector3 Position)? NearestContainer(WorldState world)
    {
        (string Id, Vector3 Position)? nearest = null;
        var nearestDistSq = AccessDistance * AccessDistance;

        // Check primary TargetId first (from world.Models)
        if (Position(world) is { } targetPos)
        {
            var d2 = Vector3.DistanceSquared(targetPos, world.PlayerPosition);
            if (d2 <= nearestDistSq)
            {
                nearest = (TargetId, targetPos);
                nearestDistSq = d2;
            }
        }

        // Check other registered world containers
        foreach (var (id, (title, defaultPos, _)) in WorldContainers)
        {
            if (id == TargetId) continue;
            var pos = Position(world, id) ?? defaultPos;
            var d2 = Vector3.DistanceSquared(pos, world.PlayerPosition);
            if (d2 <= nearestDistSq)
            {
                nearest = (id, pos);
                nearestDistSq = d2;
            }
        }

        return nearest;
    }

    public bool CanOpenNearest(WorldState world) =>
        world.Player.IsAlive && !world.Dialogue.IsOpen && !world.Vendors.IsOpen &&
        !world.Crafting.IsOpen && !world.Magic.IsCasting && !world.Rituals.IsPerforming &&
        !world.Cinematics.IsPlaying &&
        world.Melee.Controller.State == SlavicGame.Engine.Combat.MeleeAttackState.Free &&
        NearestContainer(world) is not null;

    public bool TryOpenNearest(WorldState world)
    {
        if (!CanOpenNearest(world)) return false;
        var nearest = NearestContainer(world);
        if (nearest is null) return false;

        ActiveContainerId = nearest.Value.Id;
        IsOpen = true;
        Panel = LootPanel.Container;
        SelectedIndex = 0;
        Message = "";
        world.Bow.SetAiming(world, false);
        return true;
    }

    public void Close()
    {
        IsOpen = false;
        SelectedIndex = 0;
        Panel = LootPanel.Container;
        Message = "";
    }

    public void Update(WorldState world)
    {
        if (IsOpen && !CanOpenNearest(world)) Close();
    }

    public LootContainerDefinition CurrentDefinition(WorldState world)
    {
        var id = ActiveContainerId ?? TargetId;
        if (WorldContainers.TryGetValue(id, out var info))
            return new LootContainerDefinition(id, info.InitialLoot);
        return new LootContainerDefinition(id, []);
    }

    public IReadOnlyList<LootStack> Lines(WorldState world, LootPanel panel) =>
        panel == LootPanel.Container
            ? Controller(world).View.Stacks
            : world.Progress.Inventory.Items.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new LootStack(pair.Key, pair.Value)).ToArray();

    public void SelectPanel(LootPanel panel)
    {
        if (!IsOpen) return;
        Panel = panel;
        SelectedIndex = 0;
        Message = "";
    }

    public void MoveSelection(WorldState world, int direction)
    {
        if (!IsOpen) return;
        SelectedIndex = Math.Clamp(SelectedIndex + direction, 0,
            Math.Max(0, Lines(world, Panel).Count - 1));
    }

    /// <summary>Single command surface for keyboard/gamepad UI adapters.</summary>
    public LootContainerResult? HandleCommand(WorldState world, LootCommand command)
    {
        if (!IsOpen) return null;
        switch (command)
        {
            case LootCommand.ContainerPanel: SelectPanel(LootPanel.Container); break;
            case LootCommand.InventoryPanel: SelectPanel(LootPanel.Inventory); break;
            case LootCommand.Previous: MoveSelection(world, -1); break;
            case LootCommand.Next: MoveSelection(world, 1); break;
            case LootCommand.TransferOne: return TransferSelected(world);
            case LootCommand.TransferStack: return TransferSelected(world, wholeStack: true);
            case LootCommand.TakeAll: return TakeAll(world);
            case LootCommand.Close: Close(); break;
            default: throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
        return null;
    }

    // E moves one item; Shift+E moves the selected stack.
    public LootContainerResult TransferSelected(WorldState world, bool wholeStack = false)
    {
        if (!IsOpen || !CanOpenNearest(world)) { Close(); return LootContainerResult.InvalidTarget; }
        var lines = Lines(world, Panel);
        if (lines.Count == 0) { Message = "PUSTO"; return LootContainerResult.Empty; }
        SelectedIndex = Math.Clamp(SelectedIndex, 0, lines.Count - 1);
        var line = lines[SelectedIndex];
        var quantity = wholeStack ? line.Quantity : 1;
        var activeId = ActiveContainerId ?? TargetId;
        var pos = Position(world, activeId) ?? Position(world)!.Value;
        var target = new InteractionTarget(activeId, pos, InteractionKind.Use, CurrentContainerTitle);
        var result = Panel == LootPanel.Container
            ? Controller(world).Take(target, line.ItemId, quantity)
            : Controller(world).Store(target, line.ItemId, quantity);
        SelectedIndex = Math.Min(SelectedIndex, Math.Max(0, Lines(world, Panel).Count - 1));
        Message = result is LootContainerResult.Looted or LootContainerResult.Stored
            ? $"PRZENIESIONO {quantity} / {DisplayName(line.ItemId)}" : "NIE MOZNA PRZENIESC";
        return result;
    }

    /// <summary>
    /// Atomically moves every stack from the open container into inventory.
    /// The authoritative controller preflights inventory overflow, so failure never partially empties the chest.
    /// </summary>
    public LootContainerResult TakeAll(WorldState world)
    {
        if (!IsOpen || !CanOpenNearest(world))
        {
            Close();
            return LootContainerResult.InvalidTarget;
        }

        var before = Lines(world, LootPanel.Container);
        if (before.Count == 0)
        {
            Message = "PUSTO";
            return LootContainerResult.Empty;
        }

        var activeId = ActiveContainerId ?? TargetId;
        var pos = Position(world, activeId) ?? Position(world)!.Value;
        var target = new InteractionTarget(activeId, pos, InteractionKind.Use, CurrentContainerTitle);
        var result = Controller(world).TakeAll(target);
        if (result == LootContainerResult.Looted)
        {
            SelectedIndex = 0;
            Message = $"ZABRANO WSZYSTKO ({before.Sum(stack => stack.Quantity)})";
        }
        else
        {
            Message = "NIE MOZNA PRZENIESC";
        }

        return result;
    }

    private LootContainerUiController Controller(WorldState world) =>
        new(world.Progress.LootContainers, CurrentDefinition(world), world.Progress.Inventory);

    public static string DisplayName(string id) => id switch
    {
        "arrow-basic" => "STRZALA",
        "arrow-iron" => "STRZALA ZE STALI",
        "simple-bow" => "LUK",
        "simple-bandage" => "OPATRUNEK",
        "forest-resin" => "ZYWICA LESNA",
        "marsh-herb" => "ZIELE MOKRADEL",
        "iron-chunk" => "BRYLA RUDY ZELAZA",
        "old-coin" => "STARY DENAR SLOWIANSKI",
        "ziolo-dziurawiec" => "DZIURAWIEC ZWYCZAJNY",
        "ziolo-piolun" => "PIOLUN POLNY",
        "ziolo-krwawnik" => "KRWAWNIK POSPOLITY",
        "ziolo-zywokost" => "KORZEN ZYWOKOSTU",
        "napar-leczniczy" => "NAPAR Z DZIURAWIEC I MIODU",
        "masc-zywokostowa" => "MASC ZYWOKOSTOWA",
        "odwar-piolunowy" => "ODWAR Z PIOLUNU",
        "ritual-thread" => "NIC RYTUALNA",
        "missing-person-keepsake" => "PAMIATKA ZAGINIONEGO",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };
}
