# Main Quest Cards — Akt 0 v0.1

Pierwszy production pass kart kampanii. Szczegóły MQ00 delegują do istniejącej pełnej karty `quests/LightOverSwamp.md`; ten dokument ustala kontrakt kampanii i przejście MQ00 → MQ01 → Akt I.

## MQ00 — Światło nad mokradłem

### Metadata
- **Akt:** 0 — Zawód
- **Region:** Pogranicze Żarnowca
- **Owner:** Narrative + Quest Design
- **Status:** implementation-ready na poziomie kampanii; szczegółowy przebieg: `quests/LightOverSwamp.md`
- **Zależności:** onboarding, investigation/tracking, dialogue, combat/ritual, journal/evidence, timed events, region state

### Cel narracyjny
Pokazać bohatera jako pracującego łowcę, nie wybrańca. Gracz poznaje lokalny problem i pierwszy wzorzec identity/boundary bez wyjaśnienia Sieci Progów ani prawdy Splotu.

### Cel gameplayowy
Tutorial core loop: przyjęcie zlecenia → zebranie informacji → tropienie → ocena natury zagrożenia → wybór rozwiązania → konsekwencja społeczna i trwały state.

### Przebieg krytyczny
Pełne fazy, dowody, trzy rozwiązania, checkpointy i QA pozostają w `quests/LightOverSwamp.md`. Na poziomie kampanii wymagane są tylko dwa wyjścia:

1. zapisać rezultat lokalnego konfliktu jako trwały region state;
2. odblokować MQ01 po rozliczeniu zlecenia, niezależnie od wybranego poprawnego rozwiązania.

### Reveal budget
- **Must learn:** świat ma zjawiska, których nie da się uczciwie sprowadzić wyłącznie do „potwora do zabicia”; decyzje łowcy zostają w świecie.
- **May learn:** ślady sugerujące starszą warstwę miejsca i nietypowe zachowanie granicy.
- **Must not reveal:** nazwa/architektura Sieci Progów, rola rodziców, Wszebor jako centralna postać, prawda Czwartej Sfery.

### Persistence
MQ00 zapisuje co najmniej: outcome, kluczowe evidence flags, stan najważniejszych NPC i marker `MQ00_COMPLETE`. Rewardy i reakcje nie mogą duplikować się po save/load.

### QA acceptance
Oprócz QA pełnej karty: każde prawidłowe zakończenie musi prowadzić do MQ01; MQ01 nie może wymagać jednego konkretnego rozwiązania mokradła.

### Open decisions
Brak decyzji blokujących kontrakt kampanii. Lokalne parametry balansu pozostają tuningiem, nie kontraktem questa.

---

## MQ01 — Droga dalej

