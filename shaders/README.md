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

`pbr.frag` zachowuje pełny szczyt rozkładu GGX dla gładkich materiałów.
Roughness ma dolną granicę `0.06`, dzięki czemu mianownik pozostaje dodatni
bez ograniczania go stałą, która tłumiłaby odbłyski. Dla `N dot H = 1`
rozkład powinien wynosić `1 / (PI * roughness^4)`; dla roughness `1`
powinien być stały i równy `1 / PI`. Zmiana dotyczy włączonego PBR
i specular modeli; nie dodaje próbek tekstur ani zmian zasobów Vulkan.
