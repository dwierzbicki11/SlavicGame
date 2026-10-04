# Side questy Pogranicza Żarnowca — pula v0.1

To kandydaci do rozszerzenia regionu po działającym głównym kontrakcie. Nie wszystkie muszą wejść do vertical slice.

## 1. „Zaginione narzędzia”

Typ:
- mały quest wspólnoty.

Problem:
- z miejsca pracy w puszczy zniknęły narzędzia.

Twist:
- nie musi być supernatural.

Cel projektowy:
- pokazać, że nie każdy problem prowadzi do potwora.

Rozwiązania:
- znaleźć zgubę;
- odkryć kradzież;
- odkryć błędne oskarżenie.

## 2. „Zamknięta droga”

Typ:
- mały quest przeprawy.

Problem:
- przewrócone drzewo/zalanie odcina boczną drogę.

Cel:
- wykorzystać world state i usługę crossing-keeper.

Możliwe:
- naprawa;
- obejście;
- zapłata za pomoc.

## 3. „Nocny znak”

Typ:
- investigation.

Problem:
- ktoś zostawia znaki na skraju wsi.

Twist:
- mogą być ludzkim ostrzeżeniem, nie magią.

Cel:
- uczyć odróżniania symbolu od magic signature.

## 4. „Lek dla dziecka”

Typ:
- herbalist/personal.

Problem:
- potrzebny składnik z lasu.

Cel:
- alchemy/gathering bez walki.

Ryzyko:
- unikać banalnego timer fail bez powodu.

## 5. „Niechciany gość”

Typ:
- neutral supernatural encounter.

Problem:
- byt pojawia się przy gospodarstwie.

Rozwiązania:
- odstraszenie;
- zrozumienie przyczyny;
- przeniesienie źródła problemu.

Wymaga research card przed finalną tożsamością.

## Kolejność wdrożenia

Po vertical slice:
1. Zaginione narzędzia;
2. Lek dla dziecka;
3. Zamknięta droga;
4. Nocny znak;
5. Niechciany gość.

Pierwsze trzy można zrobić bez nowej złożonej istoty.


## Runtime implementation — side-r0-missing-tools

„Zaginione narzędzia” is now implemented as a small vertical-slice side quest under the stable runtime ID:

`side-r0-missing-tools`

It deliberately does **not** use `SQ_R0_01`, because the production side-quest catalog reserves `SQ_R0_01` for „Złamany bród”.

### Owner
- giver: `settler-woodworker-01`;
- quest is optional and has no MQ dependency;
- ignoring it cannot block `light-over-swamp` or later campaign progression.

### Runtime flow
1. talk to the woodworker and accept the request;
2. search the forest worksite near the fallen trunk;
3. recover the visible `siekiera_r0_01.glb` quest tool;
4. optionally inspect the worksite context;
5. return to the woodworker.

### Outcomes
- `MisplacedConfirmed` — tool + worksite evidence; player correctly concludes there is no theft;
- `ReturnedUncertain` — tool returned without inventing an explanation;
- `FalseAccusation` — player accuses someone without evidence.

The quest is intentionally mundane: no supernatural explanation is introduced.

### Prototype rewards
- careful evidence-based conclusion: 20 Money, +4 old-village reputation, +8 woodworker Trust;
- uncertain but honest return: 15 Money, +2 reputation, +4 Trust;
- unsupported accusation: 10 Money, no reputation, -5 Trust.

Values remain playtest-tunable.

### Persistence
Quest phase/evidence, returned-tool state, outcome flags, reward claim, reputation, relationship and money all use existing save contracts. Reward synchronization is idempotent and cannot pay twice after reload.

### Evidence
- `side-r0-missing-tools.tool-found` — Observation;
- `side-r0-missing-tools.worksite-context` — ConfirmedFact that the worksite context does not support theft.

This side quest teaches the same evidence discipline as the main contract on a much smaller non-supernatural problem.
