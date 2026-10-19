using SlavicGame.Engine.NPC;

internal static class NpcScheduleRegression
{
    public static void Run(Action<bool, string> check)
    {
        var allDay = new NpcScheduleSlot(0, 24, "old-village", "watch");
        var night = new NpcScheduleSlot(20, 6, "old-village", "sleep");
        foreach (var hour in new[] { 0.0, 6.0, 12.0, 20.0, 23.999, 24.0, -1.0 })
        {
            check(allDay.Contains(hour), $"Full-day NPC schedule includes {hour}");
        }

        check(night.Contains(20) && night.Contains(0) && night.Contains(5.999) &&
              !night.Contains(6) && !night.Contains(19.999),
            "Overnight schedules retain inclusive start and exclusive end");
        check(!new NpcScheduleSlot(6, 6, "old-village", "empty").Contains(6),
            "Equal schedule endpoints still represent an empty interval");
        check(new NpcScheduleSlot(6, 30, "old-village", "watch").Contains(3),
            "Full-day schedules can start away from midnight");
        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            check(!allDay.Contains(invalid) &&
                  !new NpcScheduleSlot(invalid, 24, "old-village", "watch").Contains(12) &&
                  !new NpcScheduleSlot(0, invalid, "old-village", "watch").Contains(12),
                "Non-finite schedule values cannot activate NPCs");
        }

        var npc = new NpcDefinition("all-day-watchman", NpcRole.CommunityGuard, [allDay]);
        foreach (var hour in new[] { 0.0, 8.0, 16.0, 23.999 })
        {
            check(npc.GetSchedule(hour) == allDay,
                $"Full-day NPC has an active schedule at {hour}");
        }
    }
}
