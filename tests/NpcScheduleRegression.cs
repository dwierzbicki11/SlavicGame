using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "guard");
        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [allDay]);
        check(new[] { 0d, 6d, 12d, 23.999d, 24d, -1d, 48d }
                .All(hour => npc.GetSchedule(hour) == allDay),
            "Explicit full-day NPC schedules remain active at every normalized hour");

        var shiftedDay = allDay with { StartHour = 6, EndHour = 30 };
        check(shiftedDay.Contains(5.99) && shiftedDay.Contains(6) && shiftedDay.Contains(18),
            "Full-day NPC schedules also support shifted hour ranges");

        var night = allDay with { StartHour = 22, EndHour = 6 };
        check(night.Contains(22) && night.Contains(0) && night.Contains(5.99) &&
              !night.Contains(6) && !night.Contains(21.99),
            "Overnight NPC schedules include their start and exclude their end");

        var day = allDay with { StartHour = 6, EndHour = 22 };
        check(day.Contains(6) && !day.Contains(22) && !day.Contains(0),
            "Daytime NPC schedule boundaries remain half-open");
        check(!(allDay with { EndHour = 0 }).Contains(0),
            "Equal NPC schedule endpoints remain an empty interval");

        check(!allDay.Contains(double.NaN) && !allDay.Contains(double.PositiveInfinity) &&
              !(allDay with { StartHour = double.NegativeInfinity }).Contains(12) &&
              !(allDay with { EndHour = double.NaN }).Contains(12),
            "Non-finite NPC schedule hours and endpoints are rejected");
    }
}
