# Main Quest Cards — Akt IV: Nawia / Splot

Status: production pass v0.1. Dokument rozwija `story/MainQuestSkeleton.md` bez zmiany author truth z `world/FourthSphereTruth.md`, `story/RevelationPlan.md` i dokumentów rodziny.

## Wspólne zasady Aktu IV

- Akt IV ma nauczyć gracza rozróżnienia **Nawia ≠ Splot** przez obserwację i konsekwencje, nie pojedynczy wykład.
- Stan krytyczny kampanii musi być trwały i odtwarzalny po save/load.
- Opcjonalny Parent B nie może być wymagany do ukończenia aktu ani zrozumienia podstawowego finału.
- Każdy krytyczny dowód ma co najmniej jedną ścieżkę recovery, jeśli NPC, dialog lub eksploracyjny trigger zostanie pominięty.
- Akt kończy się powrotem do Jawii i jednoznacznym odblokowaniem MQ50.

---

## MQ40 — Droga umarłych

**Cel produkcyjny:** bezpiecznie przenieść gracza z Jawii do Nawii i ustanowić reguły eksploracji tej warstwy świata.

**Wejście:** MQ34 zakończone; gracz zna trzy projekty odpowiedzi na kryzys.

**Krytyczne beaty:**
1. przygotowanie przejścia na podstawie wiedzy zdobytej wcześniej;
2. wejście przez stabilny próg zamiast przypadkowego teleportu;
3. pierwsza obserwacja reguł Nawii;
4. ustanowienie bezpiecznego punktu powrotu;
5. trop prowadzący do dwóch podobnych ech.

**Trwały stan:** `MQ40_ENTERED_NAVIA`, `MQ40_RETURN_ANCHOR_SET`, `MQ40_COMPLETE`.

**Recovery:** utrata opcjonalnego przewodnika nie blokuje przejścia; krytyczne instrukcje mogą zostać odtworzone z Journal/znaków przy progu. Punkt powrotu jest idempotentny.

**Reveal budget:** potwierdzamy, że Nawia ma własne reguły pamięci i pozostałości osób. Nie nazywamy jeszcze pełnej natury Splotu.

**Persistence / QA:** save przed przejściem, po wejściu i po ustawieniu kotwicy; reload nie duplikuje wejścia ani kotwicy. MQ41 odblokowuje się dokładnie raz.

---

## MQ41 — Dwa echa

**Cel produkcyjny:** dać graczowi empiryczny model różnicy między echem Nawii a wzorcem związanym ze Splotem.

**Wejście:** `MQ40_COMPLETE`.

**Krytyczne beaty:**
1. znalezienie pierwszego echa osoby;
2. znalezienie drugiego, bardzo podobnego wzorca;
3. zebranie minimum dwóch niezależnych różnic zachowania/pamięci;
4. porównanie w Journal;
5. wniosek: podobieństwo nie oznacza tej samej ontologicznej warstwy.

**Trwały stan:** `MQ41_ECHO_A_FOUND`, `MQ41_ECHO_B_FOUND`, `MQ41_DIFFERENCE_01`, `MQ41_DIFFERENCE_02`, `MQ41_NAVIA_SPLOT_DISTINCTION`, `MQ41_COMPLETE`.

**Gating:** `MQ41_NAVIA_SPLOT_DISTINCTION` wymaga obu ech i minimum dwóch różnic. Nie można ukończyć questa przez obejrzenie jednego obiektu.

**Recovery:** pominięta obserwacja ma alternatywny ślad środowiskowy lub zapis w pobliżu; zniszczenie/opuszczenie sceny nie usuwa już zapisanych evidence flags.

**Reveal budget:** gracz rozumie różnicę operacyjną, ale nie otrzymuje jeszcze pełnej definicji Czwartej Sfery.

**Persistence / QA:** test kombinacji kolejności A/B, save pomiędzy dowodami i idempotentnego completion. MQ42 odblokowuje się dokładnie raz.

---

## MQ42 — Ten, który pozostał

**Cel produkcyjny:** opcjonalne spotkanie z Parent B i osobista warstwa rodzinnej prawdy bez tworzenia softlocka.

**Wejście:** MQ41 zakończone; dostępna ścieżka do miejsca spotkania.

**Krytyczne beaty dla ścieżki spotkania:**
1. odnalezienie wzorca Parent B;
2. weryfikacja tożsamości przez znane wcześniej szczegóły, nie samą deklarację;
3. rozmowa o wyborach rodziny i cenie pozostania;
4. decyzja gracza dotycząca relacji/obietnicy;
5. zapis konsekwencji na finał.

**Ścieżka pominięcia:** gracz może świadomie odejść lub nie spełnić warunków spotkania. Kampania zapisuje `MQ42_SKIPPED` i prowadzi dalej do MQ43 bez fałszywego „spotkania”.

