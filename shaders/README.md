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

## Regresja GGX materiałów modeli

`python3 tools/test-pbr-ggx.py` sprawdza bez GPU wyrażenia funkcji
`DistributionGGX` z `pbr.frag`: wartość w maksimum odbłysku, granicę przy
kącie stycznym oraz skończone, nieujemne wyniki w zakresie roughness 0.06–1.
Mianownik zachowuje mały składnik roughness⁴ bez odejmowania zbliżonych
wartości. Nie stosuje dolnego limitu tłumiącego odbłyski gładkich materiałów.
Test numeryczny nie zastępuje kompilacji SPIR-V ani oceny obrazu Vulkan.
