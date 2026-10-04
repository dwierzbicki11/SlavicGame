# Traversal / weather / day-night — P5 scenario manifests

Status: v0.1, measurement-ready. Owner: gameplay/world + performance QA.

Ten dokument definiuje reprodukowalne scenariusze P5. Nie ustala arbitralnych prędkości, czasów, częstotliwości pogody ani progów wydajności. Wyniki i locki przechodzą lifecycle z `MeasurementPlaytestEvidence.md`.

## Wspólny kontrakt uruchomienia

Każdy run zapisuje: `scenarioId`, dokładny build SHA, region/cell/route, seed jeśli istnieje, quality preset, platform/hardware profile, startowy save/state, world time, weather state, input method, telemetry/log artifact IDs oraz wynik PASS/FAIL/OBSERVED. Powtórka bez tych danych nie może zamknąć locka.

Acceptance nigdy nie może wymagać usunięcia quest/evidence cue, combat telegraphu ani obowiązkowej interakcji. Zmiana pogody lub czasu nie może unieważnić persistence ani stworzyć nieodwracalnego softlocka.

## SCN_TRV_01 — baseline route traversal

**Cel:** ustalić użyteczny baseline poruszania się i czytelności świata bez presji pogodowej.

- Start: reprezentatywna bezpieczna trasa w R0/R1, neutralna pogoda, dzienna widoczność.
- Wykonanie: przejście trasy pieszo; powtórka z dostępnymi legalnymi wariantami traversal.
- Zbieraj: czas trasy, zatrzymania, kolizje/stuck events, korekty kamery, błędy nav/streamingu, frametime/hitch markers.
- Acceptance input: brak blockerów, brak niezamierzonych skrótów omijających gates, stabilne quest/interact handles po unload/reload.
- Lock: `LOCK_TRAVERSAL_BASELINE` pozostaje OPEN do realnych runów.

## SCN_TRV_02 — difficult terrain and recovery

**Cel:** sprawdzić błoto, brzeg/rzekę, las, strome lub ciasne przejścia zależnie od finalnie dostępnych regionów.

- Start: trasa zawierająca co najmniej dwa typy trudnego terenu.
- Wykonanie: normalne przejście, celowe wejście na granice geometrii, próba odzyskania pozycji.
- Zbieraj: stuck/recovery events, collision anomalies, movement-state transitions, camera clipping, route abandonment.
- Fail-forward: gracz musi móc wrócić do legalnej przestrzeni bez utraty wymaganych stanów questa.
- Lock: `LOCK_TRAVERSAL_RECOVERY`.

## SCN_WTH_01 — weather transition continuity

**Cel:** zweryfikować przejście neutral -> adverse -> neutral bez resetu świata.

- Start: aktywny region z NPC, encounter eligibility i co najmniej jedną interakcją.
- Wykonanie: przejść przez pełną zmianę pogody podczas ruchu i interakcji.
- Zbieraj: transition timestamps, visibility/readability observations, AI state changes, audio/VFX state, streaming/hitches, persistence deltas.
- Acceptance input: brak teleportów/resetów AI, brak zniknięcia wymaganych cue, poprawne zakończenie/przerwanie efektów po zmianie stanu.
- Lock: `LOCK_WEATHER_TRANSITION`.

## SCN_WTH_02 — adverse-weather gameplay pressure

**Cel:** sprawdzić czy pogoda wpływa na decyzje, ale nie niszczy czytelności krytycznej.

- Start: reprezentatywna trasa/POI z legalnym encounterem lub evidence interaction.
- Wykonanie: ten sam przebieg w neutralnym i niekorzystnym stanie pogody.
- Zbieraj: completion time/aborts, missed cues, detection/combat observations, navigation errors, accessibility/readability notes.
- Zakaz locka: nie ustalać mnożników widoczności, movement czy AI bez danych z runów.
- Lock: `LOCK_WEATHER_GAMEPLAY`.

## SCN_TIME_01 — day/night boundary

**Cel:** sprawdzić ciągłość świata przy przekroczeniu granicy dnia/nocy.

- Start: przed planowanym transition window, aktywny NPC/service/event eligibility.
- Wykonanie: pozostać w świecie przez transition, następnie oddalić się i wrócić.
- Zbieraj: event scheduling, NPC/service state, lighting/readability, save/load result, duplicate/missed event IDs.
- Acceptance input: event odpala najwyżej zgodnie z kontraktem, nie duplikuje rewardów, persistence odtwarza właściwy stan.
- Lock: `LOCK_DAY_NIGHT_CONTINUITY`.

## SCN_TIME_02 — time-gated content fail-forward

**Cel:** upewnić się, że przegapienie okna czasu nie tworzy nieudokumentowanego hard locka.

- Start: quest/event posiadający legalny time gate.
- Wykonanie: wejść przed, w trakcie i po oknie; wykonać save/load w każdym stanie.
- Zbieraj: gate result, alternate/retry path, dialogue/evidence state, journal feedback.
- Acceptance input: zachowanie odpowiada production card; wymagany content ma retry/alternate/fail-forward tam, gdzie spec tego wymaga.
- Lock: `LOCK_TIME_GATE_FAIL_FORWARD`.

## SCN_COMBINED_01 — night + adverse weather + traversal

**Cel:** wykryć błędy pojawiające się dopiero przy nakładaniu systemów.

- Start: reprezentatywna trasa w regionie z aktywnym streamingiem, noc i niekorzystna pogoda.
- Wykonanie: traversal przez kilka cells/POI, jedna interakcja, legalny encounter jeśli wystąpi.
- Zbieraj: CPU/GPU frametime, RAM/VRAM telemetry jeśli dostępna, streaming queue/hitches, AI counts, AVFX concurrency, readability failures, stuck events.
- Acceptance input: krytyczne cue pozostają czytelne; pressure/degradation contracts mogą obniżać kosmetykę, ale nie logikę wymaganej interakcji.
- Lock: `LOCK_COMBINED_WORLD_PRESSURE`.

## Macierz regionalna

Przed production lockiem należy mieć reprezentatywne runy obejmujące R0–R6 tam, gdzie dany region istnieje w grywalnym buildzie. Nie każdy scenariusz musi być powielony 7 razy: wybieramy regiony reprezentujące najgorszy koszt lub unikalne ryzyko, a odstępstwo zapisujemy w evidence ledger. R2 powinien reprezentować gęsty las/ograniczoną widoczność, R3 wodę/brzeg i portowe przestrzenie, R6 najcięższy finalny/anomaly presentation; dokładne assety pozostają zależne od ich research/art locków.

## Wymagane artefakty

Minimalnie: log runu, build SHA, zapis konfiguracji, telemetry dostępna dla badanego locka oraz krótki observation record. Screenshot/video jest wymagany dla błędów czytelności, geometrii lub efektów, jeśli sam log nie dowodzi problemu.

## Otwarte decyzje

- final movement/traversal values;
- weather frequency/duration/intensity i gameplay multipliers;
- dokładne day/night windows oraz regionalne różnice;
- accessibility minima dla noc/pogoda;
- finalne streaming/performance ceilings podczas scenariusza łączonego.

Wszystkie pozostają measurement/playtest lockiem; ten manifest przygotowuje ich reprodukowalne zamknięcie, ale nie udaje wykonanych pomiarów.