# CULT_R2_WIELKI_BOR — evidence package v0.1

Status: implementation-ready research mapping; **nie** production research lock. Owner: regional culture evidence dla R2. Dokument nie rozstrzyga identity `forest-guardian` i nie zastępuje `RegionBibleR2WielkiBor.md`, bestiariusza ani material-culture cards.

## Scope

Kontekst obejmuje grywalne wspólnoty strefy brzegowej i sezonowych obozów Wielkiego Boru oraz materialne konsekwencje użytkowania lasu. Chronologia inspiracji pozostaje zgodna z ogólnym zakresem projektu IX–X w. Pakiet nie dowodzi istnienia jednej historycznej „kultury leśnej”, nie utożsamia fikcyjnych mieszkańców z konkretną grupą etniczną i nie wyprowadza supernatural lore z archeologii.

## Existing F decisions

Poniższe elementy są decyzjami świata gry (**F**), nie twierdzeniami historycznymi:

- duża puszcza podzielona gameplayowo na strefę brzegową, Las Użytkowany, Las Mylny i Stare Serce;
- osada brzegowa jako hub R2;
- sezonowe obozy drwali, bartników, łowców i zbieraczy;
- konflikt wokół intensywności eksploatacji i starych miejsc;
- stare trakty oraz miejsca techniczno-rytualne powiązane z centralnym lore;
- `forest-guardian` jako roboczy slot istoty z nierozstrzygniętą tożsamością.

## Evidence ledger

| claim_id | klasa | claim / ograniczenie | basis | confidence / transfer |
|---|---|---|---|---|
| `R2-F-01` | F | sezonowe obozy i osada brzegowa są strukturą produkcyjną regionu | R2 bible | high jako canon gry; nie history claim |
| `R2-R-01` | R | podstawowe rodziny budownictwa, narzędzi, pojemników i ubioru mogą startować od R0 material-culture baseline | `research/material-culture/*`, `CULT_R0_ZARNOWIEC` | medium; wymaga region-fit przed finalnym asset lockiem |
| `R2-R-02` | R | gospodarka leśna potrzebuje asset families dla drewna, miodu/wosku, łowiectwa, zbieractwa i transportu lokalnego | R2 bible + istniejący material-culture baseline | medium; gameplay need jest F, forma historyczna wymaga locatorów |
| `R2-U-01` | U | finalne narzędzia i techniki wyrębu/obróbki drewna właściwe dla czasu i region-fit | research owner: material culture | open |
| `R2-U-02` | U | finalny zestaw bartniczy / sposób pozyskania miodu i wosku | research owner: material culture | open; nie projektować z nowożytnego stereotypu |
| `R2-U-03` | U | lokalny transport surowców i konstrukcja sezonowych schronień | research owner: material culture | open |
| `R2-U-04` | U | costume/ornament/naming wyróżniające R2 od R0/R1 | culture/art/naming owners | open; różnica regionu nie oznacza automatycznie odrębnej etniczności |
| `R2-U-05` | U | praktyki kultowe dotyczące lasu i miejsc omijanych | religion research owner | open; material evidence oddzielić od interpretacji |
| `R2-U-06` | U | identity `forest-guardian` | bestiary research owner | open; nie utożsamiać automatycznie z leszym |

## Material culture consequences

### Osady i budownictwo

Do implementacji można reużywać modularny baseline R0 dla zwykłych konstrukcji, dopóki wariant R2 jest oznaczony jako rekonstrukcja (`R`). R2 potrzebuje osobnych rodzin małych obozów sezonowych, składowania surowców, śladów pracy i prostych osłon. Finalne detale konstrukcyjne pozostają zablokowane do region-fit i locatorów.

### Gospodarka leśna

Gameplay może już modelować pozyskanie drewna, miodu/wosku, łowiectwo i zbieractwo jako **F gameplay scope**. Finalne modele narzędzi, pojemników, technik pracy i animacji nie mogą wynikać z intuicji „leśnego ludu”; wymagają material-culture research. Widoczne świeże zręby, odrosty, ślady ognia i drogi pracy są językiem środowiskowym regionu, a nie deklaracją historycznej skali gospodarki.

