# Format lokacji i interakcji — v0.1

## Location card

```text
ID:
Region:
DisplayNameKey:
Role:
Bounds:
Entrances:
Exits:
Landmarks:
Services:
NPCs:
Encounters:
Resources:
DayState:
NightState:
WeatherOverrides:
QuestStates:
TravelConnections:
AudioZones:
Assets:
PerformanceBudget:
ResearchBasis:
OpenQuestions:
QA:
```

## Interactable card

```text
ID:
Location:
Type:
PromptKey:
Requirements:
AvailableActions:
Effects:
QuestEffects:
WorldFlags:
Cooldown:
OneShot:
Persistence:
Animation:
Audio:
VFX:
FailureFeedback:
QA:
```

## Typy interactable

- ItemPickup;
- Door;
- Container;
- Evidence;
- NPC;
- CraftStation;
- RitualPoint;
- RestPoint;
- WorldObject;
- TravelNode.

## Reguły

- interakcja ma jeden właścicielski system;
- renderer nie zmienia quest state bezpośrednio;
- one-shot zapisuje wykonanie;
- wymaganie niedostępne daje feedback, jeśli gracz wie o tej możliwości;
- lokalizacja nie trzyma logiki konkretnego questa na stałe, tylko referencje/stan.
