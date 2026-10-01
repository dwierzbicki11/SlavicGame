# Szablon karty przedmiotu — v0.1

Każdy finalny przedmiot projektowy powinien mieć jedną kartę albo dane równoważne temu formatowi.

```text
ID:
DisplayNameKey:
Category:
Tags:
Source/Region:
HistoricalBasis:
FictionLayer:
GameplayPurpose:
Stackable:
MaxStack:
QuestCritical:
Sellable:
Droppable:
Usable:
EquipSlot:
Effects:
Requirements:
Persistence:
RelatedRecipes:
RelatedQuests:
AssetModel:
AssetIcon:
Audio:
LocalizationNotes:
OpenQuestions:
QA:
```

## Zasady

- ID jest trwałe i nietłumaczone.
- Nazwa wyświetlana może się zmienić bez migracji save.
- QuestCritical ma pierwszeństwo przed Drop/Sell.
- HistoricalBasis odsyła do research card albo ma wartość `F-only`.
- GameplayPurpose odpowiada na pytanie „po co gracz ma ten przedmiot?”.
- Przedmiot nie dostaje statystyki tylko dlatego, że pole istnieje.

## Minimalne QA

- nie da się zgubić przedmiotu krytycznego bez alternatywy;
- stack nie przekracza limitu;
- save/load zachowuje ilość i stan;
- efekt jest przyznawany dokładnie raz;
- brak assetu daje jawny placeholder/fallback.
