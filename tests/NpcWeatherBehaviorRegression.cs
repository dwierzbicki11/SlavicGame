using SlavicGame.Engine.World;

internal static class NpcWeatherBehaviorRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var normalAmbient = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(normalAmbient.Length == 8 &&
              normalAmbient.All(actor =>
                  actor.Activity != NpcSituationalBehavior.StormShelterActivity),
            "Ambient settlers keep their normal daytime routines in clear weather");

        world.Weather.SetCondition(WeatherKind.Rain, immediate: true);
        world.NpcWorld.Update(world);

        check(world.NpcWorld.Actors
                .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
                .All(actor =>
                    actor.Activity != NpcSituationalBehavior.StormShelterActivity),
            "Ordinary rain does not evacuate the whole village into storm shelters");

        world.Weather.SetCondition(WeatherKind.Storm, immediate: true);
        world.NpcWorld.Update(world);

        var sheltered = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(sheltered.Length == 8 &&
              sheltered.All(actor =>
                  actor.Activity == NpcSituationalBehavior.StormShelterActivity),
            "All eight ambient settlers switch to storm shelter routines in severe weather");

        check(sheltered
                .Select(actor => (actor.Position.X, actor.Position.Z))
                .Distinct()
                .Count() >= 6,
            "Storm shelter routines keep residents distributed instead of stacking at one point");

        var guard = world.NpcWorld.Find("community-guard");
        var shrineKeeper = world.NpcWorld.Find("shrine-keeper");
        check(guard is not null &&
              guard.Activity == "patrol" &&
              shrineKeeper is not null &&
              shrineKeeper.Activity == "tend-shrine",
            "Core quest NPC schedules are not silently overridden by ambient storm behavior");

        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var resumed = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(resumed.All(actor =>
                  actor.Activity != NpcSituationalBehavior.StormShelterActivity) &&
              resumed.Any(actor => actor.Activity == "field-work") &&
              resumed.Any(actor => actor.Activity == "market-trade"),
            "Settlers resume authored work routines after the storm ends");
    }
}
