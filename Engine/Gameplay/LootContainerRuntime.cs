using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum LootPanel { Container, Inventory }

/// <summary>World access and selection for the existing authoritative loot controller.</summary>
public sealed class LootContainerRuntime
{
    public const string TargetId = "workstation-trader-chest";
    private const float AccessDistance = 3.2f;
    private static readonly LootContainerDefinition Definition = new(TargetId, []);
    public bool IsOpen { get; private set; }
    public LootPanel Panel { get; private set; }
    public int SelectedIndex { get; private set; }
    public string Message { get; private set; } = "";

    public static Vector3? Position(WorldState world) =>
        world.Models.FirstOrDefault(model => model.Id == TargetId)?.Position;

    public bool CanOpenNearest(WorldState world) =>
        world.Player.IsAlive && !world.Dialogue.IsOpen && !world.Vendors.IsOpen &&
        !world.Crafting.IsOpen && !world.Magic.IsCasting && !world.Rituals.IsPerforming &&
        !world.Cinematics.IsPlaying &&
        world.Melee.Controller.State == SlavicGame.Engine.Combat.MeleeAttackState.Free &&
        Position(world) is { } position &&
        Vector3.DistanceSquared(position, world.PlayerPosition) <= AccessDistance * AccessDistance;

    public bool TryOpenNearest(WorldState world)
    {
        if (!CanOpenNearest(world)) return false;
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

    // E moves one item; Shift+E moves the selected stack.
    public LootContainerResult TransferSelected(WorldState world, bool wholeStack = false)
    {
        if (!IsOpen || !CanOpenNearest(world)) { Close(); return LootContainerResult.InvalidTarget; }
        var lines = Lines(world, Panel);
        if (lines.Count == 0) { Message = "PUSTO"; return LootContainerResult.Empty; }
        SelectedIndex = Math.Clamp(SelectedIndex, 0, lines.Count - 1);
        var line = lines[SelectedIndex];
        var quantity = wholeStack ? line.Quantity : 1;
        var target = new InteractionTarget(TargetId, Position(world)!.Value, InteractionKind.Use, "KUFER");
        var result = Panel == LootPanel.Container
            ? Controller(world).Take(target, line.ItemId, quantity)
            : Controller(world).Store(target, line.ItemId, quantity);
        SelectedIndex = Math.Min(SelectedIndex, Math.Max(0, Lines(world, Panel).Count - 1));
        Message = result is LootContainerResult.Looted or LootContainerResult.Stored
            ? $"PRZENIESIONO {quantity} / {DisplayName(line.ItemId)}" : "NIE MOZNA PRZENIESC";
        return result;
    }

    private static LootContainerUiController Controller(WorldState world) =>
        new(world.Progress.LootContainers, Definition, world.Progress.Inventory);

    public static string DisplayName(string id) => id switch
    {
        "arrow-basic" => "STRZALA",
        "simple-bow" => "LUK",
        "simple-bandage" => "OPATRUNEK",
        "forest-resin" => "ZYWICA LESNA",
        "marsh-herb" => "ZIELE MOKRADEL",
        "ritual-thread" => "NIC RYTUALNA",
        "missing-person-keepsake" => "PAMIATKA ZAGINIONEGO",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };
}
