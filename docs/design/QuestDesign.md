# Quest Design Bible — v0.1

Etap 13 roadmapy.

## Pętla podstawowa

**Zlecenie → rozmowy / oględziny → hipoteza → przygotowanie → spotkanie → rozwiązanie → powrót → konsekwencje.**

Nie każde zadanie musi używać wszystkich elementów, ale pełne kontrakty łowcy powinny korzystać z większości.

## Typy zadań

- kontrakt łowcy;
- zadanie wspólnoty;
- zadanie osobiste NPC;
- zadanie frakcji;
- zadanie boskie;
- eksploracyjne;
- badawcze;
- główna historia;
- companion quest.

## Stany

Każde zadanie korzysta z jawnych stanów zgodnych z modelem technicznym:

- Unavailable;
- Offered;
- Active;
- Investigation;
- Preparation;
- Encounter;
- Resolved;
- TurnedIn;
- Failed.

Failed nie zawsze znaczy „nieodwracalnie utracone”. Autor zadania musi opisać, czy istnieje ponowna próba albo alternatywny wynik.

## Dowody

Dowód ma:
- ID;
- źródło;
- typ wiedzy;
- treść;
- wiarygodność z punktu widzenia autorów;
- wpływ na dialog i rozwiązanie.

Typy:
- pogłoska;
- obserwacja;
- potwierdzony fakt;
- interpretacja.

Dziennik nigdy nie zamienia plotki w fakt tylko dlatego, że NPC mówi pewnie.

## Rozwiązania

Quest powinien mieć:
- minimum jedno rozwiązanie podstawowe;
- alternatywy, jeśli wynikają naturalnie z problemu;
- osobno rezultat walki i rezultat problemu fabularnego.

Nie wymagamy sztucznie trzech zakończeń dla każdego małego zadania.

## Nagrody

Możliwe:
- pieniądze;
- przedmiot;
- składnik;
- receptura;
- wiedza;
- reputacja;
- relacja;
- dostęp;
- zmiana świata;
- łaska / zobowiązanie.

Nagroda za oddanie może być przyznana tylko raz.

## Szablon zadania

```text
ID:
Tytuł:
Typ:
Region:
Dawca:
Problem:
Stan faktyczny autorów:
Wersje NPC:
Dowody:
Warunki rozpoczęcia:
Etapy:
Przygotowanie:
Spotkania:
Rozwiązania:
Skutek dla NPC:
Skutek dla miejsca:
Reputacje:
Relacje:
Nagroda:
Stan po save/load:
Warunki porażki:
Ponowna próba:
Powiązania z główną historią:
```

## „Światło nad mokradłem”

To zadanie jest referencyjnym kontraktem pionowego wycinka. Jego trzy rozwiązania dotyczą więzi zjawy, natomiast los drapieżnika jest osobną decyzją.

## Kryteria jakości

- gracz rozumie cel;
- ważny koszt jest zapowiedziany;
- dowody można znaleźć bez komend debug;
- dialog reaguje na wiedzę;
- wynik zmienia przynajmniej NPC lub miejsce;
- save/load nie duplikuje nagród;
- brak jednego „prawidłowego” przycisku, jeśli fabuła przedstawia realny konflikt interesów.
