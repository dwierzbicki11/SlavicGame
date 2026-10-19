using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "rest");
        var shiftedDay = new NpcScheduleSlot(6, 30, "old-village", "rest");
        double[] hours = [-24, -1, 0, 6, 12, 23.999, 24, 48];
        check(hours.All(hour => allDay.Contains(hour) && shiftedDay.Contains(hour)),
            "Full-day NPC schedules remain active across midnight and normalized hours");

        var empty = allDay with { EndHour = 0 };
        check(hours.All(hour => !empty.Contains(hour)),
            "Equal schedule endpoints still describe an empty interval");

        var day = allDay with { StartHour = 6, EndHour = 19 };
        var night = allDay with { StartHour = 19, EndHour = 6 };
        check(day.Contains(6) && !day.Contains(19) &&
              night.Contains(19) && !night.Contains(6),
            "Adjacent NPC shifts include their start and exclude their end");

        double[] invalid = [double.NaN, double.PositiveInfinity, double.NegativeInfinity];
        check(invalid.All(value =>
                !allDay.Contains(value) &&
                !(allDay with { StartHour = value }).Contains(12) &&
                !(allDay with { EndHour = value }).Contains(12)),
            "Non-finite hours and schedule bounds never activate an NPC shift");

        var allDayNpc = new NpcDefinition("all-day-worker", NpcRole.Worker, [allDay]);
        check(hours.All(hour => allDayNpc.GetSchedule(hour) == allDay),
            "NPC definition selects its full-day activity at every hour");
    }
}
