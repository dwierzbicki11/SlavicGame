# Szablon encounteru — v0.1

```text
ID:
Region:
Type:
Purpose:
Prerequisites:
Trigger:
TimeWindow:
WeatherConditions:
Participants:
InitialPositions:
Telegraph:
PlayerGoal:
ActorGoals:
Escalation:
EscapeRoute:
NonCombatOptions:
CombatOptions:
Rewards:
WorldEffects:
QuestEffects:
RepeatPolicy:
Cooldown:
Persistence:
FailureHandling:
AudioVisualNeeds:
QA:
```

## Zasady

- encounter nie zaczyna się „znikąd” bez telegraphu, jeśli jest groźny;
- quest-critical encounter ma powtarzalne okno albo alternatywną ścieżkę;
- RepeatPolicy jest jawne: Once / Cooldown / StateDependent / Repeatable;
- NonCombatOptions może być puste tylko świadomie;
- Persistence opisuje trigger, wynik i stan aktorów osobno.

## QA

- wejście z każdej dostępnej strony nie blokuje gracza;
- można opuścić encounter, jeśli projekt nie mówi inaczej;
- trigger nie odpala się wielokrotnie w tej samej klatce;
- save/load przed i po encounterze jest deterministyczny.
