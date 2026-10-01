# SlavicGame — pełny obraz gry v0.2

**Status:** pełny obraz designu / author truth.  
**Nie oznacza:** ukończonej implementacji, finalnego balansu, zamkniętego researchu ani gotowych wszystkich assetów.

## 1. Elevator pitch

SlavicGame to trzecioosobowe **Adventure RPG / Action RPG** o zawodowym łowcy potworów w autorskim świecie inspirowanym słowiańskim folklorem, wybranymi realiami życia IX–X wieku oraz szerzej rozumianą kulturą pograniczy.

Gracz:
- przyjmuje zlecenia;
- zbiera świadectwa;
- oddziela plotkę od obserwacji;
- tropi;
- przygotowuje narzędzia;
- walczy;
- negocjuje;
- wykonuje rytuały;
- zawiera albo odrzuca boskie umowy;
- wraca do miejsc zmienionych przez własne decyzje.

Główna historia zaczyna się jako praca łowcy, a kończy pytaniem:
> **kto i w jaki sposób ma utrzymywać granice rzeczywistości, kiedy stary system przestaje działać?**

---

# 2. Bohater

## Kim jest

- człowiek;
- zawodowy łowca;
- postać tworzona przez gracza;
- bez klasycznego statusu wybrańca;
- bez boskiej krwi;
- bez wrodzonej supermocy.

## Kreator

Docelowo:
- imię;
- wygląd;
- typ sylwetki;
- głos;
- kosmetyczne elementy pochodzenia społecznego.

## Rozwój

Cztery mieszalne specjalizacje:
- Combat;
- Tracking;
- Preparation;
- Ritual/Social.

Progresja ma przede wszystkim otwierać:
- nowe zachowania;
- narzędzia;
- informacje;
- opcje w dialogu.

---

# 3. Core loop

```text
Zlecenie
→ rozmowy
→ oględziny
→ hipoteza
→ tropienie
→ przygotowanie
→ spotkanie
→ walka / negocjacja / rytuał / obejście
→ powrót
→ konsekwencje
→ rozwój
```

Najważniejsza reguła:
**rozpoznanie problemu jest częścią gameplayu, a nie tylko tekstem przed walką.**

---

# 4. System wiedzy

Dziennik rozróżnia:

- Rumor;
- Observation;
- ConfirmedFact;
- Interpretation.

NPC może:
- kłamać;
- mylić się;
- powtarzać lokalną tradycję;
- mieć częściową rację.

Gra nie zmienia automatycznie każdej wypowiedzi w „lore fact”.

---

# 5. Świat

Świat nie jest realną Europą ani historią alternatywną.

Inspiracje historyczne służą:
- życiu materialnemu;
- rzemiosłu;
- gospodarce;
- osadnictwu;
- wybranym motywom wierzeń.

Każdy element pochodzący z późnego folkloru zachowuje informację o swoim późnym źródle.

---

# 6. Główne regiony

## R0 — Pogranicze Żarnowca

Pierwszy region.

Miejsca:
- Żarnowiec;
- Puszcza Żywia;
- Czarne Mokradła;
- Kamienny Krąg.

Funkcja:
- pełna pętla zawodu;
- vertical slice.

## R1 — Nadborze

Gęsty region grodów i rzek.

Tematy:
- państwo;
- cła;
- kontrola wiedzy;
- licencjonowanie łowców;
- centralizacja.

## R2 — Wielki Bór

Duża puszcza i lokalne wspólnoty.

Tematy:
- eksploatacja;
- terytorium;
- nie-ludzkie reguły miejsca;
- tracking;
- duchy niebędące domyślnie wrogami.

## R3 — Przymorze

Porty i Liga Ujścia.

Tematy:
- handel;
- wielojęzyczność;
- przepływ informacji;
- polityka kupiecka;
- morze.

## R4 — Kamienne Wyżyny

Przełęcze, kopalnie i rywalizujące księstwa.

Tematy:
- surowce;
- pogoda;
- logistyka;
- stare materiały kotwic.

## R5 — Równiny Arel

Kultura rozwijana z osobnej, niesłowiańskiej bazy badawczej.

