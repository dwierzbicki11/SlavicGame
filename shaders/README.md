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

## GGX materiałów PBR

`pbr.frag` oblicza mianownik GGX jako `(1 - NdotH²) + NdotH² * roughness⁴`,
aby uniknąć odejmowania zbliżonych wartości przy `NdotH = 1`. Zabezpieczenie
`1e-7` działa przed podniesieniem mianownika do kwadratu i pozostaje poniżej
minimum wynikającego z obsługiwanej szorstkości `0.06`. Dzięki temu gładkie
materiały zachowują wąskie odbicia; wcześniejsze ograniczenie całego mianownika
do `0.0001` tłumiło szczyt rozkładu. Dot product jest ograniczony do `[0, 1]`.

Zmiana dotyczy istniejącej ścieżki PBR; fallback bez specular pozostaje dostępny.
Nie dodaje próbek tekstur ani przebiegów renderera i nie wpływa na gameplay.
Kontrola numeryczna powinna porównać rozkład z analitycznym GGX dla szorstkości
`0.06–1.0`, w tym szczyt `D(1) = 1 / (PI * roughness⁴)` oraz granice dot product.
Po kompilacji SPIR-V potrzebna jest również kontrola wyglądu gładkich materiałów
w Vulkanie; sama kontrola CPU nie potwierdza jakości obrazu ani kosztu GPU.