### Metadata
- **Akt:** 0 — Zawód
- **Region:** Pogranicze Żarnowca → wyjście w kierunku Nadborza
- **Owner:** Narrative + Quest Design
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ00_COMPLETE`, region state MQ00, journal, dialogue, world map/travel

### Cel narracyjny
Pokazać konsekwencje pierwszego zlecenia i dać bohaterowi zawodowy, wiarygodny powód podróży: informację o podobnym znaku w Nadborzu. Bohater jedzie sprawdzić trop; nie otrzymuje proroctwa ani „wezwania wybrańca”.

### Cel gameplayowy
Nauczyć gracza, że rezultat questa jest odczytywany przez NPC/świat, a journal i mapa prowadzą do następnego regionu bez odbierania swobody eksploracji.

### Wejście
Trigger: rozliczenie MQ00 i ustawienie `MQ00_COMPLETE`. Quest startuje przy pierwszym bezpiecznym powrocie do węzła społecznego albo przez fallback journal/map marker, jeśli gracz ominie rozmowę.

### Przebieg krytyczny
1. **Echo decyzji** — co najmniej jedna reakcja NPC odczytuje outcome MQ00. State write: `MQ01_CONSEQUENCE_SEEN`.
2. **Drugi ślad** — bohater otrzymuje wiarygodną informację o podobnym znaku/anomalii przy trasie w Nadborzu. Źródło może zależeć od outcome, lecz informacja krytyczna nie może zostać utracona. State write: `MQ01_NADBORZE_LEAD`.
3. **Ocena tropu** — journal oddziela fakt (podobny znak) od hipotezy (możliwy związek). Bez revealowania Sieci Progów.
4. **Przygotowanie drogi** — mapa odblokowuje cel przejścia do Nadborza; gracz może zakończyć lokalne side-content przed wyjazdem.
5. **Wyjazd** — przekroczenie travel boundary zapisuje `MQ01_COMPLETE` i odblokowuje MQ10.

### Ścieżki opcjonalne
- dodatkowa rozmowa z NPC zależnym od wyniku MQ00;
- ponowne obejrzenie miejsca zlecenia dla flavor/evidence;
- zakupy/przygotowanie ekwipunku;
- lokalne side questy.

Żadna ścieżka opcjonalna nie jest wymagana do uzyskania tropu Nadborza.

### Decyzje i konsekwencje
MQ01 nie wprowadza nowej wielkiej decyzji kampanii. Odczytuje wcześniejszy `MQ00 outcome`; ton reakcji, ceny/usługi lub drobna reputacja mogą się różnić, ale MQ10 pozostaje dostępny.

### Reveal budget
- **Must learn:** podobny motyw wystąpił poza lokalnym problemem; Nadborze jest następnym sensownym tropem.
- **May learn:** informacja jest starsza niż bieżące plotki i może wiązać się z dawną trasą.
- **Must not reveal:** termin „Sieć Progów”, globalny rozmiar systemu, autorzy Sieci, rodzinna prawda, Splot.

### NPC i fallback
Informację może przekazać lokalny kontakt/posłaniec/źródło dokumentowe zgodne z finalnym rosterem. **Invariant implementacyjny:** krytyczny lead nie jest związany z przeżyciem jednego opcjonalnego NPC. Jeśli podstawowy rozmówca jest niedostępny, journal otrzymuje lead przez alternatywne źródło diegetyczne.

### Rewards / costs
- odblokowanie celu podróży do Nadborza;
- wpis journal/evidence;
- reakcja reputacyjna wynikająca z MQ00;
- brak obowiązkowego power rewardu.

### Save/load i persistence
Checkpointy po: reakcji, uzyskaniu leadu i przekroczeniu granicy regionu. `MQ01_NADBORZE_LEAD` jest idempotentny; ponowne rozmowy nie tworzą duplikatów journal/rewardów.

### Fail states i recovery
Brak twardego faila. Pominięta rozmowa, nieobecny NPC albo szybkie opuszczenie huba uruchamia fallback leadu. Gracz nie może zablokować kampanii przez wynik MQ00.

### Integracje systemowe
Quest wymaga istniejących kontraktów: dialogue, journal, map/travel, persistence i region/world state. Nie wymaga jeszcze finalnej implementacji polityki Nadborza.

### Asset / content hooks
- warianty krótkiej reakcji dla outcome MQ00;
- źródło leadu + fallback;
- journal entry „podobny znak w Nadborzu”;
- map/travel marker;
- krótki departure beat bez obowiązkowego cinematica.

### QA acceptance
- wszystkie poprawne outcome MQ00 odblokowują MQ01;
- lead działa przy niedostępnym podstawowym NPC;
- save/load po każdym checkpointcie nie duplikuje reakcji ani wpisów;
- MQ10 odblokowuje się tylko raz po wyjeździe;
- side-content można wykonać przed wyjazdem;
- journal nie zdradza terminologii z późniejszych aktów.

### Open decisions
- finalny NPC/medium przekazujący lead — **Narrative**, nie blokuje logiki dzięki fallbackowi;
- dokładny travel presentation do Nadborza — **World/UX**, nie blokuje kontraktu questa.
