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

## Rozkład GGX materiałów PBR

`pbr.frag` ogranicza roughness do co najmniej `0.06` (po zwilżeniu do
co najmniej `0.08`). Mianownik GGX jest więc dodatni bez stałego epsilonu
na jego kwadracie. Postać `(1 - NdotH²) + NdotH² * alpha²` zachowuje
precyzję przy szczycie odbicia. Ograniczenie kwadratu mianownika do `0.0001`
tłumiło odbicia gładkich i mokrych materiałów.

Zmiana nie dodaje próbek tekstur ani passów i nie wpływa na gameplay.
Wyłączenie PBR/specular nadal wybiera istniejącą ścieżkę diffuse.
Weryfikacja wizualna: porównać gładki materiał w słońcu przy suchej i mokrej
pogodzie, z włączonym oraz wyłączonym specular; odbicie powinno być węższe
i jaśniejsze przy niskiej roughness, bez ciemnego środka.
