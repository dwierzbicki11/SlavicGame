# Magia pierwszego wycinka — szczegóły v0.1

Dokument uszczegóławia jeden czar i jeden rytuał pierwszego vertical slice. Całość mechaniczna jest F.

## Czar: spell.reveal-trace

**Robocza nazwa:** Odsłonięcie śladu  
**Źródło mocy:** dawna moc / nauczona technika — ostateczne źródło pozostaje do decyzji  
**Typ:** utility / tracking

### Funkcja

Przez krótki czas wzmacnia czytelność śladów posiadających aktywną sygnaturę nadnaturalną.

Nie:
- wskazuje kierunku strzałką;
- nazywa istoty;
- zastępuje dowodu;
- pokazuje wszystkich sekretów przez ściany.

### Prototyp kosztu

- stamina: niski/średni koszt;
- cooldown: krótki;
- brak składników.

Finalne wartości ustalamy po playteście.

### Użycie w zadaniu

Może pomóc zauważyć:
- ślad zjawy;
- różnicę wobec fizycznych tropów drapieżnika;
- aktywność nieszczelności.

### Nauka

Pierwszy vertical slice powinien nauczyć czaru jawnie, bez ukrycia obowiązkowej mechaniki za przypadkowym sekretem.

## Rytuał: ritual.release-bound-echo

**Robocza nazwa:** Domknięcie więzi  
**Źródło:** SoulsAndNawia  
**Miejsce:** Kamienny Krąg lub wyznaczone miejsce przy nieszczelności — wybór po grayboxie  
**Pora:** noc / aktywne okno zjawiska

### Wymagania

- `missing-person-keepsake`;
- `ritual-thread`;
- minimum jeden potwierdzony dowód tożsamości;
- wiedza o związku kotwicy ze zjawą;
- właściwe okno czasowe.

### Sekwencja F

1. wyznaczenie obszaru;
2. umieszczenie kotwicy;
3. użycie znaku rozpoznania;
4. oczekiwanie na reakcję;
5. potwierdzenie tożsamości;
6. zamknięcie więzi;
7. obserwacja skutku.

To nie jest rekonstrukcja realnego słowiańskiego obrzędu.

## Możliwe błędy

### Brak dowodu tożsamości

Rytuał rozpoczyna się, ale reakcja jest niejednoznaczna. Gracz otrzymuje komunikat, czego brakuje.

### Zła pora

Miejsce nie reaguje. Nie zużywamy krytycznego quest item.

### Brak składnika

Rytuału nie można rozpocząć; UI pokazuje brak.

### Przerwanie przez zagrożenie

Rytuał można powtórzyć po zabezpieczeniu miejsca.

## Cena

Cena pierwszego rytuału ma być:
- zrozumiała;
- ograniczona;
- nieodwracalna tylko tam, gdzie jest to fabularnie logiczne.

W vertical slice cena to głównie czas, składniki i utrata kotwicy po prawidłowym domknięciu.

## Pierwszy znak alfabetu F

Robocza funkcja: **rozpoznanie / wskazanie tożsamości celu**.

Nie nadajemy mu jeszcze finalnej nazwy ani grafiki.

Wymagania dla projektu symbolu:
- prosty do narysowania;
- odróżnialny od realnych alfabetów;
- możliwy do połączenia z kolejnymi znakami;
- czytelny w UI i na świecie.

## Drugi znak alfabetu F

Robocza funkcja: **granica / zamknięcie**.

Razem pierwszy + drugi tworzą najprostszy wzorzec rytualny vertical slice.

## Nauka znaków

Gracz:
1. widzi znak;
2. otrzymuje znaczenie praktyczne;
3. stosuje go w kontrolowanym miejscu;
4. dopiero potem używa go w prawdziwym rytuale.

## Powiązanie z UI

UI rytuału musi rozróżniać:
- warunek spełniony;
- brak składnika;
- brak dowodu;
- zła pora;
- aktywne zagrożenie;
- gotowość.

## Persistence

Save przechowuje:
- poznanie czaru;
- poznanie znaków;
- poznanie rytuału;
- wynik rytuału;
- zużyte trwałe składniki tylko po zatwierdzonym wykonaniu.

## Otwarte

- finalne źródło pierwszego czaru;
- wartości staminy/cooldown;
- finalny wygląd alfabetu;
- dokładne miejsce rytuału;
- VFX/audio.


## Runtime status — release-bound-echo

Pierwszy executable pass rytuału jest podpięty do runtime.

Sterowanie:
- `R` — próba rozpoczęcia rytuału;
- podczas aktywnego rytuału ruch, zwykłe czary i cutscenki są blokowane;
- HUD pokazuje bieżący etap lub konkretny brakujący warunek.

