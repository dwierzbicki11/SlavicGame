using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "stand-watch");
        foreach (var hour in new[] { -24.0, -0.5, 0.0, 6.0, 23.999, 24.0, 48.0 })
            check(allDay.Contains(hour), $"Full-day NPC schedule contains {hour}");

        var shifted = new NpcScheduleSlot(6, 30, "old-village", "stand-watch");
        check(shifted.Contains(5) && shifted.Contains(6) && shifted.Contains(23),
            "Full-day NPC schedule supports a shifted start");
        check(!new NpcScheduleSlot(6, 6, "old-village", "idle").Contains(6),
            "Identical schedule endpoints remain empty");

        var night = new NpcScheduleSlot(19, 6, "old-village", "night-watch");
        check(night.Contains(19) && night.Contains(0) && night.Contains(29.5) &&
              !night.Contains(6) && !night.Contains(18.999),
            "Overnight NPC schedule includes start and excludes end");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid), "Full-day schedule rejects non-finite time");
            check(!new NpcScheduleSlot(invalid, 6, "old-village", "idle").Contains(0) &&
                  !new NpcScheduleSlot(19, invalid, "old-village", "idle").Contains(23),
                "NPC schedule rejects non-finite endpoints");
        }

        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [allDay]);
        check(ReferenceEquals(npc.GetSchedule(12), allDay),
            "NPC selects its full-day activity instead of falling back to idle");
    }
}
