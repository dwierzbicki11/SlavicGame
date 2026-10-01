# Format eventów zależnych od czasu — v0.1

```text
ID:
Region:
Purpose:
StartCondition:
TimeWindow:
RequiredQuestState:
RequiredWorldFlags:
ForbiddenWorldFlags:
WeatherCondition:
Priority:
Participants:
Actions:
PlayerTelegraph:
OnMissedWindow:
RepeatPolicy:
Cooldown:
Persistence:
ConflictGroup:
QA:
```

## Priorytety

1. CriticalQuest
2. UniqueStory
3. NPCScheduleOverride
4. RegionalEncounter
5. Ambient

Eventy w tej samej `ConflictGroup` nie uruchamiają się jednocześnie, jeśli wymagałoby to sprzecznego stanu NPC/miejsca.

## Missed window

Dozwolone:
- RepeatNextCycle;
- Reschedule;
- AlternativePath;
- ExpireWithExplicitConsequence.

Główna historia nie używa cichego permanentnego failu za przegapienie godziny.

## Determinizm

Losowanie:
- używa kontrolowanego RNG;
- zapisuje wybór, jeśli wynik wpływa na quest;
- test może wymusić warunek.

## QA

- event nie odpala dwa razy po load;
- zmiana czasu do przodu i do tyłu nie duplikuje reward;
- priorytet rozwiązuje konflikt;
- brak pogody nie blokuje krytycznego eventu bez systemu oczekiwania.
