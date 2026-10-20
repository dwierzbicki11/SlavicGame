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

## Odbłyski PBR

`pbr.frag` oblicza rozkład GGX dla szorstkości ograniczonej do minimum
`0.06`. Mianownik używa postaci `(1 - NdotH²) + NdotH² * roughness⁴`,
aby zachować precyzję przy `NdotH = 1`. Nie należy ograniczać końcowego
mianownika stałą `0.0001`: tłumi to wąskie odbłyski gładkich materiałów.
Zmiana nie dodaje próbek tekstur ani zasobów GPU i obejmuje istniejący
wariant PBR; wariant bez odbłysków pozostaje dostępny w ustawieniach.
