# Boskie umowy — reguły gameplay v0.1

Projekt F. Nie jest rekonstrukcją historycznej praktyki.

## Cel

Boska umowa ma być świadomym wyborem:
- daje konkretną możliwość;
- ma jawny koszt;
- tworzy zobowiązanie;
- nie jest loot boxem z ukrytą karą.

## Elementy kontraktu

Każda umowa zawiera:

1. `deityId`;
2. ofertę;
3. efekt mechaniczny;
4. warunek aktywacji;
5. cenę;
6. obowiązek;
7. warunek naruszenia;
8. sposób wygaśnięcia;
9. czy można renegocjować;
10. skutki zerwania.

## Jawność

Przed akceptacją gracz poznaje:
- co otrzyma;
- czego zobowiązuje się przestrzegać;
- co jest uznawane za złamanie.

Gra może ukrywać:
- reakcję świata;
- przyszłą sytuację konfliktu zobowiązań.

Nie ukrywa podstawowych warunków.

## Kilku patronów

System techniczny dopuszcza kilku patronów.

Konflikt nie wynika z arbitralnej reguły:
> bogowie zawsze są zazdrośni.

Wynika z konkretnych obowiązków.

Przykład F:
- Perun: dotrzymaj jawnej przysięgi;
- Weles: zachowaj tajemnicę.

Problem powstaje, jeśli gracz przysiągł ujawnić prawdę, a jednocześnie zobowiązał się jej nie zdradzać.

## Naruszenie

Rozróżniamy:
- **failure** — gracz próbował, ale nie zdołał;
- **breach** — świadomie złamał warunek.

Nie każdy failure jest zdradą.

## Renegocjacja

Niektóre umowy można:
- spłacić;
- zakończyć;
- zmienić;
- przekazać alternatywną przysługę.

To zależy od konkretnego boga i treści.

## Brak umowy

Każda kluczowa sekwencja fabularna ma wariant:
- bez patrona;
- przez umiejętność człowieka;
- wiedzę;
- przedmiot;
- relację społeczną.

## Pierwszy vertical slice

Boski kontakt jest opcjonalny.

Nie powinien:
- dawać jedynego rozwiązania;
- automatycznie mówić prawdy o zjawie;
- wyjaśniać Czwartej Sfery.

## Persistence

Save zapisuje:
- favor;
- patron flag;
- obligations;
- fulfilled/broken state.

## QA

- warunek widoczny przed accept;
- nie można ukryć obowiązku przez UI;
- save/load zachowuje status;
- dwa kontrakty mogą współistnieć;
- konflikt kontraktów wynika z treści.
