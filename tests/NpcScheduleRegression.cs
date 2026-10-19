using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "watch");
        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [allDay]);
        check(new[] { -1d, 0d, 6d, 12d, 23.999d, 24d, 49d }
                .All(hour => npc.GetSchedule(hour) == allDay),
            "Full-day NPC schedule remains active across midnight and wrapped query hours");

        var offsetDay = new NpcScheduleSlot(6, 30, "old-village", "watch");
        check(offsetDay.Contains(5) && offsetDay.Contains(6) && offsetDay.Contains(23),
            "Full-day NPC schedules can start at an offset hour");

        var empty = new NpcScheduleSlot(6, 6, "old-village", "idle");
        check(!empty.Contains(6) && !empty.Contains(12),
            "Identical authored endpoints still describe an empty NPC schedule");

        var day = new NpcScheduleSlot(6, 19, "old-village", "work");
        var night = new NpcScheduleSlot(19, 6, "old-village", "rest");
        check(day.Contains(6) && !day.Contains(19) &&
              night.Contains(19) && !night.Contains(6) && night.Contains(0),
            "NPC day and night slots include their start and exclude their end");

        check(new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity }
                .All(hour => !allDay.Contains(hour) &&
                    !new NpcScheduleSlot(hour, 24, "old-village", "watch").Contains(12) &&
                    !new NpcScheduleSlot(0, hour, "old-village", "watch").Contains(12)),
            "Non-finite schedule endpoints and query hours never activate an NPC slot");
    }
}
