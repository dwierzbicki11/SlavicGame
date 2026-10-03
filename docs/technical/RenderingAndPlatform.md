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


## Vulkan fullscreen orientation

Renderer używa jednej kanonicznej orientacji UV dla wszystkich fullscreen passów Vulkan.

Zasada:

- scene/resolved scene — bez ręcznego Y-flipa;
- bloom — bez ręcznego Y-flipa;
- postprocess/FXAA — bez ręcznego Y-flipa;
- FSR1 EASU — bez ręcznego Y-flipa;
- FSR1 RCAS — bez ręcznego Y-flipa;
- final present — bez ręcznego Y-flipa.

Źródłem wcześniejszego błędu było uzależnianie orientacji od MSAA/presetu oraz dodatkowe odwracanie obrazu wewnątrz FSR. Ultra działało poprawnie, ponieważ jego ścieżka Native + MSAA omijała część tych wyjątków. Teraz Low/Balanced/High/Ultra oraz ręczne rozdzielczości korzystają z tej samej orientacji tekstur.

FSR1 nadal ma dwie ścieżki:

- **Native** — brak EASU/RCAS, gdy rozdzielczość wejściowa = wyjściowej;
- **Upscale** — EASU -> RCAS dla mniejszej rozdzielczości wewnętrznej.

Różnica dotyczy wyłącznie skalowania, nie orientacji obrazu. Żaden pass FSR nie może samodzielnie odwracać osi Y.

Regresje pilnują, że niższe presety rzeczywiście uruchamiają upscale FSR, ale `RequiresFinalRcasYFlip` pozostaje fałszywe. Dodatkowy check CI blokuje ponowne dodanie znanych ręcznych wzorców Y-flipa do shaderów present/EASU/RCAS.


## FSR pixel-coordinate convention

The native/Ultra presentation path uses interpolated fullscreen UV coordinates and is known to render with the correct vertical orientation.

The real FSR upscale path must therefore derive its integer output-pixel coordinates from those same fullscreen UVs instead of from `gl_FragCoord`. With Veldrid's Vulkan clip-space normalization, the viewport can be represented with a flipped Y direction; raw fragment coordinates can then disagree with the texture UV convention even though ordinary fullscreen sampling remains correct.

Current rule:
- `present.frag`: interpolated UV;
- `fsr_easu.frag`: output pixel derived from `fsin_TexCoord * outputSize`;
- `fsr_rcas.frag`: output pixel derived from `fsin_TexCoord * textureSize(EasuedColor)`;
- no FSR pass may use `gl_FragCoord` for the FSR pixel index.

This keeps Low/Balanced/High on the same orientation convention as the working Ultra/Native path while preserving the actual EASU + RCAS upscale pipeline.
