using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var fullDay = new NpcScheduleSlot(0, 24, "old-village", "night-watch");
        var shiftedDay = fullDay with { StartHour = 6, EndHour = 30 };
        foreach (var hour in new[] { -1.0, 0.0, 6.0, 12.0, 23.999, 24.0, 48.0 })
        {
            check(fullDay.Contains(hour) && shiftedDay.Contains(hour),
                $"Full-day NPC duty includes normalized hour {hour}");
        }

        var npc = new NpcDefinition("all-day-guard", NpcRole.CommunityGuard, [fullDay]);
        check(ReferenceEquals(npc.GetSchedule(12), fullDay),
            "NPC schedule lookup selects a full-day duty");

        var empty = fullDay with { StartHour = 6, EndHour = 6 };
        check(!empty.Contains(6) && !empty.Contains(12),
            "Equal schedule endpoints remain an empty interval");

        var day = fullDay with { StartHour = 6, EndHour = 19 };
        var night = fullDay with { StartHour = 19, EndHour = 6 };
        check(day.Contains(6) && !day.Contains(19) &&
              night.Contains(19) && !night.Contains(6),
            "Day and overnight NPC duties include the start and exclude the end");
        check(night.Contains(-1) && night.Contains(26) && !night.Contains(36),
            "Overnight NPC duty wraps hours outside the current day");

        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!fullDay.Contains(invalid), "Full-day duty rejects non-finite query hours");
            check(!(fullDay with { StartHour = invalid }).Contains(12) &&
                  !(fullDay with { EndHour = invalid }).Contains(12),
                "NPC duty rejects non-finite schedule endpoints");
        }
    }
}
