# Side Quest Production Cards — R4 Kamienne Wyżyny v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R4_01`–`SQ_R4_03` z `SideQuestCatalog.md`. Tytuły są robocze F. Dokument nie rozstrzyga finalnych praktyk górniczych/obróbki materiałów, creature/anomaly targetu, dialogów, reward values ani encounter composition.

## Wspólny kontrakt R4

Każdy quest zapisuje `quest_id`, fazę, cele, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Mutacje świata i nagrody są idempotentne. Szczegóły narzędzi, wydobycia, transportu rudy i obróbki metalu muszą pochodzić z material-culture research package; do czasu research locku implementacja używa neutralnych slotów danych, nie pseudohistorycznych szczegółów.

## SQ_R4_01 — Zawalona ścieżka

- **Typ / funkcja:** wspólnota; traversal, infrastruktura i trwała zmiana dostępu do POI/usługi.
- **Wejście:** dostęp do R4; brak wymaganego wcześniejszego side questu.
- **Trigger:** odkrycie zablokowanego przejścia albo lead od lokalnego NPC/usługi.
- **Fazy:** `Offered/Active` → inspekcja blokady → wybór sposobu obejścia/usunięcia → wykonanie → walidacja przejścia → `Resolved/TurnedIn`.
- **Cele:** przywrócić bezpieczne przejście albo ustanowić trwałe obejście bez wymuszania jednej technologii górniczej.
- **Legalne outcomes:** `cleared`, `bypass_built`, `marked_unsafe`. Ostatni wynik zachowuje bezpieczną alternatywę i nie może blokować MQ.
- **Stan trwały:** `sq_r4_01_outcome`; `r4_blocked_path_state`; `r4_path_service_access`.
- **World-state:** przełączenie wariantu kolizji/nawigacji i dostępności lokalnego POI/usługi; terminalny wariant odtwarza się deterministycznie.
- **Dependencies:** R4 bible, traversal/navigation, persistence, `ASSETSET_R4_CORE`; konkretne techniki i narzędzia są research lockiem.
- **Fail-forward:** brak givera nie blokuje terenowego rozwiązania; brak finalnego assetu naprawy może użyć trwałego obejścia zamiast placeholderowej technologii.
- **Nagrody:** dostęp, reputacja/usługa lub resource access; liczby pozostają tuning lockiem.
- **Save/load:** stan blokady, wybrana metoda i jednorazowa nagroda nie resetują się.
- **Minimalne QA:** trzy outcomes; navmesh/collision zgodne po reload; brak softlocku głównej trasy; brak niezatwierdzonych szczegółów material culture.

## SQ_R4_02 — Echo w szybie

- **Typ / funkcja:** kontrakt łowcy; investigation w podziemnym POI → preparation → encounter/interwencja.
- **Wejście:** dostęp do R4, tracking/evidence i encounter framework.
- **Trigger:** zgłoszenie powtarzalnych dźwięków, zniknięć lub niebezpiecznego zachowania w podziemnym miejscu.
- **Fazy:** `Offered/Active` → oględziny wejścia → evidence set → hipoteza → przygotowanie → zejście → encounter/interwencja → `Resolved/TurnedIn`.
- **Creature/anomaly lock:** `r4_shaft_contract_target` jest data-driven. Może wskazać zatwierdzoną istotę, naturalne zagrożenie albo jawny F/anomaly package po content locku. Karta nie przypisuje celu samodzielnie.
- **Evidence contract:** minimum dwa niezależne ślady i obserwacja środowiskowa; finalny target package określa sensory, reakcje i kontrdowody.
- **Legalne rozwiązania:** zabezpieczenie/ominięcie zagrożenia; odstraszenie/odprowadzenie; neutralizacja, jeśli target package ją dopuszcza; udokumentowanie i zamknięcie strefy jako `documented_unresolved`.
- **Stan trwały:** `sq_r4_02_outcome = secured|nonlethal|lethal|documented_unresolved`; `r4_shaft_state`; `r4_shaft_target_id`; evidence IDs.
- **World-state:** wariant dostępu/hazardu podziemnego POI i reakcje lokalnych NPC; nie zmienia central lore.
- **Dependencies:** R4 bible, production bestiary, TrackingSystem/evidence, encounter format, persistence, material-culture package dla finalnej scenografii i praktyk pracy.
- **Fail-forward:** utrata givera po aktywacji pozostawia terenowy quest; brak finalnego targetu blokuje tylko content lock, nie implementację state machine; `documented_unresolved` jest legalnym terminalnym wynikiem.
- **Nagrody:** hunter progression/reputation/material/knowledge zależnie od outcome; wartości pozostają playtest lockiem.
- **Save/load:** target ID, evidence, preparation i shaft state są trwałe; terminalny encounter nie respawnuje jako nierozwiązany.
- **Minimalne QA:** co najmniej jedna ścieżka non-combat; reload przed/po zejściu i encounter; target wymienialny przez dane; brak fałszywego lore/research claimu.

