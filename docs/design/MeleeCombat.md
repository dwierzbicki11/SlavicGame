# Melee Combat — szczegółowy projekt v0.1

## Cel

Walka wręcz ma być czytelna, oparta na pozycji, staminy i timingach. Pierwszy prototyp nie potrzebuje wielu broni ani combo tree.

## Stan gracza

Gracz może być w stanie:
- Free;
- Windup;
- ActiveAttack;
- Recovery;
- Blocking;
- Dodging;
- Staggered;
- Dead.

Przejścia muszą być jawne i testowalne.

## Light attack

Właściwości:
- niski koszt staminy;
- szybki windup;
- krótki recovery;
- średnie obrażenia;
- front arc.

Prototyp:
- zasięg około 1.8–2.2 m do strojenia;
- jeden podstawowy atak;
- brak pełnego combo systemu.

## Heavy attack

Dopiero po stabilizacji light attack.

Właściwości:
- większy koszt;
- dłuższy windup;
- większy stagger/damage;
- większe ryzyko.

## Hit detection

Pierwszy model:
- sphere/cone/capsule w front arc;
- tylko w active window;
- max jedno trafienie jednego celu na attack instance.

Nie zadajemy obrażeń co frame.

## Facing

Atak korzysta z kierunku gracza/kamery zgodnie z finalnym modelem sterowania.

W third-person bez lock-on:
- ruch może wpływać na facing;
- kamera definiuje kierunek intencji;
- pełny character turn zostanie spięty z animacją później.

## Stamina

Atak nie startuje bez minimalnego kosztu.

Po ataku:
- recovery;
- regen delay.

Nie zabieramy pełnej kontroli przy zerowej staminie.

## Block

Block:
- działa na określony frontal angle;
- kosztuje staminę przy trafieniu;
- może zostać przełamany.

Po guard break:
- krótki stagger.

## Dodge

Dodge:
- krótki burst ruchu;
- respektuje obstacle collision;
- kosztuje staminę;
- ma recovery.

I-frames pozostają do playtestu; pierwsza wersja może opierać się głównie na realnym przemieszczeniu.

## Stagger

Przeciwnik i gracz mogą mieć threshold/reakcję na silne trafienie.

Nie tworzymy rozbudowanego poise systemu przed prototypem.

## Damage

Pierwszy predator:
- otrzymuje Physical;
- brak armor/resistance w pierwszym pass;
- death po 0 HP.

## Feedback

Wymagane:
- swing sound;
- hit sound;
- hit reaction;
- block sound;
- stamina fail feedback;
- mały camera impulse opcjonalnie.

## QA

- nie można uderzać za plecy bez obrotu/front arc;
- jeden atak nie trafia celu wielokrotnie;
- cooldown/recovery działa niezależnie od FPS;
- brak staminy blokuje start, nie ruch;
- dodge nie przechodzi przez static obstacle;
- dead enemy nie dostaje kolejnych reakcji gameplay.


## Runtime pass — swamp predator

Pierwsza grywalna walka wręcz jest spięta z `swamp-predator`.

Sterowanie:
- **LPM** — light attack.

Parametry prototypu:
- damage: 20 Physical;
- stamina: 12;
- range: 2.1 m;
- front half-angle: 52°;
- windup: 0.18 s;
- active window: 0.12 s;
- recovery: 0.32 s;
- jeden target może zostać trafiony maksymalnie raz na attack instance.

Resolver wykonuje krótkie kroki czasowe do 30 ms, dzięki czemu active window nie znika przy pojedynczym dłuższym frame spike.

`swamp-predator`:
- 60 HP;
- trzy pełne lekkie trafienia zabijają w obecnym prototypowym balansie;
- śmierć ustawia encounter jako `Resolved / Killed`;
- wynik przechodzi przez save/load;
- HUD pokazuje HP dopiero po `Identified`.

To nadal balans prototypowy, nie finalny lock.
