# Main Quest Cards — Akt V: Decyzja

Production pass v0.1 dla MQ50–MQ56. Dokument uszczegóławia `story/MainQuestSkeleton.md`; prawda świata, endingi i epilogi pozostają własnością odpowiednich dokumentów story. Nazwy są robocze F.

## Wspólne zasady Aktu V

- Akt zaczyna się dopiero po `MQ44_COMPLETE`.
- Wszystkie konsekwencje wcześniejszych regionów są odczytywane z trwałego stanu; finał nie przepisuje historii gracza na domyślną ścieżkę.
- Brak opcjonalnego sojusznika, materiału albo kontraktu może podnieść koszt lub zmienić wariant sceny, ale nie może sam w sobie zablokować ukończenia kampanii.
- MQ50–MQ53 przygotowują i wykonują wejście do decyzji. MQ54 zapisuje architekturę E1–E5. MQ55 rozstrzyga koszty osobiste i kontrakty. MQ56 wyłącznie konsumuje zapisany stan i buduje epilog.
- Save/load musi zachowywać każdy zapisany wybór oraz etap finału bez ponownego przyznawania zasobów, wsparcia lub endingów.

---

## MQ50 — Stare zobowiązania

### Funkcja
Zebrać przed finałem ludzi, materiały, wiedzę i warunki boskie wynikające z wcześniejszej kampanii, bez zamiany side contentu w obowiązkowy grind.

### Wejście
- `MQ44_COMPLETE`.

### Krytyczne beaty
1. Bohater ocenia stan regionów po powrocie z Nawii/Splotu.
2. Journal tworzy listę kategorii przygotowania: `people`, `materials`, `knowledge`, `divine_terms`.
3. Każda kategoria ma co najmniej jedną ścieżkę bazową wynikającą z main questu oraz opcjonalne wzmocnienia z wcześniejszych decyzji.
4. Gracz potwierdza gotowość do marszu na Pierwszy Próg.

### Trwały stan
- `MQ50_STARTED`;
- `MQ50_PREPARATION_SNAPSHOT_CREATED`;
- `MQ50_PEOPLE_TIER`;
- `MQ50_MATERIALS_TIER`;
- `MQ50_KNOWLEDGE_TIER`;
- `MQ50_DIVINE_TERMS_TIER`;
- `MQ50_COMPLETE`.

Tier jest wynikiem istniejącego stanu świata, nie nową oceną moralną.

### Recovery
- śmierć lub niedostępność opcjonalnego NPC usuwa tylko jego wkład;
- utracony opcjonalny przedmiot nie blokuje bazowej ścieżki przygotowania;
- snapshot po utworzeniu jest idempotentny.

### QA / DoR
- każda kategoria ma ścieżkę ukończenia bez side questów;
- reload nie dubluje wsparcia;
- MQ51 odblokowuje się dokładnie raz po `MQ50_COMPLETE`.

---

## MQ51 — Pierwszy Próg

### Funkcja
Doprowadzić gracza do finałowej lokacji i zamknąć możliwość przypadkowego wejścia przed ukończeniem przygotowań.

### Wejście
- `MQ50_COMPLETE`.

### Krytyczne beaty
1. Podejście do strefy Pierwszego Progu reaguje na stan polityczny i zgromadzone wsparcie.
2. Gracz przechodzi ostatni traversal/encounter wejściowy.
3. Finałowa przestrzeń zostaje rozpoznana jako węzeł zdolny do przeprowadzenia jednej z architektur zakończenia.
4. Tworzony jest checkpoint przed konfrontacją MQ52.

### Trwały stan
- `MQ51_APPROACH_STARTED`;
- `MQ51_THRESHOLD_REACHED`;
- `MQ51_FINAL_SITE_UNLOCKED`;
- `MQ51_COMPLETE`.

### Recovery
- wariant wejścia nie może wymagać konkretnej frakcji;
- brak wysokiego tieru wsparcia zwiększa presję encounteru zamiast tworzyć softlock.

### QA / DoR
- wejście przed MQ50 jest odrzucone czytelnym stanem Journal;
- checkpoint odtwarza poprawny wariant region state;
- ukończenie odblokowuje MQ52 raz.

