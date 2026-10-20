using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "watch");
        var shiftedDay = new NpcScheduleSlot(6, 30, "old-village", "watch");
        foreach (var hour in new[] { -24.0, -1.0, 0.0, 6.0, 12.0, 23.999, 24.0, 49.0 })
        {
            check(allDay.Contains(hour) && shiftedDay.Contains(hour),
                $"Full-day NPC schedules stay active at hour {hour}");
        }

        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [allDay]);
        check(npc.GetSchedule(12) == allDay,
            "NPC resolves an explicitly authored all-day duty");

        var empty = new NpcScheduleSlot(6, 6, "old-village", "rest");
        check(!empty.Contains(6) && !empty.Contains(12),
            "Equal NPC schedule endpoints remain an empty interval");

        var day = new NpcScheduleSlot(6, 19, "old-village", "work");
        var night = new NpcScheduleSlot(19, 6, "old-village", "rest");
        check(day.Contains(6) && !day.Contains(19) &&
              night.Contains(19) && !night.Contains(6) && night.Contains(0),
            "Day and overnight NPC duties retain inclusive starts and exclusive ends");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid) &&
                  !new NpcScheduleSlot(invalid, 24, "old-village", "watch").Contains(12) &&
                  !new NpcScheduleSlot(0, invalid, "old-village", "watch").Contains(12),
                "NPC schedules reject non-finite times and endpoints");
        }
    }
}
