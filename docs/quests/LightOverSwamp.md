# Quest: „Światło nad mokradłem” — pełna karta v0.1

**ID:** `light-over-swamp`  
**Typ:** kontrakt łowcy  
**Region:** Pogranicze Żarnowca  
**Dawca:** `missing-family`

## Problem widoczny dla gracza

Osoba z Żarnowca nie wróciła z drogi przez mokradła. Po zmierzchu przy przeprawie widać światło i słychać wołanie. Droga staje się niebezpieczna.

## Stan faktyczny autorów F

- zaginiona osoba nie żyje;
- jej pamiątka działa jako kotwica dla lokalnego echa/zjawy;
- `swamp-predator` jest osobnym fizycznym zagrożeniem;
- drapieżnik wykorzystuje chaos, ale nie jest samym światłem;
- lokalna nieszczelność Jawia–Nawia zwiększa aktywność zjawiska;
- rozwiązanie lokalnego problemu nie wyjaśnia globalnego kryzysu.

## Warunki rozpoczęcia

- rozmowa z `missing-family`;
- brak wcześniejszego `TurnedIn`;
- gracz może odmówić i wrócić później.

## Fazy

### Offered

Cel:
- porozmawiaj z rodziną.

Przejście:
- Accept → Active.

### Active

Cele:
- porozmawiaj z crossing-keeper;
- porozmawiaj z herbalist;
- dotrzyj do przeprawy.

Po pierwszym oglądzie → Investigation.

### Investigation

Gracz zbiera dowody.

Minimalny zestaw do przejścia dalej:
- pamiątka;
- fizyczne ślady drapieżnika;
- obserwacja anomalii przy przeprawie.

Po rozpoznaniu dwóch różnych kategorii śladów → Preparation.

### Preparation

Opcjonalne:
- craft `marsh-sight-tonic`;
- naucz się `spell.reveal-trace`;
- poznaj rytuał;
- zdobądź `ritual-thread`;
- porozmawiaj ze shrine-keeper.

Quest nie wymaga wszystkich opcji.

Po nadejściu właściwej pory i wejściu do aktywnego miejsca → Encounter.

### Encounter

Dwie niezależne osie:

1. zjawa / echo;
2. predator.

Gracz może najpierw rozwiązać jedną albo drugą.

### Resolved

Warunek:
- jeden z trzech wyników zjawy został zapisany.

Predator może nadal żyć.

### TurnedIn

Rozmowa z rodziną i wypłata nagrody.

Nagroda tylko raz.

## Dowody

### light-over-swamp.witness-light

**Typ:** Rumor  
**Źródło:** mieszkańcy / community-guard  
**Treść:** nocą widziano światło przy przeprawie.

Nie potwierdza tożsamości zjawy.

### light-over-swamp.last-route

**Typ:** ConfirmedFact  
**Źródło:** rodzina + crossing-keeper  
**Treść:** zaginiony miał przejść przez mokradła.

### light-over-swamp.broken-planks

**Typ:** Observation  
**Źródło:** oględziny  
**Treść:** część przeprawy jest fizycznie uszkodzona.

### light-over-swamp.predator-tracks

**Typ:** Observation  
**Źródło:** oględziny  
**Treść:** istnieją tropy dużej istoty niezależne od śladów zjawy.

### light-over-swamp.keepsake

**Typ:** Observation  
**Źródło:** miejsce śmierci  
**Treść:** znaleziono przedmiot osobisty.

### light-over-swamp.keepsake-owner

**Typ:** ConfirmedFact  
**Źródło:** rodzina / charakterystyczna cecha przedmiotu  
**Treść:** przedmiot należał do zaginionego.

### light-over-swamp.apparition-response

**Typ:** Observation  
**Źródło:** kontakt nocny  
**Treść:** zjawa reaguje na pamiątkę.

### light-over-swamp.two-causes

**Typ:** Interpretation → ConfirmedFact po dodatkowym dowodzie  
**Treść:** drapieżnik i zjawa nie są tym samym zagrożeniem.

System powinien pozwolić zastąpić hipotezę mocniejszym wpisem, a nie przepisywać historię źródła.

## Rozwiązanie A — ritual-closure

Wymagania:
- pamiątka;
- potwierdzona tożsamość;
- poznany rytuał;
- właściwa pora.

