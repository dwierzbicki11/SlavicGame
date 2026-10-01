# Game Design Bible — v0.1

Etap 17 roadmapy. Dokument spina systemy gry w jedną całość.

## Gatunek

Third-person 3D Adventure RPG / Action RPG.

## Obietnica dla gracza

> Jestem łowcą żyjącym w świecie, który pamięta moje działania. Zanim wybiorę rozwiązanie, mogę zbadać problem, przygotować się, walczyć, negocjować albo użyć rytuału.

## Główne filary

1. **Zawód łowcy** — rozpoznanie przed rozwiązaniem.
2. **Żyjący świat** — miejsca i ludzie reagują.
3. **Niepewna wiedza** — plotka nie jest faktem.
4. **Dzień i noc** — zmieniają dostępność i ryzyko.
5. **Wiara bez przymusu** — patron jest wyborem.
6. **Sprawczość bohatera** — historia rodziny nie zastępuje decyzji gracza.

## Core loop

```text
Eksploracja
→ spotkanie / zlecenie
→ rozmowa i ślady
→ hipoteza
→ przygotowanie
→ rozwiązanie
→ konsekwencje
→ rozwój i nowe możliwości
→ eksploracja
```

## Eksploracja

Gracz:
- porusza się pieszo;
- odkrywa ścieżki;
- obserwuje pogodę i porę;
- znajduje ślady;
- odwiedza osady;
- wraca do zmienionych miejsc.

## Walka

Docelowo:
- lekki/ciężki atak;
- kierunek;
- blok;
- unik;
- stamina;
- dystans;
- łuk;
- status effects;
- magia;
- słabe punkty;
- różne typy broni.

Walka nie może być jedynym rozwiązaniem wszystkich istot.

## Tropienie

Trop ma:
- pochodzenie;
- świeżość;
- rodzaj;
- poziom pewności;
- powiązanie z wpisem dziennika.

Tropienie nie jest tylko świecącą linią do celu.

## Dialog

Dialog reaguje na:
- wiedzę;
- przedmioty;
- reputację;
- relację;
- porę;
- stan zadania;
- zobowiązania.

## Ekwipunek

Kategorie:
- broń;
- armor;
- narzędzia;
- consumables;
- składniki;
- quest items;
- artefakty.

Waga/limit ekwipunku pozostaje do testów; nie ustalamy go bez prototypu.

## Crafting i alchemia

Crafting ma wspierać przygotowanie, nie zmieniać gry w symulator zbierania każdego kamienia.

## Magia

Czar, rytuał i alchemia to różne systemy. Koszt jest częścią decyzji.

## Reputacja

Oddzielnie:
- NPC;
- wieś;
- region;
- frakcja;
- kultura;
- instytucja religijna.

Relacja z bogiem jest osobnym systemem.

## NPC

NPC mają:
- rolę;
- miejsce;
- harmonogram;
- relacje;
- stan;
- pamięć kluczowych decyzji.

Pełna symulacja każdego mieszkańca nie jest wymagana w pierwszym regionie.

## Świat

- duże gęste regiony;
- dzień/noc;
- pogoda;
- zdarzenia;
- ekonomiczne uzasadnienie osad;
- lokalne anomalie;
- powrót do zmienionych miejsc.

## Progresja

Nagrodą są:
- umiejętności;
- sprzęt;
- wiedza;
- kontakty;
- dostęp;
- receptury;
- znaki;
- reputacja;
- relacje.

## Śmierć i zapis

Śmierć → ostatni spójny checkpoint.

Checkpoint przechowuje:
- gracza;
- czas;
- inventory;
- questy;
- reputacje;
- relacje;
- stan świata.

## UI

Podstawowe ekrany:
- HUD;
- inventory;
- journal;
- map;
- character;
- dialogue;
- settings;
- pause.

## Kamera

Third-person orbit. Kamera nie może wchodzić pod teren ani tracić sterowania podczas równoczesnego ruchu i myszy.

## Pierwszy vertical slice

Musi pokazać w miniaturze:
- eksplorację;
- walkę;
- rozmowę;
- tropienie;
- przygotowanie;
- rytuał;
- konsekwencję;
- save/load.

## Anti-goals

Nie budujemy:
- gigantycznej pustej mapy;
- moralności good/evil;
- wszystkich bogów naraz;
- setek proceduralnych questów bez reaktywności;
- systemu magii udającego historyczne „słowiańskie runy”;
- fabuły, w której bohater okazuje się wybrańcem i traci sprawczość.

## Pytania do testów

Każdy prototyp odpowiada na konkretną hipotezę:
- czy tropienie jest czytelne?
- czy przygotowanie realnie pomaga?
- czy noc zmienia plan?
- czy koszt magii jest zrozumiały?
- czy gracz zauważa konsekwencję?
- czy powrót do miejsc ma wartość?