## SQ_R4_03 — Kamień dla kowala

- **Typ / funkcja:** osobisty NPC; equipment/crafting, relacja i wybór nagrody bez obowiązkowego power spike.
- **Wejście:** dostęp do R4 oraz aktywna lokalna usługa kowalska albo jej fallback service slot.
- **Trigger:** rozmowa o potrzebnym materiale lub znalezienie odpowiedniego leadu podczas eksploracji.
- **Fazy:** `Offered/Active` → ustalenie wymagań → lokalizacja źródła → pozyskanie/alternatywa → dostarczenie albo zachowanie materiału → `Resolved/TurnedIn`.
- **Material lock:** `r4_smith_material_id` i metoda pozyskania są data-driven. Finalna nazwa materiału, geologia, narzędzia i proces obróbki wymagają material-culture/geology research locku; karta nie udaje wiedzy historycznej.
- **Legalne outcomes:** `delivered`, `substitute_delivered`, `knowledge_shared`, `kept`. Każdy outcome może zmienić relację/usługę, ale żaden nie jest wymagany do ukończenia kampanii.
- **Stan trwały:** `sq_r4_03_outcome`; `r4_smith_material_id`; `r4_smith_service_variant`; jednorazowy reward flag.
- **World-state:** wariant usługi/dialogu i opcjonalny display/recipe unlock po art/content locku.
- **Dependencies:** R4 bible, EquipmentSystem/crafting, economy/vendors, NPC lifecycle, persistence, material-culture research.
- **Fail-forward:** jeśli kowal staje się niedostępny po aktywacji, fallback service/NPC lub terminalny `knowledge_shared` pozwala zamknąć quest bez utraty unikalnego krytycznego przedmiotu.
- **Nagrody:** usługa, wiedza/recipe, relacja lub gear option; finalne statystyki i ceny pozostają tuning lockiem.
- **Save/load:** materiał nie duplikuje się, reward jest idempotentny, service variant odtwarza się po reload.
- **Minimalne QA:** wszystkie legalne outcomes stabilne; fallback po utracie NPC; brak duplikacji materiału/nagrody; brak hardcoded niezatwierdzonej technologii lub surowca.

## Zależności i locki R4

1. Szczegóły górnictwa, geologii użytkowej, narzędzi, transportu i obróbki pozostają material-culture/research lockiem.
2. `SQ_R4_02` wymaga finalnego `r4_shaft_contract_target` przed content lockiem, lecz state machine i persistence są implementowalne wcześniej.
3. `SQ_R4_03` wymaga finalnego `r4_smith_material_id`; placeholder danych nie może zostać przedstawiony jako historyczny fakt.
4. Finalne dialogi, reward values, evidence thresholds, encounter composition, POI coordinates i asset manifests pozostają odpowiednio dialogue/playtest/content/art lockiem.

## Definition of Ready

R4 jest gotowe do implementacji first-pass, gdy trzy state machines używają stabilnych ID/persistence keys, traversal po `SQ_R4_01` jest deterministyczny, target `SQ_R4_02` i materiał `SQ_R4_03` są wymienialne przez dane, a fixtures/testy nie utrwalają nierozstrzygniętych szczegółów research jako canon.