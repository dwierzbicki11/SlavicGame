using System.Numerics;
using SlavicGame.Engine.Dialogue;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

internal static class SlavicShrineAndWagonRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();

        // 1. Slavic Shrines & Divine Offerings
        world.Player.SetState(world.Player.Health, 10f);
        world.Progress.Inventory.Add("forest-resin", 1);
        world.SetPlayerPosition(SlavicShrineSystem.PerunShrineSpot);
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id == "shrine.perun",
            "Player near Perun's shrine targets Perun offering interaction");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player can successfully make an offering to Perun");
        check(world.Player.Stamina >= world.Player.MaxStamina - 0.01f,
            "Perun offering restores player stamina to full");
        check(world.Progress.DivineRelationships.Get("Perun").Favor >= 15,
            "Perun offering increases Perun divine favor");
        check(world.Progress.HasFlag(SlavicShrineSystem.PerunBlessingFlag),
            "Perun blessing flag is recorded");

        // Weles Shrine
        world.Player.SetState(30f, world.Player.Stamina);
        world.Progress.Inventory.Add("marsh-herb", 1);
        world.SetPlayerPosition(SlavicShrineSystem.WelesShrineSpot);
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id == "shrine.weles",
            "Player near Weles' shrine targets Weles offering interaction");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player can successfully make an offering to Weles");
        check(world.Player.Health > 30f,
            "Weles offering heals the player");
        check(world.Progress.DivineRelationships.Get("Weles").Favor >= 15,
            "Weles offering increases Weles divine favor");

        // Mokosz Shrine
        world.Player.SetState(20f, world.Player.Stamina);
        world.Progress.Inventory.Add("simple-bandage", 1);
        world.SetPlayerPosition(SlavicShrineSystem.MokoszShrineSpot);
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id == "shrine.mokosz",
            "Player near Mokosz' shrine targets Mokosz offering interaction");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player can successfully make an offering to Mokosz");
        check(world.Player.Health >= world.Player.MaxHealth - 0.01f,
            "Mokosz offering restores player health to full");
        check(world.Progress.DivineRelationships.Get("Mokosz").Favor >= 15,
            "Mokosz offering increases Mokosz divine favor");

        // 2. Wagon Fast Travel
        var timeBefore = world.Time.TimeOfDayHours;
        world.SetPlayerPosition(WagonTravelSystem.VillageWagonSpot);
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id == "travel.cart.village",
            "Player near village travel wagon targets stronghold travel");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player can travel by wagon from village to stronghold");
        check(Vector2.Distance(
                new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z),
                new Vector2(-318f, 185f)) < 5f,
            "Wagon travel transports player to Gród Borzywoj");
        check(world.Time.TimeOfDayHours > timeBefore,
            "Wagon journey advances world time realistically");

        // 3. Tar Burner Side Quest
        world.SetPlayerPosition(new Vector3(-320f, 0f, 227f));
        world.NpcWorld.Update(world);

        check(world.Dialogue.TryStartNearest(world) &&
              world.Dialogue.SpeakerId == "stronghold-chieftain",
            "Player can speak with Kniaź Borzywoj in Gród Borzywoj");
        check(world.Dialogue.CurrentNode?.Id == "tb.offer",
            "Kniaź Borzywoj offers the tar trail sidequest");

        world.Dialogue.Confirm(world); // Accept quest
        world.Dialogue.Close();

        check(world.Progress.Quests.Get(TarBurnerSideQuest.QuestId).Phase == QuestPhase.Active,
            "Accepting quest marks Tar Burner sidequest active");
        check(world.Progress.HasFlag(TarBurnerSideQuest.AcceptedFlag),
            "Tar burner accepted flag is set");

        // Investigate ambushed cart on the trail
        world.SetPlayerPosition(TarBurnerSideQuest.AmbushedCartSpot);
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id == "side-tar-trail.inspect-cart",
            "Ambushed cart on trail is inspectable");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player inspects ambushed cart and retrieves tar barrel");
        check(world.Progress.Inventory.Contains(TarBurnerSideQuest.TarBarrelItemId),
            "Player obtains tar barrel from the cart");

        // Secure cart
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "side-tar-trail.secure-cart",
            "Player can secure the cart after retrieving the barrel");
        check(world.EnvironmentInteractions.TryInteract(world),
            "Player secures the cart with a protective ward");
        check(world.Progress.HasFlag(TarBurnerSideQuest.CartSecuredFlag),
            "Cart secured flag is recorded");

        // Deliver to Smolnik Żelisław
        var burner = world.NpcWorld.Find("smolnik-burner")
            ?? throw new Exception("Smolnik burner missing");
        world.SetPlayerPosition(burner.Position);
        world.NpcWorld.Update(world);

        check(world.Dialogue.TryStartNearest(world) &&
              world.Dialogue.SpeakerId == "smolnik-burner",
            "Player can speak with Smolnik Żelisław");
        check(world.Dialogue.CurrentNode?.Id == "tb.burner-barrel",
            "Smolnik recognizes the retrieved tar barrel");

        world.Dialogue.Confirm(world); // Deliver barrel
        world.Dialogue.Close();

        check(world.Progress.HasFlag(TarBurnerSideQuest.BurnerHelpedFlag),
            "Burner helped flag is recorded");
        check(!world.Progress.Inventory.Contains(TarBurnerSideQuest.TarBarrelItemId),
            "Tar barrel was delivered to the burner");

        // Return to Kniaź Borzywoj
        var chieftain = world.NpcWorld.Find("stronghold-chieftain")
            ?? throw new Exception("Chieftain missing");
        world.SetPlayerPosition(chieftain.Position);
        world.NpcWorld.Update(world);

        check(world.Dialogue.TryStartNearest(world) &&
              world.Dialogue.SpeakerId == "stronghold-chieftain",
            "Player can return to Kniaź Borzywoj");
        check(world.Dialogue.CurrentNode?.Id == "tb.return",
            "Kniaź Borzywoj acknowledges completed quest");

        world.Dialogue.Confirm(world); // Claim reward
        world.Dialogue.Close();

        check(world.Progress.Quests.Get(TarBurnerSideQuest.QuestId).Phase == QuestPhase.TurnedIn,
            "Tar burner sidequest is marked turned in");
        check(world.Progress.HasFlag(TarBurnerSideQuest.CompletedFlag),
            "Tar burner completed flag is set");
        check(TarBurnerSideQuest.Outcome(world) == TarBurnerOutcome.Completed,
            "Tar burner outcome resolves to Completed");
    }
}