Tematy:
- ruchome granice;
- konie;
- szlaki;
- pamięć dróg;
- różne sposoby organizacji przestrzeni.

## R6 — Pustkowie Pierwszego Progu

Późnogrowy region ruin i centralnej historii.

Tematy:
- archiwa;
- Sieć Progów;
- rodzina bohatera;
- Wszebor;
- prawda historii.

---

# 7. Polityka

Najważniejsze siły:

- Związek Grodów Nadborza;
- Liga Ujścia;
- Księstwa Przełęczy;
- Wspólnoty Wielkiego Boru;
- Konfederacja Arel;
- instytucje kultowe;
- Kolegium/środowiska łowców;
- Straż Zamknięcia;
- Krąg Rozwarcia.

Nie ma jednego „imperium zła”.

---

# 8. Gospodarka

Świat działa przez:
- drogi;
- przeprawy;
- rzeki;
- porty;
- drewno;
- zboże;
- metal;
- sól;
- konie;
- pracę warsztatów.

Anomalia jest problemem także dlatego, że:
- zamyka trasę;
- podnosi koszt transportu;
- izoluje osadę;
- zmienia władzę.

---

# 9. Bestiariusz

Gra rozróżnia:

## Praktyczną kategorię łowcy
Na podstawie zachowania.

## Nazwę ludową
Regionalną, czasem sprzeczną.

Przykłady researchowane:
- rusałka;
- duchy leśne;
- wodnik/topielec;
- zmora;
- strzyga/strzygoń;
- upiór;
- południca;
- boginka/mamuna/dziwożona;
- błędne ogniki.

`swamp-predator` pozostaje autorskim F, ponieważ żadna sprawdzona nazwa nie pasuje uczciwie do jego roli.

---

# 10. Walka

## Melee

- light attack;
- heavy attack później;
- active hit window;
- block;
- stamina;
- guard break;
- dodge;
- stagger.

## Bow

- aim;
- draw;
- projectile;
- ammo;
- odzyskiwanie części strzał.

## Magia

Nie ma jednej many.

Koszty mogą używać:
- staminy;
- zdrowia;
- przedmiotów;
- czasu;
- zobowiązań;
- środowiska.

---

# 11. Magia

## Formy

### Czar
Szybka technika.

### Rytuał
Miejsce + warunek + kolejność + koszt.

### Alchemia
Receptura + materiał.

## Pierwszy czar

`spell.reveal-trace`

Pokazuje aktywne nadnaturalne ślady, ale nie identyfikuje potwora za gracza.

## Pierwszy rytuał

`ritual.release-bound-echo`

Używa:
- tożsamości;
- kotwicy;
- granicy;
- pory.

---

# 12. Prawdziwe pochodzenie magii znaków

Magiczny alfabet F to uproszczona wersja dawnej **notacji Sieci Progów**.

Podstawowe pojęcia:
- identity;
- boundary;
- direction;
- condition;
- exchange;
- closure;
- repetition.

Nie jest „słowiańskim alfabetem magicznym” z historii.

---

# 13. Kosmologia — publiczna warstwa

Ludzie mówią o:
- Jawii;
- Nawii;
- sferze bogów;
- tajemniczej Czwartej Sferze.

Każda kultura interpretuje je inaczej.

---

# 14. Kosmologia — author truth

## Jawia
Zrealizowany stan materialny.

## Nawia
Ciągłość osoby/pamięci po śmierci.

## Sfera boska
Trwałe świadome boskie wzorce.

## Czwarta Sfera / Splot
Warstwa niezwiązanych jeszcze możliwości i relacji.

Nie jest:
- piekłem;
- bogiem;
- umysłem.

Jest starsza i bardziej fundamentalna niż obecne boskie formy.

---

# 15. Sieć Progów

Około 430 BG dawne Porozumienie Progów zbudowało system:

- kotwic;
- węzłów;
- znaków;
- przejść.

Cel:
- stabilizować granice i nie pozwalać, by zdarzenia pozostawały otwarte na Splot.

