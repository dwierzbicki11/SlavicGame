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

## Refleksy PBR modeli

`pbr.frag` ogranicza roughness materiału do zakresu 0,06–1,0. Rozkład GGX
oblicza mianownik jako `(1 - NdotH²) + NdotH² * roughness⁴`, aby zachować
precyzję przy maksimum refleksu. Nie należy ograniczać kwadratu tego mianownika
epsilonem 0,0001: tłumiłoby to refleksy gładkich materiałów. Dla `NdotH = 1`
oczekiwana wartość rozkładu to `1 / (PI * roughness⁴)` (około 24561 przy
roughness 0,06 i 0,31831 przy roughness 1). Zmiana dolnego limitu roughness
wymaga ponownego sprawdzenia stabilności tego dzielenia.
