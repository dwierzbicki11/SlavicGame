# Side Quest Production Cards — R0 Żarnowiec v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R0_01`–`SQ_R0_03` z `SideQuestCatalog.md`. Tytuły pozostają robocze F. Dokument nie zmienia central lore ani `LightOverSwamp`; wartości nagród, dokładne dialogi, finalne giver IDs i layout POI pozostają odpowiednio playtest/dialogue/art lockiem.

## Wspólny kontrakt R0

Każdy quest używa faz z `QuestDesign.md` i zapisuje `quest_id`, `phase`, ukończone cele, decyzje, evidence IDs, world-state deltas oraz checkpoint. Jednorazowe nagrody i mutacje świata są idempotentne. Sen, fast travel i save/load nie resetują śledztwa. Opcjonalny NPC może zmienić sposób rozwiązania, ale jego utrata nie może tworzyć softlocku. Wszystkie trzy questy są side-contentem: żaden nie jest wymagany do MQ00/MQ01 ani nie ujawnia nowej centralnej prawdy Sieci.

## SQ_R0_01 — Złamany bród

- **Typ / funkcja:** wspólnota; tutorial trwałej zmiany lokalnego world state i usług.
- **Region:** R0 Żarnowiec, przeprawa na lokalnym szlaku; dokładny layout pozostaje level-art lockiem.
- **Wejście:** R0 dostępne, gracz może swobodnie eksplorować po podstawowym onboardingu. MQ00 może być aktywne lub ukończone.
- **Trigger:** pierwsze odkrycie uszkodzonej przeprawy albo lokalna informacja o problemie; giver nie jest wymagany do aktywacji.
- **Fazy:** `Offered/Active` → oględziny → wybór sposobu → wykonanie → `Resolved` → opcjonalne `TurnedIn`.
- **Cele:** potwierdzić, dlaczego standardowe przejście jest niedostępne; znaleźć legalny sposób przywrócenia lub obejścia dostępu; utrwalić wynik.
- **Legalne rozwiązania:** pomoc wspólnocie w naprawie; przygotowanie bezpiecznego obejścia; świadome pozostawienie przeprawy zamkniętej po zapewnieniu alternatywnej trasy. Konkretne koszty materiałów są economy/playtest lockiem.
- **Stan trwały:** `sq_r0_01_outcome = repaired|bypass|closed_safe`; `r0_ford_access`; opcjonalny modyfikator lokalnej reputacji/usługi. Stan nie może zostać nadpisany przez ponowne wejście do triggera.
- **World-state:** wariant naprawiony/obejście zmienia traversal i późniejsze ambient reakcje; nie blokuje krytycznej trasy kampanii.
- **Dependencies:** R0 bible, traversal, interaction, inventory/economy, NPC reaction framework, `ASSETSET_R0_CORE`.
- **Fail-forward:** utrata givera lub materiałowego shortcutu pozostawia ścieżkę obejścia; brak środków nie blokuje questa permanentnie; zniszczony opcjonalny prop nie usuwa informacji potrzebnej do rozwiązania.
- **Nagrody:** reputacja/usługa/wygoda traversal w zależności od wariantu; bez gwarantowanego power spike. Liczby są playtest lockiem.
- **Save/load:** zapis przed wyborem, w trakcie wykonania i po world-state mutation musi odtworzyć właściwy wariant bez podwójnej nagrody.
- **Minimalne QA:** trzy outcomes osiągalne; krytyczny giver nie jest wymagany po aktywacji; world state i nav/traversal zgadzają się po reloadzie; MQ00/MQ01 pozostają ukończalne w każdym wariancie.

## SQ_R0_02 — Dług zielarki

