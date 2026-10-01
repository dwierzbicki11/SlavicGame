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
