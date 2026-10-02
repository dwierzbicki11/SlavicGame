# Main Quest Cards — Akt II v0.1

Production pass dla MQ20–MQ25. Karty uszczegóławiają `MainQuestSkeleton.md` bez zmiany centralnego lore. Obowiązuje kontrakt `MainQuestCardTemplate.md`. Akt II ma rozszerzyć hipotezę Sieci Progów na skalę międzyregionalną i ujawnić jej wielokulturowe pochodzenie, ale nie może jeszcze odsłonić prawdy o rodzicach, Wszeborze ani Splocie.

## MQ20 — Sól i milczenie

### Metadata
- **Akt:** II — Interesy
- **Region:** Przymorze
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ13_COMPLETE`, travel/region state, dialogue, reputation, journal/evidence, persistence

### Cel narracyjny i gameplayowy
Pierwszy trop po sformułowaniu hipotezy Sieci prowadzi do portowego regionu, gdzie kryzys ma wymierną cenę gospodarczą. Gracz musi oddzielić bieżący konflikt o blokowane ujście od informacji ukrytej przez kupców.

### Przebieg krytyczny
1. **Zablokowane ujście** — wejście w strefę kryzysu zapisuje `MQ20_ESTUARY_BLOCKED` i pokazuje skutki dla transportu.
2. **Dwa interesy** — gracz poznaje perspektywę lokalnej władzy/przewoźników oraz kupców posiadających fragment starej mapy.
3. **Dostęp do mapy** — minimum jedna z dróg: przysługa, negocjacja, reputacja albo dowód środowiskowy. Żadna pojedyncza frakcja nie może być krytycznym single point of failure.
4. **Porównanie punktów** — fragment mapy pokrywa się z częścią hipotezy z MQ13; zapis `MQ20_REMOTE_NODE_EVIDENCE`.
5. **Wyjście** — bieżący problem ujścia otrzymuje stan rozwiązania/obejścia, zapis `MQ20_COMPLETE`, odblokowanie MQ21.

### Reveal budget
- **Must learn:** punkty odpowiadające Sieci występują daleko poza Nadborzem.
- **May learn:** dawne szlaki handlowe wykorzystywały część tych samych przejść.
- **Must not reveal:** kto stworzył system, pełna funkcja kotwic, rodzice, Wszebor, Splot.

### Persistence / recovery / QA
Stan mapy jest niezależny od końcowego wyniku konfliktu portowego. Testy obejmują każdą drogę dostępu do mapy, niski reputation state, utratę opcjonalnego NPC, save/load przed pozyskaniem mapy i po nim oraz gwarantowane odblokowanie MQ21.

---

## MQ21 — Cena przejścia

### Metadata
- **Akt:** II — Interesy
- **Region:** Przymorze
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ20_COMPLETE`, faction/reputation, dialogue, travel permissions, economy/world state, persistence

### Cel narracyjny i gameplayowy
Przekuć odkrycie w decyzję polityczną. Liga i Nadborze chcą kontrolować dostęp do danych i szlaków; gracz wybiera praktyczny układ, ale kampania nie może wymagać lojalności wobec jednej strony.

### Przebieg krytyczny
1. **Roszczenia stron** — obie strony przedstawiają koszt i warunki współpracy.
2. **Weryfikacja** — gracz może sprawdzić przynajmniej jeden argument każdej strony w świecie lub dokumentach.
3. **Decyzja `MQ21-D01`** — preferencja Ligi / preferencja Nadborza / ograniczony dostęp dla obu stron.
4. **Cena** — zapis reputacji, warunków handlu i późniejszej pomocy; żaden wynik nie usuwa krytycznej trasy kampanii.
5. **Wyjście** — `MQ21_COMPLETE`; otwarcie drogi do Kamiennych Wyżyn/MQ22.

### Konsekwencje
Decyzja może zmieniać ceny, eskortę, dostęp do informacji i regionalne wsparcie w Akcie V. Krytyczne dane MQ22–MQ25 pozostają dostępne.

### Reveal budget
- **Must learn:** wiedza o dawnych przejściach jest zasobem politycznym.
- **May learn:** strony mają niepełne, wzajemnie sprzeczne archiwa.
- **Must not reveal:** prawdziwy cel Sieci i centralne author truth.

