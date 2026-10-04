# Production estimates framework

Status: production planning contract v0.1. Owner: production/design. Ten dokument nie podaje wymyślonych osobodni ani dat; definiuje sposób uzyskania estymat dopiero z rzeczywistej przepustowości projektu.

## Cel

Estymaty mają odpowiadać na pytanie ile pracy pozostało dla zatwierdzonego scope, bez zamiany niepewności research/art/playtest/performance w fałszywie precyzyjny harmonogram. Jednostką planowania jest production package z trwałym ID i mierzalnym kryterium DONE.

## Jednostki pracy

Każdy pakiet otrzymuje `workPackageId`, `domain`, `scopeIds`, `dependencies`, `lockState`, `complexityClass`, `reuseClass`, `doneEvidence` i `throughputSampleClass`.

Klasy złożoności są względne, nie czasowe:

- **E0** — aktualizacja danych/dokumentu lub reuse bez nowego pipeline;
- **E1** — pojedynczy rekord/content asset na istniejącym pipeline;
- **E2** — pakiet wymagający kilku domen lub nowego wariantu zachowania;
- **E3** — nowy system/pipeline albo pakiet z istotną integracją między domenami;
- **E4** — milestone/integracja regionalna lub ryzyko zależne od wielu niezamkniętych locków.

`E0–E4` nie mapuje się na godziny. Mapowanie powstaje wyłącznie z ukończonych próbek tego samego typu pracy.

## Baseline przepustowości

Dla każdej domeny (`code`, `quest-content`, `dialogue`, `3d-art`, `animation`, `vfx`, `audio`, `research`, `qa/integration`) utrzymujemy rolling baseline z ukończonych pakietów. Próbka jest ważna tylko gdy ma:

1. datę wejścia do aktywnej pracy i DONE;
2. stabilny scope lub zapis zmian scope;
3. klasę E0–E4;
4. informację o reuse i blokadach;
5. evidence DONE: commit/PR, asset review, test albo measurement artifact.

Nie mieszamy throughput różnych domen ani pracy eksperymentalnej z produkcyjną. Pierwsza estymata kalendarzowa dla klasy wymaga co najmniej kilku reprezentatywnych ukończonych próbek; do tego czasu status brzmi `UNBASELINED`, nie liczba dni.

## Estymata pakietu

Stan estymaty:

- `UNBASELINED` — brak wiarygodnej próbki;
- `RANGE` — istnieje baseline, podajemy przedział z mediany/rozrzutu podobnych próbek;
- `BLOCKED` — zależy od research/art/playtest/performance lock;
- `COMMITTED` — scope i dependencies są zamknięte, a zespół zaakceptował zakres milestone;
- `DONE` — istnieje evidence ukończenia.

Każda estymata zapisuje datę baseline, zestaw użytych sample IDs i confidence (`low/medium/high`). Zmiana scope unieważnia starą estymatę lub tworzy nową rewizję; nie przesuwamy po cichu daty.

## Capacity i równoległość

Capacity pochodzi z faktycznej dostępności contributorów w danym okresie. Nie zakładamy pełnego etatu, stałej liczby osób ani liniowego skalowania. Praca może być równoległa tylko gdy dependencies i ownerzy domen na to pozwalają. Wąskie gardła research, review, integration i QA są osobnymi kolejkami.

## Milestone forecasting

Forecast milestone powstaje z grafu zależności production packages, aktualnego capacity oraz baseline per domena. Raport pokazuje co najmniej wariant `likely` i szerszy `risk range`; nie używa pojedynczej daty jako gwarancji. Pakiety `BLOCKED/UNBASELINED` są jawnie wskazane i zwiększają niepewność zamiast otrzymywać arbitralny czas.

## Scope change / re-estimation

Re-estymacja jest obowiązkowa po zmianie central lore, dodaniu regionu/systemu, zmianie target platform, odrzuceniu assetu po review, zmianie performance budget albo gdy rzeczywisty czas reprezentatywnych próbek systematycznie wychodzi poza poprzedni range. Zachowujemy poprzednią rewizję do analizy błędu estymacji.

## Minimalny ledger

```text
workPackageId
domain
scopeIds[]
complexityClass: E0|E1|E2|E3|E4
reuseClass
dependencies[]
lockState
estimateState
baselineSampleIds[]
confidence
doneEvidence[]
revision
```

Ledger może być Markdown/CSV/JSON; format maszynowy wybieramy dopiero gdy pipeline produkcyjny go potrzebuje.

## Startowy podział scope

Do ledgeru należy wprowadzać pakiety z istniejących ownerów zamiast tworzyć drugi backlog: systemy z `DocumentationCoverage.md`, MQ00–MQ56 i side-quest cards, R0–R6, production asset manifests, dialogue packages oraz measurement locks. `workPackageId` wskazuje ich istniejące stable IDs.

## QA kontraktu estymacji

- żadna liczba osobodni bez wskazanego baseline;
- żadna finalna data milestone przy `UNBASELINED` dependency;
- każdy `DONE` ma evidence;
- blocked research/art/performance nie jest liczone jako zwykła implementacja;
- reuse jest jawny, żeby nie liczyć ponownie wspólnych assetów;
- raz na milestone porównujemy forecast z rzeczywistością i aktualizujemy baseline, nie historię.

## Co pozostaje otwarte

Faktyczne velocity/capacity, czasy per klasa/domena, wielkość zespołu, daty milestone i release forecast. Te wartości są produkcyjnymi danymi pomiarowymi i nie mogą zostać wiarygodnie zamknięte samą dokumentacją.