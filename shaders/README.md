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

`python3 tools/check-pbr-ggx.py` porównuje obliczenia funkcji `DistributionGGX`
z shadera PBR ze wzorem referencyjnym dla 56 kombinacji chropowatości i kąta.
Sprawdza też wzrost szczytu refleksu przy malejącej chropowatości. Mianownik
zachowuje szczyt dla minimalnej obsługiwanej chropowatości 0.06; wcześniejszy
limit 0.0001 sztucznie go tłumił. Test CPU nie zastępuje kompilacji SPIR-V
ani oceny obrazu na Vulkanie.
