# Scenki w silniku — pierwszy etap

Dwie działające scenki: wejście do świata (8 s) i odkrycie Kamiennego Kręgu (8 s). Ujęcia opisuje CinematicDefinition: pozycje początkowa/końcowa, punkt obserwacji, czas i napis. Współrzędna Y oznacza wysokość nad terenem. Sterowanie oraz symulacja świata są zatrzymane. Spacja/Escape pomijają scenkę; ukończenie i pominięcie zapisują tę samą flagę obejrzenia. Nie przyznajemy za samo obejrzenie nagród ani dowodów. Sceny nie rozpoczynają się obok żywego przeciwnika.

Docelowy budżet roboczy: 8–12 scenek, zgodnie z planem kilku lub kilkunastu. Katalog runtime ma obecnie 8 trwałych definicji: `arrival`, `shrine`, `contract-accepted`, `first-night-anomaly`, `anchor-revealed`, `ritual-preparation`, `contract-resolution` i `divine-manifestation`. Pierwsze dwie są podpięte do bieżącej gry; pozostałe są gotowe do wywołania po stabilnym ID z questów, kiedy ich warunki fabularne zostaną podłączone.

Nowe sceny mogą używać przestrzeni `PlayerRelative`: pozycje ujęć są wtedy przesunięciami względem miejsca, w którym znajdował się gracz w chwili startu scenki. Dzięki temu sceny questowe nie wymagają twardo zakodowanych współrzędnych mapy i pozostają przenośne między iteracjami level designu.

Każda przyszła scenka potrzebuje trwałego ID, warunku startu, warunku pominięcia, napisów, projektu dźwięku i jednoznacznego momentu zmian questowych. Stan świata nie może zależeć od obejrzenia całej animacji. Dubbing, animacja twarzy i montaż filmowy pozostają do wykonania.
