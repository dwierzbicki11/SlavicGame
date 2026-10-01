# Instytucje kultowe SlavicGame — projekt F v0.1

Ten dokument opisuje **fikcyjne instytucje**, nie historyczną strukturę wszystkich religii słowiańskich.

## Zasada

Bóg ≠ przejaw ≠ posłaniec ≠ kapłan/opiekun ≠ świątynia ≠ instytucja wyznawców.

Każdy element ma osobny ID i reputację/relację.

## Typy instytucji F

### Opiekun lokalnego miejsca

Mała skala.

Zadania:
- utrzymanie miejsca;
- przekazywanie lokalnych reguł;
- prowadzenie wybranych rytuałów;
- mediacja sporów.

Vertical slice:
- `shrine-keeper`.

### Wspólnota kultowa

Grupa rodzin/osób:
- wspólny patron;
- święto;
- zobowiązanie;
- zasób.

Nie musi mieć zawodowego kapłana.

### Ośrodek świątynny

Duża instytucja późniejszego regionu:
- ziemia;
- magazyny;
- pielgrzymi;
- informacje;
- polityka.

Wymaga osobnego worldbuilding/research package.

### Wędrowny specjalista

Rytualista/wróżbita/lekarz:
- niezależny od jednej świątyni;
- może łączyć praktyczne i religijne role.

Finalna nazwa wymaga researchu kulturowego.

## Reputacja

`ReputationScope.ReligiousInstitution` jest oddzielne od `DivineRelationship`.

Przykład:
- bóg może być przychylny;
- jego lokalni opiekunowie mogą nie ufać graczowi.

Albo odwrotnie.

## Władza

Instytucja może mieć interes:
- duchowy;
- gospodarczy;
- polityczny.

Gra nie zakłada, że przedstawiciel kultu zawsze mówi prawdę o bogu.

## Konflikty

- kontrola dostępu do rytuału;
- interpretacja znaku;
- własność miejsca;
- obowiązki wspólnoty;
- spór o poprawność tradycji.

## Brak patrona

Instytucje mogą reagować na niezależność gracza, ale główna gra zachowuje ścieżkę bez boskiego patrona.
