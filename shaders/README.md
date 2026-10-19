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

## Regresja GGX bez GPU

`python3 tools/test-pbr-ggx.py` wymaga Python 3 i kompilatora `c++` z C++17.
Test wykonuje funkcję `DistributionGGX` pobraną bezpośrednio z `pbr.frag`
na CPU, porównując próbki z analitycznym wzorem w podwójnej precyzji.
Obejmuje minimalną szorstkość 0.06 i wzrost maksimum refleksu przy malejącej
szorstkości. Stabilna postać mianownika zachowuje wąskie refleksy gładkich
materiałów bez wcześniejszego spłaszczenia przez limit `0.0001`.
Test nie zastępuje kompilacji SPIR-V ani oceny obrazu na Vulkan.
