# Main Quest Cards — Act III: Pamięć

Status: production documentation pass v0.1.  
Zakres: MQ30–MQ34.  
Źródła nadrzędne: `story/MainQuestSkeleton.md`, `story/MysteryChronology.md`, `story/RevelationPlan.md`, `character/FamilyTruth.md`, `story/Antagonists.md`.

## Zasady aktu

Akt III zamienia hipotezę Sieci Progów w historię ludzi, którzy próbowali nią zarządzać. Gracz może różnymi drogami zdobywać dowody, ale krytyczne fakty nie mogą zależeć od pojedynczego NPC, jednego przedmiotu ani jednego wyniku walki. Każdy krytyczny reveal ma co najmniej dwa kanały odzyskania informacji.

Akt nie ujawnia jeszcze pełnej prawdy Splotu; to należy do Aktu IV. Informacje o rodzinie bohatera, Wszeborze i Nocy Zamkniętego Progu mają być rozdzielone na dowody, interpretacje i zeznania, tak aby gracz mógł zauważyć sprzeczności przed otrzymaniem pełnego modelu.

---

## MQ30 — Pustkowie Pierwszego Progu

**Region:** R6 / Pustkowie Pierwszego Progu.  
**Rola:** wejście do obszaru katastrofy i ustanowienie reguł eksploracji.

### Wejście
- `MQ25_COMPLETE`;
- gracz posiada skonsolidowaną hipotezę Sieci;
- dostęp do R6 zostaje otwarty przez stan kampanii, nie przez konkretną frakcję.

### Krytyczne beaty
1. Dotarcie do zewnętrznej strefy Pustkowia.
2. Pierwszy kontakt z niestabilnością dawnych węzłów.
3. Odczytanie co najmniej dwóch niezależnych śladów działalności Archiwum Progów.
4. Ustalenie bezpiecznej osi dojścia do ruin archiwum.
5. Odblokowanie MQ31.

### Trwały stan
- `MQ30_COMPLETE`;
- `R6_ENTERED`;
- `FIRST_THRESHOLD_ROUTE_KNOWN`;
- evidence: `ARCHIVE_PRESENCE_CONFIRMED`.

### Reveal budget
Można potwierdzić, że katastrofa była związana z działaniem ludzi przy Sieci. Nie ujawniać jeszcze pełnej roli rodziców, motywacji Wszebora ani natury Splotu.

### Recovery
Jeżeli główna trasa eksploracji zostanie zamknięta przez stan świata, alternatywny ślad środowiskowy prowadzi do tego samego wejścia do MQ31. Krytyczne evidence nie może być zniszczalne.

### QA
- MQ31 jest osiągalne z każdego legalnego stanu końcowego MQ25;
- zapis/odczyt zachowuje odkryte ślady i trasę;
- ponowne wejście do R6 nie duplikuje evidence.

---

## MQ31 — Archiwum popiołu

**Rola:** pierwszy twardy pakiet dokumentów o Archiwum Progów i rodzinie bohatera.

### Wejście
- `MQ30_COMPLETE`.

### Krytyczne beaty
1. Wejście do ruin Archiwum.
2. Rozpoznanie systemu przechowywania danych i śladów celowego niszczenia części zapisów.
3. Pozyskanie dokumentu łączącego rodziców bohatera z pracami przy Progach.
4. Pozyskanie niezależnej wzmianki o Wszeborze i jego roli organizacyjnej/ideowej.
5. Odkrycie odniesienia do wydarzenia określanego jako Noc Zamkniętego Progu.
6. Odblokowanie MQ32.

### Trwały stan
- `MQ31_COMPLETE`;
- `ARCHIVE_RECORDS_FOUND`;
- `PARENTS_ARCHIVE_ROLE_KNOWN`;
- `WSZEBOR_ARCHIVE_LINK_KNOWN`;
- `CLOSED_THRESHOLD_NIGHT_REFERENCE_KNOWN`.

### Reveal budget
Gracz poznaje role i powiązania, ale dokumenty nie rozstrzygają jeszcze, która interpretacja Nocy jest prawdziwa. Parent B nie może zostać przedstawiony jako jednoznacznie martwy ani jednoznacznie dostępny.

### Recovery
Każdy z trzech krytycznych faktów ma dokument podstawowy i wtórny skrót/rejestr. Zniszczenie lub pominięcie opcjonalnego dokumentu nie blokuje kampanii.

### QA
- trzy krytyczne fakty są zapisywane niezależnie;
- kolejność czytania dokumentów nie zmienia flag końcowych;
- journal odróżnia fakt źródłowy od interpretacji gracza.

---

## MQ32 — Noc Zamkniętego Progu

**Rola:** investigation rekonstrujące wydarzenie 22 BG bez wymuszania jednego źródła prawdy.

### Wejście
- `MQ31_COMPLETE`.

### Kanały dowodowe
- dokumenty Archiwum;
- echo zdarzenia;
- żyjący świadek lub przekaz pośredni;
- zeznanie Wszebora, jeśli dostępne;
- zapis Straży Zamknięcia.

### Krytyczne beaty
1. Zebranie minimum trzech kategorii dowodów, z czego co najmniej jedna niepochodząca od zainteresowanej frakcji.
2. Zidentyfikowanie zgodnego rdzenia wydarzenia.
3. Oznaczenie jawnych sprzeczności między źródłami.
4. Rekonstrukcja minimalnej sekwencji przyczynowej wymaganej przez kampanię.
5. Odblokowanie MQ33.