Efekty:
- `swamp.apparition-released = true`;
- `crossing.reopened = true` po dodatkowym warunku bezpieczeństwa;
- wzrost zaufania rodziny;
- pozytywna reakcja shrine-keeper;
- zużycie odpowiednich składników.

## Rozwiązanie B — conditional-pact

Wymagania:
- potwierdzona tożsamość;
- kontakt ze zjawą;
- zaakceptowane warunki.

Efekty:
- `swamp.apparition-bound = true`;
- nocne ograniczenia pozostają;
- zjawa może później dostarczyć wskazówkę;
- crossing-keeper i guard reagują ostrożniej;
- rodzina reaguje ambiwalentnie.

Warunki umowy muszą być pokazane przed akceptacją.

## Rozwiązanie C — destroy-anchor

Wymagania:
- dostęp do pamiątki.

Efekty:
- `swamp.anchor-destroyed = true`;
- lokalne światło zanika;
- część późniejszych informacji staje się niedostępna;
- gorsza reakcja rodziny i shrine-keeper;
- quest nadal można ukończyć.

## Predator

Stan zapisywany osobno:

- alive;
- driven-off — przyszła opcja;
- dead.

Zabicie nie kończy zjawy.

## Nagroda

Pierwszy prototyp:
- pieniądze;
- niewielka reputacja old-village;
- rezultat relacji zależny od sposobu rozwiązania.

Dokładne liczby pozostają do balansu.

## Warunki porażki

Brak trwałego fail state w vertical slice z powodu:
- przegapionej nocy;
- śmierci gracza;
- nieudanego rytuału.

Możliwy trwały skutek:
- zniszczenie kotwicy jako świadoma decyzja.

## Checkpointy

1. przyjęcie questa;
2. wejście w Preparation po kluczowych dowodach;
3. przed nocnym Encounter;
4. po zapisaniu Resolution;
5. po TurnedIn.

## Dialog po wyniku

Każde rozwiązanie musi zmienić:
- rodzinę;
- crossing-keeper;
- shrine-keeper.

Herbalist i guard mogą reagować zależnie od dodatkowych stanów.

## Kryteria QA

- każda ścieżka działa z nowego save;
- predator i apparition pozostają niezależne;
- reward nie duplikuje się po load;
- noc może wrócić po przegapieniu;
- brak patrona nie blokuje;
- brak preparatu nie blokuje;
- save w każdej fazie odtwarza właściwe cele;
- dziennik zachowuje typ każdego dowodu.


## Runtime interaction pass

Vertical slice ma teraz grywalną ścieżkę opartą o kontekstowe interakcje `E`.

### Kolejność dostępna w runtime

1. **Żarnowiec / rodzina**
   - `E` przy rodzinie przyjmuje kontrakt;
   - quest przechodzi `Offered -> Active`;
   - zapisywane są `witness-light` i `last-route`.

2. **Przeprawa**
   - oględziny uszkodzonej kładki zapisują `broken-planks`;
   - pierwszy ogląd przełącza `Active -> Investigation`.

3. **Mokradło**
   - osobno bada się `predator-tracks`;
   - osobno podnosi `missing-person-keepsake`;
   - nocą 20:00–06:00 można zaobserwować `apparition-response`;
   - obserwacja dzienna nie zalicza anomalii.

4. **Preparation**
   - po pamiątce + tropach drapieżnika + obserwacji anomalii quest wchodzi w `Preparation`;
   - powrót do rodziny z pamiątką dodaje ConfirmedFact `keepsake-owner`;
   - opiekun kręgu jest dostępny w `old-shrine` zgodnie z harmonogramem dziennym;
   - rozmowa z nim zapisuje wiedzę o kotwicy, dwa znaki i znajomość rytuału;
   - zielarka wydaje `ritual-thread` dopiero po poznaniu rytuału.

5. **Encounter**
   - poprawne rozpoczęcie rytuału przełącza `Preparation -> Encounter`;
   - sukces rytuału przełącza quest do `Resolved`.

6. **TurnedIn**
   - powrót do rodziny pozwala oddać kontrakt;
   - nagroda prototypowa: 40 Money, +8 reputacji `old-village`, +10 Trust u `missing-family`;
   - `ClaimReward` i world flag blokują duplikację nagrody.

### Persistence

Cały przebieg korzysta z już zapisywanych:
- quest phase/resolution/evidence;
- inventory;
- world flags;
- money;
- reputation;
- relationships.

Nie jest wymagana nowa wersja save.
