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

        check(normalAmbient.Length == 14 &&
              normalAmbient.All(actor =>
                  !NpcSituationalBehavior.IsStormShelterActivity(actor.Activity)),
            "Ambient settlers keep their normal daytime routines in clear weather");

        world.Weather.SetCondition(WeatherKind.Rain, immediate: true);
        world.NpcWorld.Update(world);

        check(world.NpcWorld.Actors
                .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
                .All(actor =>
                    !NpcSituationalBehavior.IsStormShelterActivity(actor.Activity)),
            "Ordinary rain does not evacuate the whole village into storm shelters");

        world.Weather.SetCondition(WeatherKind.Storm, immediate: true);
        world.NpcWorld.Update(world);

        var sheltered = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(sheltered.Length == 14 &&
              sheltered.All(actor =>
                  NpcSituationalBehavior.IsStormShelterActivity(actor.Activity)),
            "All fourteen ambient settlers switch to storm shelter routines in severe weather");

        check(sheltered
                .Select(actor => (actor.Position.X, actor.Position.Z))
                .Distinct()
                .Count() >= 10,
            "Storm shelter routines keep residents distributed instead of stacking at one point");

        var guard = world.NpcWorld.Find("community-guard");
        var shrineKeeper = world.NpcWorld.Find("shrine-keeper");
        check(guard is not null &&
              guard.Activity == "patrol" &&
              shrineKeeper is not null &&
              shrineKeeper.Activity == "tend-shrine",
            "Core quest NPC schedules are not silently overridden by ambient storm behavior");

        foreach (var hour in new[] { 23.0, 0.0, 4.0 })
        {
            world.Time.SetTimeOfDay(hour);
            world.NpcWorld.Update(world);
            var resting = world.NpcWorld.Actors
                .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
                .ToArray();

            check(resting.Length == 14 &&
                  resting.All(actor => actor.Activity == "rest" && !actor.IsMoving),
                $"Storm preserves all settlers' scheduled rest at {hour}:00");
        }

        world.Time.SetTimeOfDay(5);
        world.NpcWorld.Update(world);
        check(world.NpcWorld.Find("settler-fisher-01")?.Activity ==
                  NpcSituationalBehavior.StormShelterSwampActivity &&
              world.NpcWorld.Find("settler-farmer-01")?.Activity == "rest",
            "At dawn only the working fisher seeks storm shelter while the farmer keeps resting");

        world.Time.SetTimeOfDay(10);

        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var resumed = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(resumed.All(actor =>
                  !NpcSituationalBehavior.IsStormShelterActivity(actor.Activity)) &&
              resumed.Any(actor => actor.Activity == "field-work") &&
              resumed.Any(actor => actor.Activity == "market-trade"),
            "Settlers resume authored work routines after the storm ends");

        foreach (var hour in new[] { 23.0, 2.0 })
        {
            world.Time.SetTimeOfDay(hour);
            world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
            world.NpcWorld.Update(world);
            var resting = world.NpcWorld.Actors
                .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
                .ToArray();

            world.Weather.SetCondition(WeatherKind.Storm, immediate: true);
            world.NpcWorld.Update(world);

            check(resting.Length == 14 && resting.All(before =>
                    before.Activity == "rest" &&
                    world.NpcWorld.Find(before.Id) is { } after &&
                    after.Activity == "rest" &&
                    !after.IsMoving &&
                    after.Position == before.Position),
                $"Storm preserves all settlers' home rest at {hour}:00");
        }
    }
}
