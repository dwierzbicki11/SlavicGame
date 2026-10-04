# Measurement and playtest evidence contract

Status: v0.1 — P5 evidence owner.

Ten dokument definiuje wspólny format dowodu dla wszystkich locków, których nie wolno zamykać arbitralną liczbą. Nie ustala balansu ani targetów performance. Ustala **jak realny test staje się dowodem**, jak jest powiązany z buildem i scenariuszem oraz kiedy można na jego podstawie zamknąć decyzję.

## Zasada nadrzędna

Wartość oznaczona `playtest`, `measurement` albo `performance lock` może przejść do stanu `LOCKED` tylko wtedy, gdy ledger zawiera reprodukowalny evidence record spełniający acceptance danego ownera. Brak pomiaru pozostaje `OPEN`; brak danych nie jest wynikiem zerowym ani zgodą na domyślną wartość.

## Stabilne ID

- próba: `EVT-YYYYMMDD-NNN`;
- scenariusz: `SCN_<DOMAIN>_<NAME>_V<n>`;
- lock: `LOCK_<DOMAIN>_<NAME>`;
- zestaw sprzętowy: `HW_<CLASS>_<NN>`.

ID próby jest niezmienne. Powtórzenie testu tworzy nową próbę i wskazuje `supersedes` lub `retestOf`; nie nadpisuje historii.

## Evidence record

Każda próba zapisuje co najmniej:

```yaml
evidenceId: EVT-20261004-001
lockIds: [LOCK_PERF_R0_FRAME]
scenarioId: SCN_PERF_R0_HUB_V1
build:
  commit: <git-sha>
  configuration: Release
  platform: linux-x64
hardwareId: HW_PC_01
run:
  startedUtc: <timestamp>
  durationSeconds: <measured>
  warmupSeconds: <measured>
  seed: <if-applicable>
result:
  status: PASS | FAIL | INCONCLUSIVE
  metrics: {}
artifacts:
  - <log/profile/capture path or CI artifact reference>
notes: <anomalies only>
```

Dla playtestu zamiast danych osobowych zapisujemy anonimowy `testerClass`, wersję scenariusza, liczbę ukończonych prób, obserwowane zachowania i metryki zadania. Wolny komentarz gracza jest materiałem jakościowym, nie samodzielnym liczbowym lockiem.

## Rejestr sprzętu

Każdy `hardwareId` opisuje co najmniej CPU, GPU, RAM, VRAM jeśli raportowalne, storage class, OS/driver oraz rozdzielczość/quality preset. Zmiana istotnego komponentu tworzy nową rewizję zestawu. Minimal/recommended hardware nie może być wyprowadzone z nazwy klasy bez wyników na odpowiadającym jej fizycznym sprzęcie.

## Scenariusz testowy

Scenariusz ma ownera, setup, start state/save, region/cell, world state, liczbę agentów lub encounter family gdy istotne, kroki, warm-up, czas/warunek zakończenia, zbierane metryki i acceptance rule. Zmiana setupu wpływająca na porównywalność zwiększa wersję `scenarioId`.

### Minimalny zestaw domen

| Domena | Obowiązkowy dowód przed lockiem |
|---|---|
| combat | reprezentatywne archetypy/broń, sukces/porażka, time-to-resolution, damage taken/dealt i obserwacja exploitów |
| economy/progression | źródła/sinki, tempo zdobywania, craft/purchase path, progression checkpoint i brak dominant infinite loop |
| evidence/reputation | osiągalność progów przez poprawne ścieżki, fail-forward i brak softlocku |
| traversal/weather/day-night | czas przejścia, czytelność, hazard/visibility, event timing i powtarzalność |
| CPU/GPU | frame-time distribution oraz scena/ustawienia/build |
| RAM/VRAM/streaming | peak/steady state, IO/load/eviction/hitch telemetry oraz traversal path |
| AI/encounter | active/near agents, CPU/query/path/perception telemetry, density i pressure transitions |
| AVFX/audio | concurrency/cost class, degradacja i zachowanie gameplay-critical cues |

