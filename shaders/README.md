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

## Regresja numeryczna PBR

`python3 tools/check-pbr-ggx.py` sprawdza produkcyjną funkcję `DistributionGGX`
na CPU (wymaga `c++`). Porównuje 56 próbek z analitycznym rozkładem GGX,
w tym maksimum odbłysku dla minimalnej roughness 0.06 i kąty styczne.
Test nie zastępuje kompilacji SPIR-V ani oceny obrazu na GPU.

Mianownik GGX korzysta z `dot(cross(n, h), cross(n, h))`, aby uniknąć
odejmowania bliskich wartości przy maksimum odbłysku. Nie stosujemy stałego
progu mianownika: tłumiłby energię gładkich materiałów. Dodatnią wartość
zapewnia istniejący dolny limit roughness. Wzór opisuje także
[dokumentacja Filament](https://google.github.io/filament/Filament.md.html).
Zmiana nie dodaje tekstur ani passów i nie wpływa na gameplay; istniejący
tryb bez PBR pozostaje ścieżką uproszczoną. Koszt funkcji obejmuje dodatkowy
iloczyn wektorowy i skalarny; czasu GPU jeszcze nie zmierzono.
