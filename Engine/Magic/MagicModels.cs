namespace SlavicGame.Engine.Magic;

public enum MagicSource
{
    Divine,
    Nature,
    SoulsAndNawia,
    AncientPower,
    FourthSphere
}

public enum MagicActionKind
{
    Spell,
    Ritual,
    Alchemy
}

public sealed record ItemCost(string ItemId, int Quantity);

public sealed record MagicCost(
    float Health = 0f,
    float Stamina = 0f,
    ItemCost[]? Items = null,
    int DivineFavor = 0);

public sealed record SpellDefinition(
    string Id,
    string Name,
    MagicSource Source,
    MagicCost Cost);

public sealed record RitualDefinition(
    string Id,
    string Name,
    MagicSource Source,
    string RequiredLocationId,
    double ActiveFromHour,
    double ActiveUntilHour,
    ItemCost[] RequiredItems,
    string[] RequiredEvidenceIds,
    MagicCost Cost);

public sealed record AlchemyRecipe(
    string Id,
    string ResultItemId,
    int ResultQuantity,
    ItemCost[] Ingredients);

public static class RitualRules
{
    public static bool IsActiveAt(RitualDefinition ritual, double hour)
    {
        ArgumentNullException.ThrowIfNull(ritual);
        if (!double.IsFinite(hour)) return false;
        hour = ((hour % 24.0) + 24.0) % 24.0;
        var start = ((ritual.ActiveFromHour % 24.0) + 24.0) % 24.0;
        var end = ((ritual.ActiveUntilHour % 24.0) + 24.0) % 24.0;
        return start <= end
            ? hour >= start && hour <= end
            : hour >= start || hour <= end;
    }
}