---

## MQ52 — Wszebor / Straż

### Funkcja
Rozliczyć ludzi i frakcje walczące o kontrolę nad rozwiązaniem kryzysu, zanim gracz zacznie techniczną stabilizację Sieci.

### Wejście
- `MQ51_COMPLETE`.

### Warianty
Na podstawie historii dostępne są: bitwa, negocjacja, sojusz albo konflikt wielostronny. Żaden wariant nie jest kanonicznym domyślnym wynikiem.

### Krytyczne beaty
1. System oblicza pozycje Wszebora, Straży i obecnych sojuszników z wcześniejszych flag.
2. Gracz dostaje uczciwy obraz bez cofania reveal z Aktów III–IV.
3. Konfrontacja rozstrzyga, kto pomaga, kto przeszkadza i jakie zasoby pozostają na MQ53.
4. Każda legalna ścieżka kończy się dostępem do węzłów.

### Trwały stan
- `MQ52_CONFIGURATION_LOCKED`;
- `MQ52_RESOLUTION = battle | negotiation | alliance | multi_conflict`;
- `MQ52_WSZEBO_R_STATE`;
- `MQ52_GUARD_STATE`;
- `MQ52_FINAL_SUPPORT_STATE`;
- `MQ52_COMPLETE`.

### Recovery
- konfiguracja po rozpoczęciu nie jest ponownie losowana po reloadzie;
- brak Wszebora w danej ścieżce nie blokuje informacji koniecznej do MQ53.

### QA / DoR
- wszystkie legalne kombinacje region/faction state prowadzą dalej;
- reload w połowie wariantu zachowuje wybrany wariant;
- MQ53 odblokowuje się dokładnie raz.

---

## MQ53 — Węzły

### Funkcja
Gameplayowy finał systemów: stabilizacja, obrona i aktywacja znaków przed wyborem architektury.

### Wejście
- `MQ52_COMPLETE`.

### Krytyczne beaty
1. Gracz identyfikuje zestaw krytycznych węzłów.
2. Każdy węzeł wymaga stabilizacji/aktywacji, a presja encounteru zależy od MQ50 i MQ52.
3. Utrata opcjonalnego wsparcia zwiększa koszt, lecz nie usuwa minimalnego narzędzia do aktywacji.
4. Po zabezpieczeniu wymaganych węzłów system przechodzi do stabilnego stanu decyzji.

### Trwały stan
- `MQ53_STARTED`;
- `MQ53_NODE_<ID>_STABLE` dla krytycznych węzłów;
- `MQ53_REQUIRED_NODES_STABLE`;
- `MQ53_COMPLETE`.

### Persistence
Stan każdego węzła jest zapisywany osobno. Reload nie resetuje ukończonego węzła ani nie przyznaje ponownie nagrody/zasobu aktywacyjnego.

### QA / DoR
- kolejność węzłów, jeśli nie jest fabularnie wymuszona, nie zmienia legalności finału;
- wszystkie wymagane węzły muszą być jawnie policzalne;
- po komplecie odblokowuje się MQ54.

---

## MQ54 — Architektura

### Funkcja
Właściwy wybór E1–E5 opisanych w dokumentacji zakończeń.

### Wejście
- `MQ53_COMPLETE`;
- gracz posiada wiedzę konieczną do świadomego wyboru z main questu; side content może rozszerzać kontekst, nie podstawową czytelność.

### Krytyczne beaty
1. UI/Journal przedstawia dostępne architektury i ich znane koszty bez fałszywego „dobrego” oznaczenia.
2. Dostępność wariantu wynika wyłącznie z reguł jego dokumentu ownera.
3. Gracz potwierdza jeden legalny wybór E1–E5.
4. Wybór zostaje zapisany przed MQ55 i nie jest zmieniany przez późniejsze koszty osobiste.

### Trwały stan
- `MQ54_OPTIONS_EVALUATED`;
- `ENDING_ARCHITECTURE = E1 | E2 | E3 | E4 | E5`;
- `MQ54_COMPLETE`.

