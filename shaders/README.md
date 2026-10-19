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

## Bias cieni materiałów PBR

`SampleSunShadow` przyjmuje normalną geometrii w przestrzeni świata.
W `pbr.frag` jest to `baseNormal`, sprzed nałożenia normal mapy. Normal mapa
zmienia odpowiedź oświetlenia, ale nie geometrię zapisywaną w shadow depth pass,
więc nie powinna zmieniać biasu porównania głębokości. Zapobiega to uzależnieniu
samocieniowania od detali tekstury i przełącznika normal mappingu.

Zmiana nie dodaje odczytów tekstur ani passów, korzysta z dotychczasowego
fallbacku po wyłączeniu cieni i nie wpływa na gameplay. Weryfikacja:
`./tools/compile-shaders.sh`, następnie porównanie cieni na modelu z wyraźną
normal mapą przy niskim kącie słońca, z normal mappingiem włączonym i wyłączonym.
Bias powinien pozostać taki sam, choć oświetlenie powierzchni będzie się różnić.
