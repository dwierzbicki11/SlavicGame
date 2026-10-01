# Produkcja treści i rozbudowa świata — v0.1

Dokument wspiera etap 22 roadmapy.

## Zasada

Nowy region powstaje dopiero po udowodnieniu jakości jego mechanik w mniejszej skali.

## Pipeline regionu

1. cel fabularny;
2. filary gameplay;
3. mapa topologiczna;
4. research;
5. graybox;
6. ścieżki gracza;
7. NPC i questy;
8. encounter design;
9. art pass;
10. audio;
11. optymalizacja;
12. playtest;
13. poprawki;
14. lock treści.

## Definition of Ready

Region nie wchodzi do pełnej produkcji bez:
- funkcji w głównej historii albo świecie;
- kosztorysu treści;
- listy assetów;
- research questions;
- listy mechanik;
- mapy zależności questów.

## Definition of Done

Region:
- ma działającą pętlę dnia/nocy;
- ma powody powrotu;
- posiada bezpieczne i ryzykowne strefy;
- reaguje na minimum jedną większą decyzję;
- ma save/load;
- przechodzi test wydajności;
- nie ma placeholderów krytycznych dla głównej ścieżki.

## Pipeline NPC

1. rola;
2. cel;
3. konflikt;
4. harmonogram;
5. relacje;
6. dialog;
7. reakcje na questy;
8. model/animacja;
9. głos albo plan braku VO;
10. test po zmianie świata.

## Pipeline potwora

1. karta badawcza;
2. interpretacja F;
3. ekologia;
4. telegraphing;
5. AI;
6. combat / alternatives;
7. rewards;
8. model;
9. animacje;
10. audio;
11. bestiary entry;
12. test.

## Pipeline zadania

Używa [QuestDesign.md](QuestDesign.md) i [DecisionModel.md](DecisionModel.md).

## Reuse

Asset może być ponownie użyty, ale region nie może wyglądać jak kopia z innym kolorem.

## Proceduralność

Proceduralne narzędzia mogą wspierać:
- teren;
- roślinność;
- dekoracje;
- warianty.

Nie powinny generować głównych questów bez ręcznej kontroli.

## Kontrola scope

Każdy feature musi odpowiedzieć:
- czy vertical slice go potrzebuje?
- czy odblokowuje wiele treści?
- ile kosztuje utrzymanie?
- co można odłożyć?
