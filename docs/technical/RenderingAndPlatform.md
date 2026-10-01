# Rendering i platformy — v0.1

Dokument rozwija etapy 19–20 roadmapy.

## Stos

- C#;
- .NET 11;
- Veldrid;
- Vulkan jako wymagany backend pierwszego runtime;
- SDL2 dla okna i wejścia.

## Cele PC

Pierwsza platforma: PC.

Priorytety:
- Windows;
- Linux;
- klawiatura + mysz;
- później kontroler.

Android jest platformą późniejszą i nie może ograniczać architektury PC przed wykonaniem pomiarów.

## Renderer

Aktualny prototyp obsługuje:
- teren;
- statyczne bryły;
- dynamiczne bryły przeciwnika;
- atmosferę;
- mgłę;
- podstawową zmianę oświetlenia;
- HUD.

## Docelowe warstwy

1. resource management;
2. mesh/material pipeline;
3. texture loading;
4. skeletal animation;
5. lighting;
6. shadows;
7. particles;
8. post-processing;
9. water;
10. vegetation;
11. sky/weather;
12. UI rendering;
13. profiling;
14. opcjonalnie zaawansowane efekty po stabilizacji podstaw.

## Vulkan

Nie wiążemy logiki gameplayu z obiektami GPU.

Dane gry muszą działać w testach CPU-only.

## Materiały

Docelowy materiał powinien rozdzielać:
- albedo;
- normal;
- roughness;
- metallic, jeśli potrzebne;
- emissive;
- maski środowiskowe.

Dokładny model PBR zostanie zatwierdzony po pierwszym realnym asset pipeline.

## Streaming

Potrzebny dopiero po wzroście regionów.

Planowana hierarchia:
- region;
- chunk / sector;
- asset bundle / katalog;
- stan trwały obiektów.

## Android

Przed portem sprawdzamy:
- Vulkan device coverage;
- pamięć;
- wielkość assetów;
- sterowanie dotykowe;
- shader complexity;
- CPU budget;
- termikę.

## Zaawansowane efekty

Ray tracing, FSR/DLSS i inne techniki są opcjonalne.

Nie projektujemy podstawowego wyglądu gry tak, żeby wymagał RT.

## Zasada kompatybilności

Każda nowa funkcja renderingu powinna mieć:
- fallback;
- koszt GPU;
- sposób testu;
- informację, czy wpływa na gameplay.

Mgła używana jako mechanika musi zachować czytelność również na niższych ustawieniach.

## Stan obecny

Renderer jest prototypem. Dokument nie deklaruje gotowości produkcyjnej ani końcowych wymagań sprzętowych.
