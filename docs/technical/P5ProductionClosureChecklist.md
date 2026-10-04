# P5 Production Closure Checklist

Ten dokument jest checklistą zamknięcia P5. Nie zastępuje owner-speców, scenario manifests, `P5EvidenceLedger.md`, `P5CaptureArtifactFormat.md` ani `P5EvidencePromotionRunbook.md`. Jego rolą jest odpowiedzieć jednoznacznie: **czy dokumentacja może już zostać oznaczona jako kompletna produkcyjnie?**

## Zasada nadrzędna

P5 jest zamknięte dopiero wtedy, gdy wszystkie wymagane rekordy evidence są oparte na rzeczywistych captures dla wskazanego candidate SHA. Zielone CI, ręczny summary, dokument planistyczny lub istnienie scenariusza nie jest dowodem pomiarowym.

## A. Gotowość infrastruktury pomiarowej

- [ ] recorder zapisuje pakiet zgodny z `P5CaptureArtifactFormat.md`;
- [ ] validator odrzuca brak provenance, niespójne run/scenario/EV IDs, brakujące wymagane fazy i uszkodzone artefakty;
- [ ] exact build SHA, konfiguracja, platforma i hardware są zapisywane automatycznie;
- [ ] frame-time collector działa w wymaganych scenariuszach;
- [ ] CPU/GPU collectors są dostępne tam, gdzie platforma je wspiera, a brak wsparcia jest jawny zamiast kodowany jako zero;
- [ ] RAM/VRAM i streaming telemetry mają raw samples;
- [ ] AI/encounter telemetry ma raw samples i phase markers;
- [ ] animation/VFX/audio telemetry ma raw samples lub jawny unsupported marker;
- [ ] gameplay collectors pokrywają combat, economy/progression, evidence/reputation i traversal/world;
- [ ] save/fail-forward oraz long-session runs generują kompletne artefakty.

Dopóki ten blok nie jest spełniony, scenariusz pozostaje `SPECIFIED`/`PARTIAL`, a nie `RUNNABLE`.

## B. Gameplay evidence

Wykonać manifesty z owner dokumentów i promować odpowiadające wpisy `EV-*` zgodnie z runbookiem:

- [ ] combat baseline i pressure cases;
- [ ] economy/progression baseline, alternative acquisition, restock/arbitrage;
- [ ] evidence valid path i source-loss fail-forward;
- [ ] reputation/service gates i conflicting actions;
- [ ] traversal baseline, difficult terrain i recovery;
- [ ] weather transitions i adverse-weather gameplay;
- [ ] day/night boundary i time-gated fail-forward;
- [ ] combined night + weather + traversal pressure.

Każdy finalny tuning value musi wskazywać evidence, które go uzasadnia. Liczby bez powiązanego runu pozostają otwartym playtest lockiem.

## C. Performance evidence

- [ ] R0–R6 wymagane performance scenarios wykonane;
- [ ] cross-region long run wykonany;
- [ ] combat spike wykonany;
- [ ] save round-trip wykonany;
- [ ] degraded-content/fail-forward wykonany;
- [ ] long-session stability wykonany;
- [ ] frame-time target oparty na captures;
- [ ] CPU/GPU targety oparte na captures;
- [ ] RAM/VRAM targety oparte na captures;
- [ ] streaming IO/residency/prefetch targety oparte na captures;
- [ ] AI CPU/query/agent budget oparty na captures;
- [ ] AVFX/audio ceilings oparte na captures.

## D. Hardware qualification

Minimalne i rekomendowane wymagania sprzętowe mogą zostać zapisane dopiero po pełnym wymaganym zestawie pomiarów.

- [ ] minimal hardware profile ma pełny required manifest set;
- [ ] recommended hardware profile ma pełny required manifest set;
- [ ] ustawienia renderingu/upscalingu użyte w kwalifikacji są zapisane;
- [ ] wersje sterowników/runtime/OS są zapisane;
- [ ] nie ma krytycznych invalidated runs bez ważnego rerunu.

## E. Release candidate evidence

- [ ] wskazany jest jeden exact candidate SHA;
- [ ] wymagane CI dla candidate SHA jest zielone;
- [ ] wymagane captures dotyczą candidate SHA albo mają jawnie zaakceptowaną politykę reuse;
- [ ] save compatibility/round-trip evidence jest ważne;
- [ ] fail-forward evidence jest ważne;
- [ ] long-session stability evidence jest ważne;
- [ ] krytyczne gameplay thresholds mają status co najmniej `CANDIDATE`;
- [ ] finalnie zatwierdzone targety mają status `LOCKED` w ledgerze;
- [ ] brak otwartego blocker-class evidence gap.

## F. Dokumentacyjny production lock

Dopiero po A–E można wykonać końcowy pass dokumentacji:

1. wpisać zmierzone wartości do ich owner-speców, nie do tej checklisty;
2. uaktualnić `P5EvidenceLedger.md` z raw artifact locators i statusami;
3. uaktualnić `DocumentationCoverage.md`, usuwając wyłącznie decyzje faktycznie zamknięte dowodem;
4. uaktualnić `DocumentationWorkQueue.md` i oznaczyć P5 jako zakończone;
5. sprawdzić wszystkie odwołania/indeksy i brak osieroconych `EV-*`;
6. uruchomić pełne GitHub Actions;
7. scalić wyłącznie po zielonym CI;
8. zapisać datę zakończenia i ostatni SHA `main` w raporcie końcowym.

## Warunek STOP

Jeżeli wszystkie wymagania definicji pełnej dokumentacji z `DocumentationCoverage.md` oraz A–F powyżej są spełnione, dalsze tworzenie dokumentów bez nowej potrzeby produkcyjnej jest zabronione. Od tego momentu dokumentacja ma status **production complete**, a kolejne przeglądy mają jedynie potwierdzać stan lub reagować na rzeczywistą zmianę projektu.

## Stan przy utworzeniu

Na 2026-10-04 próg swobodnej implementacji jest osiągnięty, ale A–E nie są jeszcze spełnione: recorder/collectors wymagają implementacji, ledger nie ma kompletu realnego evidence, a finalne tuning/performance/hardware/release locki pozostają otwarte. Dlatego ta checklista nie deklaruje ukończenia produkcyjnego.