using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var fullDay = new NpcScheduleSlot(0, 24, "old-village", "watch");
        var guard = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [fullDay]);
        foreach (var hour in new[] { -1d, 0d, 6d, 12d, 23.999d, 24d, 48d })
        {
            check(guard.GetSchedule(hour) == fullDay,
                $"All-day NPC duty remains available at hour {hour}");
        }

        var shiftedDay = fullDay with { StartHour = 6, EndHour = 30 };
        check(shiftedDay.Contains(3) && shiftedDay.Contains(6),
            "Full-day duty also works when it starts away from midnight");

        var empty = fullDay with { EndHour = 0 };
        check(!empty.Contains(0) && !empty.Contains(12),
            "Equal schedule endpoints retain empty-slot semantics");

        var day = fullDay with { StartHour = 6, EndHour = 22 };
        var night = fullDay with { StartHour = 22, EndHour = 6 };
        foreach (var hour in new[] { -2d, 0d, 5.999d, 6d, 12d, 21.999d, 22d, 24d, 30d })
        {
            check(day.Contains(hour) != night.Contains(hour),
                $"Day and overnight NPC shifts have no gap or overlap at hour {hour}");
        }
        check(day.Contains(6) && !day.Contains(22) && night.Contains(22) && !night.Contains(6),
            "NPC shifts include their start and exclude their end");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!fullDay.Contains(invalid), "All-day duty rejects a non-finite query hour");
            check(!(fullDay with { StartHour = invalid }).Contains(12),
                "NPC duty rejects a non-finite start hour");
            check(!(fullDay with { EndHour = invalid }).Contains(12),
                "NPC duty rejects a non-finite end hour");
        }
    }
}
