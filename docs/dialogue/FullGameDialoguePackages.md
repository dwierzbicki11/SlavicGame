# Full-game dialogue packages — v0.1

## Cel

Ten dokument rozszerza istniejące dialogi vertical slice na pełny scope R0–R6. Nie jest finalnym skryptem literackim ani VO script. Definiuje implementation-ready pakiety, stabilne ID, zależności, persistence i kryteria QA, tak aby questy/NPC mogły być implementowane bez czekania na finalny wording.

## Jednostka danych

Każdy pakiet ma stabilne `dialoguePackageId` i zawiera grafy należące do jednego celu produkcyjnego. Minimalny rekord:

- `dialoguePackageId`;
- `ownerNpcId` albo `ownerRoleId`;
- `regionId`;
- `questIds[]` / `serviceIds[]` / `encounterIds[]`;
- `entryNodes[]`;
- `requirements[]`;
- `effects[]`;
- `memoryFlags[]`;
- `fallbackNode`;
- `postResolutionNodes[]`;
- `localizationKeyPrefix`;
- opcjonalne `voiceAssetId` — nigdy wymagane do działania grafu.

Finalny tekst gracza/NPC jest lokalizowanym contentem. Logika questa nie może zależeć od porównywania tekstu linii.

## Rodziny pakietów

| ID | Zakres | Wymagany kontrakt |
|---|---|---|
| `DLG_R0_SLICE_CORE` | pięć ról vertical slice | istniejące grafy `dialogue/*Dialogue.md`, reakcja po rozwiązaniu |
| `DLG_R0_COMMUNITY` | mieszkańcy i ambient R0 | world-state variants bez questowego softlocku |
| `DLG_R1_CROSSING` | przeprawa, kontrola, transport, spory | quest/evidence/reputation gates + fallback przejścia |
| `DLG_R1_TRADE` | handel/usługi R1 | service availability i world-state reaction |
| `DLG_R2_FOREST_EDGE` | osady skraju boru | reakcje na wejście/wyjście, wiedzę i stan regionu |
| `DLG_R2_GUARDIAN` | kontakt związany z `ENTITY_FOREST_GUARDIAN_F` | bez fałszywego historycznego nazewnictwa; avoidance/fail-forward |
| `DLG_R3_PORT` | port, łodzie, salvage, handel | service/quest gates; warianty po zmianie sytuacji portu |
| `DLG_R3_WATER` | wiedza/ostrzeżenia dotyczące water actor | evidence gate; bez ekspozycyjnego ujawnienia author truth |
| `DLG_R4_WORKSITE` | wyrobiska, warsztaty, transport zasobów | stan pracy/konfliktu + service hooks |
| `DLG_R4_OPEN_GROUND` | otwarte pola i lokalne ostrzeżenia | time/place knowledge; południca tylko zgodnie z region-fit lockiem |
| `DLG_R5_ROUTE` | trasy, obozy, wymiana, przewodnicy | camp/route state + reputation/service hooks |
| `DLG_R5_OLD_SITE` | stare drogi/miejsca i pamięć kulturowa | knowledge/evidence gates; bez wymuszania jednej realnej analogii Arelów |
| `DLG_R6_EXPEDITION` | finałowa wyprawa i wsparcie | party/world-state variants, ograniczone usługi |
| `DLG_R6_THRESHOLD` | anomalie, odkrycia, central lore | progresywne ujawnianie; nie może ominąć questowych evidence gates |
| `DLG_CAMPAIGN_COMPANIONS` | companion lifecycle R0–R6 | relationship/world-state/death/availability variants |
| `DLG_CAMPAIGN_EPILOGUE` | reakcje i epilog | ending/world flags; brak nowych obowiązkowych decyzji po finale |

## Pakiet questa

Każdy MQ/SQ, który używa dialogu, może referować jedną z powyższych rodzin i własny `graphId`. Minimalny przebieg ma obsłużyć:

1. wejście przed przyjęciem / przed odkryciem problemu;
2. stan aktywny;
3. co najmniej jedną reakcję na istotny dowód lub zmianę świata, jeśli quest ją posiada;
4. rozwiązanie;
5. reakcję po rozwiązaniu;
6. fallback, gdy owner NPC jest niedostępny, martwy albo przeniesiony, jeżeli informacja jest krytyczna dla progresji.

