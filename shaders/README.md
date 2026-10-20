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

`pbr.frag` zachowuje szczyt rozkładu GGX dla gładkich materiałów.
Mianownik używa postaci `(1 - NdotH²) + NdotH² * roughness⁴`,
bez stałego dolnego limitu, który tłumił odblaski. Roughness materiału
pozostaje ograniczona do co najmniej `0.06`, co zapobiega dzieleniu przez zero.
Zmiana nie dodaje próbek tekstur ani wariantu pipeline'u i nie wpływa na gameplay;
ścieżka z wyłączonym PBR lub specular zachowuje dotychczasowy fallback diffuse.
Weryfikacja obrazu: porównać odblask słońca na modelach o roughness
`0.06`, `0.2` i `1.0`, również podczas deszczu; gładki materiał powinien mieć
węższy i jaśniejszy odblask. Wymaga to działającego renderera Vulkan.