- **Typ / funkcja:** osobisty NPC; uczy relacji, alchemii i alternatywnego supply/fallback.
- **Region:** R0 Żarnowiec i bezpieczny lokalny obszar zbioru/pozyskania składników.
- **Wejście:** system alchemii/inventory dostępny; zielarka lub jej jawny lifecycle fallback może zaoferować sprawę. Quest nie wymaga konkretnego wyniku MQ00.
- **Trigger:** rozmowa o brakującym zobowiązaniu/supply albo odkrycie powiązanego wpisu/przedmiotu po niedostępności givera.
- **Fazy:** `Offered` → ustalenie długu → pozyskanie/identyfikacja potrzebnych zasobów → decyzja komu i w jakiej formie pomóc → `Resolved/TurnedIn`.
- **Cele:** zrozumieć lokalny problem zaopatrzenia; użyć istniejącego systemu składników/receptur bez tworzenia nowej historycznej praktyki; zdecydować między pomocą bezpośrednią, substytutem albo odmową.
- **Legalne rozwiązania:** dostarczenie zaakceptowanego zestawu zasobów; przygotowanie zatwierdzonego substytutu przez alchemię; rozwiązanie długu przez alternatywną usługę, jeśli stan NPC na to pozwala. Konkretne receptury muszą pochodzić z `alchemy/RecipesV01.md` lub przyszłego zatwierdzonego wpisu.
- **Stan trwały:** `sq_r0_02_outcome = supplied|substitute|service|declined`; `r0_herbal_supply_state`; relacja z odpowiednim slotem NPC. Supply unlock jest wygodą, nie wymogiem kampanii.
- **Dependencies:** `ProductionNpcRoster.md`, alchemy recipes, inventory/economy, dialogue/reputation, R0 bible.
- **Fail-forward:** jeśli giver staje się niedostępny zgodnie z lifecycle, turn-in przejmuje zatwierdzony fallback albo quest kończy się terminalnym outcome bez utraty krytycznego main-story zasobu; zużycie opcjonalnego składnika nie blokuje alternatywnej ścieżki.
- **Research boundary:** nazwy roślin, praktyki lecznicze i material culture nie mogą być dopisywane jako fakt historyczny bez research ownera; karta implementacyjna może używać istniejących zatwierdzonych item IDs albo jawnego F.
- **Nagrody:** relacja, supply/vendor variant, wiedza/receptura tylko jeśli już zatwierdzona; wartości cen i ilości pozostają economy lockiem.
- **Save/load:** zużyte składniki, wybrany wariant i jednorazowy unlock muszą być atomowe z checkpointem turn-in.
- **Minimalne QA:** brak duplikacji składników/nagród; wszystkie legalne warianty zapisują spójny supply state; utrata givera ma fallback/terminal outcome; quest nie zmienia wymaganych flag MQ.

## SQ_R0_03 — Stary kopiec

- **Typ / funkcja:** eksploracyjny/evidence; mały chain uczący provenance i ostrożnej interpretacji lokalnej pamięci miejsca.
- **Region:** R0, opcjonalny kopiec/teren pamięci poza krytycznym przebiegiem MQ00.
- **Wejście:** tracking/journal evidence dostępne; miejsce można odkryć samodzielnie albo przez lokalny lead.
- **Trigger:** wejście w POI lub uzyskanie leadu; brak obowiązkowego givera.
- **Fazy:** odkrycie → oględziny → zebranie co najmniej dwóch niezależnych kategorii evidence → interpretacja → decyzja o zachowaniu/udostępnieniu wiedzy → `Resolved`.
- **Evidence contract:** wymagane są jawne IDs danych terenowych/artefaktowych/relacyjnych z provenance. Production content wybiera co najmniej dwa źródła, ale żadna pojedyncza notatka ani NPC nie może być jedynym kluczem.
- **Granica lore:** quest może potwierdzić, że miejsce było używane/pamiętane w więcej niż jednym okresie lub przez więcej niż jedną interpretację lokalną; nie może potwierdzić nowej prawdy o Sieci, bogach ani Czwartej Sferze. Niepewność pozostaje oznaczona jako hipoteza w journalu.
- **Legalne rozwiązania:** udokumentowanie i pozostawienie miejsca; przekazanie informacji lokalnej osobie/instytucji; zachowanie wiedzy dla siebie. Nie ma obowiązkowego rabunku grobu ani niszczenia miejsca.
- **Stan trwały:** `sq_r0_03_outcome = documented|shared|kept`; `r0_old_mound_known`; evidence provenance oraz opcjonalny lokalny reaction flag.
- **Dependencies:** TrackingSystem, evidence/journal, R0 bible, interaction, persistence; konkretne formy pochówku, wyposażenie i chronologia wymagają material-culture/research locku przed finalnym art/contentem.
- **Fail-forward:** zniszczenie/utrata jednego opcjonalnego evidence pozostawia drugą kombinację; brak NPC do turn-in nie blokuje `documented`/`kept`; POI nie może zostać nieodwracalnie zamknięty przed minimalnym evidence setem bez alternatywy.
- **Nagrody:** knowledge/reputation/exploration credit; żadnego obowiązkowego unikalnego artefaktu potrzebnego później w main quest.
- **Save/load:** journal zachowuje provenance i rozróżnienie fakt/hipoteza; ponowne wejście nie respawnuje jednorazowego evidence.
- **Minimalne QA:** co najmniej dwie niezależne ścieżki do minimalnego evidence setu; wszystkie outcomes respektują lore gate; save/load nie zmienia hipotezy w fakt; MQ00/MQ01 nie czytają tego questa jako prerequisite.

