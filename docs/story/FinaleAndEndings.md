# Finał i zakończenia — author design v0.1

## Sytuacja finałowa

Krąg Przejścia uruchamia Pierwszy Próg. Sieć zaczyna łączyć lokalne nieszczelności, echa Nawii i niezrealizowane warianty Czwartej Sfery. Gracz nie wybiera koloru; realizuje plan przygotowany wcześniejszymi decyzjami.

## E1 — Zamknięcie

Gracz odcina Pierwszy Próg i wygasza sieć.

Skutki:
- nieszczelności maleją;
- część bezpiecznych rytuałów przestaje działać;
- dostęp do dawnej wiedzy zostaje ograniczony;
- bogowie zachowują domeny, ale tracą część łatwych przejść.

Koszt: utrata części kontaktów z Nawią i polityczna walka o pozostałe miejsca. Nie jest automatycznie „good ending”.

## E2 — Stabilizacja

Gracz przywraca sieć jako system ograniczonych progów.

Wymaga wysokiej wiedzy, zachowanych kluczowych węzłów i współpracy kilku regionów.

Kryzys ustaje, ale pojawia się pytanie, kto kontroluje system. Epilog zależy od nadzoru wybranego przez gracza.

## E3 — Rozproszenie

Gracz niszczy centralną rolę Pierwszego Progu i rozdziela funkcję na lokalne węzły.

Skutki:
- brak jednego punktu kontroli;
- regiony same utrzymują granice;
- większa autonomia;
- większe ryzyko lokalnych błędów.

## E4 — Otwarcie

Gracz świadomie pozwala na ograniczony kontakt z Czwartą Sferą.

Wymaga bardzo wysokiej wiedzy i akceptacji ryzyka.

Skutki:
- dostęp do nowych form wiedzy;
- bardziej płynne granice rzeczywistości;
- trwałe zmiany części istot i ludzi;
- najmniej przewidywalna przyszłość.

## Zakończenie awaryjne — Niestabilność

Przy zbyt małej wiedzy, utracie węzłów lub braku sojuszy gracz może tylko zatrzymać natychmiastową katastrofę. Kryzys zostaje ograniczony, ale nie rozwiązany. To pełnoprawny epilog, nie ekran „bad ending”.

## Ścieżka bez patrona

Każde E1–E4 jest możliwe bez patrona. Boskie umowy zmieniają koszt i narzędzia, lecz nie są obowiązkowym kluczem.

## Opcjonalna śmierć boga

Możliwa tylko przy konflikcie z konkretnym bogiem, poznaniu jego kotwic i kontroli odpowiednich progów. Zniszczenie przejawu nie wystarcza. Prawdziwa śmierć zmienia domenę i wywołuje kryzys religijny; nigdy nie jest obowiązkowa.

## Epilogi

Finał generuje osobno:
- epilog świata;
- epilogi regionów;
- politykę;
- relacje boskie;
- rodzinę;
- dalszy los bohatera.

Gracz może ujawnić prawdę o rodzinie, ukryć ją, oczyścić rodziców albo uznać ich współodpowiedzialność.

## Minimalna matryca implementacyjna

```text
FinalePlan = Close | Stabilize | Disperse | Open | Emergency
KnowledgeTier = 0..3
NetworkIntegrity = 0..3
RegionalCoalition = 0..3
DivineSupport = set
DivineConflicts = set
FamilyTruthKnown = bool
KuratorState = Hostile | Convinced | Dead | Allied
```

Dostęp do planu wynika z warunków zgromadzonych przed finałem, a nie z jednego końcowego dialogu.
