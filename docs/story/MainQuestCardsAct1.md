# Main Quest Cards — Akt I v0.1

Production pass dla MQ10–MQ13. Karty uszczegóławiają `MainQuestSkeleton.md` bez zmiany centralnego lore. Obowiązuje kontrakt `MainQuestCardTemplate.md`.

## MQ10 — Znak pod drogą

### Metadata
- **Akt:** I — Wzór
- **Region:** Nadborze
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ01_COMPLETE`, tracking/investigation, journal/evidence, region state, persistence

### Cel narracyjny i gameplayowy
Pierwszy trop poza Pograniczem ma potwierdzić, że podobieństwo z MQ00 nie jest przypadkiem. Gracz bada anomalię przy ważnej trasie i starym węźle, używając śledztwa, obserwacji środowiska i rozmów zamiast otrzymywać gotową odpowiedź.

### Przebieg krytyczny
1. **Zakłócenie trasy** — wejście w obszar zapisuje `MQ10_SITE_FOUND`; gracz poznaje lokalny problem.
2. **Warstwy miejsca** — minimum dwa niezależne dowody wskazują, że znaki są starsze niż obecny kult. Zapis `MQ10_OLD_LAYER_CONFIRMED`.
3. **Porównanie** — journal zestawia cechy z MQ00 bez nazywania Sieci Progów. Zapis `MQ10_PATTERN_02`.
4. **Zabezpieczenie drogi** — gracz rozwiązuje bieżące zagrożenie przez wariant zgodny z finalnym encounterem; rozwiązanie nie może zniszczyć krytycznego dowodu.
5. **Wyjście** — `MQ10_COMPLETE`; odblokowanie tropu prowadzącego do Dębrzyna/MQ11.

### Reveal budget
- **Must learn:** znak i funkcja miejsca są starsze niż aktualna praktyka religijna.
- **May learn:** węzeł był kiedyś częścią infrastruktury trasy.
- **Must not reveal:** nazwa Sieci, jej pełny zasięg, twórcy, Splot.

### Persistence / recovery / QA
Dowody i wynik zabezpieczenia są idempotentne. Utrata opcjonalnego NPC nie blokuje porównania: krytyczne dane mają co najmniej jedno źródło środowiskowe. Testy obejmują wejście przed quest markerem, zebranie dowodów w odwrotnej kolejności i save/load po każdym dowodzie.

---

## MQ11 — Prawo łowcy

### Metadata
- **Akt:** I — Wzór
- **Region:** Dębrzyn
- **Status:** implementation-ready v0.1
- **Zależności:** MQ10, dialogue, reputation, faction/world state, journal, persistence

### Cel narracyjny i gameplayowy
Wprowadzić duży hub i konflikt polityczny bez sprowadzania go do jednej poprawnej frakcji. Kryzys daje władzom argument za kontrolą łowców; bohater musi określić praktyczne warunki współpracy.

### Przebieg krytyczny
1. **Wezwanie do rejestru** — wejście do huba ujawnia nowe przepisy; `MQ11_POLICY_SEEN`.
2. **Głosy stron** — gracz poznaje stanowisko administracji oraz co najmniej jedną perspektywę łowców/lokalnej społeczności.
3. **Przypadek testowy** — małe zadanie lub dowód pokazuje koszt jednej z procedur w praktyce.
4. **Stanowisko** — decyzja `MQ11-D01`: współpraca warunkowa / sprzeciw proceduralny / obejście przez lokalne układy. Żadna opcja nie zamyka kampanii.
5. **Konsekwencja** — zapis reputacji i `MQ11_COMPLETE`; informacja kieruje do Wielkiego Boru.

### Konsekwencje
`MQ11-D01` wpływa na dostęp, ceny/usługi, pomoc urzędową i późniejsze odczyty reputacji. Nie może usuwać krytycznych danych MQ12–MQ13.

### Reveal budget
- **Must learn:** kryzys zmienia instytucje i status zawodu łowcy.
- **May learn:** władza zbiera raporty o podobnych anomaliach.
- **Must not reveal:** centralna architektura Sieci ani motywy Wszebora.

### Persistence / recovery / QA
Decyzja ma trwałe ID i jest zapisywana przed reakcjami świata. Testy: wszystkie trzy opcje, wrogi/niski reputation state, save/load przed i po decyzji oraz dostępność MQ12 w każdym wyniku.

---

## MQ12 — Las, który myli drogę

### Metadata
- **Akt:** I — Wzór
- **Region:** Wielki Bór
- **Status:** implementation-ready v0.1
- **Zależności:** MQ11, tracking, navigation/map, forest encounter state, timed events, journal/evidence

### Cel narracyjny i gameplayowy
Pokazać, że anomalia wpływa nie tylko na pojedyncze miejsce, lecz także na orientację i zachowanie lokalnych istot. Gracz odkrywa, że część miejsc uznawanych dziś za święte pełniła wcześniej dodatkową funkcję.

### Przebieg krytyczny
1. **Błędna ścieżka** — kontrolowane zaburzenie nawigacji zapisuje `MQ12_ROUTE_ANOMALY`.
2. **Trzy punkty odniesienia** — gracz odnajduje trzy ślady; do progresji wystarczą dwa, trzeci daje kontekst.
3. **Istota / świadek Boru** — encounter potwierdza, że zmiana jest nowsza niż samo miejsce; brak jednego NPC/istoty ma fallback środowiskowy.
4. **Stary węzeł** — odkrycie materialnego/znakowego elementu zapisuje `MQ12_NODE_CONFIRMED`.
5. **Wyjście z Boru** — journal tworzy porównywalny zestaw danych i zapisuje `MQ12_COMPLETE`.

### Reveal budget
- **Must learn:** niektóre obecne miejsca święte nakładają się na starszą funkcję węzłów.
- **May learn:** lokalne istoty reagują na zmianę granic/trasy, niekoniecznie ją powodują.
- **Must not reveal:** kto zbudował cały system i jak łączy się z Czwartą Sferą.

### Persistence / recovery / QA
Stan zaburzonej nawigacji nie może uwięzić gracza po reloadzie. Testy obejmują noc/dzień, różną kolejność punktów, brak opcjonalnego encounteru i opuszczenie regionu w trakcie questa.

---

## MQ13 — Dwie mapy

### Metadata
- **Akt:** I — Wzór
- **Region:** Nadborze / Dębrzyn / wiedza Wielkiego Boru
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ10_COMPLETE`, `MQ11_COMPLETE`, `MQ12_COMPLETE`, journal/evidence, map overlay, persistence

