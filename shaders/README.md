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

```bash
python3 -m unittest discover -s tests/renderer -p 'test_*.py'
```

Test wykonuje wyrażenia z funkcji `DistributionGGX` w `pbr.frag` i sprawdza
szczyt oraz kształt rozkładu dla obsługiwanego zakresu szorstkości.
Mianownik GGX zachowuje małe wartości dla gładkich materiałów (roughness
od 0.06); zbyt wysoki próg tłumił ich refleksy. Test nie zastępuje kompilacji
SPIR-V ani kontroli obrazu na GPU.
