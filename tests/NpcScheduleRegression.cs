using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "rest");
        var shiftedDay = new NpcScheduleSlot(6, 30, "old-village", "rest");
        foreach (var hour in new[] { -24d, -0.5d, 0d, 6d, 23.999d, 24d, 48d })
        {
            check(allDay.Contains(hour), $"Full-day schedule includes hour {hour}");
            check(shiftedDay.Contains(hour), $"Shifted full-day schedule includes hour {hour}");
        }

        var day = new NpcScheduleSlot(6, 18, "old-village", "patrol");
        var night = new NpcScheduleSlot(18, 6, "old-village", "night-watch");
        var guard = new NpcDefinition("guard", NpcRole.CommunityGuard, [day, night]);
        check(guard.GetSchedule(6) == day && guard.GetSchedule(18) == night,
            "Schedule includes start and excludes end at activity transitions");
        check(guard.GetSchedule(0) == night && guard.GetSchedule(24) == night &&
              guard.GetSchedule(-1) == night,
            "Night watch spans midnight and normalizes hours outside one day");
        check(!new NpcScheduleSlot(6, 6, "old-village", "rest").Contains(6),
            "Equal schedule endpoints remain empty");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid), "Full-day schedule rejects nonfinite query hours");
            check(!new NpcScheduleSlot(invalid, 18, "old-village", "rest").Contains(12),
                "Schedule rejects nonfinite start hours");
            check(!new NpcScheduleSlot(6, invalid, "old-village", "rest").Contains(12),
                "Schedule rejects nonfinite end hours");
        }
    }
}
