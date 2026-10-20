using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var fullDay = new NpcScheduleSlot(0, 24, "old-village", "watch");
        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [fullDay]);
        foreach (var hour in new[] { -24d, -0.5d, 0d, 6d, 12d, 23.999d, 24d, 48d })
            check(npc.GetSchedule(hour) == fullDay,
                $"Full-day NPC remains scheduled at hour {hour}");

        check(new NpcScheduleSlot(6, 30, "old-village", "watch").Contains(3),
            "Full-day schedule supports a shifted start");
        check(!new NpcScheduleSlot(6, 6, "old-village", "watch").Contains(6),
            "Equal schedule endpoints remain an empty interval");

        var day = new NpcScheduleSlot(6, 18, "old-village", "work");
        var night = new NpcScheduleSlot(18, 6, "old-village", "rest");
        foreach (var hour in new[] { -1d, 0d, 5.999d, 6d, 17.999d, 18d, 24d, 30d })
            check(day.Contains(hour) != night.Contains(hour),
                $"Day and night schedules cover hour {hour} exactly once");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!fullDay.Contains(invalid), "Full-day schedule rejects non-finite query hours");
            check(!new NpcScheduleSlot(invalid, 6, "old-village", "rest").Contains(23),
                "Schedule rejects non-finite start hours");
            check(!new NpcScheduleSlot(18, invalid, "old-village", "rest").Contains(23),
                "Schedule rejects non-finite end hours");
        }
    }
}