## R0 cross-quest constraints

- `SQ_R0_01` może zmieniać lokalny traversal, ale nie może odciąć gracza od `SQ_R0_02`, `SQ_R0_03`, MQ00 ani wyjścia do MQ01.
- `SQ_R0_02` może zmienić supply, ale żaden wymagany main-quest consumable nie może istnieć wyłącznie za tym unlockiem.
- `SQ_R0_03` może wzbogacać journal, ale jego evidence nie zastępuje wymaganych dowodów `LightOverSwamp` ani MQ10+.
- Wspólne NPC reactions czytają zapisane outcomes; nie wyliczają ich ponownie z aktualnego stanu propsów.
- Wszystkie trzy questy muszą przejść test rozpoczęcia przed MQ00, podczas MQ00 i po MQ00 tam, gdzie world state na to pozwala.

## Otwarte decyzje R0

1. finalne tytuły, giver IDs, personalia i dialogi;
2. dokładne położenie brodu, kopca i punktów zbioru;
3. finalne item/reagent IDs oraz koszty/nagrody;
4. research lock formy kopca, material culture i zatwierdzonych praktyk zielarskich;
5. finalne ambient reactions i presentation/VFX;
6. które outcomes dostaną liczbowy wpływ reputacji po playtestach.

## Definition of Ready

`SQ_R0_01`–`SQ_R0_03` mają stabilne wejścia, fazy, legalne outcomes, persistence, fail-forward, dependencies i minimalne QA. Implementacja quest state machines może ruszyć bez wymyślania brakującego lore; content oznaczony research/art/dialogue/playtest lockiem pozostaje jawnie poza kartą.

## Vertical-slice compatibility note

The implemented micro side quest `side-r0-missing-tools` is an additional vertical-slice teaching quest derived from the older candidate list in `VerticalSliceSideQuests.md`. It does not consume or rename any production catalog slot.

In particular:
- `SQ_R0_01` remains **Złamany bród**;
- `SQ_R0_02` remains **Dług zielarki**;
- `SQ_R0_03` remains **Stary kopiec**.

The micro quest may coexist with all three and must never be treated as their completion flag or prerequisite.


## Runtime implementation — SQ_R0_01 Złamany bród

`SQ_R0_01` now has a playable first-pass runtime implementation.

### Discovery
The quest does not require a giver. Inspecting the damaged local ford starts the quest directly in `Investigation`, records `sq_r0_01.broken-ford`, and discovers landmark `r0-broken-ford`.

### Local traversal
The R0 river now contains two authored shallow crossing profiles:
- direct damaged ford at `z=-110`;
- alternate shallow route at `z=-72`.

Before a legal outcome, both local crossing corridors are blocked by the world traversal resolver. This is a local convenience/state change, not a campaign gate; the rest of the open world remains recoverable.

### Legal outcomes
- `Repaired`: consumes two prototype `ford-repair-timber` units and opens the direct ford;
- `Bypass`: requires field discovery of the alternate shallow route, keeps the damaged ford closed, opens the bypass and places a persistent route marker;
- `ClosedSafe`: deliberately leaves the damaged ford closed while preserving the discovered alternate route as the safe crossing.

Runtime flags:
- `sq_r0_01.outcome.repaired`;
- `sq_r0_01.outcome.bypass`;
- `sq_r0_01.outcome.closedsafe`;
- common access flag `r0_ford_access`.

### World presentation
Dynamic GLB state uses existing tracked assets:
- unresolved / bypass / closed-safe: `bridge_broken_r0_01.glb`;
- repaired: `kladka_bagienna_03.glb`;
- alternate route marker: `drogowskaz_r0_01.glb`.

Presentation reads the durable outcome; it does not infer state from transient model presence.

### Prototype rewards
- repaired: 25 Money, +6 old-village reputation;
- bypass: 15 Money, +4 reputation;
- closed-safe: 10 Money, +3 reputation.

Numbers remain playtest lock.

### Persistence and fail-forward
Quest phase/evidence, navigation landmarks, outcome/access flags, inventory cost, reputation and reward claim all use existing save contracts. Each outcome is terminal and idempotent. Repair materials are optional because both alternate-route outcomes remain available without them.
