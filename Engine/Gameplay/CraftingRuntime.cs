using SlavicGame.Engine.Magic;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum CraftingResult
{
    None,
    Completed,
    Locked,
    MissingIngredients,
    Unavailable
}

public sealed record CraftingRecipeDefinition(
    AlchemyRecipe Recipe,
    string DisplayName,
    string StationNpcId,
    string RequiredActivity,
    string? RequiredFlag);

public sealed record CraftingViewLine(
    string RecipeId,
    string DisplayName,
    string RequirementText,
    bool Unlocked,
    bool CanCraft);

public sealed class CraftingRuntime
{
    private static readonly CraftingRecipeDefinition[] Definitions =
    [
        new(
            new AlchemyRecipe(
                "recipe.marsh-sight-tonic",
                "marsh-sight-tonic",
                1,
                [
                    new ItemCost("marsh-herb", 1),
                    new ItemCost("forest-resin", 1)
                ]),
            "NAPAR TROPICIELA",
            "herbalist",
            "trade-and-prepare",
            "recipe.marsh-sight-tonic.learned")
    ];

    public bool IsOpen { get; private set; }
    public int SelectedIndex { get; private set; }
    public string Message { get; private set; } = "";

    public static IReadOnlyList<CraftingRecipeDefinition> Catalog =>
        Definitions;

    public bool CanOpenNearest(WorldState world) =>
        FindStation(world) is not null;

    public bool TryOpenNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (FindStation(world) is null)
        {
            Message = "BRAK DOSTEPNEGO STANOWISKA ALCHEMICZNEGO";
            return false;
        }

        IsOpen = true;
        SelectedIndex = 0;
        Message = "";
        return true;
    }

    public void Close()
    {
        IsOpen = false;
        SelectedIndex = 0;
        Message = "";
    }

    public void MoveSelection(WorldState world, int direction)
    {
        if (!IsOpen || direction == 0)
            return;

        var lines = BuildLines(world);
        if (lines.Count == 0)
        {
            SelectedIndex = 0;
            return;
        }

        SelectedIndex =
            (SelectedIndex + Math.Sign(direction) + lines.Count) %
            lines.Count;
    }

    public CraftingResult Confirm(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!IsOpen || FindStation(world) is null)
        {
            Message = "STANOWISKO NIE JEST JUZ DOSTEPNE";
            return CraftingResult.Unavailable;
        }

        if (Definitions.Length == 0)
            return CraftingResult.None;

        SelectedIndex = Math.Clamp(
            SelectedIndex,
            0,
            Definitions.Length - 1);
        var definition = Definitions[SelectedIndex];

        if (!IsUnlocked(world, definition))
        {
            Message = "NIE ZNASZ TEJ RECEPTURY";
            return CraftingResult.Locked;
        }

        var missing = definition.Recipe.Ingredients
            .Where(cost =>
                world.Progress.Inventory.Count(cost.ItemId) <
                cost.Quantity)
            .ToArray();

        if (missing.Length > 0)
        {
            Message =
                "BRAKUJE: " +
                string.Join(
                    ", ",
                    missing.Select(cost =>
                        $"{ItemName(cost.ItemId)} " +
                        $"{world.Progress.Inventory.Count(cost.ItemId)}/{cost.Quantity}"));
            return CraftingResult.MissingIngredients;
        }

        // Validate the full recipe first, then consume. This keeps crafting
        // atomic even if later recipes have multiple ingredients.
        foreach (var cost in definition.Recipe.Ingredients)
        {
            if (!world.Progress.Inventory.Remove(
                    cost.ItemId,
                    cost.Quantity))
            {
                throw new InvalidOperationException(
                    $"Validated ingredient '{cost.ItemId}' disappeared during crafting.");
            }
        }

        world.Progress.Inventory.Add(
            definition.Recipe.ResultItemId,
            definition.Recipe.ResultQuantity);

        Message =
            $"WYTWORZONO {definition.DisplayName} x{definition.Recipe.ResultQuantity}";
        return CraftingResult.Completed;
    }

    public IReadOnlyList<CraftingViewLine> BuildLines(
        WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        return Definitions
            .Select(definition =>
            {
                var unlocked = IsUnlocked(world, definition);
                var requirements = string.Join(
                    " + ",
                    definition.Recipe.Ingredients.Select(cost =>
                        $"{ItemName(cost.ItemId)} " +
                        $"{world.Progress.Inventory.Count(cost.ItemId)}/{cost.Quantity}"));
                var canCraft =
                    unlocked &&
                    definition.Recipe.Ingredients.All(cost =>
                        world.Progress.Inventory.Count(cost.ItemId) >=
                        cost.Quantity);

                return new CraftingViewLine(
                    definition.Recipe.Id,
                    definition.DisplayName,
                    requirements,
                    unlocked,
                    canCraft);
            })
            .ToArray();
    }

    private static CraftingRecipeDefinition? FindStation(
        WorldState world)
    {
        foreach (var definition in Definitions)
        {
            var actor = world.NpcWorld.Find(
                definition.StationNpcId);
            if (actor is null ||
                !string.Equals(
                    actor.Activity,
                    definition.RequiredActivity,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var delta =
                actor.Position - world.PlayerPosition;
            delta.Y = 0f;
            if (delta.LengthSquared() <= 4.25f * 4.25f)
                return definition;
        }

        return null;
    }

    private static bool IsUnlocked(
        WorldState world,
        CraftingRecipeDefinition definition) =>
        string.IsNullOrWhiteSpace(definition.RequiredFlag) ||
        world.Progress.HasFlag(definition.RequiredFlag);

    private static string ItemName(string id) => id switch
    {
        "marsh-herb" => "ZIELE MOKRADEL",
        "forest-resin" => "ZYWICA LESNA",
        "marsh-sight-tonic" => "NAPAR TROPICIELA",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };
}
