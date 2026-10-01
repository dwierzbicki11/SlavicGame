# Format definicji broni i pancerza — v0.1

## Weapon

```text
ID:
Class:
DamageType:
BaseDamage:
Range:
AttackSet:
StaminaCostMultiplier:
WindupModifier:
RecoveryModifier:
BlockCapability:
ProjectileDefinition:
AmmoType:
Requirements:
Durability:
HistoricalBasis:
Assets:
Audio:
VFX:
QA:
```

## Armor

```text
ID:
Slot:
PhysicalProtection:
Resistances:
MobilityModifier:
StaminaModifier:
Durability:
HistoricalBasis:
Assets:
Audio:
QA:
```

## Reguły

- brak jednego GearScore;
- `HistoricalBasis` nie oznacza, że statystyki są historyczne;
- balans jest F;
- finalny typ broni musi mieć osobną kartę źródłową, jeśli używa historycznej nazwy;
- `Durability` może być wyłączone globalnie bez łamania definicji;
- projectile weapon oddziela definicję broni od pocisku.

## QA wspólne

- equip/unequip nie duplikuje itemu;
- invalid slot jest odrzucany;
- statystyki są skończone i w dozwolonym zakresie;
- zapis wyposażenia używa ID, nie indeksu tablicy.
