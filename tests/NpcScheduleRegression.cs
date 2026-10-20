using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "work");
        var shiftedDay = allDay with { StartHour = 6, EndHour = 30 };
        foreach (var hour in new[] { -1.0, 0.0, 6.0, 12.0, 23.999, 24.0, 49.0 })
        {
            check(allDay.Contains(hour) && shiftedDay.Contains(hour),
                $"Explicit full-day NPC schedules include hour {hour}");
        }

        var day = allDay with { StartHour = 6, EndHour = 20 };
        var night = allDay with { StartHour = 20, EndHour = 6, Activity = "rest" };
        var npc = new NpcDefinition("schedule-test", NpcRole.Worker, [day, night]);
        check(npc.GetSchedule(6) == day && npc.GetSchedule(20) == night,
            "NPC changes activity exactly at schedule boundaries");
        check(npc.GetSchedule(0) == night && npc.GetSchedule(24) == night &&
              npc.GetSchedule(-1) == night && npc.GetSchedule(30) == day,
            "Overnight NPC schedules wrap across midnight and other days");
        check(!(allDay with { StartHour = 6, EndHour = 6 }).Contains(6),
            "Equal NPC schedule endpoints remain an empty interval");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid) &&
                  !(allDay with { StartHour = invalid }).Contains(12) &&
                  !(allDay with { EndHour = invalid }).Contains(12),
                "Non-finite times cannot activate an NPC schedule");
        }
    }
}
