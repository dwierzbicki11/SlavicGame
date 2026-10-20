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

## Stabilność GGX w materiałach PBR

`pbr.frag` ogranicza chropowatość materiału do minimum 0,06. Rozkład GGX
oblicza mianownik jako `(1 - NdotH²) + NdotH² * roughness⁴`, aby uniknąć
odejmowania niemal równych liczb przy gładkich powierzchniach. Nie należy
dodawać limitu `0.0001` do końcowego mianownika `PI * denom²`: tłumi on
refleksy gładkich materiałów. Dla `NdotH = 1` wynik powinien wynosić
`1 / (PI * roughness⁴)` (około 24560,95 dla chropowatości 0,06).