Side-content nie musi mieć wszystkich sześciu stanów, jeśli karta questa jawnie ich nie potrzebuje.

## Ujawnianie wiedzy

Dialog rozróżnia:

- `heard`: pogłoska/opinia NPC;
- `observed`: informacja wynikająca z obserwacji gracza;
- `evidence`: potwierdzony dowód mechaniczny;
- `authorTruth`: prawda dokumentacji, której NPC ani gracz nie muszą znać.

`authorTruth` nie jest automatycznie linią dialogową. Central lore ujawniamy tylko w punktach przewidzianych przez main quest/evidence progression.

## Persistence

Zapisujemy stabilne skutki, nie historię każdej linii:

- istotne wybory;
- przekazane informacje/dowody;
- quest phase;
- relację/reputację, jeśli zmieniona;
- jednorazowe ujawnienia;
- status owner NPC potrzebny do wyboru fallbacku.

Historia UI dialogu może być osobnym logiem sesji/save, ale nie jest źródłem prawdy gameplayu.

## Fail-forward

Informacja obowiązkowa dla MQ nie może istnieć wyłącznie w jednym killable/optional NPC. Dopuszczalne fallbacki:

- drugi NPC/rola;
- dokument/ślad/evidence interaction;
- world-state interaction;
- zastępczy node po zmianie ownera usługi.

Fallback nie musi dawać identycznego tonu, nagrody ani reputacji; musi zachować możliwość progresji.

## Localization i VO

- wszystkie finalne linie używają kluczy lokalizacyjnych;
- choice text ma osobny klucz od wypowiedzi NPC;
- placeholder copy może istnieć przed finalnym writing pass;
- VO jest opcjonalną warstwą assetową;
- brak nagrania, przerwany asset audio lub wyłączone VO nie blokuje dialogu;
- timing wyborów nie może zależeć od długości nagrania.

## Otwarte locki

Ten pass **nie** zamyka:

- finalnego wording/tone każdej linii;
- castingu i pełnego VO;
- finalnych nazw roboczych NPC oznaczonych F;
- wszystkich ambient barks i banterów;
- dokładnych relationship/reputation thresholds;
- finalnej lokalizacji i korekty językowej.

Nie są to blokery implementacji grafów i quest hooks.

## Minimalne QA

Dla każdego pakietu produkcyjnego:

1. każdy `entryNode` osiąga koniec albo jawny loop;
2. każdy effect używa stabilnego ID/flag contract;
3. brak wymaganej informacji tylko w jednym niedostępnym NPC;
4. save/load zachowuje jednorazowe ujawnienia i wybory;
5. quest resolution przełącza właściwy post-resolution node;
6. brak VO nadal pozwala przejść cały graf;
7. localization key missing daje diagnostyczny fallback, nie crash;
8. author-truth nie wycieka przed przewidzianym gate'em.

## Definition of done pakietu

Pakiet jest implementation-ready, gdy ma ownera/rolę, entry conditions, graph/state list, effects, persistence, fallback i QA. `writing-final` wymaga dodatkowo finalnego tekstu i localization pass; `VO-final` jest osobnym późniejszym lockiem.

## Implemented runtime package — DLG_R0_COMMUNITY

`DLG_R0_COMMUNITY` is now executable for all fourteen ambient Żarnowiec settlers:

- `settler-farmer-01`;
- `settler-farmer-02`;
- `settler-woodworker-01`;
- `settler-potter-01`;
- `settler-trader-01`;
- `settler-carrier-01`;
- `settler-elder-01`;
- `settler-traveler-01`;
- `settler-smith-helper-01`;
- `settler-weaver-01`;
- `settler-shepherd-01`;
- `settler-gatherer-01`;
- `settler-fisher-01`;
- `settler-youth-01`.

Each graph has stable entry nodes for:

- normal daytime fallback;
- active `light-over-swamp` investigation;
- predator removed;
- apparition released;
- both threats resolved;
- rain/storm;
- night.

The selector uses current world state at conversation start. Ambient community graphs are intentionally non-critical: they never carry unique mandatory quest information, never advance the main quest and always expose a safe exit choice.

The package therefore adds world reactivity without creating a softlock dependency on optional settlers. Dialogue start still uses normal proximity and freezes only the active speaker while the rest of the village routine continues.

Current text remains production-placeholder Polish copy. Final localization/writing polish and optional VO remain separate later locks.
