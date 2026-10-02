# Production region bibles — indeks R0–R6

## Status

Po scaleniu bible R6 wszystkie siedem regionów z `MacroGeography.md` ma osobny production region bible v0.1. Ten indeks jest punktem wejścia dla implementacji i produkcji contentu; nie zastępuje dokumentów regionów.

| ID | Region | Production bible | Status |
|---|---|---|---|
| R0 | Żarnowiec | `RegionBibleR0Zarnowiec.md` | v0.1 |
| R1 | Nadborze | `RegionBibleR1Nadborze.md` | v0.1 |
| R2 | Wielki Bór | `RegionBibleR2WielkiBor.md` | v0.1 |
| R3 | Przymorze | `RegionBibleR3Przymorze.md` | v0.1 |
| R4 | Kamienne Wyżyny | `RegionBibleR4KamienneWyzyny.md` | v0.1 |
| R5 | Równiny Arel | `RegionBibleR5RowninyArel.md` | v0.1 |
| R6 | Pustkowie Pierwszego Progu | `RegionBibleR6PustkowiePierwszegoProgu.md` | v0.1 |

## Co oznacza v0.1

Każdy region ma wystarczający szkielet do dalszego planowania implementacji: rolę w kampanii, topologię i traversal, rodziny encounterów/contentu, persistence/save-load, wymagania assetowe oraz kryteria QA. Szczegóły oznaczone w bible jako research lock, art lock, playtest lock lub performance measurement pozostają otwarte i nie mogą być wypełniane przez zgadywanie.

## Wspólne zależności

Implementacja regionów powinna korzystać ze wspólnych kontraktów zamiast tworzyć regionalne duplikaty:
- `WorldMap.md` i `MacroGeography.md` — topologia świata;
- `CampaignRegionalFlow.md` i main quest cards — kolejność/reveal kampanii;
- `SavePersistence.md` — persistence;
- `TrackingSystem.md`, `WeatherGameplay.md`, `DayNightEvents.md` — systemy świata;
- `EncounterDesign.md` i formaty w `docs/content/` — kontrakty contentu;
- `ResearchPolicy.md` — rozdzielenie źródeł historycznych od fikcji autorskiej.

## Otwarte klasy decyzji po zamknięciu R0–R6

Region coverage jest kompletne na poziomie v0.1, ale nie oznacza production lock. Do późniejszego domknięcia pozostają przede wszystkim:
- finalne roster NPC, istot i encounterów dla regionów;
- dokładne liczby oraz rozmieszczenie osad, landmarków i side questów;
- finalne asset listy poza vertical slice;
- szczegóły architektury, stroju, propsów i ruin wymagające research/art locku;
- tuning pogody, traversal, ekonomii i anomalii po playtestach;
- zmierzone budżety streamingu, VFX, cieni, AI i targety performance;
- finalne questowe progi evidence/gating tam, gdzie właścicielem decyzji jest quest card.

## Następny próg dokumentacji

Po region bibles priorytetem nie jest tworzenie kolejnych regionów. Należy domykać brakujące production packages wymagane przez `DocumentationCoverage.md`: komplet głównych quest cards, finalne research cards/rosters, asset listy poza vertical slice, culture research packages oraz później parametry wynikające z pomiarów i playtestów.