Problem:
- system był rozwiązaniem awaryjnym;
- przez wieki tracił konserwację;
- przenosił naprężenie między węzłami.

---

# 16. Przyczyna obecnego kryzysu

Kryzys ma **dwie przyczyny naraz**:

1. stara Sieć jest zużyta i przeciążona;
2. Krąg Rozwarcia celowo aktywuje węzły i przyspiesza problem.

Zabicie lidera frakcji nie naprawia Sieci.

---

# 17. Rodzina bohatera

Rodzice biologiczni byli:
- badaczami/praktykami Archiwum Progów;
- współpracownikami Wszebora.

Podczas Nocy Zamkniętego Progu 22 BG:

- Parent A ginie fizycznie;
- Parent B przechodzi do Nawii z częścią wzorca przebudowy;
- dane zostają rozproszone;
- dziecko zostaje ukryte.

Bohater nie dziedziczy mocy.

Dziedziczy:
- historię;
- pozostawione tropy;
- możliwość dotarcia do ludzi, którzy znali rodziców.

---

# 18. Wszebor

Główny ludzki antagonista.

Ma rację, że:
- stary system upada;
- proste ponowne zamknięcie nie rozwiązuje problemu na zawsze.

Myli się, wierząc:
- że pełne Rozwarcie da się kontrolować wystarczającą wiedzą;
- że Splot może bezpiecznie rekonstruować osoby.

Może:
- zginąć;
- zostać pokonany;
- przejść na stronę ograniczonego rozwiązania;
- współtworzyć finał.

---

# 19. Bogowie

Bogowie:
- są realnymi osobami;
- nie są wszechwiedzący;
- mają interesy;
- mogą zawierać jawne umowy;
- nie są podzieleni na prostą oś dobro/zło.

Gracz:
- może mieć wielu patronów;
- może nie mieć żadnego.

---

# 20. Czy można zabić boga?

Manifestację:
- tak.

Prawdziwego boga:
- również tak, ale tylko przez:
  - zerwanie kotwic;
  - uniemożliwienie ponownej stabilizacji;
  - rozwiązanie boskiego wzorca przy użyciu Splotu.

Nie jest to obowiązkowa ścieżka.

---

# 21. Główna historia

## Akt 0 — Zawód
Żarnowiec i „Światło nad mokradłem”.

## Akt I — Wzór
Nadborze + Wielki Bór. Odkrycie sieci miejsc.

## Akt II — Interesy
Przymorze + Wyżyny + Arel. Polityka i różne fragmenty pamięci.

## Akt III — Pamięć
Pustkowie Pierwszego Progu. Prawda o rodzinie i Wszeborze.

## Akt IV — Nawia / Splot
Spotkanie Parent B i rozpoznanie rzeczywistej natury Czwartej Sfery.

## Akt V — Pierwszy Próg
Decyzja o przyszłej architekturze granic świata.

---

# 22. Finał

Nie jest jednym bossem.

Etapy:
- zebranie wsparcia;
- dostęp do Pierwszego Progu;
- konflikt frakcji;
- Wszebor;
- stabilizacja węzłów;
- boskie zobowiązania;
- wybór architektury;
- decyzja rodzinna.

---

# 23. Pięć głównych zakończeń

## E1 — Odnowione Zamknięcie
Mocna stabilność teraz, problem wróci później.

## E2 — Rozproszona Straż
Lokalne węzły pod opieką ludzi i łowców.

## E3 — Przymierze Progów
Kontrolowane przejścia i badania.

## E4 — Mandat Boski
Bogowie przejmują kluczowe kotwice.

## E5 — Wielkie Rozwarcie
Radykalne otwarcie świata na Splot.

Każde ma:
- warianty polityczne;
- rodzinne;
- boskie;
- regionalne.

Nie ma jednej etykiety „good ending”.

---

# 24. Vertical slice

Pierwsza pełna próbka:
**„Światło nad mokradłem”**

Problem:
- zaginiona osoba;
- zjawa związana z pamiątką;
- osobny predator;
- lokalna nieszczelność.

Trzy rozwiązania zjawy:
1. rytuał;
2. warunkowa umowa;
3. zniszczenie kotwicy.

