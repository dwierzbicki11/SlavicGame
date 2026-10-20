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

`python3 tools/check-pbr-ggx.py` sprawdza 56 próbek produkcyjnej funkcji
`DistributionGGX` względem wzorca o podwójnej precyzji, z tolerancją 0.1%.
Wymaga kompilatora C (`cc` lub zmienna `CC`) i wykonuje obliczenia CPU float;
nie zastępuje kompilacji SPIR-V ani testu obrazu Vulkan.

Dla roughness 0.06–1.0 mianownik GGX zachowuje wartości małych refleksów:
przy N.H = 1 i roughness = 0.06 wynosi około 5.3e-10. Dawny próg 1e-4
tłumił szczyt refleksu. Obliczenie używa teraz postaci ograniczającej utratę
precyzji przy N.H bliskim 1 oraz zabezpieczenia 1e-12.
