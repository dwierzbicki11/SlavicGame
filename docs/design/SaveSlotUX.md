# Save Slot UX — v0.1

## Cel

Gracz rozumie, który zapis jest aktualny i nie może łatwo nadpisać ważnego stanu przez przypadek.

## Typy zapisu

### Autosave
Tworzony przez system w bezpiecznych punktach.

### Checkpoint
Techniczny stan do odtworzenia po śmierci.

### Manual save
Do decyzji po playtestach. Jeśli dostępny, posiada oddzielne sloty.

### Quick save
Nie jest wymagany w vertical slice.

## Slot card

Pokazuje:
- nazwę/profil postaci;
- region;
- czas gry;
- datę zapisu;
- godzinę świata opcjonalnie;
- aktualny główny quest;
- screenshot opcjonalnie.

## Overwrite

Manual overwrite wymaga potwierdzenia.

Autosave używa rotacji, np. kilka ostatnich zapisów, zamiast jednego pliku bez backupu.

## Loading

Przed load:
- opcjonalne potwierdzenie, jeśli istnieją niezapisane ręczne zmiany;
- brak sztucznego ostrzegania przy każdym autosave.

## Corruption

Jeśli save jest uszkodzony:
- nie kasujemy go automatycznie;
- próbujemy backup;
- pokazujemy jasny błąd;
- log zawiera przyczynę.

## Version mismatch

Developersko:
- dokładny numer schema;
- komunikat migracji/niezgodności.

Release:
- migracja, jeśli wspierana;
- jasny komunikat, jeśli nie.

## Death flow

Death:
1. krótki feedback;
2. Load Last Checkpoint;
3. pełne odtworzenie snapshotu;
4. brak zachowania loot/kar z odrzuconej próby.

## Vertical slice

Minimum:
- autosave/checkpoint;
- load latest;
- czytelny timestamp;
- brak duplikacji nagród;
- backup poprzedniego zapisu przy zapisie plikowym.
