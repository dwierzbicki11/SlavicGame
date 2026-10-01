# Combat Design — v0.1

## Cel

Walka ma być czytelna, oparta na pozycji, staminy i przygotowaniu. Nie powinna zastępować tropienia ani rozwiązań społecznych/rytualnych.

## Rdzeń melee

Docelowe akcje:
- lekki atak;
- ciężki atak;
- blok;
- unik;
- atak kierunkowy;
- zmiana celu opcjonalna;
- użycie przedmiotu;
- czar.

## Stamina

Zużywana przez:
- sprint;
- ataki;
- blok;
- unik;
- wybrane czary.

Wyczerpanie nie powinno całkowicie odbierać sterowania. Gracz może się poruszać, ale traci dostęp do kosztownych akcji.

## Timing

Atak posiada:
- windup;
- active window;
- recovery;
- stamina cost;
- range;
- damage type.

Animacja i hit detection muszą korzystać z jednego źródła czasu.

## Blok

Blok:
- redukuje albo zatrzymuje określone typy ataków;
- kosztuje staminę;
- może zostać przełamany.

Parry pozostaje opcjonalne do testów.

## Unik

Unik:
- przemieszcza gracza;
- respektuje kolizję;
- kosztuje staminę;
- ma cooldown/recovery.

I-frames nie są jeszcze zatwierdzone.

## Broń

Planowane kategorie:
- miecz;
- topór;
- włócznia;
- nóż;
- maczuga;
- łuk;
- broń magiczna / artefakt;
- broń z materiałów potworów.

Każda kategoria musi różnić się zachowaniem, nie tylko statystyką DPS.

## Łuk

Osobny etap po stabilizacji melee:
- naciąg;
- celowanie;
- stamina albo stabilność;
- typy strzał;
- odzyskiwanie części amunicji.

## Armor

Armor wpływa na:
- redukcję;
- odporności;
- ewentualnie mobilność.

Nie zakładamy jeszcze klas wagowych.

## Damage types

Technicznie przygotowane:
- Physical;
- Fire;
- Cold;
- Poison;
- Spirit;
- Divine;
- Ancient.

Lista może zostać zmieniona po pierwszych rzeczywistych potworach.

## AI telegraphing

Przeciwnik musi komunikować:
- wykrycie;
- przygotowanie ataku;
- zasięg;
- zmianę fazy;
- odwrót.

## Śmierć przeciwnika

Nie każdy przeciwnik wymaga śmierci. AI może mieć stany:
- pokonany;
- odstraszony;
- uspokojony;
- związany rytuałem;
- martwy.

## Pierwszy cel implementacyjny

1. melee attack;
2. front arc;
3. cooldown;
4. stamina;
5. damage to predator;
6. block;
7. dodge;
8. HUD feedback;
9. save state;
10. tuning.
