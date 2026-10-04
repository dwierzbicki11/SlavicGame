# P5 capture artifact format

Status: v0.1 implementation contract

Ten dokument definiuje minimalny, maszynowo czytelny format artefaktów wymaganych przez `MeasurementPlaytestEvidence.md`, `P5EvidenceLedger.md` i manifesty P5. Nie ustala żadnych targetów liczbowych. Jego celem jest umożliwienie implementacji recordera/runnera bez dalszych decyzji dokumentacyjnych oraz odrzucanie niepełnych captures zanim zostaną uznane za evidence.

## 1. Pakiet runu

Każdy run zapisuje osobny katalog:

```text
p5/<run-id>/
  manifest.json
  samples.ndjson
  events.ndjson
  summary.json
  artifacts/
```

`manifest.json`, `samples.ndjson`, `events.ndjson` i `summary.json` są obowiązkowe. `artifacts/` może zawierać profiler capture, screenshoty, save przed/po, crash dump lub inne pliki wskazane przez scenariusz.

## 2. Tożsamość i provenance

`manifest.json` MUSI zawierać:

- `schema_version`;
- `run_id` — globalnie unikalny, niezmienny identyfikator runu;
- `scenario_id` — dokładne `SCN_*`/`R*` z właściwego manifestu;
- `evidence_targets[]` — docelowe `EV-*` z `P5EvidenceLedger.md`;
- `build_sha` — pełny Git SHA uruchomionego buildu;
- `build_configuration` i platformę;
- UTC start/end timestamp;
- identyfikator hardware profile i pełny snapshot CPU/GPU/RAM/VRAM/OS/driver;
- rozdzielczość, display mode, preset i wszystkie override wpływające na pomiar;
- seed, region/map, save/scenario fixture i wariant świata;
- instrumentation version oraz listę aktywnych collectorów;
- powód runu: `baseline`, `candidate`, `regression`, `hardware_qualification` albo `release_candidate`.

Brak któregokolwiek pola wymaganego przez konkretny scenariusz powoduje `INVALID`, a nie uzupełnianie wartości po fakcie.

## 3. Samples

`samples.ndjson` zawiera jeden obiekt JSON na próbkę. Wspólne pola:

```text
run_id, t_monotonic_ns, frame_index, domain, metric, value, unit
```

Dozwolone domeny v0.1: `frame`, `cpu`, `gpu`, `memory`, `streaming`, `ai`, `avfx`, `audio`, `gameplay`, `traversal`, `save`.

Collector nie zapisuje zera, gdy metryka jest niedostępna. Zapisuje brak próbki i deklaruje niedostępność w `summary.json`. Dzięki temu brak telemetry nie może wyglądać jak idealny wynik.

## 4. Events

`events.ndjson` służy do zdarzeń dyskretnych i markerów faz scenariusza. Każdy rekord ma:

```text
run_id, t_monotonic_ns, frame_index, event_type, phase, payload
```

Wymagane markery to co najmniej `run_start`, `phase_start`, `phase_end`, `run_end`; scenariusz może wymagać dodatkowych markerów combat/streaming/weather/save/AI. `payload` musi być JSON object i nie może zawierać wyłącznie tekstowego opisu, jeśli dane mają istniejący stabilny ID.

## 5. Summary

`summary.json` jest wynikiem deterministycznej agregacji raw samples/events, nie ręcznie wpisanym raportem. Zawiera:

- `run_id`, `scenario_id`, `build_sha`;
- `validity`: `VALID` albo `INVALID`;
- `invalid_reasons[]`;
- zakres czasu i liczbę ramek/próbek per collector;
- `metrics` z agregatami wymaganymi przez scenariusz;
- `missing_metrics[]` i `collector_errors[]`;
- `artifact_index[]` z relatywną ścieżką, typem, rozmiarem i SHA-256;
- `candidate_evidence_targets[]`.

Agregaty nie zastępują raw data. Ledger może wskazać run jako `MEASURED` tylko wtedy, gdy raw files oraz summary są dostępne i zgodne hashami.

## 6. Integralność i powtarzalność

- zegar telemetry jest monotoniczny; wall clock służy wyłącznie provenance;
- jednostki są jawne i stabilne; nie wolno mieszać ms/us/ns bez pola `unit`;
- schema version zmienia się przy niekompatybilnej zmianie formatu;
- runner nie nadpisuje istniejącego `run_id`;
- wszystkie dodatkowe artefakty dostają SHA-256 w indeksie;
- ręczna edycja raw capture po runie unieważnia evidence, chyba że zachowano oryginał i transformację jako osobny, audytowalny artefakt.

## 7. Walidator

Minimalny validator przed promocją evidence sprawdza:

1. kompletność provenance i zgodność `run_id` między plikami;
2. zgodność scenario/evidence IDs z dokumentacją;
3. pełny `build_sha`;
4. wymagane collectory i fazy scenariusza;
5. monotoniczność timestampów i poprawne jednostki;
6. obecność raw data dla każdej metryki użytej w summary;
7. hashe wszystkich dodatkowych artefaktów;
8. brak `INVALID`/collector error wpływającego na dany target.

Walidator zwraca machine-readable wynik i kod != 0 dla niepoprawnego pakietu, aby można było użyć go w CI.

## 8. Relacja z ledgerem

`P5EvidenceLedger.md` pozostaje autorytatywnym indeksem statusów `EV-*`. Ten format jest jedynie kontraktem danych. Sam poprawny capture nie awansuje automatycznie targetu do `CANDIDATE` lub `LOCKED`; promocja musi spełnić lifecycle i acceptance criteria z `MeasurementPlaytestEvidence.md` oraz odpowiedniego scenario manifestu.

## 9. Następny krok implementacyjny

Najmniejszy pionowy slice instrumentacji to: writer `manifest.json` + frame-time collector + event markers + `summary.json` + validator. Po nim pierwszy rzeczywisty run może przejść od `SPECIFIED` do `RUNNABLE`, bez oczekiwania na komplet collectorów pozostałych domen.