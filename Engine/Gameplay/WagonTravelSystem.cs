using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class WagonTravelSystem
{
    public static readonly Vector3 VillageWagonSpot = new(12f, 0f, -112f);
    public static readonly Vector3 StrongholdWagonSpot = new(-324f, 0f, 194f);
    public static readonly Vector3 FishingWagonSpot = new(138f, 0f, -182f);

    public static bool TravelFromVillage(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.SetPlayerPosition(new Vector3(-318f, 0f, 185f));
        world.Time.SetTimeOfDay(world.Time.TimeOfDayHours + 0.75);
        world.NpcWorld.Update(world);
        message = "PODROZ WOZEM: DOTARTO DO GRODU BORZYWOJA";
        return true;
    }

    public static bool TravelFromStronghold(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.SetPlayerPosition(new Vector3(142f, 0f, -180f));
        world.Time.SetTimeOfDay(world.Time.TimeOfDayHours + 0.75);
        world.NpcWorld.Update(world);
        message = "PODROZ WOZEM: DOTARTO DO PRZYSTANI RYBACKIEJ";
        return true;
    }

    public static bool TravelFromFishing(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        world.SetPlayerPosition(new Vector3(10f, 0f, -108f));
        world.Time.SetTimeOfDay(world.Time.TimeOfDayHours + 0.75);
        world.NpcWorld.Update(world);
        message = "PODROZ WOZEM: DOTARTO DO ZARNOWCA";
        return true;
    }
}
