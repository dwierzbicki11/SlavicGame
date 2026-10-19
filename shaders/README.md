# Shadery SlavicGame

Źródła GLSL są w `shaders/src/`:

- `terrain.vert` / `terrain.frag` — sześciowarstwowe materiały PBR podłoża,
- `actor.vert` / `actor.frag` — prosty dynamiczny pass postaci i przeciwników,
- `hud.vert` / `hud.frag` — HUD,
- `pbr.vert` / `pbr.frag` — modele GLB z materiałami PBR,
- `sky.vert` / `sky.frag` — proceduralne niebo, horyzont i słońce.

Renderer **nie przechowuje shaderów jako stringów C#**. Vulkan ładuje skompilowane pliki SPIR-V z:

```text
shaders/bin/*.spv
```

## Kompilacja

```bash
./tools/compile-shaders.sh
```

Skrypt użyje `glslc` albo `glslangValidator`.

Linux Mint / Ubuntu:

```bash
sudo apt install glslang-tools
```

lub, jeśli pakiet jest dostępny:

```bash
sudo apt install glslc
```

`./run.sh` automatycznie kompiluje shadery przed kompilacją i uruchomieniem gry.

Pliki `.spv` są artefaktami generowanymi i nie są źródłem prawdy. Modyfikuj pliki `.vert` / `.frag`.

## GGX modeli PBR

`pbr.frag` oblicza mianownik rozkładu GGX jako
`|N × H|² + roughness⁴ * (N · H)²` dla znormalizowanych wektorów.
Ta postać ogranicza utratę precyzji przy `N · H` bliskim 1. Zabezpieczenie
końcowego mianownika wynosi `1e-12`, aby nie tłumić refleksów przy minimalnej
obsługiwanej chropowatości `0.06`. Zmiana nie wymaga nowych zasobów ani funkcji
Vulkan; dotyczy materiałów z włączonym PBR i odbiciami zwierciadlanymi.
Ocenę wyglądu najgładszych materiałów należy wykonać także w runtime Vulkan.
