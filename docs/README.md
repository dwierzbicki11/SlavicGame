# Dokumentacja SlavicGame

Wersja projektu świata: **0.1**, 2026-10-01. Dokumentacja jest po polsku; nazwy typów i identyfikatory w kodzie pozostają po angielsku.

SlavicGame to trzecioosobowy Adventure / Action RPG w autorskim świecie inspirowanym Słowiańszczyzną. Poniższe dokumenty rozwijają [wizję w README](../README.md), zachowując C#, .NET 11 i Vulkan jako podstawę techniczną.

| Dokument | Co rozstrzyga |
|---|---|
| [World Bible](world/WorldBible.md) | Tożsamość świata, codzienność, region startowy, zakres projektu |
| [Kosmologia](world/Cosmology.md) | Jawia, Nawia, sfera boska, Czwarta Sfera, przejścia i ograniczenia |
| [Pantheon Bible](pantheon/PantheonBible.md) | Katalog 52 pozycji, warianty nazw, status materiału i proponowane role fantasy |
| [Źródła](pantheon/Sources.md) | Bibliografia, zakres faktycznie sprawdzonych materiałów, kolejka badań |
| [Pierwszy grywalny wycinek](design/VerticalSlice.md) | Pętla łowcy, zadanie, decyzje, systemy i kryteria odbioru |
| [Kolejność produkcji](design/ProductionRoadmap.md) | Ustalona kolejność 23 etapów, obecny stan i następne konkretne rezultaty |
| [Architektura silnika](design/EngineArchitecture.md) | Granice systemów, stan trwały, przygotowane moduły i zasady integracji |

## Jak czytać statusy

- **Ustalone założenie** — wymaganie z wizji gry, które obowiązuje dalsze projektowanie.
- **Projekt v0.1 / F** — nowy, autorski materiał do rozwijania w repo; może zmienić się podczas pracy nad grą.
- **Otwarte** — decyzja jeszcze nie zapadła; dokument nie sugeruje, że jest już kanonem.
- **Zaimplementowane** — zachowanie istnieje w kodzie. Opis fabuły lub mechaniki sam w sobie nie oznacza implementacji.

Klasy A–F opisują pochodzenie materiału, a powyższe statusy opisują stan pracy. Są to dwie różne rzeczy. Informacja od kapłana w grze może być błędna; dokument źródłowy musi osobno podawać, co wiemy poza fikcją.

## Zasady rozbudowy

1. Nowe historyczne twierdzenie otrzymuje źródło, region, datę świadectwa i zakres niepewności.
2. Nowy pomysł fantasy jest oznaczany F. Nie staje się dowodem na dawny kult.
3. Katalog obejmuje również postacie sporne i obrzędowe, ale nie sumuje ich jako pewnych historycznych bogów.
4. Zadania używają lokalnych interesów ludzi i istot, a konsekwencje zapisują oddzielnie dla poszczególnych stron.
5. Etap produkcji kończy konkretny dokument lub działający system z kryteriami odbioru.
6. Tajemnice głównej fabuły pozostają otwarte do odpowiednich etapów; pierwszy wycinek nie rozwiązuje ich za gracza.
