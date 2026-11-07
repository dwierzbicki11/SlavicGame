using SlavicGame.Engine.Magic;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public sealed record BlacksmithRecipe(
    string Id,
    string ResultItemId,
    int ResultQuantity,
    string DisplayName,
    string Description,
    IReadOnlyList<ItemCost> Ingredients);

public sealed record BlacksmithViewLine(
    string RecipeId,
    string DisplayName,
    string Description,
    string RequirementsText,
    bool CanCraft);

public sealed class BlacksmithCraftingRuntime
{
    private static readonly BlacksmithRecipe[] Recipes =
    [
        new(
            "blacksmith.druzhina-sword",
            "miecz-druzynnika",
            1,
            "MIECZ DRUZYNNIKA",
            "+10 OBRAZEN WRACZ",
            [
                new ItemCost("iron-chunk", 2),
                new ItemCost("forest-resin", 1),
                new ItemCost("leather-strip", 1)
            ]),

        new(
            "blacksmith.war-axe",
            "topor-bojowy",
            1,
            "TOPOR BOJOWY PERUNA",
            "+18 OBRAZEN WRACZ",
            [
                new ItemCost("iron-chunk", 3),
                new ItemCost("forest-resin", 2)
            ]),

        new(
            "blacksmith.slavic-shield",
            "tarcza-slowianska",
            1,
            "TARCZA SLOWIANSKA Z UMBO",
            "+30% OCHRONY PRZED CIOSAMI",
            [
                new ItemCost("plank-timber", 2),
                new ItemCost("iron-chunk", 1)
            ]),

        new(
            "blacksmith.chainmail",
            "kolczuga-slowianska",
            1,
            "KOLCZUGA WOJA",
            "+25% PANCERZA I REDUKCJI RAN",
            [
                new ItemCost("iron-chunk", 4),
                new ItemCost("leather-strip", 2)
            ]),

        new(
            "blacksmith.iron-arrows",
            "arrow-iron",
            16,
            "STRZALY ZADZIORKOWE x16",
            "PRECYZYJNY GROT ZE STALI",
            [
                new ItemCost("iron-chunk", 1),
                new ItemCost("forest-resin", 1)
            ])
    ];

    public static IReadOnlyList<BlacksmithRecipe> Catalog => Recipes;

    public bool IsOpen { get; private set; }
    public int SelectedIndex { get; private set; }
    public string Message { get; private set; } = "";

    public bool CanOpenNearest(WorldState world) =>
        FindForge(world) is not null;

    public bool TryOpenNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (FindForge(world) is null)
        {
            Message = "BRAK DOSTEPNEJ KUZNI";
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

    public void MoveSelection(int direction)
    {
        if (!IsOpen || direction == 0) return;
        SelectedIndex = (SelectedIndex + Math.Sign(direction) + Recipes.Length) % Recipes.Length;
    }

    public bool Confirm(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!IsOpen || FindForge(world) is null)
        {
            Message = "KUZNIA NIE JEST JUZ DOSTEPNA";
            return false;
        }

        SelectedIndex = Math.Clamp(SelectedIndex, 0, Recipes.Length - 1);
        var recipe = Recipes[SelectedIndex];

        var missing = recipe.Ingredients
            .Where(cost => world.Progress.Inventory.Count(cost.ItemId) < cost.Quantity)
            .ToArray();

        if (missing.Length > 0)
        {
            Message = "BRAKUJE: " + string.Join(", ", missing.Select(c => $"{ItemName(c.ItemId)} {world.Progress.Inventory.Count(c.ItemId)}/{c.Quantity}"));
            return false;
        }

        foreach (var cost in recipe.Ingredients)
        {
            world.Progress.Inventory.Remove(cost.ItemId, cost.Quantity);
        }

        world.Progress.Inventory.Add(recipe.ResultItemId, recipe.ResultQuantity);
        Message = $"WYKUTO: {recipe.DisplayName} x{recipe.ResultQuantity}";
        return true;
    }

    public IReadOnlyList<BlacksmithViewLine> BuildLines(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        return Recipes.Select(recipe =>
        {
            var reqs = string.Join(" + ", recipe.Ingredients.Select(cost =>
                $"{ItemName(cost.ItemId)} {world.Progress.Inventory.Count(cost.ItemId)}/{cost.Quantity}"));
            var canCraft = recipe.Ingredients.All(cost =>
                world.Progress.Inventory.Count(cost.ItemId) >= cost.Quantity);

            return new BlacksmithViewLine(
                recipe.Id,
                recipe.DisplayName,
                recipe.Description,
                reqs,
                canCraft);
        }).ToArray();
    }

    public static string? FindForge(WorldState world)
    {
        var smithIds = new[] { "stronghold-smith", "settler-smith-helper-01" };
        foreach (var id in smithIds)
        {
            var actor = world.NpcWorld.Find(id);
            if (actor is null || !string.Equals(actor.Activity, "forge-work", StringComparison.Ordinal))
                continue;

            var delta = actor.Position - world.PlayerPosition;
            delta.Y = 0f;
            if (delta.LengthSquared() <= 4.5f * 4.5f)
                return id;
        }

        return null;
    }

    private static string ItemName(string id) => id switch
    {
        "iron-chunk" => "BRYLA ZELAZA",
        "leather-strip" => "PAS RZEMIENIA",
        "plank-timber" => "DESKA DEBOWA",
        "forest-resin" => "ZYWICA LESNA",
        "miecz-druzynnika" => "MIECZ DRUZYNNIKA",
        "topor-bojowy" => "TOPOR BOJOWY PERUNA",
        "tarcza-slowianska" => "TARCZA SLOWIANSKA",
        "kolczuga-slowianska" => "KOLCZUGA WOJA",
        "arrow-iron" => "STRZALY ZADZIORKOWE",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };
}
