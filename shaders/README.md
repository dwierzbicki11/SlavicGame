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

## Regresja rozkładu GGX

```bash
python3 tools/test-pbr-ggx.py
```

Test wymaga kompilatora `c++` z C++17, bez GPU. Wykonuje funkcję
`DistributionGGX` odczytaną z `pbr.frag` w arytmetyce `float` i porównuje
42 przypadki z referencją o podwójnej precyzji. Sprawdza też ograniczenie
cosinusa przy błędzie zaokrąglenia. Nie zastępuje kompilacji GLSL ani testu obrazu.

Mianownik GGX zachowuje dodatni składnik `roughness^4` także na szczycie
refleksu. Stały dolny limit mianownika tłumił refleksy gładkich materiałów.
Funkcja zakłada chropowatość co najmniej `0.06`, zgodnie ze ścieżką materiału.
