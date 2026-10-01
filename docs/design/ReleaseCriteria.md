# Kryteria pełnej gry i wydania — v0.1

Dokument wspiera etap 23 roadmapy. Nie oznacza, że projekt znajduje się blisko wydania.

## Milestone'y

### Prototype
Podstawowe systemy techniczne.

### Vertical Slice
Jedna pełna, reprezentatywna pętla jakości docelowej.

### Alpha
Cała główna ścieżka możliwa do przejścia; część treści i polishu brakująca.

### Beta
Feature complete; nacisk na błędy, balans, performance i UX.

### Release Candidate
Brak znanych blockerów wydania.

### 1.0
Wersja publiczna spełniająca kryteria jakości.

## Blockery wydania

- utrata lub korupcja save;
- softlock głównej historii;
- niemożliwy do ukończenia obowiązkowy quest;
- crash na wspieranym sprzęcie;
- poważne problemy wejścia;
- brak czytelności obowiązkowej informacji;
- krytyczny spadek wydajności w głównej ścieżce.

## Wymagania save

- migracje wersji;
- kopia bezpieczeństwa;
- odporność na przerwany zapis;
- testy kluczowych stanów;
- brak wielokrotnego przyznawania nagród.

## Wymagania content

- główna historia kompletna;
- epilogi działają;
- brak krytycznych placeholderów;
- źródła dokumentacji historycznej uporządkowane;
- nazwy i lore spójne.

## Wymagania techniczne

- określone minimalne i zalecane wymagania;
- stabilny installer/build;
- logowanie błędów;
- ustawienia grafiki;
- audio settings;
- input rebinding;
- testy Windows/Linux zgodnie z listą wsparcia.

## Wymagania UX

- tutorial nie blokuje doświadczonego gracza;
- napisy;
- skalowanie UI;
- ustawienia czułości;
- alternatywy dla efektów utrudniających czytelność;
- accessibility review.

## Android

Nie jest warunkiem PC 1.0, chyba że plan wydawniczy zostanie później zmieniony.

## Post-launch

Przed 1.0 ustalamy:
- format save compatibility;
- politykę patchy;
- crash reports;
- listę wspieranych wersji;
- sposób dodawania treści bez niszczenia istniejących zapisów.
