using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.World;

public sealed class EnvironmentInteractionSystem
{
    public const string ResinCollectedFlag =
        "resource.forest-resin.first-collected";
    public const string MarshHerbCollectedFlag =
        "resource.marsh-herb.first-collected";

    private const float InteractionDistance = 3.6f;
    private static readonly Vector3 ResinSpot = new(31f, 0f, 15f);
    private static readonly Vector3 MarshHerbSpot = new(107f, 0f, 20f);
    private static readonly Vector3 MissingToolSpot = new(22f, 0f, 17f);
    private static readonly Vector3 MissingToolsWorksiteSpot = new(18.5f, 0f, 15.5f);
    private static readonly Vector3 FordRepairTimberSpot = new(16f, 0f, -96f);

    public InteractionTarget? Current { get; private set; }
    public string Message { get; private set; } = "";
    public string HudText => Current?.Prompt ?? Message;

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        Current = FindNearest(world);
    }

    public bool TryInteract(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        Current = FindNearest(world);
        if (Current is null)
            return false;

        if (Current.Id == "resource.forest-resin")
        {
            if (world.Progress.HasFlag(ResinCollectedFlag))
                return false;

            world.Progress.Inventory.Add("forest-resin", 3);
            world.Progress.SetFlag(ResinCollectedFlag);
            Message = "ZDOBYTO: ZYWICA LESNA x3";
            Current = FindNearest(world);
            return true;
        }

        if (Current.Id == "resource.marsh-herb")
        {
            if (world.Progress.HasFlag(MarshHerbCollectedFlag))
                return false;

            world.Progress.Inventory.Add("marsh-herb", 2);
            world.Progress.SetFlag(MarshHerbCollectedFlag);
            Message = "ZDOBYTO: ZIOLO BAGIENNE x2";
            Current = FindNearest(world);
            return true;
        }

        if (Current.Id == "side-r0-missing-tools.tool")
        {
            var collected = MissingToolsSideQuest.RecordToolFound(world);
            if (collected)
            {
                Message = "ODNALEZIONO: SIEKIERA CIESLI";
                Current = FindNearest(world);
            }
            return collected;
        }

        if (Current.Id == "side-r0-missing-tools.worksite")
        {
            var inspected = MissingToolsSideQuest.RecordWorksiteContext(world);
            if (inspected)
            {
                Message = "SLAD: NARZEDZIE ZOSTAWIONO PRZY NIEDOKONCZONEJ PRACY";
                Current = FindNearest(world);
            }
            return inspected;
        }

        if (Current.Id == "sq-r0-01.inspect-ford")
        {
            var changed = R0FordSideQuest.InspectFord(world);
            if (changed)
                Message = "ZLAMANY BROD: PRZEJSCIE WYMAGA NAPRAWY ALBO OBEJSCIA";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.take-timber")
        {
            var changed = R0FordSideQuest.CollectRepairTimber(world);
            if (changed)
                Message = "ZDOBYTO: BELKI DO NAPRAWY x2";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.inspect-bypass")
        {
            var changed = R0FordSideQuest.DiscoverBypass(world);
            if (changed)
                Message = "ODKRYTO: PLYTSZE OBEJSCIE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.repair")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.Repaired);
            if (changed)
                Message = "BROD NAPRAWIONY / BEZPOSREDNIE PRZEJSCIE OTWARTE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.mark-bypass")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.Bypass);
            if (changed)
                Message = "OBEJSCIE OZNACZONE / ALTERNATYWNE PRZEJSCIE OTWARTE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.close-ford")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.ClosedSafe);
            if (changed)
                Message = "USZKODZONY BROD ZAMKNIETY / BEZPIECZNA TRASA OZNACZONA";
            Current = FindNearest(world);
            return changed;
        }

        const string harvestPrefix = "wildlife.harvest.";
        if (Current.Id.StartsWith(
                harvestPrefix,
                StringComparison.Ordinal))
        {
            var wildlifeId =
                Current.Id[harvestPrefix.Length..];
            var actor =
                world.Wildlife.Actors.FirstOrDefault(item =>
                    string.Equals(
                        item.Id,
                        wildlifeId,
                        StringComparison.Ordinal));

            if (actor is null ||
                !world.Wildlife.TryHarvest(
                    world,
                    wildlifeId))
            {
                return false;
            }

            var lootText = string.Join(
                " / ",
                WildlifeHarvestCatalog.For(actor.Species)
                    .Select(item =>
                        $"{item.ItemId.ToUpperInvariant()} x{item.Quantity}"));

            Message =
                $"ZEBRANO LUP: {WildlifeCatalog.DisplayName(actor.Species)} / {lootText}";
            Current = FindNearest(world);
            return true;
        }

        const string bestiaryHarvestPrefix = "bestiary.harvest.";
        if (Current.Id.StartsWith(
                bestiaryHarvestPrefix,
                StringComparison.Ordinal))
        {
            var monsterId = Current.Id[bestiaryHarvestPrefix.Length..];
            var actor = world.Bestiary.Actors.FirstOrDefault(item =>
                string.Equals(item.Id, monsterId, StringComparison.Ordinal));

            if (actor is null || !world.Bestiary.TryHarvest(world, monsterId))
                return false;

            Message = $"ZEBRANO TROFEUM: {actor.DisplayName.ToUpperInvariant()} / {actor.TrophyItemId.ToUpperInvariant()} x1";
            Current = FindNearest(world);
            return true;
        }

        const string herbPrefix = "resource.herb.";
        if (Current.Id.StartsWith(herbPrefix, StringComparison.Ordinal))
        {
            var spawnId = Current.Id[herbPrefix.Length..];
            var harvested = SlavicHerbalismSystem.Harvest(world, spawnId, out var msg);
            Message = msg;
            Current = FindNearest(world);
            return harvested;
        }

        const string waymarkerPrefix = "waymarker-";
        if (Current.Id.StartsWith(waymarkerPrefix, StringComparison.Ordinal))
        {
            var inspected = SlavicWaymarkerSystem.Inspect(world, Current.Id, out var msg);
            Message = msg;
            Current = FindNearest(world);
            return inspected;
        }

        const string prefix = "campfire.";
        if (Current.Id.StartsWith(prefix, StringComparison.Ordinal))
        {
            if (Current.Id.EndsWith(".rest", StringComparison.Ordinal))
            {
                var fireId = Current.Id[prefix.Length..^(".rest".Length)];
                var rested = world.Campfires.TryRest(world, fireId, out var restMsg);
                Message = restMsg;
                Current = FindNearest(world);
                return rested;
            }

            var fireIdNormal = Current.Id[prefix.Length..];
            if (world.Campfires.IsLit(world, fireIdNormal))
            {
                var changed = world.Campfires.Extinguish(
                    world,
                    fireIdNormal,
                    "OGNISKO ZGASZONE");
                Message = world.Campfires.Message;
                Current = FindNearest(world);
                return changed;
            }

            var lit = world.Campfires.TryLight(world, fireIdNormal);
            Message = world.Campfires.Message;
            Current = FindNearest(world);
            return lit;
        }

        if (Current.Id == "shrine.perun")
        {
            if (SlavicShrineSystem.MakeOfferingToPerun(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "shrine.weles")
        {
            if (SlavicShrineSystem.MakeOfferingToWeles(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "shrine.mokosz")
        {
            if (SlavicShrineSystem.MakeOfferingToMokosz(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "shrine.svarog")
        {
            if (SlavicShrineSystem.MakeOfferingToSvarog(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "travel.cart.village")
        {
            if (WagonTravelSystem.TravelFromVillage(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "travel.cart.stronghold")
        {
            if (WagonTravelSystem.TravelFromStronghold(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "travel.cart.fishing")
        {
            if (WagonTravelSystem.TravelFromFishing(world, out var msg))
            {
                Message = msg;
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "side-tar-trail.inspect-cart")
        {
            if (TarBurnerSideQuest.RecordCartFound(world))
            {
                Message = "ODNALEZIONO: OCALALA BECZKA SMOLY";
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "side-tar-trail.secure-cart")
        {
            if (TarBurnerSideQuest.RecordCartSecured(world))
            {
                Message = "WOZ ZABEZPIECZONY: POZOSTAWIONO ZNAK OCHRONNY";
                Current = FindNearest(world);
                return true;
            }
        }

        if (Current.Id == "act1.mq01.lead")
        {
            var success = ActOneCampaignSystem.TakeLead(world);
            if (success)
                Message = "OTRZYMANO: LIST POLECAJACY DO GRODU BORZYWOJ / ROZPOCZETO MQ01";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act1.mq01.gate")
        {
            var success = ActOneCampaignSystem.EnterStronghold(world);
            if (success)
                Message = "WROTA GRODU OTWARTE / ZAKONCZONO MQ01 / ODBLOKOWANO MQ10";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act1.mq10.kurhan")
        {
            var success = ActOneCampaignSystem.InspectKurhan(world);
            if (success)
                Message = "PRZEBADANO KURHAN / ZAKONCZONO MQ10 / ODBLOKOWANO MQ11";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act1.mq11.council")
        {
            var success = ActOneCampaignSystem.HoldCouncil(world);
            if (success)
                Message = "RADA ZAKONCZONA: UZGODNIONO PRAWIDLA / ZAKONCZONO MQ11 / ODBLOKOWANO MQ12";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act1.mq12.perun")
        {
            var success = ActOneCampaignSystem.InvestigatePerunBór(world);
            if (success)
                Message = "WEZEL BORU PERUNOWEGO ODKRYTY / ZAKONCZONO MQ12 / ODBLOKOWANO MQ13";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act1.mq13.map-table")
        {
            var success = ActOneCampaignSystem.SynthesizeMapTable(world);
            if (success)
                Message = "★ UKONCZONO AKT I (25% GRY) / ODBLOKOWANO AKT II (MQ20) / TYTUL: OBRONCA PUSZCZY ★";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq20.begin")
        {
            var success = ActTwoCampaignSystem.BeginAct2(world);
            if (success)
                Message = "ROZPOCZETO AKT II / BADAJ BLOKADE UJSCIA RZEKI [MQ20]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq20.negotiate")
        {
            var success = ActTwoCampaignSystem.NegotiateEstuary(world);
            if (success)
                Message = "UZYSKANO DOSTEP DO PRZEPRAWY UJSCIA / ZBADANIE WIEZY [MQ20]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq20.beacon")
        {
            var success = ActTwoCampaignSystem.InspectRemoteBeacon(world);
            if (success)
                Message = "ODKRYTO DOWOD ZEWNETRZNEGO WEZLA RZECZNEGO [MQ20]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq20.resolve")
        {
            var success = ActTwoCampaignSystem.ResolveEstuary(world);
            if (success)
                Message = "PRZEPRAWA RZECZNA OTWARTA / ZAKONCZONO MQ20 / ROZPOCZETO MQ21";
            Current = FindNearest(world);
            return success;
        }

        // MQ21 handlers
        if (Current.Id == "act2.mq21.begin")
        {
            var success = ActTwoCampaignSystem.BeginMq21(world);
            if (success)
                Message = "ROZPOCZETO MQ21: SPOR O TRAKT RZECZNY";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq21.league")
        {
            var success = ActTwoCampaignSystem.InspectLeagueClaim(world);
            if (success)
                Message = "ZBADANO DOWODY LIGI KUPIECKIEJ [MQ21]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq21.nadbor")
        {
            var success = ActTwoCampaignSystem.InspectNadborClaim(world);
            if (success)
                Message = "ZBADANO ROSZCZENIA STARSZYZNY NADBORA [MQ21]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq21.resolve")
        {
            var success = ActTwoCampaignSystem.ResolveTradeTreaty(world, Quest.Mq21AccessDecision.LimitedBoth);
            if (success)
                Message = "PODPISANO TRAKTAT RZECZNY / ZAKONCZONO MQ21 / ODBLOKOWANO MQ22";
            Current = FindNearest(world);
            return success;
        }

        // MQ22 handlers
        if (Current.Id == "act2.mq22.begin")
        {
            var success = ActTwoCampaignSystem.BeginMq22(world);
            if (success)
                Message = "ROZPOCZETO MQ22: WYLOM W STAREJ SZTOLNI";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq22.breach")
        {
            var success = ActTwoCampaignSystem.EnterMineBreach(world);
            if (success)
                Message = "WEJSCIE W PRADAWNA SZTOLNIE [MQ22]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq22.timber")
        {
            var success = ActTwoCampaignSystem.InspectTimbering(world);
            if (success)
                Message = "ZBADANO PRADAWNE OSTEMPLOWANIE RUNICZNE [MQ22]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq22.tools")
        {
            var success = ActTwoCampaignSystem.InspectTools(world);
            if (success)
                Message = "ODNALEZIONO PRADAWNE CZAKANY GORNICZE [MQ22]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq22.anchor")
        {
            var success = ActTwoCampaignSystem.AnalyzeAnchorOre(world);
            if (success)
                Message = "ZANALIZOWANO MAGNETYCZNA RUDE ANOMALII [MQ22]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq22.stabilize")
        {
            var success = ActTwoCampaignSystem.StabilizeMine(world, Quest.Mq22Stabilization.SecureSite);
            if (success)
                Message = "SZTOLNIA ZABEZPIECZONA / ZAKONCZONO MQ22 / ODBLOKOWANO MQ23";
            Current = FindNearest(world);
            return success;
        }

        // Springs & wells handler
        const string springPrefix = "spring.";
        if (Current.Id.StartsWith(springPrefix, StringComparison.Ordinal))
        {
            var springId = Current.Id[springPrefix.Length..];
            var success = world.Springs.TryDrink(world, springId);
            if (success)
                Message = "NAPITO SIE CZYSTEJ ZRODLANEJ WODY / ENERGIA ODNOWIONA / REGENERACJA +50%";
            Current = FindNearest(world);
            return success;
        }

        // MQ23 handlers
        if (Current.Id == "act2.mq23.begin")
        {
            var success = ActTwoCampaignSystem.BeginMq23(world);
            if (success)
                Message = "ROZPOCZETO MQ23: PODZIAL ZASOBOW STAREJ SZTOLNI";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq23.extraction")
        {
            var success = ActTwoCampaignSystem.ReviewImpact(world, Quest.Mq23Impact.Extraction);
            if (success)
                Message = "PRZEANALIZOWANO WYDAJNOSC WYDOBYCIA RUDY [MQ23]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq23.safety")
        {
            var success = ActTwoCampaignSystem.ReviewImpact(world, Quest.Mq23Impact.Safety);
            if (success)
                Message = "PRZEANALIZOWANO BEZPIECZENSTWO CHODNIKOW [MQ23]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq23.stabilization")
        {
            var success = ActTwoCampaignSystem.ReviewImpact(world, Quest.Mq23Impact.Stabilization);
            if (success)
                Message = "PRZEANALIZOWANO WZMOCNIENIE RUNICZNE PODPARCIA [MQ23]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq23.resolve")
        {
            var success = ActTwoCampaignSystem.ChooseResourceControl(world, Quest.Mq23ResourceDecision.SupervisedSplit);
            if (success)
                Message = "PODPISANO DECYZJE O KONTROLI WYDOBYCIA / ZAKONCZONO MQ23 / ROZPOCZETO MQ24";
            Current = FindNearest(world);
            return success;
        }

        // MQ24 handlers
        if (Current.Id == "act2.mq24.begin")
        {
            var success = ActTwoCampaignSystem.BeginMq24(world);
            if (success)
                Message = "ROZPOCZETO MQ24: PROBA PRZEJSCIA PRZEZ BOR PERUNOWY";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq24.sign")
        {
            var success = ActTwoCampaignSystem.InspectRouteClue(world, Quest.Mq24RouteClue.StoryOrSign);
            if (success)
                Message = "ODCZYTANO PIORUNOWY DROGOWSKAZ W BORZE [MQ24]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq24.terrain")
        {
            var success = ActTwoCampaignSystem.InspectRouteClue(world, Quest.Mq24RouteClue.TerrainObservation);
            if (success)
                Message = "ODCZYTANO UKLAD MCHU I GLAZOW GRANICZNYCH [MQ24]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq24.passage")
        {
            var success = ActTwoCampaignSystem.AttemptTrialPassage(world, true);
            if (success)
                Message = "PRZEKROCZONO BURZOWY PRZESMYK / ZAKONCZONO MQ24 / ODBLOKOWANO WIELKA SYNTEZE";
            Current = FindNearest(world);
            return success;
        }

        // MQ25 handlers
        if (Current.Id == "act2.mq25.begin")
        {
            var success = ActTwoCampaignSystem.BeginMq25(world);
            if (success)
                Message = "ROZPOCZETO MQ25: WIELKA SYNTEZA SZLAKOW SLOWIANSKICH";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq25.validate")
        {
            var success = ActTwoCampaignSystem.ValidateCriticalPackages(world);
            if (success)
                Message = "ZWERYFIKOWANO CZTERY PAKIETY DANYCH REGIONALNYCH [MQ25]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq25.connect.route")
        {
            var success = ActTwoCampaignSystem.RecordSharedConnection(world, Quest.Mq25SharedConnection.CrossRegionalRoute);
            if (success)
                Message = "POWIAZANO SZLAKI HANDLOWE MIEDZYREGIONOWE [MQ25]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq25.connect.material")
        {
            var success = ActTwoCampaignSystem.RecordSharedConnection(world, Quest.Mq25SharedConnection.AnchorMaterialDistribution);
            if (success)
                Message = "USTALONO DYSTRYBUCJE RUDY KOTWICZNEJ [MQ25]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act2.mq25.conclude")
        {
            var success = ActTwoCampaignSystem.ConcludeSynthesis(world);
            if (success)
                Message = "DOKONANO WIELKIEJ SYNTEZY SZLAKOW / UKONCZONO AKT II (50% GRY)!";
            Current = FindNearest(world);
            return success;
        }

        // --- AKT III: MQ30 ---
        if (Current.Id == "act3.mq30.begin")
        {
            var success = ActThreeCampaignSystem.BeginMq30(world);
            if (success)
                Message = "ROZPOCZETO AKT III: PUSTKOWIE PIERWSZEGO PROGU [MQ30]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq30.trace.structural")
        {
            var success = ActThreeCampaignSystem.RecordArchiveTrace(world, Quest.Mq30ArchiveTrace.StructuralRecord);
            if (success)
                Message = "ZBADANO SLAD STRUKTURALNY ARCHIWUM [MQ30]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq30.trace.field")
        {
            var success = ActThreeCampaignSystem.RecordArchiveTrace(world, Quest.Mq30ArchiveTrace.FieldRecord);
            if (success)
                Message = "ZBADANO SLAD POLOWY ANOMALII PROGOWEJ [MQ30]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq30.route")
        {
            var success = ActThreeCampaignSystem.EstablishSafeRoute(world);
            if (success)
                Message = "WYZNACZONO BEZPIECZNY SZLAK DO ARCHIWUM / UKONCZONO MQ30!";
            Current = FindNearest(world);
            return success;
        }

        // --- AKT III: MQ31 ---
        if (Current.Id == "act3.mq31.begin")
        {
            var success = ActThreeCampaignSystem.BeginMq31(world);
            if (success)
                Message = "ROZPOCZETO BADANIE ARCHIWUM POPIOŁU [MQ31]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq31.evidence.parents")
        {
            var success = ActThreeCampaignSystem.RecordArchiveEvidence(world, Quest.Mq31ArchiveFact.ParentsArchiveRole);
            if (success)
                Message = "ODNALEZIONO ZAPISKI DOTYCZACE ROLI RODZICOW [MQ31]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq31.evidence.wszebor")
        {
            var success = ActThreeCampaignSystem.RecordArchiveEvidence(world, Quest.Mq31ArchiveFact.WszeborArchiveLink);
            if (success)
                Message = "ODNALEZIONO DOKUMENTY POWIAZAN WSZEBORA Z PROGIEM [MQ31]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq31.evidence.night")
        {
            var success = ActThreeCampaignSystem.RecordArchiveEvidence(world, Quest.Mq31ArchiveFact.ClosedThresholdNightReference);
            if (success)
                Message = "ODNALEZIONO KRONIKE NOCY ZAMKNIETEGO PROGU (22 BG) [MQ31]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq31.conclude")
        {
            var success = ActThreeCampaignSystem.ConcludeMq31(world);
            if (success)
                Message = "ARCHIWUM POPIOŁU ZBADANE / UKONCZONO MQ31!";
            Current = FindNearest(world);
            return success;
        }

        // --- AKT III: MQ32 ---
        if (Current.Id == "act3.mq32.begin")
        {
            var success = ActThreeCampaignSystem.BeginMq32(world);
            if (success)
                Message = "ROZPOCZETO REKONSTRUKCJE NOCY ZAMKNIETEGO PROGU [MQ32]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq32.evidence.echo")
        {
            var success = ActThreeCampaignSystem.RecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.EventEcho, fromInterestedFaction: false);
            if (success)
                Message = "ZAREJESTROWANO NIEZALEZNE ECHO WYDARZEN 22 BG [MQ32]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq32.evidence.guard")
        {
            var success = ActThreeCampaignSystem.RecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.ClosureGuardRecord, fromInterestedFaction: true);
            if (success)
                Message = "ZBADANO RAPORT STRAZY PROGOWEJ [MQ32]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq32.evidence.field")
        {
            var success = ActThreeCampaignSystem.RecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.AlternativeFieldRecord, fromInterestedFaction: false);
            if (success)
                Message = "ODNALEZIONO ALTERNATYWNY ZAPIS POLOWY SWIADKA [MQ32]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq32.contradiction")
        {
            var success = ActThreeCampaignSystem.MarkContradiction(world, Quest.Mq32EvidenceCategory.ClosureGuardRecord, Quest.Mq32EvidenceCategory.AlternativeFieldRecord);
            if (success)
                Message = "WYKRYTO SPRZECZNOSC MIEDZY RAPORTEM STRAZY A SWIADKIEM [MQ32]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq32.conclude")
        {
            var success = ActThreeCampaignSystem.ConcludeMq32(world);
            if (success)
                Message = "ZREKONSTRUOWANO PRZEBIEG KRYZYSU 22 BG / UKONCZONO MQ32!";
            Current = FindNearest(world);
            return success;
        }

        // --- AKT III: MQ33 ---
        if (Current.Id == "act3.mq33.begin")
        {
            var success = ActThreeCampaignSystem.BeginMq33(world);
            if (success)
                Message = "TROPIENIE WSZEBORA ROZPOCZETE [MQ33]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq33.position")
        {
            var success = ActThreeCampaignSystem.RecordWszeborPosition(world, Quest.Mq33WszeborStance.Guarded);
            if (success)
                Message = "USTALONO KRYJOWKE I POSTAWE WSZEBORA [MQ33]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq33.resolve")
        {
            var success = ActThreeCampaignSystem.ResolveWszeborConfrontation(world, Quest.Mq33ConfrontationOutcome.Conversation, Quest.Mq33WszeborStance.Cooperative, directDataAvailable: true);
            if (success)
                Message = "POROZUMIENIE Z WSZEBOREM / ZABEZPIECZONO PAKIET DANYCH [MQ33]!";
            Current = FindNearest(world);
            return success;
        }

        // --- AKT III: MQ34 ---
        if (Current.Id == "act3.mq34.begin")
        {
            var success = ActThreeCampaignSystem.BeginMq34(world);
            if (success)
                Message = "ROZPOCZETO NARADE TRZECH PROJEKTOW [MQ34]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq34.model.hard_closure")
        {
            var success = ActThreeCampaignSystem.PresentModel(world, Quest.Mq34CrisisModel.HardClosure, optionalDetailAvailable: true);
            if (success)
                Message = "PRZEDSTAWIONO PROJEKT TWARDEGO ZAMKNIECIA [MQ34]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq34.model.distributed_rebuild")
        {
            var success = ActThreeCampaignSystem.PresentModel(world, Quest.Mq34CrisisModel.DistributedRebuild, optionalDetailAvailable: true);
            if (success)
                Message = "PRZEDSTAWIONO PROJEKT ODBUDOWY ROZPROSZONEJ [MQ34]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq34.model.opening")
        {
            var success = ActThreeCampaignSystem.PresentModel(world, Quest.Mq34CrisisModel.Opening, optionalDetailAvailable: true);
            if (success)
                Message = "PRZEDSTAWIONO PROJEKT ROZWARCIA PROGOW [MQ34]";
            Current = FindNearest(world);
            return success;
        }

        if (Current.Id == "act3.mq34.conclude")
        {
            var success = ActThreeCampaignSystem.ConcludeActThree(world);
            if (success)
                Message = "SYNTEZA TRZECH PROJEKTOW / UKONCZONO AKT III (75% GRY)!";
            Current = FindNearest(world);
            return success;
        }

        return false;
    }

    private InteractionTarget? FindNearest(WorldState world)
    {
        var targets = new List<InteractionTarget>();

        if (!world.Progress.HasFlag(ResinCollectedFlag))
        {
            targets.Add(new InteractionTarget(
                "resource.forest-resin",
                Ground(world, ResinSpot),
                InteractionKind.Take,
                "E ZBIERZ ZYWICE LESNA"));
        }

        if (!world.Progress.HasFlag(MarshHerbCollectedFlag))
        {
            targets.Add(new InteractionTarget(
                "resource.marsh-herb",
                Ground(world, MarshHerbSpot),
                InteractionKind.Take,
                "E ZBIERZ ZIOLO BAGIENNE"));
        }

        if (MissingToolsSideQuest.CanInvestigate(world))
        {
            if (!world.Progress.HasFlag(MissingToolsSideQuest.ToolCollectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-r0-missing-tools.tool",
                    Ground(world, MissingToolSpot),
                    InteractionKind.Take,
                    "E PODNIES SIEKIERE CIESLI"));
            }

            if (!world.Progress.HasFlag(MissingToolsSideQuest.WorksiteInspectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-r0-missing-tools.worksite",
                    Ground(world, MissingToolsWorksiteSpot),
                    InteractionKind.Inspect,
                    "E OBEJRZYJ MIEJSCE PRACY"));
            }
        }

        if (R0FordSideQuest.Outcome(world) == R0FordOutcome.None)
        {
            if (!world.Progress.HasFlag(R0FordSideQuest.FordInspectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "sq-r0-01.inspect-ford",
                    Ground(world, R0FordSideQuest.FordWestBankSpot),
                    InteractionKind.Inspect,
                    "E OBEJRZYJ USZKODZONY BROD"));
            }
            else
            {
                if (!world.Progress.HasFlag(
                        R0FordSideQuest.RepairTimberCollectedFlag))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.take-timber",
                        Ground(world, FordRepairTimberSpot),
                        InteractionKind.Take,
                        "E WEZ BELKI DO NAPRAWY BRODU"));
                }

                if (world.Progress.Inventory.Contains(
                        R0FordSideQuest.RepairTimberItemId,
                        2))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.repair",
                        Ground(world, R0FordSideQuest.FordWestBankSpot),
                        InteractionKind.Use,
                        "E NAPRAW BROD"));
                }

                if (!world.Progress.HasFlag(
                        R0FordSideQuest.BypassDiscoveredFlag))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.inspect-bypass",
                        Ground(world, R0FordSideQuest.BypassWestBankSpot),
                        InteractionKind.Inspect,
                        "E SPRAWDZ PLYTSZE OBEJSCIE"));
                }
                else
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.mark-bypass",
                        Ground(world, R0FordSideQuest.BypassWestBankSpot),
                        InteractionKind.Activate,
                        "E OZNACZ OBEJSCIE JAKO TRASE"));

                    targets.Add(new InteractionTarget(
                        "sq-r0-01.close-ford",
                        Ground(world, R0FordSideQuest.CloseMarkerSpot),
                        InteractionKind.Activate,
                        "E ZAMKNIJ BROD I OZNACZ OBEJSCIE"));
                }
            }
        }

        var carcass =
            world.Wildlife.FindNearestHarvestable(
                world.PlayerPosition,
                InteractionDistance);
        if (carcass is not null)
        {
            targets.Add(new InteractionTarget(
                $"wildlife.harvest.{carcass.Id}",
                carcass.Position,
                InteractionKind.Take,
                $"E ZBIERZ LUP: {WildlifeCatalog.DisplayName(carcass.Species)}"));
        }

        var monsterCarcass =
            world.Bestiary.FindNearestHarvestable(
                world.PlayerPosition,
                InteractionDistance);
        if (monsterCarcass is not null)
        {
            targets.Add(new InteractionTarget(
                $"bestiary.harvest.{monsterCarcass.Id}",
                monsterCarcass.Position,
                InteractionKind.Take,
                $"E ZBIERZ TROFEUM: {monsterCarcass.DisplayName}"));
        }

        foreach (var fire in CampfireSystem.Fires)
        {
            var position = Ground(
                world,
                new Vector3(
                    fire.Position.X,
                    0f,
                    fire.Position.Y));

            var lit = world.Campfires.IsLit(world, fire.Id);
            string prompt;
            if (lit)
            {
                prompt = "E ZGAS OGNISKO";
            }
            else if (!string.IsNullOrWhiteSpace(fire.IgnitionItem) &&
                     !world.Progress.Inventory.Contains(fire.IgnitionItem))
            {
                prompt = "E OGNISKO / POTRZEBNA ZYWICA";
            }
            else
            {
                prompt = "E ROZPAL OGNISKO";
            }

            targets.Add(new InteractionTarget(
                $"campfire.{fire.Id}",
                position,
                InteractionKind.Activate,
                prompt));

            if (lit)
            {
                var restPos = Ground(world, new Vector3(fire.Position.X + 0.75f, 0f, fire.Position.Y + 0.75f));
                targets.Add(new InteractionTarget(
                    $"campfire.{fire.Id}.rest",
                    restPos,
                    InteractionKind.Activate,
                    "E ODPOCZNIJ PRZY OGNISKU [REGENERACJA I CZAS]"));
            }
        }

        foreach (var spawn in SlavicHerbalismSystem.Spawns)
        {
            if (SlavicHerbalismSystem.CanHarvest(world, spawn.Id))
            {
                var herbPos = Ground(world, new Vector3(spawn.Position.X, 0f, spawn.Position.Y));
                if (Vector3.Distance(world.PlayerPosition, herbPos) <= 6.5f)
                {
                    var herbDef = SlavicHerbalismSystem.HerbCatalog[spawn.ItemId];
                    targets.Add(new InteractionTarget(
                        $"resource.herb.{spawn.Id}",
                        herbPos,
                        InteractionKind.Take,
                        $"E ZBIERZ {herbDef.DisplayName}"));
                }
            }
        }

        foreach (var wm in SlavicWaymarkerSystem.Waymarkers)
        {
            var wmPos = Ground(world, new Vector3(wm.Position.X, 0f, wm.Position.Y));
            if (Vector3.Distance(world.PlayerPosition, wmPos) <= 6.5f)
            {
                targets.Add(new InteractionTarget(
                    wm.Id,
                    wmPos,
                    InteractionKind.Inspect,
                    $"E ODCZYTAJ {wm.Title}"));
            }
        }

        targets.Add(new InteractionTarget(
            "shrine.perun",
            Ground(world, SlavicShrineSystem.PerunShrineSpot),
            InteractionKind.Activate,
            "E ZLOZ OBIATE PERUNOWI [DAR GROMU]"));

        targets.Add(new InteractionTarget(
            "shrine.weles",
            Ground(world, SlavicShrineSystem.WelesShrineSpot),
            InteractionKind.Activate,
            "E ZLOZ OBIATE WELESOWI [DAR WOD]"));

        targets.Add(new InteractionTarget(
            "shrine.mokosz",
            Ground(world, SlavicShrineSystem.MokoszShrineSpot),
            InteractionKind.Activate,
            "E ZLOZ OBIATE MOKOSZY [DAR ZIEMI]"));

        targets.Add(new InteractionTarget(
            "shrine.svarog",
            Ground(world, SlavicShrineSystem.SvarogShrineSpot),
            InteractionKind.Activate,
            "E ZLOZ OBIATE SWAROGOWI [DAR KUZNI]"));

        targets.Add(new InteractionTarget(
            "travel.cart.village",
            Ground(world, WagonTravelSystem.VillageWagonSpot),
            InteractionKind.Activate,
            "E PODROZ WOZEM DO GRODU BORZYWOJA"));

        targets.Add(new InteractionTarget(
            "travel.cart.stronghold",
            Ground(world, WagonTravelSystem.StrongholdWagonSpot),
            InteractionKind.Activate,
            "E PODROZ WOZEM DO PRZYSTANI RYBACKIEJ"));

        targets.Add(new InteractionTarget(
            "travel.cart.fishing",
            Ground(world, WagonTravelSystem.FishingWagonSpot),
            InteractionKind.Activate,
            "E PODROZ WOZEM DO ZARNOWCA"));

        if (TarBurnerSideQuest.CanInvestigate(world))
        {
            if (!world.Progress.HasFlag(TarBurnerSideQuest.BarrelFoundFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-tar-trail.inspect-cart",
                    Ground(world, TarBurnerSideQuest.AmbushedCartSpot),
                    InteractionKind.Inspect,
                    "E ZBADAJ WYWROCONY WOZ SMOLNIKOW"));
            }
            else if (!world.Progress.HasFlag(TarBurnerSideQuest.CartSecuredFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-tar-trail.secure-cart",
                    Ground(world, TarBurnerSideQuest.AmbushedCartSpot),
                    InteractionKind.Activate,
                    "E ZABEZPIECZ WOZ I POZOSTAW ZNAK"));
            }
        }

        if (ActOneCampaignSystem.CanTakeLead(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq01.lead",
                Ground(world, ActOneCampaignSystem.ZarnowiecLeadSpot),
                InteractionKind.Inspect,
                "E ODBIERZ LIST POLECAJACY DO GRODU BORZYWOJ [MQ01]"));
        }

        if (ActOneCampaignSystem.CanEnterStronghold(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq01.gate",
                Ground(world, ActOneCampaignSystem.StrongholdGateSpot),
                InteractionKind.Activate,
                "E POKAZ LIST POLECAJACY STRAZNIKOWI [WROTA GRODU]"));
        }

        if (ActOneCampaignSystem.CanInspectKurhan(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq10.kurhan",
                Ground(world, ActOneCampaignSystem.KurhanSteleSpot),
                InteractionKind.Inspect,
                "E ZBADAJ PRADAWNA STELE KURHANU [SLADY PRZESZLOSCI - MQ10]"));
        }

        if (ActOneCampaignSystem.CanHoldCouncil(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq11.council",
                Ground(world, ActOneCampaignSystem.StrongholdCouncilSpot),
                InteractionKind.Activate,
                "E ZAPOCZATKUJ WIELKA RADE SLOWIANSKA [SPOR O GRANICE - MQ11]"));
        }

        if (ActOneCampaignSystem.CanInvestigatePerunBór(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq12.perun",
                Ground(world, ActOneCampaignSystem.PerunBoundaryStoneSpot),
                InteractionKind.Inspect,
                "E ZBADAJ ZNAK BORU PERUNOWEGO [ANOMALIA - MQ12]"));
        }

        if (ActOneCampaignSystem.CanSynthesizeMapTable(world))
        {
            targets.Add(new InteractionTarget(
                "act1.mq13.map-table",
                Ground(world, ActOneCampaignSystem.StrongholdMapTableSpot),
                InteractionKind.Activate,
                "E DOKONAJ SYNTEZY MAPY (FINAL AKTU I - 25% GRY) [MQ13]"));
        }

        if (ActTwoCampaignSystem.CanBeginAct2(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq20.begin",
                Ground(world, ActTwoCampaignSystem.EstuaryBarrierSpot),
                InteractionKind.Inspect,
                "E ZBADAJ BLOKADE UJSCIA RZEKI (POCZATEK AKTU II) [MQ20]"));
        }
        else if (ActTwoCampaignSystem.CanNegotiateEstuary(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq20.negotiate",
                Ground(world, ActTwoCampaignSystem.EstuaryBarrierSpot),
                InteractionKind.Activate,
                "E NEGOCJUJ ODBLOKOWANIE PRZEPRAWY Z RYBAKAMI [MQ20]"));
        }
        else if (ActTwoCampaignSystem.CanInspectRemoteBeacon(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq20.beacon",
                Ground(world, ActTwoCampaignSystem.RemoteBeaconSpot),
                InteractionKind.Inspect,
                "E ZBADAJ LATARNIE ZEWNETRZNEGO WEZLA RZEKI [MQ20]"));
        }
        else if (ActTwoCampaignSystem.CanResolveEstuary(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq20.resolve",
                Ground(world, ActTwoCampaignSystem.EstuaryBarrierSpot),
                InteractionKind.Activate,
                "E OTWORZ SZLAK WODNY I UZGODNIJ PRZEPRAWY [MQ20]"));
        }

        // MQ21 targets
        if (ActTwoCampaignSystem.CanBeginMq21(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq21.begin",
                Ground(world, ActTwoCampaignSystem.RiverCrossingBridgeSpot),
                InteractionKind.Inspect,
                "E ZBADAJ SPOR O MOST RZECZNY [MQ21]"));
        }
        else if (ActTwoCampaignSystem.CanInspectLeagueClaim(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq21.league",
                Ground(world, ActTwoCampaignSystem.LeagueClaimSpot),
                InteractionKind.Inspect,
                "E ZBADAJ PRZYWILEJ I KSIEGE LIGI KUPIECKIEJ [MQ21]"));
        }
        else if (ActTwoCampaignSystem.CanInspectNadborClaim(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq21.nadbor",
                Ground(world, ActTwoCampaignSystem.NadborClaimSpot),
                InteractionKind.Inspect,
                "E ZBADAJ PRAWA STARSZYZNY FLISAKOW NADBORA [MQ21]"));
        }
        else if (ActTwoCampaignSystem.CanResolveTradeTreaty(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq21.resolve",
                Ground(world, ActTwoCampaignSystem.TradeTreatySpot),
                InteractionKind.Activate,
                "E PODPISZ WIELKI TRAKTAT RZECZNY [MQ21]"));
        }

        // MQ22 targets
        if (ActTwoCampaignSystem.CanBeginMq22(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.begin",
                Ground(world, ActTwoCampaignSystem.MineBreachSpot),
                InteractionKind.Inspect,
                "E ZBADAJ WIESCI O STAREJ SZTOLNI GOR [MQ22]"));
        }
        else if (ActTwoCampaignSystem.CanEnterMineBreach(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.breach",
                Ground(world, ActTwoCampaignSystem.MineBreachSpot),
                InteractionKind.Activate,
                "E WEJDZ W PRADAWNY WYLOM SZTOLNI [MQ22]"));
        }
        else if (ActTwoCampaignSystem.CanInspectTimbering(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.timber",
                Ground(world, ActTwoCampaignSystem.AncientTimberingSpot),
                InteractionKind.Inspect,
                "E ZBADAJ RUNICZNE OSTEMPLOWANIE SZTOLNI [MQ22]"));
        }
        else if (ActTwoCampaignSystem.CanInspectTools(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.tools",
                Ground(world, ActTwoCampaignSystem.RunicToolsSpot),
                InteractionKind.Inspect,
                "E PODNIES PRADAWNE CZAKANY GORNICZE [MQ22]"));
        }
        else if (ActTwoCampaignSystem.CanAnalyzeAnchorOre(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.anchor",
                Ground(world, ActTwoCampaignSystem.AnchorOreSpot),
                InteractionKind.Inspect,
                "E ZANALIZOWAC MAGNETYCZNA RUDE ANOMALII [MQ22]"));
        }
        else if (ActTwoCampaignSystem.CanStabilizeMine(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq22.stabilize",
                Ground(world, ActTwoCampaignSystem.StabilizationSpot),
                InteractionKind.Activate,
                "E ZABEZPIECZ I WZMOCNIJ WEJSCIE DO SZTOLNI [MQ22]"));
        }

        // Springs & wells
        var nearestSpring = world.Springs.FindNearest(world.PlayerPosition, InteractionDistance);
        if (nearestSpring is not null)
        {
            targets.Add(new InteractionTarget(
                "spring." + nearestSpring.Id,
                Ground(world, nearestSpring.Position),
                InteractionKind.Use,
                nearestSpring.Prompt));
        }

        // MQ23 targets
        if (ActTwoCampaignSystem.CanBeginMq23(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq23.begin",
                Ground(world, ActTwoCampaignSystem.MineCouncilTableSpot),
                InteractionKind.Activate,
                "E ROZPOCZNIJ RADE GORNICZA W GRODZIE [MQ23]"));
        }
        else if (ActTwoCampaignSystem.CanReviewImpact(world, Quest.Mq23Impact.Extraction))
        {
            targets.Add(new InteractionTarget(
                "act2.mq23.extraction",
                Ground(world, ActTwoCampaignSystem.MineImpactExtractionSpot),
                InteractionKind.Inspect,
                "E PRZEANALIZUJ WYDAJNOSC WYDOBYCIA RUDY [MQ23]"));
        }
        else if (ActTwoCampaignSystem.CanReviewImpact(world, Quest.Mq23Impact.Safety))
        {
            targets.Add(new InteractionTarget(
                "act2.mq23.safety",
                Ground(world, ActTwoCampaignSystem.MineImpactSafetySpot),
                InteractionKind.Inspect,
                "E ZBADAJ PLAN BEZPIECZENSTWA CHODNIKOW [MQ23]"));
        }
        else if (ActTwoCampaignSystem.CanReviewImpact(world, Quest.Mq23Impact.Stabilization))
        {
            targets.Add(new InteractionTarget(
                "act2.mq23.stabilization",
                Ground(world, ActTwoCampaignSystem.MineImpactStabilizationSpot),
                InteractionKind.Inspect,
                "E OCEN WZMOCNIENIE RUNICZNE PODPARCIA [MQ23]"));
        }
        else if (ActTwoCampaignSystem.CanChooseResourceControl(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq23.resolve",
                Ground(world, ActTwoCampaignSystem.MineCouncilTableSpot),
                InteractionKind.Activate,
                "E USTAL PODZIAL ZASOBOW SZTOLNI [MQ23]"));
        }

        // MQ24 targets
        if (ActTwoCampaignSystem.CanBeginMq24(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq24.begin",
                Ground(world, ActTwoCampaignSystem.LightningWaypostSpot),
                InteractionKind.Activate,
                "E ROZPOCZNIJ PRZEPRAWE PRZEZ BOR PERUNOWY [MQ24]"));
        }
        else if (ActTwoCampaignSystem.CanInspectRouteClue(world, Quest.Mq24RouteClue.StoryOrSign))
        {
            targets.Add(new InteractionTarget(
                "act2.mq24.sign",
                Ground(world, ActTwoCampaignSystem.LightningWaypostSpot),
                InteractionKind.Inspect,
                "E ZBADAJ PIORUNOWY DROGOWSKAZ W BORZE [MQ24]"));
        }
        else if (ActTwoCampaignSystem.CanInspectRouteClue(world, Quest.Mq24RouteClue.TerrainObservation))
        {
            targets.Add(new InteractionTarget(
                "act2.mq24.terrain",
                Ground(world, ActTwoCampaignSystem.TerrainMossRockSpot),
                InteractionKind.Inspect,
                "E ODCZYTAJ UKLAD MCHU I GLAZOW GRANICZNYCH [MQ24]"));
        }
        else if (ActTwoCampaignSystem.CanAttemptTrialPassage(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq24.passage",
                Ground(world, ActTwoCampaignSystem.MistPassageTrialSpot),
                InteractionKind.Activate,
                "E PODEJMIJ PROBE PRZEJSCIA PRZEZ MGLISTY JAR [MQ24]"));
        }

        // MQ25 targets
        if (ActTwoCampaignSystem.CanBeginMq25(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq25.begin",
                Ground(world, ActTwoCampaignSystem.GrandSynthesisSpot),
                InteractionKind.Activate,
                "E ZWOLAJ WIELKA SYNTEZE SZLAKOW SLOWIANSKICH [MQ25]"));
        }
        else if (ActTwoCampaignSystem.CanValidateCriticalPackages(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq25.validate",
                Ground(world, ActTwoCampaignSystem.GrandSynthesisSpot),
                InteractionKind.Inspect,
                "E ZWERYFIKUJ CZTERY PAKIETY DANYCH REGIONALNYCH [MQ25]"));
        }
        else if (ActTwoCampaignSystem.CanRecordSharedConnection(world, Quest.Mq25SharedConnection.CrossRegionalRoute))
        {
            targets.Add(new InteractionTarget(
                "act2.mq25.connect.route",
                Ground(world, ActTwoCampaignSystem.GrandSynthesisSpot),
                InteractionKind.Activate,
                "E POWIAZ SZLAKI HANDLOWE MIEDZYREGIONOWE [MQ25]"));
        }
        else if (ActTwoCampaignSystem.CanRecordSharedConnection(world, Quest.Mq25SharedConnection.AnchorMaterialDistribution))
        {
            targets.Add(new InteractionTarget(
                "act2.mq25.connect.material",
                Ground(world, ActTwoCampaignSystem.GrandSynthesisSpot),
                InteractionKind.Activate,
                "E USTAL DYSTRYBUCJE RUDY KOTWICZNEJ [MQ25]"));
        }
        else if (ActTwoCampaignSystem.CanConcludeSynthesis(world))
        {
            targets.Add(new InteractionTarget(
                "act2.mq25.conclude",
                Ground(world, ActTwoCampaignSystem.GrandSynthesisSpot),
                InteractionKind.Activate,
                "E DOKONAJ WIELKIEJ SYNTEZY (50% KAMPANII) [MQ25]"));
        }

        // --- AKT III: MQ30 ---
        if (ActThreeCampaignSystem.CanBeginMq30(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq30.begin",
                Ground(world, ActThreeCampaignSystem.ThresholdWasteEntranceSpot),
                InteractionKind.Activate,
                "E WEJDZ NA PUSTKOWIE PIERWSZEGO PROGU [MQ30]"));
        }
        else if (ActThreeCampaignSystem.CanRecordArchiveTrace(world, Quest.Mq30ArchiveTrace.StructuralRecord))
        {
            targets.Add(new InteractionTarget(
                "act3.mq30.trace.structural",
                Ground(world, ActThreeCampaignSystem.ThresholdWasteEntranceSpot),
                InteractionKind.Inspect,
                "E ZBADAJ SLADY STRUKTURY ARCHIWUM [MQ30]"));
        }
        else if (ActThreeCampaignSystem.CanRecordArchiveTrace(world, Quest.Mq30ArchiveTrace.FieldRecord))
        {
            targets.Add(new InteractionTarget(
                "act3.mq30.trace.field",
                Ground(world, ActThreeCampaignSystem.ArchiveRuinsEntranceSpot),
                InteractionKind.Inspect,
                "E ZBADAJ SLADY POLA ANOMALII [MQ30]"));
        }
        else if (ActThreeCampaignSystem.CanEstablishSafeRoute(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq30.route",
                Ground(world, ActThreeCampaignSystem.ArchiveRuinsEntranceSpot),
                InteractionKind.Activate,
                "E WYZNACZ BEZPIECZNY SZLAK DO ARCHIWUM [MQ30]"));
        }

        // --- AKT III: MQ31 ---
        if (ActThreeCampaignSystem.CanBeginMq31(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq31.begin",
                Ground(world, ActThreeCampaignSystem.ArchiveRecordChamberSpot),
                InteractionKind.Activate,
                "E ROZPOCZNIJ BADANIE ARCHIWUM POPIOŁU [MQ31]"));
        }
        else if (ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Quest.Mq31ArchiveFact.ParentsArchiveRole))
        {
            targets.Add(new InteractionTarget(
                "act3.mq31.evidence.parents",
                Ground(world, ActThreeCampaignSystem.ParentsArchiveSpot),
                InteractionKind.Inspect,
                "E ZBADAJ ZAPISKI DOTYCZACE RODZICOW [MQ31]"));
        }
        else if (ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Quest.Mq31ArchiveFact.WszeborArchiveLink))
        {
            targets.Add(new InteractionTarget(
                "act3.mq31.evidence.wszebor",
                Ground(world, ActThreeCampaignSystem.WszeborArchiveSpot),
                InteractionKind.Inspect,
                "E ZBADAJ DOKUMENTY POWIAZANIA WSZEBORA [MQ31]"));
        }
        else if (ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Quest.Mq31ArchiveFact.ClosedThresholdNightReference))
        {
            targets.Add(new InteractionTarget(
                "act3.mq31.evidence.night",
                Ground(world, ActThreeCampaignSystem.ClosedThresholdNightRecordSpot),
                InteractionKind.Inspect,
                "E ODCZYTAJ KRONIKE NOCY ZAMKNIETEGO PROGU [MQ31]"));
        }
        else if (ActThreeCampaignSystem.CanConcludeMq31(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq31.conclude",
                Ground(world, ActThreeCampaignSystem.ArchiveRecordChamberSpot),
                InteractionKind.Activate,
                "E PODSUMUJ BADANIA ARCHIWUM [MQ31]"));
        }

        // --- AKT III: MQ32 ---
        if (ActThreeCampaignSystem.CanBeginMq32(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.begin",
                Ground(world, ActThreeCampaignSystem.NightOfThresholdEchoSpot),
                InteractionKind.Activate,
                "E ROZPOCZNIJ REKONSTRUKCJE KRYZYSU 22 BG [MQ32]"));
        }
        else if (ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.EventEcho))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.evidence.echo",
                Ground(world, ActThreeCampaignSystem.NightOfThresholdEchoSpot),
                InteractionKind.Inspect,
                "E ZAREJESTRUJ ECHO WYDARZEN 22 BG [MQ32]"));
        }
        else if (ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.ClosureGuardRecord))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.evidence.guard",
                Ground(world, ActThreeCampaignSystem.ClosureGuardRecordSpot),
                InteractionKind.Inspect,
                "E PRZEANALIZUJ DZIENNIK STRAZY PROGOWEJ [MQ32]"));
        }
        else if (ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Quest.Mq32EvidenceCategory.AlternativeFieldRecord))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.evidence.field",
                Ground(world, ActThreeCampaignSystem.AlternativeFieldRecordSpot),
                InteractionKind.Inspect,
                "E ODCZYTAJ ALTERNATYWNY RAPORT POLOWY [MQ32]"));
        }
        else if (ActThreeCampaignSystem.CanMarkContradiction(world, Quest.Mq32EvidenceCategory.ClosureGuardRecord, Quest.Mq32EvidenceCategory.AlternativeFieldRecord))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.contradiction",
                Ground(world, ActThreeCampaignSystem.NightOfThresholdEchoSpot),
                InteractionKind.Activate,
                "E SKONFRONTUJ SPRZECZNE RELACJE [MQ32]"));
        }
        else if (ActThreeCampaignSystem.CanConcludeMq32(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq32.conclude",
                Ground(world, ActThreeCampaignSystem.NightOfThresholdEchoSpot),
                InteractionKind.Activate,
                "E ODTWORZ PRZEBIEG NOCY ZAMKNIETEGO PROGU [MQ32]"));
        }

        // --- AKT III: MQ33 ---
        if (ActThreeCampaignSystem.CanBeginMq33(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq33.begin",
                Ground(world, ActThreeCampaignSystem.WszeborRetreatSpot),
                InteractionKind.Activate,
                "E ZLOKALIZUJ KRYJOWKE WSZEBORA [MQ33]"));
        }
        else if (ActThreeCampaignSystem.CanRecordWszeborPosition(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq33.position",
                Ground(world, ActThreeCampaignSystem.WszeborRetreatSpot),
                InteractionKind.Inspect,
                "E OCEŃ POSTAWE I ZAMIARY WSZEBORA [MQ33]"));
        }
        else if (ActThreeCampaignSystem.CanResolveWszeborConfrontation(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq33.resolve",
                Ground(world, ActThreeCampaignSystem.WszeborRetreatSpot),
                InteractionKind.Activate,
                "E PODEJMIJ ROZMOWE I ZABEZPIECZ PAKIET DANYCH [MQ33]"));
        }

        // --- AKT III: MQ34 ---
        if (ActThreeCampaignSystem.CanBeginMq34(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq34.begin",
                Ground(world, ActThreeCampaignSystem.ThreeProjectsSynthesisSpot),
                InteractionKind.Activate,
                "E ZWOLAJ NARADE TRZECH PROJEKTOW [MQ34]"));
        }
        else if (ActThreeCampaignSystem.CanPresentModel(world, Quest.Mq34CrisisModel.HardClosure))
        {
            targets.Add(new InteractionTarget(
                "act3.mq34.model.hard_closure",
                Ground(world, ActThreeCampaignSystem.ThreeProjectsSynthesisSpot),
                InteractionKind.Activate,
                "E PRZEDSTAW PROJEKT TWARDEGO ZAMKNIECIA [MQ34]"));
        }
        else if (ActThreeCampaignSystem.CanPresentModel(world, Quest.Mq34CrisisModel.DistributedRebuild))
        {
            targets.Add(new InteractionTarget(
                "act3.mq34.model.distributed_rebuild",
                Ground(world, ActThreeCampaignSystem.ThreeProjectsSynthesisSpot),
                InteractionKind.Activate,
                "E PRZEDSTAW PROJEKT ODBUDOWY ROZPROSZONEJ [MQ34]"));
        }
        else if (ActThreeCampaignSystem.CanPresentModel(world, Quest.Mq34CrisisModel.Opening))
        {
            targets.Add(new InteractionTarget(
                "act3.mq34.model.opening",
                Ground(world, ActThreeCampaignSystem.ThreeProjectsSynthesisSpot),
                InteractionKind.Activate,
                "E PRZEDSTAW PROJEKT ROZWARCIA PROGOW [MQ34]"));
        }
        else if (ActThreeCampaignSystem.CanConcludeActThree(world))
        {
            targets.Add(new InteractionTarget(
                "act3.mq34.conclude",
                Ground(world, ActThreeCampaignSystem.ThreeProjectsSynthesisSpot),
                InteractionKind.Activate,
                "E ZAKONCZ AKT III: ARCHIWISTA PROGOW (75% GRY) [MQ34]"));
        }

        return InteractionSystem.FindNearest(
            world.PlayerPosition,
            targets,
            InteractionDistance);
    }

    private static Vector3 Ground(
        WorldState world,
        Vector3 position)
    {
        position.Y = world.Terrain.SampleHeight(position);
        return position;
    }
}
