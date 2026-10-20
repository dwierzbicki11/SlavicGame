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

## Rozkład odbicia PBR

Shader modeli używa GGX z szorstkością ograniczoną do zakresu 0,06–1.
Mianownik rozkładu jest liczony jako `(1 - NdotH²) + NdotH² * roughness⁴`,
aby zachować precyzję przy wąskich refleksach. Zabezpieczenie dzielenia
`1e-12` leży poniżej najmniejszego mianownika w tym zakresie; większy próg
`1e-4` tłumił odbicia gładkich materiałów. Zmiana nie dotyczy uproszczonego
oświetlenia terenu ani filtrowania map cieni.
