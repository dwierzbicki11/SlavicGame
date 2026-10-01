# Save i persistence — v0.1

## Cel

Zapis odtwarza spójny świat, a nie tylko pozycję gracza.

## Obecny snapshot

Zapis obejmuje:
- wersję;
- pozycję;
- czas;
- pogodę;
- health/stamina;
- profil;
- pieniądze;
- tytuły;
- inventory;
- questy;
- evidence;
- reputation;
- divine relationships;
- character relationships;
- world flags;
- boundary phenomena;
- enemies.

## Checkpoint

Checkpoint powinien powstać:
- w bezpiecznych momentach;
- przed ważną sekwencją;
- po trwałym rozwiązaniu zadania;
- ręcznie, jeśli finalny design dopuści manual save.

## Śmierć

Po śmierci:
- przywracamy cały checkpoint;
- nie zachowujemy loot/kar z nieudanej próby;
- nie przyznajemy drugi raz nagród.

## Sloty

Plan:
- autosave;
- kilka manual slots;
- quick save zależnie od testów.

## Atomic write

Docelowy zapis plikowy:
1. zapis do pliku tymczasowego;
2. walidacja;
3. podmiana;
4. backup poprzedniego.

## Wersje

Każdy save ma version.

Przy zmianie schema:
- migracja;
- albo jawna niezgodność w wersji developerskiej.

Nie odczytujemy cicho niepasujących danych.

## Persisted ID

Trwałe obiekty wymagają stabilnych ID niezależnych od kolejności ładowania.

## Test

Każdy nowy system trwały dostaje roundtrip test.

## Co nie powinno trafiać do save

- GPU handles;
- chwilowy cache renderera;
- obiekty okna;
- dane możliwe do bezpiecznego odtworzenia z definicji.
