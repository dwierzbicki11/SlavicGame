# „Światło nad mokradłem” — macierz reakcji v0.1

Skala nie jest moralnością. Tabela opisuje kierunek reakcji.

| Strona | Ritual closure | Conditional pact | Destroy anchor |
|---|---|---|---|
| missing-family | ++ | 0/+ | -- |
| crossing-keeper | ++ | -/0 | + |
| herbalist | ++ | + | 0/- |
| community-guard | + | 0/- | + |
| shrine-keeper | ++ | zależnie od warunków | -- |
| old-village reputation | + | 0/+ | 0/+ |
| apparition | released | remains under terms | link broken |
| crossing | reopened when safe | night restrictions | may reopen quickly |
| knowledge preserved | high | high | lower |

## Modyfikatory

### Predator alive
- guard/crossing-keeper nie uznają miejsca za całkiem bezpieczne;
- crossing may remain partially restricted.

### Predator dead
- physical danger reduced;
- nie zmienia apparition outcome.

### Full evidence collected
- family reaction łagodniejsza nawet przy trudnym wyniku;
- Journal ma pełniejsze wyjaśnienie.

### No owner confirmation + destroy
- największy spadek zaufania rodziny;
- shrine-keeper krytykuje działanie bez rozpoznania.

### Pact terms disclosed
- guard/crossing-keeper reagują lepiej.

### Pact terms hidden
- krótkoterminowo brak konfliktu;
- późniejsza utrata zaufania, jeśli prawda wyjdzie.

## World flags

Ritual:
- `swamp.apparition-released`

Pact:
- `swamp.apparition-bound`
- `swamp.pact-terms-known` opcjonalnie

Destroy:
- `swamp.anchor-destroyed`

Independent:
- `swamp.predator-dead`
- `crossing.reopened`
- `light-over-swamp.full-evidence`

## Epilog lokalny

Vertical slice nie potrzebuje cinematic epilogue.

Wystarczy:
- dialog;
- zmieniony stan przeprawy;
- zmieniony ruch NPC;
- Journal resolution.
