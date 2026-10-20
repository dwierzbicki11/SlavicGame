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

## Rozkład odbłysków PBR

`pbr.frag` używa rozkładu GGX z minimalnym roughness `0.06` (mokre
materiały mają minimum `0.08`). Mianownik rozkładu jest liczony jako
`(1 - NdotH²) + NdotH² * roughness⁴`, bez dodatkowego ograniczania jego
kwadratu. Poprzedni próg `0.0001` spłaszczał odbłyski gładkich materiałów
i zmniejszał energię odbicia. Minimalne roughness zapewnia dodatni mianownik;
ograniczenie `NdotH` do `[0, 1]` chroni go przed błędem zaokrągleń.
Zmiana nie dodaje próbek tekstur ani zasobów GPU i dotyczy wyłącznie wyglądu
materiałów przy włączonych opcjach PBR modeli oraz specular.
