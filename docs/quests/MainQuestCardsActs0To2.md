# Main Quest Cards — Acts 0–II v0.1

Production pass dla MQ00–MQ25. Dokument uszczegóławia `story/MainQuestSkeleton.md`; centralne lore pozostaje własnością dokumentów `story/` i `world/`. Nazwy questów są robocze F.

## Wspólny kontrakt implementacyjny

Każdy quest zapisuje `quest_id`, `phase`, ukończone cele, decyzje, evidence IDs, region-state deltas i checkpoint. Reload nie może ponownie przyznawać nagród ani odpalać jednorazowych scen. Krytyczny postęp ma mieć co najmniej jedną ścieżkę odporną na utratę opcjonalnego NPC/przedmiotu. Fast travel, sen i zmiana regionu nie resetują śledztwa. Quest journal pokazuje cel gracza, nie author truth.

## MQ00 — Światło nad mokradłem

- **Region:** R0 Żarnowiec / Czarne Mokradła.
- **Owner szczegółów:** `quests/LightOverSwamp.md`.
- **Wejście:** nowa gra po onboardingowym wejściu do świata.
- **Fazy:** przyjęcie sprawy → oględziny → tropienie/evidence → konfrontacja → wybór rozwiązania → reakcje.
- **Core systems:** movement, interaction, tracking, dialogue, combat/avoidance, inventory, journal, save/load.
- **Wyjście:** zapisany wynik sprawy i pierwsze flagi reputacji; MQ01 odblokowany niezależnie od jednego z trzech legalnych rozwiązań.
- **QA:** wszystkie rozwiązania muszą przeżyć save/load przed i po konfrontacji; brak softlocku po utracie opcjonalnego evidence.

## MQ01 — Droga dalej

- **Cel:** pokazać konsekwencje MQ00 i skierować gracza ku Nadborzu bez motywu „wybrańca”.
- **Wejście:** `MQ00=complete`.
- **Fazy:** reakcje Żarnowca → informacja o podobnym znaku → przygotowanie podróży → opuszczenie R0.
- **Decyzje:** komu gracz ujawnia szczegóły sprawy; czy bierze rekomendację/kontrakt na drogę.
- **Stan:** `r0_departure_context`, `nadborze_lead_known`; wariant MQ00 modyfikuje dialog, nie blokuje wyjazdu.
- **Zależności:** reaction matrix MQ00, world map, economy/vendor, travel/region transition.
- **QA:** każdy wynik MQ00 prowadzi do poprawnego wariantu; reload podczas przejścia regionu nie duplikuje zasobów.

# Akt I — Wzór

## MQ10 — Znak pod drogą

- **Region:** R1 Nadborze.
- **Cel:** zbadać anomalię przy ważnej trasie i starym węźle.
- **Fazy:** zgłoszenie → inspekcja trasy → dwa niezależne źródła evidence → wejście do starego węzła → interpretacja.
- **Odkrycie:** znak jest starszy niż obecny kult miejsca.
- **Stan:** `r1_old_node_confirmed`, evidence provenance; opcjonalna reputacja kupców/władzy.
- **Systems:** tracking, encounter, time/weather, evidence journal.
- **Fail-forward:** zniszczony ślad terenowy można zastąpić relacją/artefaktem; kosztuje czas lub reputację.

## MQ11 — Prawo łowcy

- **Region:** R1, hub Dębrzyn.
- **Cel:** wprowadzić politykę kryzysową i status łowców.
- **Fazy:** wezwanie → przesłuchanie/negocjacja → demonstracja kompetencji lub poręczenie → decyzja prawna.
- **Decyzje:** współpraca, ograniczona zgoda albo jawny sprzeciw; żadna ścieżka nie zamyka kampanii.
- **Stan:** `hunter_legal_status`, `debrzyn_authority_rep`, modyfikatory cen/dostępu.
- **QA:** vendor, straż i dialogi konsumują tę samą flagę; brak sprzecznych reakcji po reloadzie.

## MQ12 — Las, który myli drogę

- **Region:** R2 Wielki Bór.
- **Cel:** przejść przez zaburzony układ ścieżek i ustalić relację lokalnych istot ze starym węzłem.
- **Fazy:** wejście → utrata pewnej nawigacji → lokalne tropy → kontakt/konflikt → stabilizacja drogi.
- **Odkrycie:** część miejsc uznawanych za święte pełniła funkcję Sieci.
- **Stan:** `r2_node_function_known`, `forest_relation_state`.
- **Systems:** map uncertainty, tracking, AI/encounter, ritual/magic, day-night.
- **QA:** quest pozostaje czytelny bez minimapy; alternatywne rozwiązania istoty nie niszczą krytycznego evidence.

## MQ13 — Dwie mapy

- **Cel:** złożyć dane Nadborza i Wielkiego Boru w pierwszą hipotezę Sieci Progów.
- **Fazy:** zebranie materiałów → porównanie → wskazanie korelacji → weryfikacja w terenie/archiwum → hipoteza.
- **Wejście:** MQ10 i MQ12 complete; MQ11 wpływa na dostęp, ale nie jest twardym blockerem.
- **Stan:** `threshold_network_hypothesis=true`, `act1_complete`.
- **Wyjście:** lead do Przymorza i Aktu II.
- **QA:** brak wymagania zebrania 100% opcjonalnych notatek; journal rozróżnia fakt od hipotezy.

