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

## Rozkład GGX modeli PBR

`pbr.frag` zachowuje szczyt GGX dla minimalnej roughness 0,06. Mianownik
jest liczony jako `(1 - NdotH²) + NdotH² * roughness⁴`, co ogranicza utratę
precyzji przy `NdotH = 1`. Zabezpieczenie końcowego mianownika wynosi `1e-12`;
wartość `1e-4` tłumiła refleksy gładkich materiałów. Nie zmienia to zestawu
tekstur, flag jakości ani kosztu próbkowania; poprawka dotyczy tylko wyglądu.

Kontrola numeryczna: dla roughness 0,06 / 0,08 / 0,12 / 0,25 / 0,5 / 1
szczyt powinien wynosić `1 / (PI * roughness⁴)`, a całka `D(N,H) * NdotH`
po półsferze powinna być bliska 1. Pełna weryfikacja wymaga kompilacji SPIR-V
i porównania gładkich materiałów w runtime Vulkan.
