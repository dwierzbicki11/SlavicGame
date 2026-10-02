# Main Quest Cards — Akt 0–I v0.1

Status: first production pass. Dokument rozwija `story/MainQuestSkeleton.md` do kontraktu implementacyjnego bez zmiany centralnego lore.

## Wspólny kontrakt kart

Każdy MQ ma: wejście, cel gracza, fazy, wymagane systemy, zapis stanu, wyjścia i minimalne QA. Quest nie może opierać krytycznej informacji wyłącznie na jednym skill checku, jednym NPC ani jednym przedmiocie możliwym do zgubienia.

### Wspólne flagi
- `mq.<id>.state`: `locked|available|active|resolved|failed_safe`;
- `mq.<id>.outcome`: stabilny ID wyniku;
- `mq.<id>.evidence.*`: trwałe odkrycia;
- `world.region.<id>.*`: skutki regionalne;
- autosave: start fazy, wejście do lokacji krytycznej, przed decyzją nieodwracalną, po resolve.

## MQ00 — Światło nad mokradłem

Źródło prawdy: `quests/LightOverSwamp.md`. Ta karta nie duplikuje jego pełnego grafu.

**Wejście:** nowa gra, tutorial aktywny.  
**Cel:** nauczyć investigation → przygotowanie → konfrontacja/decyzja → konsekwencja.  
**Systemy:** movement, interaction, dialogue, tracking, combat, journal/evidence, time/weather, save.  
**Wyjście:** `mq.mq00.outcome` + regionalne reakcje zgodne z `LightOverSwampReactionMatrix.md`.

QA gate: każda z trzech głównych ścieżek kończy quest, zapis/odczyt nie gubi evidence, brak wymagania wygrania walki jedyną metodą.

## MQ01 — Droga dalej

**Wejście:** MQ00 resolved.  
**Cel gracza:** rozliczyć konsekwencje w Żarnowcu, zdobyć wiarygodny trop Nadborza i przygotować wyjazd.

### Fazy
1. `aftermath` — reakcje NPC zależne od MQ00.
2. `second_mark` — gracz otrzymuje informację o podobnym znaku przy starej drodze w Nadborzu; co najmniej dwa kanały informacji.
3. `prepare_departure` — opcjonalne zakupy/usługi, wybór aktywnego wyposażenia.
4. `leave_r0` — potwierdzenie podróży i checkpoint.

**Nieodwracalne:** opuszczenie regionu w ramach tej fazy kampanii; powrót później jest dozwolony przez world flow.  
**Output:** `mq.mq01.outcome=departed_for_nadborze`, odblokowanie MQ10.  
**QA:** quest działa po każdym wyniku MQ00; brak softlocku po śmierci/odejściu opcjonalnego NPC; journal wskazuje cel podróży.

# Akt I — Wzór

## MQ10 — Znak pod drogą

**Region:** Nadborze.  
**Wejście:** MQ01 resolved.  
**Cel:** zbadać anomalie przy ważnej trasie i starym węźle.

### Fazy
1. `road_incident` — zdarzenie pokazuje koszt anomalii bez exposition dumpu.
2. `survey_node` — tracking/interactables ujawniają warstwy konstrukcji.
3. `compare_accounts` — źródło kultowe i praktyczne opisują miejsce inaczej.
4. `date_mismatch` — gracz zdobywa dowód, że znaki są starsze niż obecny kult.
5. `report_or_keep` — wybór komu ujawnić część danych.

**Evidence:** `node_masonry`, `cult_layer`, `older_mark`.  
**Output:** hipoteza starej infrastruktury; reputacyjny modyfikator dostępu do danych.  
**QA:** minimum dwie drogi do `older_mark`; walka nie jest obowiązkowa.

## MQ11 — Prawo łowcy

**Region:** Dębrzyn.  
**Wejście:** MQ10 resolved lub kampanijny fallback po dotarciu do hubu.  
**Cel:** ustalić relację łowcy z władzą podczas kryzysu.

### Fazy
1. `summons` — wezwanie przez władzę.
2. `case_demo` — lokalny problem pokazuje konflikt jurysdykcji.
3. `stakeholders` — rozmowy: władza, łowcy/praktycy, mieszkańcy.
4. `terms` — gracz przyjmuje, negocjuje albo odrzuca warunki.
5. `public_result` — reakcja huba.

**Outcomes:** `licensed`, `conditional`, `independent`. Żaden nie blokuje main story; zmienia ceny, pomoc, checkpointy i dialogi.  
**QA:** wszystkie wyniki utrzymują dostęp do MQ12; konsekwencje są czytelne przed wyborem.

## MQ12 — Las, który myli drogę

**Region:** Wielki Bór.  
**Cel:** dotrzeć do starego węzła mimo zaburzonej nawigacji i ustalić relację między węzłem a lokalnymi istotami.

### Fazy
1. `enter_forest` — ustanowienie reguł nawigacji.
2. `false_routes` — co najmniej dwa symptomy reaktywnej przestrzeni.
3. `local_knowledge` — NPC/ślady/rytuał dają alternatywne wskazówki.
4. `node_contact` — kontakt z węzłem i encounter zależny od wcześniejszych działań.
5. `sacred_layer` — odkrycie, że część miejsc świętych nadbudowano na elementach Sieci.

**Systemy:** navigation landmarks, tracking, encounter state, ritual interaction, weather/day-night modifiers.  
**Output:** `evidence.sacred_network_overlap=true`.  
**QA:** brak proceduralnego przestawienia świata uniemożliwiającego ukończenie; zawsze istnieje deterministyczny fallback trasy.

## MQ13 — Dwie mapy

**Wejście:** MQ10 i MQ12 resolved; MQ11 może modyfikować dostęp, nie blokować.  
**Cel:** zestawić mapę/informacje Nadborza z wiedzą Wielkiego Boru i sformułować hipotezę Sieci Progów.

### Fazy
1. `collect_records` — walidacja wymaganych evidence z fallbackiem archiwalnym.
2. `overlay` — interakcja mapowa łączy punkty; nie jest testem zręcznościowym.
3. `challenge_hypothesis` — NPC lub dokument przedstawia alternatywne wyjaśnienie.
4. `pattern_confirmed` — trzeci niezależny szczegół potwierdza wzór.
5. `act_transition` — odblokowanie tropów Przymorza/Kamiennych Wyżyn/Arel zgodnie z campaign flow.

**Output:** `story.network_hypothesis=formed`; Akt I resolved.  
**QA:** gracz rozumie różnicę między hipotezą a author truth; journal zapisuje przesłanki, nie tylko wniosek.

## Zależności implementacyjne Aktu 0–I

Minimalna kolejność implementacji:
1. MQ00 jako referencyjny quest runtime;
2. trwałe quest/evidence flags;
3. MQ01 transition;
4. MQ10 investigation;
5. MQ11 reputation/political outcome;
6. MQ12 navigation + encounter state;
7. MQ13 evidence aggregation.

## Otwarte decyzje

- konkretne nazwy urzędów i NPC Aktu I — zależne od finalnego rosteru regionalnego;
- dokładne lokacje w hubach — region bible pass;
- liczby reputacji, ceny i combat tuning — po playtestach;
- finalna tożsamość `forest-guardian` pozostaje poza tą kartą i wymaga research locka.