### Granica własności
Ten dokument nie redefiniuje znaczenia E1–E5. Semantyka, warunki i skutki należą do `story/EndingVariants.md`, `story/Finale.md` i `story/EpilogueMatrix.md`.

### QA / DoR
- nielegalny wariant nie może zostać zapisany przez UI ani load manipulation;
- save po wyborze odtwarza identyczną architekturę;
- MQ55 nie może samoczynnie zamienić E1–E5.

---

## MQ55 — To, co zostaje

### Funkcja
Rozliczyć osobisty koszt finału: Parent B, boskie kontrakty i ostatnie zobowiązania, już po wyborze architektury świata.

### Wejście
- `MQ54_COMPLETE`;
- `ENDING_ARCHITECTURE` ustawione.

### Krytyczne beaty
1. Gra materializuje konsekwencje wybranej architektury dla dostępnych relacji i kontraktów.
2. Jeśli Parent B jest obecny w tej ścieżce, gracz podejmuje przewidzianą decyzję; jeśli nie — quest używa jawnego wariantu `not_present`, bez sztucznego zastępstwa.
3. Aktywne boskie kontrakty otrzymują finalne rozliczenie zgodne z ich owner spec.
4. Ostatni koszt zostaje zatwierdzony i zamyka interaktywny finał.

### Trwały stan
- `MQ55_PARENT_B_RESOLUTION`;
- `MQ55_DIVINE_CONTRACT_RESOLUTION`;
- `MQ55_FINAL_COST_LOCKED`;
- `MQ55_COMPLETE`.

### Recovery
- pominięcie MQ42 nie tworzy softlocka;
- brak kontraktu jest legalnym stanem;
- rozliczenia są idempotentne po reloadzie.

### QA / DoR
- testy z Parent B present/absent;
- testy z 0/1/wieloma aktywnymi kontraktami;
- MQ56 odblokowuje się dokładnie raz.

---

## MQ56 — Epilog

### Funkcja
Zbudować końcowy montaż/sceny wyłącznie z utrwalonych konsekwencji kampanii.

### Wejście
- `MQ55_COMPLETE`.

### Krytyczne beaty
1. System zamraża `EpilogueSnapshot` z ending architecture, region states, kluczowych NPC, kontraktów i decyzji osobistych.
2. `story/EpilogueMatrix.md` wybiera legalne moduły epilogu.
3. Moduły są prezentowane w deterministycznej kolejności narracyjnej.
4. Po zakończeniu zapis otrzymuje flagę ukończenia kampanii.

### Trwały stan
- `MQ56_EPILOGUE_SNAPSHOT_CREATED`;
- `MQ56_MODULES_LOCKED`;
- `CAMPAIGN_COMPLETE`;
- `MQ56_COMPLETE`.

### Persistence
Snapshot jest niezmienny dla danego ukończenia. Reload epilogu nie może ponownie wykonywać efektów świata ani przeliczać modułów na podstawie później zmienionych danych runtime.

### QA / DoR
- test co najmniej jednego legalnego przebiegu dla E1–E5;
- test regionów/NPC o przeciwnych stanach;
- test reloadu przed, w trakcie i po montażu;
- brak brakującego opcjonalnego modułu nie może przerwać epilogu.

---

## Definition of Ready — Akt V

Akt V jest gotowy do implementacji kontraktów questowych, gdy:
- owner specs E1–E5 i epilogue matrix pozostają spójne z powyższym handoffem;
- trwałe ID zostaną zweryfikowane względem `content/IdConventions.md` przy implementacji;
- system questów potrafi przechować enum/string state oraz per-node flags;
- każdy quest ma regresję gating + save/load + idempotencja;
- żaden optional content nie jest jedyną ścieżką do `CAMPAIGN_COMPLETE`.

## Otwarte decyzje produkcyjne

Nie blokują kontraktu questowego, ale muszą zostać zamknięte przed content lock:
- dokładna liczba i geometria krytycznych węzłów MQ53;
- konkretne encountery i pacing MQ51–MQ53 po playtestach;
- finalna prezentacja UI wyboru E1–E5;
- forma montażu MQ56 (in-engine, sekwencje mieszane, inne rozwiązanie);
- finalne voice/cinematic budgets.
