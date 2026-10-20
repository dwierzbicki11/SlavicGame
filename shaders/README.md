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

## Regresja GGX

`python3 -m unittest discover -s tests -p 'test_pbr_ggx.py'` sprawdza
wyrażenia skalarne funkcji `DistributionGGX` ze źródła shadera PBR względem
rozkładu GGX, w tym szczyt refleksu dla minimalnej szorstkości `0.06`.
Mianownik zachowuje mały składnik `roughness^4`; próg bezpieczeństwa `1e-12`
nie tłumi refleksów w obsługiwanym zakresie szorstkości `0.06–1.0`.
Test CPU nie zastępuje kompilacji SPIR-V ani weryfikacji obrazu Vulkan.
