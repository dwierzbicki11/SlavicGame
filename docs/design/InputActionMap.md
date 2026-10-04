# Input Action Map — v0.1

## Cel

Gameplay korzysta z nazwanych akcji zamiast bezpośrednich Key checks w wielu systemach.

## Exploration

| Action | Default KBM |
|---|---|
| MoveForward | W |
| MoveBackward | S |
| MoveLeft | A |
| MoveRight | D |
| Look | Mouse |
| Sprint | Left Shift |
| Interact | E |
| Jump | Niezaimplementowany — binding otwarty |
| Crouch | C/Ctrl — otwarte |

## Combat

| Action | Default KBM |
|---|---|
| LightAttack | LMB |
| HeavyAttack | hold LMB albo osobny binding — do testu |
| Block | RMB |
| Dodge | Space + WASD; bez WASD unik w tył |
| Aim | RMB przy broni dystansowej |
| RangedAttack | LMB podczas Aim |
| UseQuickItem | Q |
| CastPrimary | R/F — do testu |

Konflikt kontekstowy RMB musi być rozwiązany przez aktualny tryb broni.

## UI

| Action | Default |
|---|---|
| Inventory | I |
| Journal | J |
| Map | M |
| Character | K/C — otwarte |
| Pause | Escape |
| Confirm | Enter/LMB |
| Cancel | Escape/RMB |

## System

| Action | Default |
|---|---|
| ToggleFullscreen | F11 |
| Screenshot | opcjonalnie |
| DebugOverlay | developerski binding |

## Rebinding rules

- jeden binding może zgłosić konflikt;
- UI pokazuje konflikt;
- użytkownik może zaakceptować zamianę albo anulować;
- movement axes wspierają keyboard i analog stick.

## Hold/toggle

Konfigurowalne tam, gdzie ma sens:
- sprint;
- aim;
- crouch.

## Mouse

Settings:
- sensitivity X/Y;
- invert Y;
- smoothing tylko jako opcja, nie ukryte opóźnienie.

## Controller draft

- LS move;
- RS look;
- A interact/jump kontekstowo;
- B dodge/cancel;
- X/Y actions;
- LT aim/block;
- RT attack;
- bumpers quick actions.

Finalny controller map dopiero po KBM playtest.