Runtime wymaga:
- flagi poznania `magic.ritual.release-bound-echo.learned`;
- znaków `magic.sign.identity.learned` i `magic.sign.boundary.learned`;
- miejsca `old-shrine` / Kamienny Krąg;
- okna 20:00–06:00;
- `missing-person-keepsake`;
- `ritual-thread`;
- ConfirmedFact `light-over-swamp.keepsake-owner`;
- wiedzy `light-over-swamp.anchor-link-known`;
- braku aktywnego zagrożenia w promieniu 25 m.

Sekwencja runtime:
1. wyznaczenie granicy;
2. umieszczenie kotwicy;
3. znak rozpoznania;
4. oczekiwanie na reakcję;
5. potwierdzenie tożsamości;
6. domknięcie więzi;
7. obserwacja skutku.

Przerwanie przez obrażenia lub zagrożenie nie zużywa krytycznych przedmiotów. `missing-person-keepsake` i `ritual-thread` są usuwane dopiero w finalnym commit point po ukończeniu wszystkich etapów.

Sukces:
- ustawia `swamp.apparition-released`;
- ustawia `magic.ritual.release-bound-echo.completed`;
- zapisuje `QuestResolution.RitualClosure` dla `light-over-swamp`;
- może uruchomić scenę `contract-resolution`;
- rezultat, zużycie przedmiotów i quest resolution przechodzą przez istniejący save/load.


## Pierwszy pass VFX rytuału

Runtime rytuału ma lekki proceduralny efekt geometryczny bez dodatkowych tekstur ani ciężkich particle systemów:

- krąg na ziemi narasta razem z ogólnym postępem rytuału;
- po umieszczeniu kotwicy pojawia się centralny marker;
- znak rozpoznania dodaje główne osie symbolu;
- oczekiwanie na reakcję dodaje cztery markery odpowiedzi;
- domknięcie więzi dodaje przekątne i zamyka wzór;
- sukces zostawia krótki około 2-sekundowy błysk/pełny krąg, który następnie znika.

Budżet jest celowo mały i regresje pilnują, aby aktywny efekt pozostawał poniżej 512 dodatkowych wierzchołków. To pierwszy pass pod słabsze iGPU; finalne VFX mogą później zastąpić lub rozszerzyć ten mesh zależnie od benchmarków.


## Nauka czarów — runtime progression

Czary nie są już automatycznie dostępne od początku. Wiedza jest trwałym stanem świata zapisywanym jako flagi `magic.spell.*.learned`.

Sterowanie:
- `L` — spróbuj nauczyć się dostępnego czaru w bieżącym miejscu;
- `Q` — zmienia tylko pomiędzy już poznanymi czarami;
- `F` — nie pozwala rzucić niepoznanego czaru.

HUD nie pokazuje inkantacji ani kosztu czaru przed jego poznaniem.

### spell.spark — ISKRA / ZAR VEK

Źródło lekcji: `shrine-keeper`  
Miejsce: `old-shrine` / Kamienny Krąg

Wymagania:
- dotrzeć do Kamiennego Kręgu;
- kontrakt `light-over-swamp` musi być co najmniej w stanie `Offered`.

To jest pierwszy kontrolowany trening i nie zużywa przedmiotu.

### spell.mend — SZEPT ŻYCIA / ZIVA DAR

Źródło lekcji: `herbalist`  
Miejsce: `old-village` / Żarnowiec

Wymagania:
- znać `spell.spark`;
- wcześniej realnie spaść do 70% zdrowia lub niżej;
- posiadać `simple-bandage`.

Koszt nauki:
- 1 × `simple-bandage` jako materiał praktyczny.

Nowa gra dostaje dwa startowe bandaże zgodnie z założeniem ekwipunku startowego vertical slice.

### spell.reveal-trace — ODSŁONIĘCIE ŚLADU / VEDA NAW

Źródło lekcji: `shrine-keeper`  
Miejsce: `old-shrine`

Wymagania:
- znać `spell.spark`;
- posiadać potwierdzony dowód `light-over-swamp.keepsake-owner`;
- znać związek kotwicy ze zjawą: `light-over-swamp.anchor-link-known`.

Lekcja nie zużywa przedmiotu. Ma być nagrodą za poprawne połączenie wiedzy śledczej, nie za farmienie zasobów.

## Persistence i fail-forward

- poznane czary są zapisywane przez istniejące world flags;
- save/load zachowuje wiedzę;
- po load wybrany czar jest normalizowany do faktycznie poznanego;
- brak spełnionego warunku nie zużywa żadnego materiału;
- wymagania są data-driven przez `SpellLessonDefinition` i `SpellLessonRequirement`, więc kolejne czary mogą wymagać przedmiotów, reputacji, dowodów, wcześniejszych czarów lub etapów questa.