### Persistence / recovery / QA
`MQ21-D01` jest trwałą, pojedynczą decyzją. Testy: wszystkie trzy wyniki, skrajne reputacje, odmowa współpracy, save/load przed wyborem i po nim oraz dostępność MQ22 w każdym wariancie.

---

## MQ22 — Kamień pod kamieniem

### Metadata
- **Akt:** II — Interesy
- **Region:** Kamienne Wyżyny
- **Status:** implementation-ready v0.1
- **Zależności:** MQ21, exploration/tracking, environmental interaction, hazard/encounter state, journal/evidence, persistence

### Cel narracyjny i gameplayowy
Pokazać fizyczny komponent Sieci. Eksploatacja kopalni narusza starą strukturę, a gracz musi zbadać ją bez zamiany questa w prosty wykład technologiczny.

### Przebieg krytyczny
1. **Zakłócenie kopalni** — zapis `MQ22_MINE_BREACH`; lokalny hazard ogranicza dostęp.
2. **Warstwy konstrukcji** — dwa niezależne ślady odróżniają dawną strukturę od nowszej kopalni.
3. **Materiał kotwicy** — analiza/interakcja zapisuje `MQ22_ANCHOR_MATERIAL`; gracz rozumie, że część węzłów ma materialny komponent pochodzący z regionu górskiego.
4. **Stabilizacja lokalna** — gracz zabezpiecza miejsce, omija zagrożenie albo ogranicza wydobycie zgodnie z finalnym encounterem.
5. **Wyjście** — `MQ22_COMPLETE`; dowód prowadzi do konfliktu MQ23.

### Reveal budget
- **Must learn:** kotwice nie są wyłącznie symbolem lub rytuałem; mają materialny komponent.
- **May learn:** materiał był transportowany poza region.
- **Must not reveal:** pełny mechanizm stabilizacji granic ani związek ze Splotem.

### Persistence / recovery / QA
Krytyczny fragment materiału nie może zostać zniszczony przez encounter. Testy obejmują różną kolejność śladów, opuszczenie kopalni, save/load podczas hazardu oraz fallback, gdy opcjonalna analiza NPC jest niedostępna.

---

## MQ23 — Żelazna Brama

