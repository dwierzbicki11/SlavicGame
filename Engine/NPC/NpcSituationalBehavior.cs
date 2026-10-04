using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public static class NpcSituationalBehavior
{
    public const string StormShelterActivity = "shelter-storm";
    public const string StormShelterSwampActivity = "shelter-storm-swamp";

    public static bool IsStormShelterActivity(string activity) =>
        string.Equals(activity, StormShelterActivity, StringComparison.Ordinal) ||
        string.Equals(activity, StormShelterSwampActivity, StringComparison.Ordinal);

    public static string ResolveActivity(
        WorldState world,
        NpcDefinition npc,
        NpcScheduleSlot slot)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(slot);

        if (!npc.Id.StartsWith("settler-", StringComparison.Ordinal))
            return slot.Activity;

        var severeWeather =
            world.Weather.Condition == WeatherKind.Storm ||
            world.Weather.RainIntensity >= 0.85f;

        if (!severeWeather)
            return slot.Activity;

        return string.Equals(npc.Id, "settler-fisher-01", StringComparison.Ordinal) &&
               string.Equals(slot.LocationId, "black-swamp", StringComparison.Ordinal)
            ? StormShelterSwampActivity
            : StormShelterActivity;
    }
}
