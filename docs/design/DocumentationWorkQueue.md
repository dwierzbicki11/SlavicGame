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
- [ ] format kart przedmiotów poza vertical slice;
- [ ] format definicji broni i armor;
- [ ] format encounterów;
- [ ] format lokacji/interactable;
- [ ] format eventów zależnych od czasu.

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
- [ ] logging/error-reporting policy.

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
- [ ] weather gameplay effects.

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
- [ ] pierwsze usługi/warsztaty.

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
- [ ] research praktyk religijnych używanych jako inspiracja — przeniesiony do Poziomu 7, bo wymaga tej samej krytyki źródeł co panteon.

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
- [ ] polskie/regionalne postacie leśne;
- [ ] topielec/topielica i polski wodnik;
- [ ] południca;
- [ ] boginka/mamuna;
- [ ] upiór;
- [ ] ogniki/błędne światła jako zjawisko.

## Poziom 7 — bardzo trudne, panteon i religia

- [ ] kontrola Rod/Rodzanice;
- [ ] Jarowit;
- [ ] Radegast–Swarożyc;
- [ ] Siwa–Żywie;
- [ ] późny katalog polski;
- [ ] kandydaci literaccy;
- [ ] kolejne karty bogów;
- [ ] finalniejsze relacje F;
- [ ] instytucje kultowe świata gry;
- [ ] ograniczenia boskich umów.

Wymaga bezpośredniejszej kontroli źródeł i ostrożności interpretacyjnej.

## Poziom 8 — bardzo trudne, świat makro

- [ ] finalne kultury;
- [ ] języki/naming rules;
- [ ] państwa;
- [ ] geografia świata;
- [ ] gospodarka międzyregionowa;
- [ ] konflikty polityczne;
- [ ] timeline głównych epok;
- [ ] przepływ głównej historii między regionami.

## Poziom 9 — najtrudniejsze, centralne lore

Na końcu, po zebraniu wystarczającej liczby danych i przetestowaniu świata:

- [ ] prawdziwa natura Czwartej Sfery;
- [ ] prawdziwa przyczyna kryzysu;
- [ ] pełna historia rodziny bohatera;
- [ ] finalny antagonista / przeciwnicy;
- [ ] czy i jak można zabić boga;
- [ ] główny finał;
- [ ] warianty zakończeń;
- [ ] epilogi;
- [ ] ostateczna chronologia tajemnicy.

Te decyzje mają największy koszt retconu, więc robimy je dopiero wtedy, gdy wcześniejsze warstwy są stabilne.

## Reguła przechodzenia dalej

Nie trzeba ukończyć 100% jednego poziomu, by rozpocząć następny, ale:

- preferujemy najniższy niezamknięty poziom;
- pomijamy element, jeśli wymaga informacji z trudniejszego poziomu;
- wracamy po zdobyciu brakującego researchu;
- nie zamrażamy finalnego lore tylko po to, by „odhaczyć dokument”.
