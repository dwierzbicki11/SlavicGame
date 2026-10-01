# Model decyzji i konsekwencji — v0.1

Etap 14 roadmapy.

## Założenie

SlavicGame nie używa jednej osi dobra i zła. Decyzja zapisuje **konkretne skutki**.

## Warstwy konsekwencji

### Natychmiastowa
Efekt widoczny w scenie: dialog, walka, przedmiot, otwarcie drogi.

### Lokalna
Zmiana NPC, usługi, ceny, harmonogramu, bezpieczeństwa lub miejsca.

### Relacyjna
Zmiana zaufania, reputacji, łaski albo zobowiązania.

### Systemowa
Zmiana dostępności zasobu, przejścia, rytuału, potwora, wydarzenia.

### Długoterminowa
Wpływ na frakcję, region, akt fabularny lub zakończenie.

## Rekord decyzji

Każda ważna decyzja ma:

- ID;
- kontekst;
- strony;
- wiedzę dostępną graczowi;
- opcje;
- jawny koszt;
- skutek natychmiastowy;
- skutek opóźniony;
- odwracalność;
- flagi świata;
- reputacje;
- relacje;
- znaczenie dla zakończeń.

## Zasada informacji

Gra może ukrywać długoterminowe skutki, ale nie powinna ukrywać podstawowego znaczenia akcji.

Przykład: gracz powinien wiedzieć, że zgadza się na zobowiązanie wobec boga. Nie musi znać wszystkich przyszłych sytuacji, w których zobowiązanie okaże się trudne.

## Konflikty

Dobra decyzja projektowa ma strony z realnymi interesami.

Przykład „Światła nad mokradłem”:
- rodzina chce prawdy;
- opiekun przeprawy chce bezpieczeństwa i ruchu;
- zielarka chce ograniczyć szkody;
- zjawa ma własny interes;
- drapieżnik jest osobnym problemem.

## Brak punktów moralności

Nie zapisujemy:
- Good +10;
- Evil -10.

Zapisujemy:
- village/old-village +8;
- npc/missing-family -5;
- divine/perun obligation broken;
- crossing-open true;
- apparition-bound true.

## Odwracalność

Decyzje mogą być:
- natychmiast odwracalne;
- odwracalne po zadaniu;
- kosztownie odwracalne;
- trwałe.

Trwałość musi wynikać z logiki świata, nie z arbitralnej potrzeby „ważnego wyboru”.

## Reaktywność

Każdy duży wybór powinien mieć minimum:
- jedną zmianę dialogu;
- jedną zmianę stanu świata, usługi, NPC albo spotkania.

## Kontrola zakresu

Nie każda drobna odpowiedź w dialogu wymaga flagi. Flagi tworzymy tylko, gdy zmiana ma późniejsze znaczenie.

## Audyt decyzji

Przed wydaniem zadania sprawdzamy:
- czy każda strona ma interes;
- czy gracz otrzymał wystarczającą informację;
- czy opcje różnią się skutkiem, a nie wyłącznie tekstem;
- czy wynik zapisuje się poprawnie;
- czy istnieje reakcja po wczytaniu.