### Ubiór i ornament

Do czasu osobnego locku R2 używa wspólnych rodzin bazowych z R0/R1. Nie tworzymy „dzikiego”, futrzanego ani prymitywizującego kostiumu tylko po to, by wizualnie odróżnić mieszkańców boru. Różnicowanie może pochodzić z funkcjonalnych warstw pracy/pogody po researchu.

### Transport i handel

R2 ma lokalne trasy piesze i transport surowców jako wymóg gameplayu. Konkretne środki transportu, uprząż, pakowanie i skala wymiany pozostają `U`, jeśli nie mają odpowiednich locatorów. Powiązanie z R0/R1 nie dowodzi jednego modelu ekonomicznego.

## Social/legal/economic constraints

Lokalne konflikty o dostęp do zasobów i starych miejsc są **F**. Nie przypisujemy mieszkańcom historycznego prawa leśnego, własności zbiorowej ani konkretnej hierarchii bez źródeł. Questy mogą operować na jawnie fikcyjnych zwyczajach i umowach, o ile dialog nie przedstawia ich jako rekonstrukcji historycznej.

## Religion / death / supernatural

Stare miejsca, ich techniczno-rytualna funkcja i anomalie należą do central lore (**F**). Historyczny research religii może ograniczać materialny język rekwizytów, ale nie jest dowodem Splotu ani `forest-guardian`. Praktyki pogrzebowe dziedziczą tylko te elementy baseline, które mają adekwatny region/time fit; lokalne wyjątki pozostają `U`.

## Naming/language constraints

Nazwy `Wielki Bór`, `Stare Serce` i obecne sloty NPC/POI są robocze lub F. Finalne nazewnictwo nie może być generowane przez przypadkowe archaizowanie. Do czasu `NamingRules`/language lock content korzysta ze stabilnych technicznych ID.

## Asset/content consequences

Pakiet odblokowuje implementacyjnie:

- modularne warianty osady brzegowej i obozów sezonowych;
- generic forestry/resource props z wymienialnymi modelami finalnymi;
- environmental states: ślady wyrębu, odrost, składowanie, opuszczony obóz;
- quest/encounter data odnoszące się do resource stakeholders bez zamrażania historycznych nazw zawodów;
- reuse bazowych rodzin R0/R1 tam, gdzie finalny region-fit nie jest wymagany do działania systemu.

Nie odblokowuje finalnego costume, ornamentu, narzędzi specjalistycznych, ikonografii kultowej ani creature identity.

## Open locks

1. `R2-U-01` — narzędzia i techniki pracy leśnej; owner: material-culture research; close: źródła + locatory + region/time fit.
2. `R2-U-02` — bartnictwo/miód/wosk; owner: material-culture research; close: źródła + locatory, bez transferu z nowożytności bez oznaczenia R.
3. `R2-U-03` — sezonowe schronienia i transport surowców; owner: material-culture research.
4. `R2-U-04` — costume/ornament/naming; owner: culture/art/naming.
5. `R2-U-05` — materialna strona praktyk kultowych; owner: pantheon/religion research.
6. `R2-U-06` — `forest-guardian`; owner: bestiary research; może pozostać świadomie F, jeśli brak wystarczającego identity locku.

Żaden z tych locków nie blokuje systemowego content pipeline ani implementacji quest state machines; blokuje jedynie oznaczenie zależnych assetów/lore jako finalnych.

## Sources / provenance

Pakiet v0.1 jest **mappingiem istniejącego researchu repo**, nie nową bibliografią. Korzysta z `research/material-culture/*`, `research/cultures/CULT_R0_ZARNOWIEC.md`, R2 bible i centralnej source policy. Pełne bibliograficzne locatory dla finalnych assetów należy dopisać przy zamykaniu `R2-U-*` zamiast kopiować ogólne źródła bez wskazania, które twierdzenie wspierają.

Ostatni przegląd: 2026-10-03.