### Metadata
- **Akt:** II — Interesy
- **Region:** Kamienne Wyżyny
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ22_COMPLETE`, faction/reputation, resource/world state, dialogue, persistence

### Cel narracyjny i gameplayowy
Związać wiedzę o materiale z konfliktem politycznym księstw. Decyzja gracza ma tworzyć późniejszy region state, a nie wybierać jedyną poprawną stronę.

### Przebieg krytyczny
1. **Spór o złoże** — strony przedstawiają roszczenia do materiału ujawnionego w MQ22.
2. **Ocena skutków** — gracz otrzymuje informację o konsekwencjach dla wydobycia, bezpieczeństwa i potencjalnej stabilizacji.
3. **Decyzja `MQ23-D01`** — kontrola jednej strony / podział nadzorowany / ograniczenie wydobycia i rezerwa stabilizacyjna.
4. **Region state** — zapis zasobów, relacji i przyszłej dostępności materiału.
5. **Wyjście** — `MQ23_COMPLETE`; trop o dawnych trasach prowadzi do Arel/MQ24.

### Konsekwencje
W Akcie V stan może wpływać na ilość materiału, koszt pozyskania i rodzaj wsparcia. Brak idealnego wyniku jest zamierzony; żaden wariant nie może uniemożliwić finału.

### Reveal budget
- **Must learn:** kontrola nad komponentem Sieci ma współczesne skutki strategiczne.
- **May learn:** dawne dostawy nie respektowały obecnych granic politycznych.
- **Must not reveal:** autorzy całej Sieci i wydarzenia 22 BG.

### Persistence / recovery / QA
Testy: wszystkie decyzje, wrogie relacje z jedną ze stron, save/load, powrót do regionu oraz poprawne odczytanie stanu przez późniejsze questy.

---

## MQ24 — Droga bez granicy

### Metadata
- **Akt:** II — Interesy
- **Region:** Arel
- **Status:** implementation-ready v0.1
- **Zależności:** MQ23, travel/navigation, dialogue, tracking, map/journal, persistence

### Cel narracyjny i gameplayowy
Pokazać inną kulturę pamięci przestrzeni. Arelowie zachowali relacje między drogami i punktami, których nie widać na mapach osiadłych państw; gracz ma nauczyć się czytać trasę jako relację, nie tylko linię na mapie.

### Przebieg krytyczny
1. **Niepasująca mapa** — standardowa mapa okazuje się niewystarczająca; `MQ24_MAP_MISMATCH`.
2. **Pamięć trasy** — gracz poznaje co najmniej dwa sposoby opisu relacji między punktami: opowieść/znak oraz obserwację terenową.
3. **Przejście próbne** — zastosowanie wiedzy prowadzi do dawnego punktu bez wymagania jednego konkretnego NPC.
4. **Relacja węzłów** — zapis `MQ24_ROUTE_RELATION`; dane uzupełniają luki między mapami z poprzednich regionów.
5. **Wyjście** — `MQ24_COMPLETE`; odblokowanie syntezy MQ25.

### Reveal budget
- **Must learn:** różne kultury przechowały różne fragmenty wiedzy o tej samej infrastrukturze.
- **May learn:** relacje dróg są starsze niż część obecnych granic i nazw.
- **Must not reveal:** kto koordynował pierwotną budowę ani prawda rodzinna.

### Persistence / recovery / QA
Quest nie może egzotyzować Arelów jako „mistycznego klucza”; ich wiedza jest praktycznym, kulturowo odmiennym systemem pamięci. Testy obejmują alternatywną kolejność wskazówek, brak przewodnika, błędną próbę trasy, save/load i powrót po opuszczeniu regionu.

---

## MQ25 — Archiwum bez jednego języka

### Metadata
- **Akt:** II — Interesy
- **Region:** synteza danych Przymorza, Kamiennych Wyżyn, Arel i Nadborza
- **Status:** implementation-ready v0.1
- **Zależności:** `MQ20_COMPLETE`–`MQ24_COMPLETE`, journal/evidence, map overlay, language/context tags, persistence

### Cel narracyjny i gameplayowy
Zamknąć Akt II syntezą czterech tradycji danych. Gracz odkrywa, że Sieć nie jest dziełem jednej obecnej kultury ani jednego państwa, co otwiera drogę do historycznego śledztwa Aktu III.

### Przebieg krytyczny
1. **Cztery pakiety danych** — walidacja krytycznych evidence z Nadborza, Przymorza, Wyżyn i Arel; opcjonalne dane wzbogacają wynik.
2. **Normalizacja** — journal/archiwum pozwala zestawić różne nazwy, miary i sposoby opisu bez udawania jednego źródłowego języka.
3. **Wspólne zależności** — gracz identyfikuje co najmniej dwa zgodne połączenia między tradycjami.
4. **Wniosek** — zapis `NETWORK_MULTICULTURAL_ORIGIN`; system był budowany/utrzymywany przez więcej niż jedną kulturę.
5. **Nowe pytanie** — ślady prowadzą ku R6 i Archiwum Progów; `MQ25_COMPLETE`, odblokowanie Aktu III/MQ30.

### Reveal budget
- **Must learn:** system ma wielokulturową historię i nie da się go uczciwie przypisać jednemu współczesnemu państwu.
- **May learn:** część nazw i praktyk jest późniejszą reinterpretacją starszych funkcji.
- **Must not reveal:** role rodziców, pełne wydarzenia Nocy Zamkniętego Progu, motyw Wszebora i prawda Splotu.

### Persistence / recovery / QA
Synteza jest idempotentna i nie wymaga wszystkich opcjonalnych dowodów. Testy: minimalny/pełny evidence set, różne wyniki MQ21 i MQ23, ponowne otwarcie archiwum, save/load przed wnioskiem i po nim oraz poprawne odblokowanie MQ30.

## Definition of Ready — Akt II

MQ20–MQ25 mają jawne wejścia, state writes, decyzje i konsekwencje, krytyczne fallbacki, reveal budget, persistence oraz kryteria QA. Szczegółowe dialogi, finalne nazwy frakcyjnych NPC, encounter parametry, asset IDs i tuning pozostają content passem i nie mogą zmieniać kontraktu kampanii bez aktualizacji kart.