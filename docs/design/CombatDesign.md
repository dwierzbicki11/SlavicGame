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

### Runtime uniku

Left Alt + WASD wykonuje krótki unik względem kamery; bez WASD gracz cofa się.
Kierunek zostaje ustalony na początku, a kamera nadal reaguje na mysz.

| Parametr | Wartość prototypowa |
|---|---|
| Koszt staminy | 20, płatne raz przy rozpoczęciu |
| Ruch | 3 m w 0,25 s na suchym podłożu |
| Recovery | 0,55 s po ruchu; kolejny unik po 0,8 s |
| Kolizje | Istniejące przeszkody, granice mapy i wysokość terenu |

Ruch jest dzielony na małe kroki także przy długiej klatce. Płytka woda
stosuje istniejącą karę prędkości, a głęboka woda blokuje unik. Podczas ruchu
nie można rozpocząć melee, czaru ani rytuału. Trwający melee, inkantacja,
rytuał, UI i cinematic blokują rozpoczęcie uniku. Aim/Draw łuku zostaje
anulowane bez zużycia strzały, zanim release w tej klatce może wystrzelić.
Po ruchu można ponownie celować i normalnie chodzić, mimo cooldownu uniku.
Unik pozostaje niedostępny w powietrzu aż do lądowania po skoku.

HUD pokazuje binding, koszt, brak staminy i odnowienie. Unik unika zamachu
przez przemieszczenie poza obszar trafienia; nie przyznaje i-frames.
Save/load usuwa przejściowy ruch i cooldown, zachowując zapisaną pozycję
i staminę. Parametry wymagają ręcznego playtestu.

### Skok

Spacja wykonuje skok bez kosztu staminy. Impuls 6,5 m/s i grawitacja 16 m/s²
dają około 1,32 m wysokości i 0,81 s lotu na płaskim podłożu. Ruch poziomy
i kamera pozostają aktywne, a istniejące przeszkody nadal blokują przejście.
Nie można wykonać drugiego skoku w powietrzu. Skok jest blokowany podczas
uniku, melee, inkantacji, rytuału, UI, cinematic i głębokiego brodzenia.
Skok zachowuje Aim/Draw łuku. Zapis w powietrzu jest wczytywany na podłożu
z wyzerowaną prędkością pionową. Pauza zatrzymuje fizykę skoku.

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