**Trwały stan:** `MQ42_PARENT_B_FOUND`, `MQ42_IDENTITY_VERIFIED`, `MQ42_PARENT_B_DECISION`, `MQ42_SKIPPED`, `MQ42_COMPLETE`.

**Reguła wzajemnego wykluczenia:** `MQ42_PARENT_B_DECISION` i `MQ42_SKIPPED` nie mogą powstać jako sprzeczne zakończenia tej samej instancji kampanii.

**Recovery:** brak Parent B nie blokuje głównego reveal; podstawowe informacje wymagane do MQ43 istnieją niezależnie w ścieżce krytycznej.

**Reveal budget:** można ujawnić prawdę rodzinną przypisaną do Parent B, ale quest nie zastępuje MQ43 jako źródła pełnej prawdy Splotu.

**Persistence / QA:** save przed wyborem, po wyborze i na ścieżce skip; test obu wariantów oraz późniejszej dostępności MQ43.

---

## MQ43 — Czwarta nie jest miejscem

**Cel produkcyjny:** bezpośrednie doświadczenie niestabilnego wzorca i właściwy reveal natury Splotu.

**Wejście:** MQ42 zakończone lub jawnie pominięte.

**Krytyczne beaty:**
1. wejście w obszar niestabilnego wzorca;
2. obserwacja, że zwykłe reguły przestrzeni nie opisują zjawiska;
3. zestawienie doświadczenia z dowodami MQ41;
4. krytyczne zdarzenie demonstrujące relacyjny/wzorcowy charakter zjawiska;
5. zapis pełnego wniosku w Journal.

**Trwały stan:** `MQ43_UNSTABLE_PATTERN_ENTERED`, `MQ43_RULE_BREAK_OBSERVED`, `MQ43_SPLOT_TRUTH_KNOWN`, `MQ43_COMPLETE`.

**Gating:** pełny reveal wymaga wcześniejszego `MQ41_NAVIA_SPLOT_DISTINCTION`; prezentacja może się różnić zależnie od MQ42, ale prawda systemowa pozostaje ta sama.

**Recovery:** jeśli prezentacyjny event zostanie przerwany, krytyczna sekwencja ma checkpoint przed revealem i może zostać wznowiona bez powtórnego naliczania konsekwencji.

**Reveal budget:** tutaj wolno ujawnić author truth Splotu przewidzianą dla Aktu IV. Nie rozstrzygamy za gracza architektury finału E1–E5.

**Persistence / QA:** reload przed/w trakcie/po reveal; prawda nie może zostać zapisana przed spełnieniem gatingu ani utracona po zapisie. MQ44 odblokowuje się raz.

---

## MQ44 — Powrót z wiedzą

**Cel produkcyjny:** sprowadzić gracza do Jawii, pokazać eskalację kryzysu i przygotować Akt V bez odbierania skutków wcześniejszych decyzji regionalnych.

**Wejście:** `MQ43_COMPLETE` i `MQ43_SPLOT_TRUTH_KNOWN`.

**Krytyczne beaty:**
1. uruchomienie drogi powrotnej przez kotwicę MQ40;
2. powrót do Jawii;
3. aktualizacja stanu świata pokazująca pogorszenie kryzysu;
4. zebranie wiadomości z regionów zależnych od wcześniejszych decyzji;
5. synteza potrzeb finału: ludzie, materiały, wiedza i boskie warunki;
6. odblokowanie MQ50.

**Trwały stan:** `MQ44_RETURNED_TO_JAWIA`, `MQ44_CRISIS_ESCALATED`, `MQ44_FINAL_NEEDS_KNOWN`, `MQ44_COMPLETE`.

**Recovery:** brak konkretnego posłańca/NPC nie może zatrzymać kampanii; wiadomości krytyczne mają fallback przez Journal, tablicę wiadomości lub stan huba. Regionalne warianty zmieniają treść i zasoby, nie sam fakt przejścia do Aktu V.

**Reveal budget:** żadnego nowego centralnego retconu. Quest porządkuje znaną prawdę i pokazuje koszt zwłoki.

**Persistence / QA:** test powrotu z wariantem MQ42 spotkanie/skip, różnych region states oraz reloadu po eskalacji. `MQ50` ma być oferowane dokładnie raz.

---

## Definition of Ready — Akt IV

Akt IV jest gotowy do implementacji systemowej, gdy implementator może bez zgadywania:
- odtworzyć kolejność MQ40→MQ44;
- zapisać wszystkie krytyczne flagi i warunki przejścia;
- obsłużyć opcjonalność Parent B bez softlocka;
- zachować granicę reveal między MQ41, MQ42 i MQ43;
- zapewnić recovery krytycznych informacji;
- przetestować save/load i idempotencję każdego przejścia;
- zakończyć akt deterministycznym odblokowaniem MQ50.

Nazwy prezentacyjne, dialogi i staging mogą być rozwijane osobno, o ile nie zmieniają powyższego kontraktu kampanii.