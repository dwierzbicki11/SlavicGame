using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class SlavicShrineSystem
{
    public static readonly Vector3 PerunShrineSpot = new(-248f, 0f, 160f);
    public static readonly Vector3 WelesShrineSpot = new(100f, 0f, -145f);
    public static readonly Vector3 MokoszShrineSpot = new(-78.5f, 0f, -170f);
    public static readonly Vector3 SvarogShrineSpot = new(-295f, 0f, 218f);

    public const string PerunBlessingFlag = "divine.blessing.perun";
    public const string WelesBlessingFlag = "divine.blessing.weles";
    public const string MokoszBlessingFlag = "divine.blessing.mokosz";
    public const string SvarogBlessingFlag = "divine.blessing.svarog";

    public static bool MakeOfferingToPerun(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Progress.Inventory.Contains("forest-resin"))
        {
            world.Progress.Inventory.Remove("forest-resin", 1);
        }
        else if (world.Progress.Inventory.Contains("arrow-basic"))
        {
            world.Progress.Inventory.Remove("arrow-basic", 1);
        }

        world.Progress.DivineRelationships.Get("Perun").ChangeFavor(15);
        world.Player.SetState(world.Player.Health, world.Player.MaxStamina);
        world.Progress.SetFlag(PerunBlessingFlag);
        message = "PERUN PRZYJAL OBIATE: PELNA STAMINA I PRZYCHYLNOSC GROMU";
        return true;
    }

    public static bool MakeOfferingToWeles(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Progress.Inventory.Contains("marsh-herb"))
        {
            world.Progress.Inventory.Remove("marsh-herb", 1);
        }
        else if (world.Progress.Profile.Money >= 5)
        {
            world.Progress.Profile.ChangeMoney(-5);
        }

        world.Progress.DivineRelationships.Get("Weles").ChangeFavor(15);
        world.Player.Heal(40f);
        world.Progress.SetFlag(WelesBlessingFlag);
        message = "WELES PRZYJAL OBIATE: DAR OCHRONY PRZED POTWORAMI";
        return true;
    }

    public static bool MakeOfferingToMokosz(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Progress.Inventory.Contains("simple-bandage"))
        {
            world.Progress.Inventory.Remove("simple-bandage", 1);
        }
        else if (world.Progress.Inventory.Contains("marsh-herb"))
        {
            world.Progress.Inventory.Remove("marsh-herb", 1);
        }

        world.Progress.DivineRelationships.Get("Mokosz").ChangeFavor(15);
        world.Player.Restore();
        world.Progress.SetFlag(MokoszBlessingFlag);
        message = "MOKOSZ PRZYJELA OBIATE: PELNE OZDROWIENIE I SPOKOJ ZIEMI";
        return true;
    }

    public static bool MakeOfferingToSvarog(WorldState world, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Progress.Inventory.Contains("iron-chunk"))
        {
            world.Progress.Inventory.Remove("iron-chunk", 1);
        }
        else if (world.Progress.Inventory.Contains("forest-resin"))
        {
            world.Progress.Inventory.Remove("forest-resin", 1);
        }
        else if (world.Progress.Profile.Money >= 10)
        {
            world.Progress.Profile.ChangeMoney(-10);
        }

        world.Progress.DivineRelationships.Get("Svarog").ChangeFavor(15);
        world.Player.SetState(world.Player.Health, world.Player.MaxStamina);
        world.Progress.SetFlag(SvarogBlessingFlag);
        message = "SWAROG PRZYJAL OBIATE: OGIEN KUZNICZY I BLOGOSLAWIEŃSTWO ZELAZA";
        return true;
    }
}