### Cel narracyjny i gameplayowy
Zamknąć Akt I poprzez syntezę danych, nie exposition dump. Gracz sam zestawia mapę tras Nadborza z informacjami z Boru i po raz pierwszy może uczciwie sformułować hipotezę o połączonej sieci miejsc.

### Przebieg krytyczny
1. **Zebranie danych** — walidacja trzech wymaganych pakietów evidence; brak duplikacji wpisów.
2. **Nałożenie map** — interakcja/journal overlay pokazuje zgodności między punktami.
3. **Test hipotezy** — gracz wskazuje co najmniej jeden przewidywany punkt; system potwierdza zależność bez odsłaniania całej mapy.
4. **Nazwa robocza** — journal zapisuje hipotezę `NETWORK_HYPOTHESIS`; termin „Sieć Progów” może pojawić się jako robocza nazwa badawcza, nie author-confirmed prawda.
5. **Nowe kierunki** — odblokowanie tropów prowadzących do Przymorza i Aktu II; `MQ13_COMPLETE`.

### Reveal budget
- **Must learn:** miejsca tworzą wzór większy niż jeden region.
- **May learn:** wzór łączy funkcje drogi, granicy i stabilizacji.
- **Must not reveal:** pełny zasięg, wielokulturowe autorstwo, rodziców, Wszebora, Splot.

### Fail states, persistence i QA
Brak twardego faila. Jeśli gracz ma dodatkowe evidence, overlay może być bogatszy, ale minimalna hipoteza zawsze wynika z obowiązkowych danych. `NETWORK_HYPOTHESIS` i `MQ13_COMPLETE` są idempotentne. Testy: minimalny i rozszerzony evidence set, ponowne otwarcie overlay, save/load przed syntezą i po niej oraz prawidłowe odblokowanie Aktu II.

## Definition of Ready — Akt I

MQ10–MQ13 mają jawne wejścia, state writes, krytyczne fallbacki, reveal budget i kryteria QA. Szczegółowe dialogi, encounter parametry, asset IDs i tuning mogą powstać jako content pass bez zmiany kontraktu kampanii. Jeśli implementacja ujawni brak semantyki systemowej, karta wraca do statusu review zamiast dopisywać zachowanie ad hoc.