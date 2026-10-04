using System.Numerics;
using SlavicGame.Engine.World;

internal static class NpcWorkstationRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);

        var workstationModels =
            NpcWorkstationCatalog.BuildModels(world.Terrain);

        check(NpcWorkstationCatalog.Workstations.Count >= 10,
            "R0 exposes a substantial set of visible NPC workstations");
        check(workstationModels.Count >= 20,
            "NPC workstations add a visible set of tools and work props");
        check(workstationModels.Select(model => model.Id)
                .Distinct(StringComparer.Ordinal)
                .Count() == workstationModels.Count,
            "NPC workstation prop IDs are unique");
        check(workstationModels.All(model =>
                world.Models.Any(worldModel =>
                    string.Equals(
                        worldModel.Id,
                        model.Id,
                        StringComparison.Ordinal))),
            "All workstation props are registered in the generated world");

        foreach (var workstation in NpcWorkstationCatalog.Workstations)
        {
            var sampledTime = FindWorkTime(
                workstation.NpcId,
                workstation.Activity);

            check(sampledTime is not null,
                $"{workstation.NpcId} has a deterministic workstation duty phase");

            world.Time.SetTimeOfDay(sampledTime!.Value);
            world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
            world.NpcWorld.Update(world);

            var actor = world.NpcWorld.Find(workstation.NpcId);
            check(actor is not null,
                $"{workstation.NpcId} exists in the runtime roster");
            check(string.Equals(
                    actor!.Activity,
                    workstation.Activity,
                    StringComparison.Ordinal),
                $"{workstation.NpcId} keeps the authored work activity at the workstation");
            check(!actor.IsMoving,
                $"{workstation.NpcId} stops walking during workstation duty");

            var horizontal = new Vector2(
                actor.Position.X,
                actor.Position.Z);
            check(Vector2.Distance(
                    horizontal,
                    workstation.WorkerPosition) < 0.55f,
                $"{workstation.NpcId} stands at its authored workstation point");

            var desired =
                Vector2.Normalize(
                    workstation.FacingTarget -
                    workstation.WorkerPosition);
            var actual =
                WorldPlacementOrientation.ForwardFromYaw(
                    actor.YawRadians);
            check(Vector2.Dot(actual, desired) > 0.96f,
                $"{workstation.NpcId} faces the visible workstation while working");
        }

        check(NpcWorkstationCatalog.Find(
                "settler-weaver-01",
                "weave-work")?.Props.Any(prop =>
                    prop.AssetPath.EndsWith(
                        "krosno_r0_01.glb",
                        StringComparison.Ordinal)) == true,
            "Weaver has a real loom workstation");

        check(NpcWorkstationCatalog.Find(
                "settler-woodworker-01",
                "wood-work")?.Props.Any(prop =>
                    prop.AssetPath.EndsWith(
                        "stol_warsztatowy_r0_01.glb",
                        StringComparison.Ordinal)) == true,
            "Woodworker has a real workbench");

        check(NpcWorkstationCatalog.Find(
                "settler-fisher-01",
                "mend-nets")?.Props.Any(prop =>
                    prop.AssetPath.EndsWith(
                        "rope_coil_r0_01.glb",
                        StringComparison.Ordinal)) == true,
            "Fisher net-mending station uses visible rope");

        static double? FindWorkTime(
            string npcId,
            string activity)
        {
            for (var minute = 0; minute < 24 * 60; minute++)
            {
                var hour = minute / 60.0;
                if (NpcWorkstationCatalog.TrySample(
                        npcId,
                        activity,
                        hour,
                        out _))
                {
                    return hour;
                }
            }

            return null;
        }
    }
}
