# Architektura silnika i systemów gry

**Wersja 0.1 — fundament techniczny.** Dokument opisuje granice kodu przygotowane pod wymagania z World Bible, Kosmologii i pierwszego grywalnego wycinka. Istnienie modułu nie oznacza ukończenia odpowiadającej mu mechaniki lub zawartości.

## Zasada podziału

Stan świata, dane projektu i prezentacja nie powinny być jednym systemem. Kod rozdziela:

1. **stan i reguły gry** — możliwe do testowania bez okna i GPU;
2. **prezentację** — Vulkan/Veldrid, HUD, później animację i audio;
3. **zawartość** — identyfikatory, definicje NPC, dialogi, questy, rytuały i assety;
4. **stan trwały** — checkpoint zapisujący spójny obraz postępu.

Dzięki temu zadanie, reputacja, relacja z bogiem albo dowód nie zależą od konkretnego modelu 3D czy widżetu UI.

## Moduły

| Katalog | Odpowiedzialność | Stan |
|---|---|---|
| Core | pętla gry, czas klatki i uruchomienie silnika | działa |
| Window, Input | SDL2, klawiatura, mysz i sterowanie | działa |
| Renderer | Vulkan/Veldrid, teren, statyczne i dynamiczne bryły, HUD | prototyp działa |
| World | teren, regiony, pogoda, czas świata, przeszkody i lokalne zjawiska granic sfer | fundament działa |
| AI | zachowanie dynamicznych przeciwników | pierwszy prototyp działa |
| Gameplay | stan gracza i wspólny postęp kampanii | fundament |
| Entity | identyfikowalne obiekty i komponenty | fundament |
| Scene | rejestracja i wybór scen | fundament |
| Assets | stabilne identyfikatory i katalog zasobów | fundament; brak loadera modeli/tekstur |
| Physics | warstwy i kontrakty colliderów | fundament; obecna kolizja terenu/przeszkód pozostaje własna |
| Animation | niezależny stan animacji | fundament; brak skeletal animation |
| Audio | cue, magistrale i poziomy głośności | fundament; brak backendu playback |
| UI | stan ekranów i blokowanie wejścia | fundament; renderer UI do rozwoju |
| Interaction | wybór celu interakcji | fundament; wejście E do podpięcia |
| Inventory | ilości przedmiotów i snapshot | fundament |
| Quest | fazy zadań, rozwiązania i dowody | fundament |
| Dialogue | graf dialogowy, wymagania i efekty | model danych |
| NPC | role oraz harmonogramy dnia/nocy | pięć ról wycinka przygotowanych w WorldState |
| Reputation | oddzielna reputacja NPC/wsi/regionu/frakcji/kultury/instytucji | fundament |
| Relationships | zaufanie, przyjaźń, towarzysze, romans i rywalizacja | fundament |
| Gods | łaska, patronat i jawne zobowiązania | fundament; oddzielny od reputacji kapłanów |
| Magic | źródła magii, koszty, rytuały i receptury | model danych |
| Combat | definicje ataków, kierunku, kosztów i obrażeń | model danych; pełna walka do wdrożenia |
| Save | wersjonowany checkpoint całego istotnego stanu | serializacja i restore działają; brak UI slotów i zapisu na dysk |

## Wspólny postęp

WorldState.Progress jest trwałą warstwą postępu i zawiera:

- profil bohatera;
- ekwipunek;
- dziennik zadań i dowodów;
- reputacje społeczne;
- relacje z bogami;
- relacje z postaciami;
- flagi konsekwencji świata.

WorldState.Cosmology przechowuje lokalne fenomeny granicy sfer oddzielnie od interpretacji NPC.

To celowe rozdzielenie realizuje zasadę dokumentacji: **pogłoska, obserwacja, potwierdzony fakt i interpretacja nie są tym samym**, a **relacja z bogiem nie jest reputacją u jego wyznawców**.

## Checkpoint

SaveGameService zapisuje wersjonowany snapshot:

- pozycję gracza;
- czas i pogodę;
- zdrowie i staminę;
- profil i tytuły;
- ekwipunek;
- zadania, rozwiązania i dowody;
- reputacje;
- relacje boskie i osobowe;
- flagi świata;
- fenomeny kosmologiczne;
- stan przeciwników.

Restore przywraca stan jako całość. To jest techniczny fundament zasady z Kosmologii, że śmierć gracza cofa do spójnego checkpointu bez zachowywania kar z odrzuconej próby.

## Dane pierwszego wycinka

VerticalSliceBootstrap przygotowuje wyłącznie dane uzgodnione w dokumentacji:

- kontrakt light-over-swamp jako oferowany;
- lokalną nieszczelność black-swamp-leak, aktywną nocą;
- flagę przygotowania wycinka.

WorldState przygotowuje pięć roboczych ról NPC z harmonogramem dzień/noc. Nie otrzymują jeszcze ostatecznych imion, modeli ani dialogów.

## Zasady dalszej implementacji

- Systemy runtime korzystają z identyfikatorów, nie z nazw wyświetlanych.
- Quest nie powinien przyznawać nagrody więcej niż raz.
- Dialog wykonuje efekty przez systemy postępu zamiast bezpośrednio modyfikować renderer.
- Bóstwo, jego przejaw, posłaniec, artefakt i instytucja kultowa pozostają osobnymi typami danych.
- Czwarta Sfera ma typ techniczny, ale nie otrzymuje mechaniki swobodnie dostępnej graczowi w pierwszym wycinku.
- Zapis ma wersję; migracje starszych zapisów będą jawne zamiast cichego odczytywania niezgodnych danych.
- Moduł oznaczony jako fundament nie jest traktowany w dokumentacji jako gotowa mechanika.
