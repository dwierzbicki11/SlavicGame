using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public static class NpcSituationalBehavior
{
    public const string StormShelterActivity = "shelter-storm";

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

        return severeWeather
            ? StormShelterActivity
            : slot.Activity;
    }
}
