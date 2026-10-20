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

## Odblaski terenu

Shader terenu wyznacza kierunek patrzenia od fragmentu do pozycji kamery,
odtworzonej z macierzy `View` w vertex shaderze (tak jak modele PBR).
Dzięki temu odblaski mokrego podłoża podążają za kamerą także poza początkiem świata.
Zmiana nie dodaje tekstur ani deskryptorów Vulkan; koszt to dodatkowy interpolant
`vec3` i odtworzenie pozycji kamery na wierzchołek. Ustawienia wyłączające
PBR lub odbicia nadal pomijają składnik specular. Zmiana dotyczy wyłącznie obrazu.

Weryfikacja: kompilacja shaderów oraz kontrola w grze przy włączonym PBR
terenu i odbiciach, najlepiej podczas deszczu. Przesuwając kamerę i obserwując
ten sam fragment terenu daleko od `(0, 0, 0)`, sprawdź, czy odblask zmienia
położenie wraz z punktem obserwacji. Warto powtórzyć próbę po wyłączeniu odbić.
