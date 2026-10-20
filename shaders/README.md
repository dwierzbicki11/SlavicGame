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

## Regresja GGX bez GPU

```bash
PYTHONDONTWRITEBYTECODE=1 python3 -m unittest discover -s tests -p 'test_pbr_ggx.py'
```

Test odczytuje skalarną funkcję `DistributionGGX` bezpośrednio z `pbr.frag`
i sprawdza analityczną wysokość szczytu, skończone nieujemne wartości oraz
całkę po rzutowanej półsferze. Mianownik zachowuje składnik `roughness^4`
przy `N.H = 1`; wcześniejsze ograniczenie go do `0.0001` tłumiło odbicia
gładkich materiałów. Dodatniość zapewnia istniejący zakres roughness
`[0.06, 1.0]`. Zmiana nie dodaje próbek tekstur ani opcji, nie wpływa na
gameplay i pozostawia uproszczone oświetlenie przy wyłączonym PBR/specular.
Test CPU nie zastępuje kompilacji SPIR-V ani oceny obrazu na Vulkanie.
