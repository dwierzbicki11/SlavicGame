using System.Numerics;

namespace SlavicGame.Engine.World;

public readonly record struct NpcRoutineSample(
    Vector2 Position,
    Vector2 Forward,
    bool IsMoving);

public static class NpcRoutineMotion
{
    public static NpcRoutineSample Sample(
        string npcId,
        string activity,
        Vector2 fallback,
        double timeOfDayHours)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId);
        ArgumentException.ThrowIfNullOrWhiteSpace(activity);

        var route = RouteFor(npcId, activity);
        if (route is null || route.Points.Length < 2)
        {
            return new NpcRoutineSample(
                fallback,
                Vector2.Zero,
                false);
        }

        var phase =
            PositiveModulo(
                timeOfDayHours / route.CycleHours +
                StablePhase(npcId),
                1.0);

        var sample = SampleClosedRoute(
            route.Points,
            (float)phase);

        return new NpcRoutineSample(
            sample.Position,
            sample.Forward,
            true);
    }

    private static Route? RouteFor(
        string npcId,
        string activity) =>
        (npcId, activity) switch
        {
            ("missing-family", "home-and-search") => new(
                0.34,
                [
                    new Vector2(-6f, -84f),
                    new Vector2(-11f, -80f),
                    new Vector2(-15f, -85f),
                    new Vector2(-10f, -89f)
                ]),

            ("crossing-keeper", "maintain-crossing") => new(
                0.28,
                [
                    new Vector2(91f, 41f),
                    new Vector2(95f, 43f),
                    new Vector2(99f, 40f),
                    new Vector2(96f, 36f),
                    new Vector2(92f, 37f)
                ]),

            ("herbalist", "trade-and-prepare") => new(
                0.36,
                [
                    new Vector2(5f, -92f),
                    new Vector2(2f, -88f),
                    new Vector2(7f, -86f),
                    new Vector2(10f, -91f)
                ]),

            ("community-guard", "patrol") => new(
                0.62,
                [
                    new Vector2(1f, -106f),
                    new Vector2(20f, -104f),
                    new Vector2(27f, -88f),
                    new Vector2(19f, -72f),
                    new Vector2(2f, -68f),
                    new Vector2(-19f, -72f),
                    new Vector2(-27f, -88f),
                    new Vector2(-17f, -103f)
                ]),

            ("community-guard", "night-watch") => new(
                0.48,
                [
                    new Vector2(-2f, -105f),
                    new Vector2(9f, -105f),
                    new Vector2(14f, -94f),
                    new Vector2(2f, -88f),
                    new Vector2(-10f, -96f)
                ]),

            ("shrine-keeper", "tend-shrine") => new(
                0.42,
                [
                    new Vector2(-82f, 58f),
                    new Vector2(-80f, 53f),
                    new Vector2(-85f, 50f),
                    new Vector2(-90f, 53f),
                    new Vector2(-89f, 59f)
                ]),

            ("settler-farmer-01", "go-to-fields") => new(
                0.30,
                [
                    new Vector2(-20f, -92f),
                    new Vector2(-22f, -83f),
                    new Vector2(-25f, -75f),
                    new Vector2(-25f, -70f)
                ]),

            ("settler-farmer-01", "field-work") => new(
                0.38,
                [
                    new Vector2(-25f, -70f),
                    new Vector2(-31f, -66f),
                    new Vector2(-34f, -72f),
                    new Vector2(-29f, -77f)
                ]),

            ("settler-farmer-02", "field-work") => new(
                0.41,
                [
                    new Vector2(-30f, -76f),
                    new Vector2(-36f, -72f),
                    new Vector2(-38f, -65f),
                    new Vector2(-32f, -61f),
                    new Vector2(-27f, -68f)
                ]),

            ("settler-woodworker-01", "wood-work") => new(
                0.26,
                [
                    new Vector2(15f, -97f),
                    new Vector2(18f, -94f),
                    new Vector2(22f, -96f),
                    new Vector2(19f, -101f)
                ]),

            ("settler-potter-01", "craft-work") => new(
                0.31,
                [
                    new Vector2(-7f, -74f),
                    new Vector2(-4f, -72f),
                    new Vector2(-2f, -76f),
                    new Vector2(-6f, -78f)
                ]),

            ("settler-trader-01", "market-trade") => new(
                0.32,
                [
                    new Vector2(3f, -75f),
                    new Vector2(-1f, -73f),
                    new Vector2(-5f, -76f),
                    new Vector2(0f, -79f),
                    new Vector2(5f, -78f)
                ]),

            ("settler-carrier-01", "carry-goods") => new(
                0.44,
                [
                    new Vector2(15f, -95f),
                    new Vector2(11f, -86f),
                    new Vector2(4f, -79f),
                    new Vector2(-5f, -76f),
                    new Vector2(-12f, -82f),
                    new Vector2(-4f, -90f),
                    new Vector2(7f, -94f)
                ]),

            ("settler-elder-01", "village-square") => new(
                0.52,
                [
                    new Vector2(-3f, -84f),
                    new Vector2(2f, -82f),
                    new Vector2(4f, -87f),
                    new Vector2(-1f, -90f),
                    new Vector2(-6f, -87f)
                ]),

            ("settler-traveler-01", "arrive-and-trade") => new(
                0.36,
                [
                    new Vector2(2f, -109f),
                    new Vector2(2f, -101f),
                    new Vector2(3f, -92f),
                    new Vector2(3f, -83f),
                    new Vector2(3f, -75f)
                ]),

            ("settler-smith-helper-01", "forge-work") => new(
                0.27,
                [
                    new Vector2(22f, -91f),
                    new Vector2(19f, -88f),
                    new Vector2(24f, -89f),
                    new Vector2(17f, -96f)
                ]),

            ("settler-weaver-01", "weave-work") => new(
                0.34,
                [
                    new Vector2(-17f, -78f),
                    new Vector2(-13f, -80f),
                    new Vector2(-18f, -84f),
                    new Vector2(-22f, -81f)
                ]),

            ("settler-shepherd-01", "drive-flock") => new(
                0.42,
                [
                    new Vector2(24f, -80f),
                    new Vector2(25f, -74f),
                    new Vector2(27f, -68f),
                    new Vector2(30f, -64f)
                ]),

            ("settler-shepherd-01", "graze-flock") => new(
                0.58,
                [
                    new Vector2(30f, -64f),
                    new Vector2(36f, -61f),
                    new Vector2(39f, -68f),
                    new Vector2(33f, -72f),
                    new Vector2(27f, -68f)
                ]),

            ("settler-gatherer-01", "gather-herbs") => new(
                0.74,
                [
                    new Vector2(10f, -98f),
                    new Vector2(2f, -89f),
                    new Vector2(-9f, -76f),
                    new Vector2(-21f, -65f),
                    new Vector2(-34f, -56f),
                    new Vector2(-27f, -48f),
                    new Vector2(-16f, -56f)
                ]),

            ("settler-gatherer-01", "sort-herbs") => new(
                0.30,
                [
                    new Vector2(7f, -90f),
                    new Vector2(4f, -88f),
                    new Vector2(8f, -85f),
                    new Vector2(11f, -90f)
                ]),

            ("settler-fisher-01", "river-fishing") => new(
                0.52,
                [
                    new Vector2(103f, 29f),
                    new Vector2(108f, 26f),
                    new Vector2(113f, 27f),
                    new Vector2(117f, 31f),
                    new Vector2(112f, 34f),
                    new Vector2(106f, 33f)
                ]),

            ("settler-fisher-01", "mend-nets") => new(
                0.33,
                [
                    new Vector2(20f, -75f),
                    new Vector2(24f, -77f),
                    new Vector2(23f, -82f),
                    new Vector2(18f, -80f)
                ]),

            ("settler-youth-01", "run-errands") => new(
                0.39,
                [
                    new Vector2(-1f, -84f),
                    new Vector2(20f, -90f),
                    new Vector2(4f, -75f),
                    new Vector2(-19f, -81f),
                    new Vector2(-4f, -101f),
                    new Vector2(15f, -95f)
                ]),

            ("settler-farmer-01", "shelter-storm") => ShelterRoute(
                new Vector2(-20f, -92f),
                new Vector2(-25f, -90f)),
            ("settler-farmer-02", "shelter-storm") => ShelterRoute(
                new Vector2(-24f, -98f),
                new Vector2(-27f, -94f)),
            ("settler-woodworker-01", "shelter-storm") => ShelterRoute(
                new Vector2(20f, -101f),
                new Vector2(22f, -96f)),
            ("settler-potter-01", "shelter-storm") => ShelterRoute(
                new Vector2(-13f, -90f),
                new Vector2(-8f, -88f)),
            ("settler-trader-01", "shelter-storm") => ShelterRoute(
                new Vector2(8f, -88f),
                new Vector2(4f, -84f)),
            ("settler-carrier-01", "shelter-storm") => ShelterRoute(
                new Vector2(15f, -95f),
                new Vector2(11f, -91f)),
            ("settler-elder-01", "shelter-storm") => ShelterRoute(
                new Vector2(-12f, -86f),
                new Vector2(-7f, -86f)),
            ("settler-traveler-01", "shelter-storm") => ShelterRoute(
                new Vector2(18f, -76f),
                new Vector2(12f, -80f)),
            ("settler-smith-helper-01", "shelter-storm") => ShelterRoute(
                new Vector2(17f, -99f),
                new Vector2(21f, -94f)),
            ("settler-weaver-01", "shelter-storm") => ShelterRoute(
                new Vector2(-21f, -94f),
                new Vector2(-17f, -89f)),
            ("settler-shepherd-01", "shelter-storm") => ShelterRoute(
                new Vector2(24f, -80f),
                new Vector2(26f, -84f)),
            ("settler-gatherer-01", "shelter-storm") => ShelterRoute(
                new Vector2(10f, -98f),
                new Vector2(8f, -94f)),
            ("settler-fisher-01", "shelter-storm") => ShelterRoute(
                new Vector2(24f, -82f),
                new Vector2(21f, -84f)),
            ("settler-fisher-01", "shelter-storm-swamp") => ShelterRoute(
                new Vector2(111f, 27f),
                new Vector2(99f, 39f)),
            ("settler-youth-01", "shelter-storm") => ShelterRoute(
                new Vector2(6f, -96f),
                new Vector2(4f, -92f)),

            _ => null
        };

    private static Route ShelterRoute(
        Vector2 home,
        Vector2 shelter)
    {
        var side = new Vector2(
            -(shelter.Y - home.Y),
            shelter.X - home.X);

        if (side.LengthSquared() > 0.000001f)
            side = Vector2.Normalize(side) * 0.9f;

        return new Route(
            0.22,
            [
                home,
                Vector2.Lerp(home, shelter, 0.55f),
                shelter - side,
                shelter + side
            ]);
    }

    private static (
        Vector2 Position,
        Vector2 Forward)
        SampleClosedRoute(
            IReadOnlyList<Vector2> points,
            float phase)
    {
        var lengths = new float[points.Count];
        var totalLength = 0f;

        for (var i = 0; i < points.Count; i++)
        {
            var next = (i + 1) % points.Count;
            var length =
                Vector2.Distance(points[i], points[next]);

            lengths[i] = MathF.Max(length, 0.0001f);
            totalLength += lengths[i];
        }

        var targetDistance =
            Math.Clamp(phase, 0f, 0.999999f) *
            totalLength;

        var travelled = 0f;
        for (var i = 0; i < points.Count; i++)
        {
            var segmentLength = lengths[i];
            if (targetDistance >
                travelled + segmentLength)
            {
                travelled += segmentLength;
                continue;
            }

            var next = (i + 1) % points.Count;
            var t =
                (targetDistance - travelled) /
                segmentLength;

            var from = points[i];
            var to = points[next];
            var direction = to - from;
            var forward =
                direction.LengthSquared() > 0.000001f
                    ? Vector2.Normalize(direction)
                    : Vector2.UnitY;

            return (
                Vector2.Lerp(from, to, t),
                forward);
        }

        return (
            points[0],
            Vector2.Normalize(points[1] - points[0]));
    }

    private static double StablePhase(string id)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in id)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (hash % 1000u) / 1000.0;
        }
    }

    private static double PositiveModulo(
        double value,
        double modulus)
    {
        var result = value % modulus;
        return result < 0
            ? result + modulus
            : result;
    }

    private sealed record Route(
        double CycleHours,
        Vector2[] Points);
}