Predator ma osobny stan.

---

# 25. Żyjący świat

NPC:
- mają harmonogram;
- pamiętają kluczowe decyzje;
- reagują na region state.

Dzień/noc:
- zmienia usługi;
- encountery;
- zagrożenia;
- ślady.

Pogoda:
- wpływa na widoczność i atmosferę;
- nie losuje permanentnego failu głównego questa.

---

# 26. Reputacja

Oddzielne zakresy:
- NPC;
- wieś;
- region;
- frakcja;
- kultura;
- instytucja religijna.

Relacja z bogiem:
- osobny system.

Nie ma globalnego paska moralności.

---

# 27. Ekwipunek i przygotowanie

Kategorie:
- Weapon;
- Armor;
- Tool;
- Consumable;
- Ingredient;
- Quest;
- Artifact.

Przygotowanie ma zmieniać realną sytuację, ale nie być obowiązkowym grindowaniem.

---

# 28. Save

Wersjonowany snapshot:
- gracz;
- inventory;
- questy;
- reputacje;
- relacje;
- czas;
- region states;
- divine contracts.

Śmierć:
- powrót do spójnego checkpointu.

Nie jest kanonicznym cofaniem czasu.

---

# 29. UI

Główne ekrany:
- HUD;
- Inventory;
- Journal;
- Evidence;
- Bestiary;
- Map;
- Character;
- Dialogue;
- Settings;
- Save/Load.

Dziennik śledztwa jest jednym z najważniejszych ekranów gry.

---

# 30. Audio-wizualny kierunek

Dzień:
- wiarygodny;
- żywy;
- materialny.

Noc:
- bardziej niepewna;
- ale czytelna gameplayowo.

Nawia:
- pamięć i brak.

Boskie:
- mocna spójność motywu.

Splot:
- sprzeczne, jednocześnie prawie poprawne wersje przestrzeni.

---

# 31. Technologia

## Engine
C#, własny framework/silnik.

## Rendering
Veldrid + Vulkan jako obecny kierunek.

## Platformy
1. PC;
2. później Android po realnej ocenie wydajności i UI.

## Obecny prototyp
Ma już m.in.:
- teren;
- third-person camera;
- regiony;
- czas;
- pogodę;
- health/stamina;
- statyczne kolizje;
- pierwszy predator AI;
- save-state fundamenty części systemów.

Pełna gra nadal wymaga dużej implementacji.

---

# 32. Zasady researchu

Historyczne/foklorystyczne:
- źródło;
- region;
- data;
- typ materiału;
- niepewność.

Fikcja F:
- oznaczona jako F.

Nie:
- cofamy automatycznie XIX-wiecznego folkloru do X wieku;
- tworzymy „52 pewnych bogów” z katalogu badawczego;
- nazywamy autorskiego alfabetu historycznymi runami;
- przypisujemy potworowi znanej nazwy tylko dla marketingu.

---

# 33. Co jest już pełnym obrazem

Ustalono:
- bohatera;
- zawód;
- core loop;
- system wiedzy;
- kosmologię autorów;
- przyczynę kryzysu;
- rodzinę;
- antagonistę;
- mapę makro;
- kultury;
- politykę;
- gospodarkę;
- główne regiony;
- akty kampanii;
- zasady bogów;
- możliwość śmierci boga;
- finał;
- pięć architektur zakończenia;
- epilogi;
- systemy gameplay na poziomie designu;
- vertical slice.

---

# 34. Co nadal pozostaje do produkcyjnego dopracowania

Pełny obraz **nie oznacza finalnego content lock**.

Nadal potrzebne są:
- finalne karty wszystkich main questów;
- pełne region bibles R1–R6;
- wszystkie NPC i companion quests;
- finalny katalog potworów;
- research kultury Arel;
- pełne dane material culture dla wszystkich kultur;
- finalne nazwy części miejsc/postaci;
- balans;
- asset budgets;
- measured hardware requirements;
- pełna implementacja;
- testy i optymalizacja.

To są już prace **wewnątrz znanego obrazu gry**, a nie pytania „czym właściwie ma być gra?”.
