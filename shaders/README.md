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

`python3 tools/test-pbr-ggx.py` sprawdza numerycznie funkcję `DistributionGGX`
odczytaną z `pbr.frag`: analityczną wysokość refleksu, skończoność,
monotoniczność i odporność na błąd zaokrąglenia iloczynu skalarnego.
Mianownik zachowuje składnik roughness⁴ przy wierzchołku refleksu;
wcześniejsze ograniczenie całego mianownika do `0.0001` tłumiło refleksy
gładkich materiałów. Poprawka korzysta z istniejącego minimum roughness `0.06`,
nie dodaje odczytów tekstur i nie zmienia ścieżki wyłączonego PBR ani gameplayu.
Test CPU nie zastępuje kompilacji SPIR-V i oceny obrazu na Vulkan.
