# P5 Operations Index

Status: v0.1 — operational navigation for measurement/playtest closure.

Ten dokument jest punktem wejścia do wykonywania P5. Nie zastępuje specyfikacji źródłowych i nie definiuje nowych targetów. Jego rolą jest wskazanie jednej kolejności pracy oraz ownera każdego artefaktu, aby implementacja instrumentacji i późniejsze captures nie rozjechały się między dokumentami.

## Source of truth map

| Pytanie | Owner |
|---|---|
| Jakie dowody są wymagane i jaki mają lifecycle? | `../design/MeasurementPlaytestEvidence.md` |
| Jakie targety `EV-*` pozostają otwarte? | `../design/P5EvidenceLedger.md` |
| Jak odtwarzać combat/economy/progression/evidence/reputation? | `../design/BalancePlaytestScenarioManifests.md` |
| Jak odtwarzać traversal/weather/day-night? | `../design/TraversalWeatherDayNightScenarioManifests.md` |
| Jak odtwarzać performance/release qualification? | `../design/PerformanceReleaseMeasurementManifests.md` |
| Jak ma wyglądać raw capture package? | `P5CaptureArtifactFormat.md` |
| Jaką instrumentację trzeba dostarczyć? | `P5InstrumentationHandoff.md` |
| Jak awansować evidence OPEN→MEASURED→CANDIDATE→LOCKED? | `P5EvidencePromotionRunbook.md` |
| Co nadal blokuje production-complete documentation? | `../design/DocumentationCoverage.md` |
| Jaka jest kolejność prac dokumentacyjnych? | `../design/DocumentationWorkQueue.md` |

## Minimalny vertical slice instrumentacji

Pierwszy runnable pion powinien dostarczyć razem:

1. provenance writer z exact build SHA, scenario ID, run ID, platformą, sprzętem i konfiguracją;
2. monotonic clock oraz phase/event markers;
3. frame-time collector z rozróżnieniem CPU/GPU tam, gdzie backend udostępnia wiarygodne dane;
4. writer `manifest.json`, `samples.ndjson`, `events.ndjson` i `summary.json` zgodny z `P5CaptureArtifactFormat.md`;
5. validator odrzucający brak provenance, niespójne ID, niedozwolone jednostki, brak raw backing i uszkodzone artefakty;
6. co najmniej jeden deterministyczny scenario entry point bez ręcznej zmiany stanu świata w trakcie runu.

Brak metryki jest stanem `unavailable`/brakiem collectora zgodnie z kontraktem, nigdy wartością `0` udającą pomiar.

## Kolejność rozszerzania collectorów

Po minimalnym pionie dodawać collectory od najmniejszej liczby zależności:

1. process/system memory oraz streaming counters;
2. AI counters i encounter-density markers;
3. animation/VFX/audio counters;
4. combat/economy/progression telemetry;
5. evidence/reputation telemetry;
6. traversal/weather/day-night telemetry;
7. save/fail-forward i long-session stability evidence.

Każdy collector musi deklarować wersję schematu, jednostki, częstotliwość/przyczynę próbkowania i warunek `unavailable`. Dodanie collectora nie awansuje żadnego `EV-*` samo w sobie.

## Definition of runnable scenario

Scenario może zmienić readiness na `RUNNABLE` tylko gdy:

- istnieje jednoznaczny entry point i kontrolowany setup;
- recorder zapisuje pełne provenance;
- wymagane dla scenariusza collectory są dostępne albo scenariusz jawnie kończy się INVALID;
- validator akceptuje package bez ręcznego poprawiania plików;
- rerun z tym samym setupem nie wymaga edycji kodu lub save'a podczas pomiaru;
- raw artifacts są zachowane i można je powiązać z exact SHA.

## Definition of documentation closure for P5

P5 jest dokumentacyjnie production-locked dopiero wtedy, gdy wszystkie targety wymagane przez finalny scope mają w `P5EvidenceLedger.md` status `LOCKED` oparty o ważne captures, a hardware qualification i release evidence odnoszą się do konkretnego candidate SHA. Zielone CI, istnienie instrumentacji albo pojedynczy udany run nie spełniają tego warunku.

Jeśli implementacja lub scope zmieni semantykę scenariusza, collectora albo targetu, należy najpierw zaktualizować owner document i oznaczyć zależne evidence jako wymagające ponownej walidacji zgodnie z `P5EvidencePromotionRunbook.md`.
