# Panteon — critical source lock

**Stan: PASS v0.1, 2026-10-03.** Ten dokument zamyka krytyczny source-policy lock potrzebny do produkcji. Nie rozstrzyga sporów naukowych i nie zamienia rekonstrukcji w fakty historyczne.

## Cel

Implementacja potrzebuje stabilnej odpowiedzi na pytanie, co wolno przedstawiać jako historycznie poświadczone, co jako rekonstrukcję, a co wyłącznie jako fikcję SlavicGame. Szczegółowe noty pozostają w `Sources.md`, `MethodAndSources02.md` i kartach poszczególnych nazw; tutaj jest kontrakt produkcyjny.

## Klasy dowodu

- **H — attested/history:** nazwa, praktyka lub relacja ma wskazane świadectwo; zakres twierdzenia nie może przekraczać treści i regionu źródła.
- **R — reconstruction:** współczesna lub dawna interpretacja porównawcza. Może inspirować design, ale UI/kodeks nie przedstawia jej jako pewnego faktu IX–X w.
- **F — fiction:** świadomy author truth/gameplay SlavicGame. F może łączyć motywy, lecz nie może podszywać się pod źródło.
- **U — unresolved:** brak wystarczającego locku. U nie może być wymagane przez core implementation; używamy neutralnego placeholdera albo odkładamy detal.

## Krytyczna hierarchia źródeł

1. Dla konkretnego twierdzenia pierwszeństwo ma możliwie bliskie świadectwo pierwotne z jawnym autorem, datą i regionem.
2. Krytyczne wydania i nowoczesne opracowania służą do kontroli przekładu, kontekstu i `interpretatio christiana`; samo poświadczenie nazwy nie dowodzi kompletnej domeny, genealogii ani jednolitej teologii.
3. Późny folklor i późnośredniowieczne katalogi mogą dokumentować późniejszą tradycję lub historię recepcji, ale nie są automatycznym dowodem kultu IX–X w.
4. Dawne opracowania są mapą sporów i odsyłaczy, nie finalnym arbitrem.
5. Gdy źródła są regionalne, nie budujemy z nich jednego obowiązkowego „panteonu wszystkich Słowian”.

## Rejestr krytyczny

| Grupa | Owner / źródła | Status produkcyjny | Dozwolone użycie |
|---|---|---|---|
| metodologia czytania kronik i tekstów chrześcijańskich | `Sources.md` S11, S20–S23; `MethodAndSources02.md` | **LOCKED** | kontrola biasu, regionu, daty i różnicy między zapisem a rekonstrukcją |
| późny katalog polski / Długosz | S05, S12, S24 + `LatePolishCatalogue.md` | **LOCKED-AS-LATE** | recepcja i inspiracja; nie bezpośredni spis bóstw IX–X w. |
| zachodniosłowiańskie nazwy z kronik i żywotów | S13–S17, S21–S23 + karty nazw | **LOCKED-REGIONAL** | nazwa i zakres tylko zgodnie z konkretnym świadectwem; bez automatycznej unifikacji |
| Rod/Rodzanice | S18 + `RodRodzanice.md` | **LOCKED-WITH-RECONSTRUCTION** | rozdzielać późne świadectwa, folklor i model porównawczy |
| Siwa/Živa/Żywie | S17, S24 + `SiwaZywie.md` | **LOCKED-REGIONAL** | Helmold/Połabie nie uprawnia do automatycznego rozszerzenia na inne regiony |
| Radegast/Swarożyc | S15 + `RadegastSvarozic.md` | **LOCKED-DISPUTED** | zachować spór tożsamości; nie scalać nazw jako pewnika |
| Jarowit | `Jarowit.md` + wskazane tam świadectwa | **LOCKED-REGIONAL** | używać tylko zakresu wynikającego z karty; szersza domena = R/F |
| kandydaci literaccy i późniejsze imiona | `LiteraryCandidates.md` | **NOT CORE-H** | mogą wejść jako R/F po osobnej decyzji; nie podnoszą się automatycznie do H |

## Kontrakt dla implementacji i narracji

Każdy finalny rekord bóstwa/kultu powinien mieć co najmniej `evidenceClass`, `sourceIds`, `sourceRegion`, `sourcePeriod` oraz opcjonalne `designFictionNotes`. Kodeks może prezentować przekonania NPC diegetycznie, ale dane autorskie muszą odróżniać „NPC twierdzi” od H. System patronów, reakcje bóstw, boskie relacje, questowe interwencje i metafizyka świata są przede wszystkim F/author truth, chyba że konkretna część ma osobny lock H/R.

Nie wolno generować brakującej genealogii, domeny, symbolu, małżeństwa, święta, koloru, zwierzęcia ani ikonografii tylko dlatego, że potrzebuje tego UI. Brak historycznego locku oznacza neutralny design F z jawną etykietą, nie fałszywą rekonstrukcję.

## QA / acceptance

- żadna nazwa z późnego katalogu nie jest opisana jako bezpośrednio poświadczona dla IX–X w. bez osobnego wcześniejszego świadectwa;
- regionalne świadectwo nie staje się automatycznie pan-słowiańskie;
- karta może zawierać spór bez wymuszania jednej odpowiedzi;
- gameplayowa metafizyka nie zmienia klasy źródłowej historycznego motywu;
- każde nowe krytyczne twierdzenie wskazuje ownera/source ID albo pozostaje R/F/U.

## Pozostające otwarte prace

Lock zamyka politykę źródłową potrzebną do implementacji, ale nie oznacza końca researchu. Nadal warto pogłębiać pełne krytyczne wydania P03–P12, gdy będą dostępne, oraz dodawać dokładne locatory do finalnych assetów, dialogów i kodeksu. Takie pogłębienie może zwiększyć pewność lub zawęzić twierdzenia, ale nie blokuje obecnego content pipeline.
