# Time-dependent event format — v0.1

## Cel

Ten kontrakt opisuje eventy, których dostępność lub zachowanie zależy od czasu świata. Uzupełnia `world/DayNightEvents.md`: tabela świata mówi **co** ma się wydarzać, a ten dokument definiuje **jak zapisać to jako dane contentowe** bez kodowania pojedynczych questów w schedulerze.

## Zasady

- czas świata jest wejściem do eventu, nie jedynym warunkiem;
- event może mieć kilka okien czasowych;
- przegapienie zwykłego okna nie może samo z siebie failować questa, chyba że karta questa jawnie definiuje deadline;
- priorytet rozstrzygania pozostaje: quest event > unique encounter > schedule > ambient;
- event jednorazowy zapisuje trwałą flagę;
- definicja contentowa nie może zawierać logiki specyficznej dla kodu/UI.

## Minimalny rekord

```yaml
id: event.swamp.light_manifestation
version: 1
kind: quest
priority: 300
location_ids:
  - loc.black_marsh
windows:
  - phase: night
    start: "20:00"
    end: "05:00"
conditions:
  all:
    - quest_state: quest.light_over_swamp.investigation
    - flag_not_set: event.swamp.light_manifestation.completed
cooldown:
  mode: next_matching_window
repeat:
  mode: until_completed
activation:
  radius_m: 45
  requires_player_present: true
outcomes:
  on_start:
    - set_flag: event.swamp.light_manifestation.seen
  on_complete:
    - set_flag: event.swamp.light_manifestation.completed
persistence: persistent
fallback:
  missed_window: reschedule
  invalidated: disable
```

## Pola

### `id`

Trwałe ID zgodne z `IdConventions.md`. Nie zmieniamy go po publikacji save-compatible contentu.

### `version`

Liczba całkowita używana przy migracji danych i diagnostyce save'ów.

### `kind`

Dozwolone wartości v0.1:

- `quest` — zdarzenie wynikające z aktywnego questa;
- `unique_encounter` — unikalne spotkanie świata;
- `schedule` — zmiana zachowania/usługi/NPC wynikająca z harmonogramu;
- `ambient` — powtarzalne zdarzenie budujące świat.

### `priority`

Liczba do deterministycznego sortowania w obrębie `kind`. Wyższa wygrywa. Nie zastępuje globalnej kolejności rodzajów eventów.

### `location_ids`

Lista miejsc, w których event może zostać aktywowany. Pusta lista jest niedozwolona dla eventów przestrzennych.

### `windows`

Jedno lub więcej okien. Każde może użyć:

- `phase`: `dawn`, `day`, `dusk`, `night`;
- opcjonalnie `start` / `end` w czasie świata dla dokładniejszego okna;
- opcjonalnie później `season`, gdy system sezonów zostanie zamrożony.

Okno przechodzące przez północ (`20:00`–`05:00`) jest prawidłowe.

### `conditions`

Warunki wejściowe są deklaratywne. v0.1 dopuszcza co najmniej:

- stan questa;
- ustawioną/nieustawioną flagę;
- obecność gracza;
- warunek pogodowy;
- wymagany/zakazany stan NPC;
- wymagany przedmiot/tag.

Silnik powinien traktować nieznany typ warunku jako błąd walidacji contentu, a nie jako `false`.

### `cooldown`

- `none`;
- `next_matching_window`;
- `duration_game_minutes` + wartość.

### `repeat.mode`

- `once`;
- `until_completed`;
- `repeatable`.

### `activation`

Parametry uruchomienia, np. promień oraz wymóg obecności gracza. Event może być aktywny logicznie bez natychmiastowego odpalania sceny/encounteru.

### `outcomes`

Deklaratywne skutki wejścia, ukończenia lub anulowania. Operacje muszą korzystać z jawnie wspieranych akcji content runtime (np. flagi/quest state), bez arbitralnego kodu.

### `persistence`

- `persistent` — stan musi trafić do save;
- `session` — może zniknąć po wyjściu do menu;
- `ephemeral` — bez zapisu.

Quest/unique event, który wpływa na historię, musi być `persistent`.

### `fallback`

`missed_window`:

- `reschedule` — wraca w kolejnym pasującym oknie;
- `disable` — znika;
- `quest_defined` — zachowanie opisuje karta questa.

`invalidated` określa zachowanie po utracie warunków (`disable` albo `wait`).

## Scheduler — kontrakt deterministyczny

Dla każdej zmiany czasu / wejścia do lokacji scheduler:

1. zbiera eventy pasujące do lokacji i okna;
2. odrzuca niespełnione `conditions`;
3. odrzuca eventy z aktywnym cooldownem lub zakończone `once`;
4. sortuje po globalnym `kind`, następnie malejąco po `priority`, na końcu leksykograficznie po `id`;
5. aktywuje pierwszy event, jeśli jego typ wymaga wyłączności;
6. zapisuje trwałe zmiany atomowo z odpowiednim quest/world state.

Leksykograficzne `id` jako ostatni tie-breaker zapewnia powtarzalność testów.

## Walidacja contentu

Build/CI powinien odrzucić rekord, jeśli:

- brakuje `id`, `kind`, `windows`, `repeat` lub `persistence`;
- ID jest zduplikowane;
- czas ma nieprawidłowy format;
- `repeat.mode: once` nie ma trwałego sposobu rozpoznania wykonania;
- story-changing event nie jest persistent;
- wskazane location/quest/item ID nie istnieje;
- typ condition/outcome jest nieznany.

## Save/load

Save przechowuje wyłącznie stan runtime potrzebny do odtworzenia eventu, np. `completed`, `seen`, cooldown i wymagane flagi. Definicja eventu pozostaje w danych gry i nie jest kopiowana do save.

Po load scheduler ponownie oblicza dostępność na podstawie aktualnego czasu, lokacji i zapisanych flag. Event nie może odpalić drugi raz tylko dlatego, że zapis wykonano w jego oknie czasowym.

## QA minimum

Dla każdego eventu czasowego sprawdzamy:

1. minutę przed początkiem okna;
2. dokładny początek;
3. środek okna;
4. dokładny koniec;
5. okno przechodzące przez północ;
6. save/load przed aktywacją i po aktywacji;
7. konflikt dwóch eventów tego samego `kind`;
8. konflikt quest eventu z ambientem;
9. przegapienie okna i poprawny fallback;
10. zmianę warunku podczas oczekiwania.

## Otwarte decyzje

- dokładny format serializacji runtime (`YAML/JSON/binary`) pozostaje decyzją implementacyjną, o ile zachowuje powyższy kontrakt;
- sezony nie wchodzą do v0.1 schedulera, dopóki kalendarz/sezonowość świata nie zostaną zamrożone;
- jednoczesne eventy niewyłączne wymagają osobnej polityki concurrency po pierwszych playtestach.
