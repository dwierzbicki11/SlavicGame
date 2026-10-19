using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "guard");
        check(new[] { 0d, 6d, 12d, 23.999d, 24d, -1d, 48d }.All(allDay.Contains),
            "Full-day NPC duty remains active at midnight and across wrapped hours");

        var shiftedDay = allDay with { StartHour = 6, EndHour = 30 };
        check(shiftedDay.Contains(3) && shiftedDay.Contains(6) && shiftedDay.Contains(23),
            "Full-day NPC duty can start at an offset hour");

        var empty = allDay with { StartHour = 6, EndHour = 6 };
        check(!empty.Contains(6) && !empty.Contains(12),
            "Equal authored schedule endpoints retain empty-interval semantics");

        var work = allDay with { StartHour = 6, EndHour = 18 };
        var night = allDay with { StartHour = 18, EndHour = 6 };
        check(work.Contains(6) && !work.Contains(18) &&
              night.Contains(18) && !night.Contains(6) && night.Contains(0),
            "NPC schedule boundaries belong to exactly one adjacent duty");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid) &&
                  !(allDay with { StartHour = invalid }).Contains(12) &&
                  !(allDay with { EndHour = invalid }).Contains(12),
                "Non-finite NPC schedule hours never activate a duty");
        }

        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [allDay]);
        check(npc.GetSchedule(0) == allDay && npc.GetSchedule(12) == allDay,
            "NPC definition resolves an authored full-day schedule");
    }
}