## Acceptance i stan locka

Lock ma jeden z czterech stanów:

- `OPEN` — brak wystarczającego dowodu;
- `MEASURED` — istnieją poprawne próby, ale acceptance nie jest jeszcze ustalone albo próbka jest za mała;
- `CANDIDATE` — proponowana wartość/range ma dowód i oczekuje wymaganych retestów;
- `LOCKED` — owner zaakceptował wartość/range na podstawie wymaganej macierzy testów.

`LOCKED` zapisuje `value/range`, listę `evidenceIds`, datę, commit/build oraz ownera. Zmiana systemu, która unieważnia założenia scenariusza, nie kasuje historii: lock przechodzi do `OPEN-RETEST` z powodem.

## Minimalna macierz przed production lock

Performance target wymaga co najmniej reprezentatywnych scen R0–R6 dla domen, które dany region obciążają, testu cold i warm tam gdzie dotyczy streamingu oraz sceny łączącej koszt AI + rendering + AVFX zamiast izolowanych mikrobenchmarków. Balance lock wymaga więcej niż jednej ścieżki/archetypu tam, gdzie system dopuszcza alternatywy. Release evidence musi wskazywać dokładny build kandydata.

Nie ustalamy tutaj arbitralnej liczby powtórzeń. Owner konkretnego locka definiuje próbkę przed rozpoczęciem zbierania danych i zapisuje ją w scenariuszu, aby nie dobierać acceptance po zobaczeniu wyniku.

## Ledger

Ledger może być Markdown/CSV/JSON generowany z danych testowych, ale jego minimalny widok zawiera:

| Evidence ID | Lock | Scenario | Build SHA | Hardware | Status | Artifacts | Retest |
|---|---|---|---|---|---|---|---|

Surowe profile/logi nie muszą być commitowane do repo, jeśli są duże; ledger musi jednak mieć trwały odnośnik do CI artifact/release evidence albo archiwum projektu. Wynik bez dostępnego artefaktu może służyć diagnostyce, ale nie zamyka production locka.

## Fail/inconclusive policy

`FAIL` jest dowodem negatywnym i pozostaje w historii. `INCONCLUSIVE` stosujemy dla awarii narzędzia, nieporównywalnego buildu, brakującej telemetry lub zakłóconej próby. Nie wolno usuwać outlierów bez zapisanej reguły i uzasadnienia.

## Powiązanie z istniejącymi ownerami

- `CombatDesign`, `Progression`, economy i item catalog są ownerami znaczenia wartości gameplayowych; ten dokument jest ownerem formatu dowodu.
- `AnimationVfxAudioBudgetContract`, `StreamingMemoryBudgetContract` i `AiEncounterDensityBudgetContract` definiują co mierzyć i jak degradować; ledger dostarcza dowód do ich liczbowych locków.
- `TestingAndPerformance` jest ownerem narzędzi/test harness; ten kontrakt nie zastępuje procedur technicznych.
- `ReleaseCriteria` konsumuje tylko evidence odnoszące się do dokładnego release candidate.

## QA kontraktu

Przed uznaniem locka za zamknięty sprawdź: stabilne ID, dokładny SHA, scenariusz i jego wersję, hardware/preset, surowy artifact, wynik, acceptance, ownera oraz retest impact. Brak któregokolwiek pola wymaganego przez scenariusz oznacza `MEASURED`/`INCONCLUSIVE`, nie `LOCKED`.

## Otwarte decyzje

Konkretne targety, sample sizes, minimal/recommended hardware, finalne progi balansu i acceptance ranges pozostają otwarte do realnych pomiarów/playtestów. Ich brak jest teraz śledzony wspólnym kontraktem zamiast rozproszonych TBD.