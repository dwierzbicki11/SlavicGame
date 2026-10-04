# Streaming i memory budget contract — v0.1

Status: production planning / measurement-gated. Owner: engine + content production.

## Cel

Ten dokument definiuje wspólny kontrakt dla resident memory, streamingu regionów i degradacji zasobów. Nie wpisuje niezmierzonych limitów MB/GB. Konkretne wartości stają się targetami dopiero po profilowaniu reprezentatywnych scen R0–R6 na docelowych klasach sprzętu.

## Zasada nadrzędna

Każdy asset z `ProductionAssetManifests.md` musi mieć klasę residency/streaming, ownera i bezpieczny fallback. Przekroczenie budżetu nie może usuwać elementu wymaganego przez quest, evidence, combat telegraph, nawigację ani accessibility.

## Klasy residency

| Klasa | Znaczenie | Przykłady | Reguła |
|---|---|---|---|
| M0 | always-resident core | UI core, player rig, podstawowe interakcje | tylko dane wymagane globalnie |
| M1 | region-resident | wspólne materiały/props/AI archetypes aktywnego regionu | ładowane przy wejściu do regionu, zwalniane po bezpiecznym przejściu |
| M2 | cell/POI streamed | cięższa geometria, warianty środowiska, lokalne audio/VFX | streamowane z wyprzedzeniem wg traversal graph |
| M3 | event/quest burst | cutscene, boss/event variants, unikalne dialogue/AVFX | prefetch przed gate; zwolnienie po persistence-safe zakończeniu |
| M4 | optional cosmetic | dalekie warianty, dekoracja, high-detail extras | pierwsze do degradacji/odroczenia |

Klasa nie jest miarą MB. Określa cykl życia i priorytet odzyskiwania pamięci.

## Domeny pamięci

Telemetry musi osobno raportować co najmniej: CPU heap/runtime, geometry/mesh, textures/materials, animation, audio, VFX buffers, AI/navigation/world-state oraz transient upload/staging. Nie wolno maskować wzrostu jednej domeny samym spadkiem innej.

Każdy pomiar zapisuje: build SHA, region/scenę, preset jakości, rozdzielczość, czas sesji, klasę sprzętu, peak i steady-state oraz informację czy wystąpił eviction/reload spike.

## Streaming contract

1. Świat jest dzielony na region -> cell/POI -> event payload; granice techniczne nie muszą odpowiadać granicom fabularnym.
2. Traversal graph wskazuje sąsiadów, których prefetch jest dozwolony. Teleport/fast travel używa osobnego loading gate zamiast udawać płynny traversal.
3. Quest/evidence dependency ma pin do chwili bezpiecznego checkpointu; streamer nie może wyrzucić jedynego aktywnego obiektu potrzebnego do progresji.
4. Save zapisuje stan logiczny i stable IDs, nigdy zależność od tego, czy konkretna cell jest obecnie resident.
5. Unload wymaga braku aktywnej referencji gameplayowej lub kontrolowanego proxy. Referencje do niezaładowanych obiektów rozwiązujemy przez ID/handle, nie raw object lifetime.
6. Prefetch distance/time jest tuningiem zależnym od zmierzonej prędkości traversal i IO; bez pomiaru pozostaje parametrem konfiguracyjnym.
7. Streaming failure ma prowadzić do placeholder/proxy/loading gate, nie do utraty questa ani crasha.

## Pressure states i degradacja

Runtime wystawia cztery stany jakości pamięci, niezależne od konkretnych progów liczbowych:

- `NORMAL` — pełny preset;
- `PRESSURE_1` — ograniczenie M4, agresywniejsze LOD/texture residency i audio virtualization;
- `PRESSURE_2` — redukcja wariantów M2/M3, wcześniejsze eviction niekrytycznych payloadów, ograniczenie kosmetycznych VFX;
- `CRITICAL` — zachowanie gameplay-critical payloadów, minimalny bezpieczny zestaw render/audio, kontrolowany loading gate zamiast thrash/OOM.

Nigdy nie degradujemy czytelności combat telegraphów, quest/evidence cues, napisów ani wymaganych interakcji. Kolejność degradacji musi respektować `AnimationVfxAudioBudgetContract.md`.

## Regiony R0–R6 — sceny pomiarowe

Do locku liczbowego wymagamy przynajmniej jednego cold-load, traversal i steady-state capture na region:

- R0: gęsta osada + wnętrze/wyjście + NPC;
- R1: gród/przeprawa + tłum/usługi;
- R2: las + ograniczona widoczność + encounter/guardian payload;
- R3: port/woda + łodzie + audio środowiskowe;
- R4: otwarty teren/worksite + dalekie LOD;
- R5: route/camp/settlement z culture-specific variants;
- R6: finałowy obszar + anomaly VFX/audio + event burst.

Dodatkowo wymagany jest długi traversal przez co najmniej trzy kolejne cell bez restartu, aby wykryć leak, fragmentation i reload thrash.

## Measurement gate

Liczbowy budget może przejść do `LOCKED` dopiero gdy:

- istnieje reprezentatywny content zamiast pustej testowej sceny;
- pomiar jest powtarzalny na co najmniej minimalnej i rekomendowanej klasie testowej;
- znane są peak, steady-state i transient spike;
- cold/warm cache oraz szybki traversal nie powodują nieakceptowalnego stutter/thrash;
- save/load po eviction odtwarza identyczny stan logiczny;
- najgorsza reprezentatywna scena ma zapas ustalony na podstawie pomiarów, nie intuicji.

Do tego czasu wszystkie MB/GB pozostają `TBD-MEASURED`.

## Dane/telemetry

Minimalny rekord:

```text
memorySampleId
buildSha
regionId
cellId
scenarioId
hardwareClass
qualityPreset
resolution
residentClassCounts
cpuMemory
gpuMemory
streamingReadRate
pendingLoads
evictions
reloads
frameHitchCount
measurementState
```

Format jednostek i backend telemetry może się zmienić; semantyka pól i stable scenario IDs pozostają kontraktem.

## QA / failure tests

- szybki obrót kamery i sprint przez granicę cell nie może ujawnić gameplay-critical missing asset;
- powtarzane R0->R1->R0 oraz analogiczne przejścia nie mogą wykazywać monotonicznego wzrostu pamięci bez uzasadnionego cache;
- quest object przypięty przez dependency pozostaje logicznie dostępny po unload/reload;
- save wykonany tuż przed unloadem i load po restarcie odtwarza ten sam world-state;
- `PRESSURE_1/2/CRITICAL` można wymusić w developer overlay i sprawdzić kolejność degradacji;
- brak zasobu M4 nie może blokować progresji;
- brak zasobu krytycznego ma dać kontrolowany fallback/log, nie silent corruption.

## Otwarte locki

1. konkretne CPU RAM i GPU VRAM ceilings per hardware class;
2. dopuszczalny peak/transient headroom;
3. cell size, prefetch horizon i IO throughput targets;
4. dopuszczalny hitch/frame-time percentile podczas streamingu;
5. finalne texture/mesh residency policy per quality preset;
6. minimal/recommended hardware.

Wszystkie powyższe są performance locks i wymagają telemetry. Nie blokują implementacji streamera, stable handles, pressure states ani instrumentacji.