### Trwały stan
- `MQ32_COMPLETE`;
- `CLOSED_THRESHOLD_CORE_RECONSTRUCTED`;
- licznik/kolekcja kategorii evidence;
- jawne flagi sprzeczności dla późniejszych dialogów.

### Reveal budget
Można potwierdzić, że Zamknięcie było decyzją pod presją i że skutki nie odpowiadały w pełni intencjom uczestników. Nie ujawniać jeszcze technicznej prawdy Nawia/Splot.

### Recovery
Żaden pojedynczy świadek nie jest wymagany. Jeśli Wszebor jest niedostępny lub wrogi, dokumenty + echo + Straż zapewniają pełny krytyczny rdzeń. Jeśli echo jest pominięte, alternatywny zapis terenowy zapewnia trzecią kategorię.

### QA
- wszystkie legalne kombinacje trzech kategorii prowadzą do MQ33;
- sprzeczne źródła nie nadpisują się wzajemnie;
- save/load zachowuje częściową rekonstrukcję.

---

## MQ33 — Rozwierający

**Rola:** pierwsza pełna konfrontacja ideowa z Wszeborem.

### Wejście
- `MQ32_COMPLETE`.

### Krytyczne beaty
1. Ustanowienie kontaktu lub wymuszonego spotkania.
2. Konfrontacja Wszebora z dowodami gracza.
3. Przedstawienie jego diagnozy błędu Zamknięcia i argumentu za Rozwarciem.
4. Reakcja zależna od wcześniejszych relacji i ujawnionych sprzeczności.
5. Zakończenie przez rozmowę, zerwanie, czasowy układ albo walkę bez obowiązkowego zabicia Wszebora.
6. Uzyskanie dostępu do danych potrzebnych MQ34 niezależnie od wyniku relacji.

### Trwały stan
- `MQ33_COMPLETE`;
- `WSZEBOR_POSITION_KNOWN`;
- relacja/stance Wszebora;
- `WSZEBOR_CONFRONTATION_OUTCOME`.

### Reveal budget
Wszebor może przedstawić własną interpretację rodziny i katastrofy, ale jego wypowiedzi są testimony, nie automatycznym author truth. Nie zdradza pełnej natury Czwartej Sfery.

### Recovery
Jeżeli spotkanie kończy się walką lub ucieczką, pakiet danych pozostaje dostępny przez zabezpieczony zapis/pośrednika. MQ34 nie może wymagać przeżycia, przyjaźni ani współpracy Wszebora.

### QA
- wariant pokojowy i bojowy kończą quest;
- żadna opcja dialogowa nie blokuje MQ34;
- outcome pozostaje dostępny dla MQ52.

---

## MQ34 — Trzy projekty

**Rola:** zamknięcie Aktu III i przedstawienie trzech konkurencyjnych odpowiedzi na kryzys.

### Wejście
- `MQ33_COMPLETE`.

### Krytyczne beaty
1. Zestawienie danych z Aktów I–III.
2. Przedstawienie projektu twardego Zamknięcia.
3. Przedstawienie projektu rozproszonej przebudowy/stabilizacji.
4. Przedstawienie projektu Rozwarcia.
5. Pokazanie kosztów i niewiadomych każdego rozwiązania bez wyboru finałowego.
6. Ustalenie, że brakującym elementem jest wiedza o tym, co faktycznie znajduje się po drugiej stronie obserwowanych zjawisk.
7. Handoff do MQ40 i Aktu IV.

### Trwały stan
- `MQ34_COMPLETE`;
- `HARD_CLOSURE_MODEL_KNOWN`;
- `DISTRIBUTED_REBUILD_MODEL_KNOWN`;
- `OPENING_MODEL_KNOWN`;
- `ACT4_NAVIA_LEAD_AVAILABLE`.

### Reveal budget
Gracz zna trzy projekty, ich autorów/zwolenników i obserwowalne koszty. Gra nadal nie potwierdza, który model metafizyczny jest poprawny; odpowiedź przynosi Akt IV.

### Recovery
Wszystkie trzy projekty mają streszczenie krytyczne generowane z trwałego stanu kampanii. Brak opcjonalnych notatek zmniejsza szczegółowość, ale nigdy nie usuwa modelu z porównania.

### QA
- trzy modele zawsze są dostępne przed zakończeniem aktu;
- UI/journal nie oznacza żadnego modelu jako „prawdziwy”;
- MQ40 odblokowuje się dokładnie raz;
- save/load po zakończeniu zachowuje komplet modeli i lead do Nawii.

---

## Definition of Ready — Act III

Akt III jest gotowy do implementacji, gdy implementator może jednoznacznie ustalić:
- warunki wejścia i wyjścia MQ30–MQ34;
- krytyczne state writes;
- minimalne evidence potrzebne do rekonstrukcji;
- recovery dla utraconych NPC/tras/źródeł;
- granicę reveal między Aktem III i IV;
- dane przekazywane do MQ40 oraz późniejszego MQ52.

Nazwy scen, dokładne dialogi, liczby encounterów i finalne assety pozostają produkcyjne i nie mogą zmieniać powyższego kontraktu kampanii.