# Akt II — Interesy

## MQ20 — Sól i milczenie

- **Region:** R3 Przymorze.
- **Cel:** odblokować śledztwo wokół ujścia i starej mapy ukrywanej przez kupców.
- **Fazy:** blokada → rozpoznanie interesów → zdobycie dostępu → mapa → konsekwencja.
- **Ścieżki:** negocjacja, usługa, infiltracja zgodna z designem; przemoc nie może być jedyną drogą.
- **Odkrycie:** punkty Sieci występują daleko poza Nadborzem.
- **Stan:** `r3_network_extent_known`, `league_relation`.

## MQ21 — Cena przejścia

- **Region:** R3.
- **Cel:** rozstrzygnąć dostęp do danych i szlaku w konflikcie Liga–Nadborze.
- **Decyzja:** preferowany układ handlowy/polityczny albo kosztowny kompromis.
- **Stan:** `r3_route_access`, `league_nadborze_outcome`; wpływa na logistykę MQ50, nie na możliwość ukończenia gry.
- **Systems:** reputation, economy, dialogue, world-state services.
- **QA:** ceny/usługi i późniejsze reakcje wynikają z jednej zapisanej decyzji.

## MQ22 — Kamień pod kamieniem

- **Region:** R4 Kamienne Wyżyny.
- **Cel:** zbadać kopalnię przecinającą starą strukturę.
- **Fazy:** wejście do konfliktu → eksploracja kopalni → zagrożenie → identyfikacja materiału → decyzja o zabezpieczeniu.
- **Odkrycie:** kotwice Sieci mają materialny komponent powiązany z regionem.
- **Stan:** `anchor_material_known`, `mine_state`.
- **Systems:** traversal, hazard, combat, interactables, material evidence.

## MQ23 — Żelazna Brama

- **Region:** R4.
- **Cel:** rozwiązać konflikt księstw o materiał potrzebny do stabilizacji.
- **Decyzje:** kontrola jednego ośrodka, podział, neutralny depozyt lub inny wariant przewidziany przez polityczne state machine.
- **Stan:** `r4_material_control`, `r4_finale_support`.
- **Fail-forward:** utrata poparcia zmniejsza późniejsze zasoby, ale nie blokuje MQ24/25 ani finału.
- **QA:** MQ50 musi móc odczytać wynik bez reinterpretacji.

## MQ24 — Droga bez granicy

- **Region:** R5 Równiny Arel.
- **Cel:** poznać pamięć dawnych tras zachowaną poza mapami państw osiadłych.
- **Fazy:** kontakt → próba podróży/nawigacji → zebranie przekazu → porównanie z mapami → zapis relacji tras.
- **Odkrycie:** dane Arel uzupełniają brakujące połączenia Sieci.
- **Stan:** `arel_route_memory_known`, `r5_relation`.
- **Zasada lore:** wiedza Arel nie jest przedstawiana jako „prymitywna wersja” kartografii; jest innym systemem pamięci i orientacji.

## MQ25 — Archiwum bez jednego języka

- **Cel:** połączyć dane portowe, górskie, Arel i nadborskie.
- **Wejście:** MQ20, MQ22, MQ24 complete; MQ21/23 modyfikują warunki dostępu i wsparcie.
- **Fazy:** zgromadzenie źródeł → normalizacja znaków/języków → sprzeczność → rozwiązanie korelacji → wniosek.
- **Odkrycie:** Sieć była przedsięwzięciem więcej niż jednej kultury.
- **Stan:** `network_multicultural_origin_known`, `act2_complete`, lead do R6.
- **QA:** każdy wymagany pakiet danych ma jawny provenance; UI nie ujawnia author truth ponad stan wiedzy gracza.

## Wspólne checkpointy Acts 0–II

Minimalne checkpointy: wejście do regionu, rozpoczęcie krytycznej lokacji, przed nieodwracalną decyzją i po jej zapisie. Autosave po decyzji następuje dopiero po atomowym zapisaniu quest state + world state. Ponowne wczytanie nie może ponownie odpalić nagrody, reputation delta ani jednorazowego encounteru.

## Otwarte decyzje — jawnie niezamknięte

- dokładne nazwy większości questów (F);
- finalne liczby nagród, cen, reputation deltas i progów evidence — balance/playtest lock;
- dokładne layouty lokacji i liczba encounterów — level/art/performance lock;
- finalne dialogi i VO — narrative/VO lock;
- konkretna postać/instytucja przekazująca część leadów, jeśli roster regionu nie jest jeszcze finalny;
- dokładne warianty polityczne MQ21/MQ23 wymagają finalnego faction-state package;
- historyczno-kulturowe detale propsów/stroju/rytuałów podlegają `ResearchPolicy.md` i nie mogą być dopowiadane bez research locku.

Te otwarte punkty nie blokują implementacji quest state machine, persistence, objective graph, region transitions ani podstawowego content pipeline.