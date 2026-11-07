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

        var workstation = NpcWorkstationCatalog.Find(npcId, activity);
        float phase;
        if (workstation is not null)
        {
            var wsPhase = NpcWorkstationCatalog.SamplePhase(
                workstation,
                timeOfDayHours);
            phase = wsPhase < workstation.WorkFraction
                ? 0f
                : (float)((wsPhase - workstation.WorkFraction) /
                          Math.Max(1.0 - workstation.WorkFraction, 0.0001));
        }
        else
        {
            phase = (float)PositiveModulo(
                timeOfDayHours / route.CycleHours +
                StablePhase(npcId),
                1.0);
        }

        var sample = SampleClosedRoute(
            route.Points,
            phase);

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
                0.65,
                [
                    new Vector2(-5.5f, -83.5f),
                    new Vector2(-10f, -82.5f),
                    new Vector2(-15.2f, -83.5f),
                    new Vector2(-15.2f, -88.0f)
                ]),

            ("crossing-keeper", "maintain-crossing") => new(
                0.45,
                [
                    new Vector2(94.2f, 40.8f),
                    new Vector2(95.4f, 41.4f),
                    new Vector2(95.6f, 40.1f),
                    new Vector2(94.4f, 39.8f)
                ]),

            ("herbalist", "trade-and-prepare") => new(
                0.45,
                [
                    new Vector2(5.0f, -92.0f),
                    new Vector2(4.2f, -90.8f),
                    new Vector2(5.4f, -90.2f),
                    new Vector2(6.2f, -91.6f)
                ]),

            ("community-guard", "patrol") => new(
                1.65,
                [
                    new Vector2(0.5f, -105f),
                    new Vector2(18f, -104f),
                    new Vector2(26f, -87f),
                    new Vector2(19f, -71f),
                    new Vector2(2f, -68f),
                    new Vector2(-19f, -71f),
                    new Vector2(-22f, -86f),
                    new Vector2(-16f, -103f)
                ]),

            ("community-guard", "night-watch") => new(
                1.10,
                [
                    new Vector2(-1.5f, -105f),
                    new Vector2(9f, -104f),
                    new Vector2(13f, -92f),
                    new Vector2(2f, -86f),
                    new Vector2(-8f, -95f)
                ]),

            ("shrine-keeper", "tend-shrine") => new(
                0.75,
                [
                    new Vector2(-82f, 58f),
                    new Vector2(-81.5f, 52.5f),
                    new Vector2(-85f, 51.8f),
                    new Vector2(-88.5f, 52.5f),
                    new Vector2(-88.5f, 58f)
                ]),

            ("settler-farmer-01", "go-to-fields") => new(
                0.85,
                [
                    new Vector2(-20f, -92f),
                    new Vector2(-22f, -83f),
                    new Vector2(-25f, -75f),
                    new Vector2(-29.0f, -69.4f)
                ]),

            ("settler-farmer-01", "field-work") => new(
                0.45,
                [
                    new Vector2(-29.0f, -69.4f),
                    new Vector2(-27.8f, -68.6f),
                    new Vector2(-27.6f, -70.1f),
                    new Vector2(-28.8f, -70.6f)
                ]),

            ("settler-farmer-02", "field-work") => new(
                0.45,
                [
                    new Vector2(-34.1f, -67.2f),
                    new Vector2(-32.9f, -66.4f),
                    new Vector2(-32.7f, -67.8f),
                    new Vector2(-33.8f, -68.4f)
                ]),

            ("settler-woodworker-01", "wood-work") => new(
                0.40,
                [
                    new Vector2(15.2f, -97.2f),
                    new Vector2(16.3f, -96.8f),
                    new Vector2(16.2f, -98.0f),
                    new Vector2(14.9f, -98.1f)
                ]),

            ("settler-potter-01", "craft-work") => new(
                0.42,
                [
                    new Vector2(-6.7f, -74.8f),
                    new Vector2(-5.6f, -74.5f),
                    new Vector2(-5.7f, -75.7f),
                    new Vector2(-6.9f, -75.8f)
                ]),

            ("settler-trader-01", "market-trade") => new(
                0.45,
                [
                    new Vector2(2.7f, -75.2f),
                    new Vector2(2.2f, -74.0f),
                    new Vector2(3.4f, -74.2f),
                    new Vector2(3.5f, -75.6f)
                ]),

            ("settler-carrier-01", "carry-goods") => new(
                1.20,
                [
                    new Vector2(14f, -94f),
                    new Vector2(10f, -86f),
                    new Vector2(4f, -79f),
                    new Vector2(-5f, -79f),
                    new Vector2(-13f, -82f),
                    new Vector2(-3f, -91f),
                    new Vector2(8f, -91f)
                ]),

            ("settler-elder-01", "village-square") => new(
                0.85,
                [
                    new Vector2(-3f, -84f),
                    new Vector2(2f, -82f),
                    new Vector2(4f, -87f),
                    new Vector2(-1f, -90f),
                    new Vector2(-5f, -86f)
                ]),

            ("settler-traveler-01", "arrive-and-trade") => new(
                1.25,
                [
                    new Vector2(0.8f, -109f),
                    new Vector2(1.2f, -101f),
                    new Vector2(2.5f, -92f),
                    new Vector2(3.0f, -83f),
                    new Vector2(3.2f, -75f)
                ]),

            ("settler-smith-helper-01", "forge-work") => new(
                0.40,
                [
                    new Vector2(19.6f, -87.2f),
                    new Vector2(18.4f, -87.0f),
                    new Vector2(18.3f, -88.1f),
                    new Vector2(19.5f, -88.2f)
                ]),

            ("settler-weaver-01", "weave-work") => new(
                0.42,
                [
                    new Vector2(-16.4f, -76.5f),
                    new Vector2(-15.3f, -76.2f),
                    new Vector2(-15.3f, -77.4f),
                    new Vector2(-16.3f, -77.5f)
                ]),

            ("settler-shepherd-01", "drive-flock") => new(
                0.90,
                [
                    new Vector2(21.5f, -80f),
                    new Vector2(21.5f, -74f),
                    new Vector2(25f, -68f),
                    new Vector2(29f, -64f)
                ]),

            ("settler-shepherd-01", "graze-flock") => new(
                1.10,
                [
                    new Vector2(29f, -64f),
                    new Vector2(36f, -56f),
                    new Vector2(39f, -62f),
                    new Vector2(35f, -68f),
                    new Vector2(27f, -68f)
                ]),

            ("settler-gatherer-01", "gather-herbs") => new(
                1.80,
                [
                    new Vector2(11f, -94f),
                    new Vector2(2f, -89f),
                    new Vector2(-8f, -76f),
                    new Vector2(-21f, -65f),
                    new Vector2(-34f, -56f),
                    new Vector2(-27f, -48f),
                    new Vector2(-16f, -56f)
                ]),

            ("settler-gatherer-01", "sort-herbs") => new(
                0.42,
                [
                    new Vector2(7.0f, -90.0f),
                    new Vector2(6.0f, -89.8f),
                    new Vector2(6.2f, -90.9f),
                    new Vector2(7.3f, -90.8f)
                ]),

            ("settler-fisher-01", "river-fishing") => new(
                0.95,
                [
                    new Vector2(103f, 29f),
                    new Vector2(108f, 26f),
                    new Vector2(113f, 27f),
                    new Vector2(117f, 31f),
                    new Vector2(112f, 34f),
                    new Vector2(106f, 33f)
                ]),

            ("settler-fisher-01", "mend-nets") => new(
                0.42,
                [
                    new Vector2(20.0f, -75.3f),
                    new Vector2(19.0f, -75.6f),
                    new Vector2(19.2f, -76.6f),
                    new Vector2(20.3f, -76.3f)
                ]),

            ("settler-youth-01", "run-errands") => new(
                1.55,
                [
                    new Vector2(-1f, -84f),
                    new Vector2(8f, -85f),
                    new Vector2(17f, -86f),
                    new Vector2(6f, -73f),
                    new Vector2(-8f, -73f),
                    new Vector2(-15f, -83f),
                    new Vector2(-2f, -94f),
                    new Vector2(12f, -91f)
                ]),

            ("settler-farmer-01", "shelter-storm") => ShelterRoute(
                new Vector2(-20f, -93.5f),
                new Vector2(-22f, -90.5f)),
            ("settler-farmer-02", "shelter-storm") => ShelterRoute(
                new Vector2(-19.2f, -97.5f),
                new Vector2(-19.5f, -94.0f)),
            ("settler-woodworker-01", "shelter-storm") => ShelterRoute(
                new Vector2(17.5f, -95.0f),
                new Vector2(16.0f, -92.5f)),
            ("settler-potter-01", "shelter-storm") => ShelterRoute(
                new Vector2(-5.2f, -89.5f),
                new Vector2(-4.5f, -86.5f)),
            ("settler-trader-01", "shelter-storm") => ShelterRoute(
                new Vector2(8.0f, -88.0f),
                new Vector2(4.5f, -84.5f)),
            ("settler-carrier-01", "shelter-storm") => ShelterRoute(
                new Vector2(12.5f, -94.5f),
                new Vector2(11.0f, -91.0f)),
            ("settler-elder-01", "shelter-storm") => ShelterRoute(
                new Vector2(-9.5f, -83.2f),
                new Vector2(-6.5f, -83.5f)),
            ("settler-traveler-01", "shelter-storm") => ShelterRoute(
                new Vector2(17.8f, -75.5f),
                new Vector2(17.5f, -79.5f)),
            ("settler-smith-helper-01", "shelter-storm") => ShelterRoute(
                new Vector2(17.5f, -88.5f),
                new Vector2(17.8f, -91.5f)),
            ("settler-weaver-01", "shelter-storm") => ShelterRoute(
                new Vector2(-15.2f, -80.0f),
                new Vector2(-14.5f, -77.5f)),
            ("settler-shepherd-01", "shelter-storm") => ShelterRoute(
                new Vector2(21.5f, -76.0f),
                new Vector2(21.5f, -79.5f)),
            ("settler-gatherer-01", "shelter-storm") => ShelterRoute(
                new Vector2(10.8f, -95.5f),
                new Vector2(10.5f, -92.5f)),
            ("settler-fisher-01", "shelter-storm") => ShelterRoute(
                new Vector2(21.0f, -82.5f),
                new Vector2(19.5f, -84.5f)),
            ("settler-fisher-01", "shelter-storm-swamp") => ShelterRoute(
                new Vector2(111f, 27f),
                new Vector2(99f, 39f)),
            ("settler-youth-01", "shelter-storm") => ShelterRoute(
                new Vector2(4.5f, -91.8f),
                new Vector2(2.5f, -91.5f)),

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
