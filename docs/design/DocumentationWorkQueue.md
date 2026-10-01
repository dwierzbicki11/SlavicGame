# Kolejność dalszej dokumentacji — od najprostszej do najtrudniejszej

Ten dokument ustala praktyczną kolejność pogłębiania dokumentacji po zbudowaniu pełnego szkieletu v0.1.

## Poziom 1 — proste, niskie ryzyko decyzji

Cel: ustalić rzeczy techniczne i produkcyjne, które nie zamrażają lore.

- [x] konwencje trwałych ID;
- [x] lista przedmiotów vertical slice;
- [x] pierwszy czar i rytuał jako funkcje gameplay;
- [x] pięć kart NPC vertical slice;
- [x] pełna karta „Światła nad mokradłem”;
- [x] asset list vertical slice;
- [x] format kart przedmiotów poza vertical slice;
- [x] format definicji broni i armor;
- [x] format encounterów;
- [x] format lokacji/interactable;
- [x] format eventów zależnych od czasu.

## Poziom 2 — proste/średnie, głównie system design

- [x] szczegółowy tutorial/onboarding;
- [x] HUD spec;
- [x] inventory UI flow;
- [x] quest journal UI flow;
- [x] dialogue UX flow;
- [x] map UI;
- [x] settings matrix;
- [x] input action map;
- [x] save slot UX;
- [x] debug/developer overlay spec;
- [x] logging/error-reporting policy.

## Poziom 3 — średnie, gameplay i content

- [x] pełny melee moveset;
- [x] podstawowy bow design;
- [x] status effects;
- [x] equipment slots;
- [x] progression/skill tree;
- [x] economy first pass;
- [x] vendor design;
- [x] alchemy recipes v0.1;
- [x] tracking system;
- [x] encounter tables startowego regionu;
- [x] day/night event table;
- [x] weather gameplay effects.

## Poziom 4 — średnie/trudne, konkretna zawartość

- [x] finalniejsze profile pięciu NPC;
- [x] pełne grafy dialogowe vertical slice;
- [x] wszystkie wpisy dziennika/evidence text;
- [x] wszystkie możliwe reakcje po trzech rozwiązaniach;
- [x] karta Żarnowca;
- [x] karta Puszczy Żywia;
- [x] karta Czarnych Mokradeł;
- [x] karta Kamiennego Kręgu;
- [x] pierwsze side questy regionu;
- [x] pierwsze usługi/warsztaty.

## Poziom 5 — trudne, wymagają researchu

- [x] research budownictwa;
- [x] research ubioru i materiałów;
- [x] research żywności;
- [x] research rolnictwa i narzędzi;
- [x] research transportu;
- [x] research uzbrojenia;
- [x] research pochówków;
- [x] research handlu;
- [x] research struktur osad;
- [x] research praktyk religijnych używanych jako inspiracja — first-pass zasad źródłowych wykonany w Poziomie 7.

Każdy temat powstaje jako osobna karta źródłowa, zanim przeniesiemy szczegóły do finalnej kultury.

## Poziom 6 — trudne, bestiariusz

**Pierwszy pakiet ukończony:**
- [x] rusałka;
- [x] duch leśny / kandydat leszy-type;
- [x] wodnik / vodník;
- [x] zmora;
- [x] strzygoń / strzyga;
- [x] macierz dopasowania do vertical slice.

Wynik: `swamp-predator` pozostaje autorskim F, ponieważ żaden sprawdzony kandydat nie pasuje wystarczająco dobrze. `forest-guardian` może być dalej rozwijany jako lokalna istota leśna, ale finalna nazwa wymaga regionalnego researchu.

**Drugi pakiet do wykonania później:**
- [x] polskie/regionalne postacie leśne;
- [x] topielec/topielica i polski wodnik;
- [x] południca;
- [x] boginka/mamuna;
- [x] upiór;
- [ ] ogniki/błędne światła jako zjawisko.

## Poziom 7 — bardzo trudne, panteon i religia

- [x] kontrola Rod/Rodzanice;
- [x] Jarowit;
- [x] Radegast–Swarożyc;
- [x] Siwa–Żywie;
- [x] późny katalog polski;
- [x] kandydaci literaccy;
- [x] kolejne karty bogów;
- [x] finalniejsze relacje F;
- [x] instytucje kultowe świata gry;
- [x] ograniczenia boskich umów.

Wymaga bezpośredniejszej kontroli źródeł i ostrożności interpretacyjnej.

## Poziom 8 — bardzo trudne, świat makro

- [x] finalne kultury;
- [x] języki/naming rules;
- [x] państwa;
- [x] geografia świata;
- [x] gospodarka międzyregionowa;
- [x] konflikty polityczne;
- [x] timeline głównych epok;
- [ ] przepływ głównej historii między regionami.

## Poziom 9 — najtrudniejsze, centralne lore

Na końcu, po zebraniu wystarczającej liczby danych i przetestowaniu świata:

- [x] prawdziwa natura Czwartej Sfery;
- [x] prawdziwa przyczyna kryzysu;
- [x] pełna historia rodziny bohatera;
- [x] finalny antagonista / przeciwnicy;
- [x] czy i jak można zabić boga;
- [x] główny finał;
- [x] warianty zakończeń;
- [x] epilogi;
- [ ] ostateczna chronologia tajemnicy.

Te decyzje mają największy koszt retconu, więc robimy je dopiero wtedy, gdy wcześniejsze warstwy są stabilne.

## Reguła przechodzenia dalej

Nie trzeba ukończyć 100% jednego poziomu, by rozpocząć następny, ale:

- preferujemy najniższy niezamknięty poziom;
- pomijamy element, jeśli wymaga informacji z trudniejszego poziomu;
- wracamy po zdobyciu brakującego researchu;
- nie zamrażamy finalnego lore tylko po to, by „odhaczyć dokument”.

## Stan po Poziomie 9

**Poziomy 1–9 mają kompletny first pass.**

Od tej chwili kolejka nie odpowiada już na pytanie „czym ma być gra?”, tylko „jak szczegółowo produkcyjnie rozpisać znany już projekt?”.

Następna faza dokumentacji:
- main quest cards;
- region bibles;
- companion/NPC roster;
- final content lists;
- production budgets;
- implementation specs wynikające z playtestów.
