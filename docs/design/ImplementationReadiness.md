# Implementation Readiness — v0.1

## Purpose

This document separates two milestones:
- **ready to implement**: stable contracts exist for programmers;
- **production complete**: content, balance, assets and measured budgets are final.

## Ready now

The following areas have first-pass specifications suitable for implementation:
- stable content IDs and save persistence;
- input/action mapping;
- HUD, inventory, journal, dialogue and map flows;
- melee, bow, equipment and status effects;
- progression and tracking;
- encounters and timed events;
- locations and interactables;
- economy, vendors and alchemy;
- NPC/dialogue state;
- quest/evidence state;
- day/night and weather;
- divine relationships as an optional layer;
- region/world state;
- developer overlay and logging.

## Vertical slice contract

The implementation target is already defined for:
- Żarnowiec;
- Puszcza Żywia;
- Czarne Mokradła;
- Kamienny Krąg;
- five core NPCs;
- Światło nad mokradłem;
- three resolution paths;
- first ritual and spell;
- starter items;
- persistent world reactions.

## Campaign contract

Acts 0–V, the regional flow, central author truth, main quest skeleton and ending architecture now provide enough context to build systems without inventing story rules in code.

## Decisions that remain soft

These do not block programming:
- exact balance numbers;
- final names of some characters and places;
- exact number of side quests;
- final art assets;
- dialogue polish;
- full constructed languages;
- measured hardware requirements;
- final performance budgets;
- exact ordering of some Act I/II content.

## Change-control rule

Changing a stable contract should record:
1. why it changed;
2. affected documents/systems;
3. save/content migration impact;
4. regression-test impact.

## Remaining path to production-complete documentation

The largest remaining packages are:
- complete main quest cards;
- full region bibles;
- recurring NPC/companion production cards;
- final content catalogues;
- final creature cards;
- a dedicated research package for the non-Slavic-inspired Arel culture;
- asset and production budgets;
- measured performance targets;
- final balance after playtests.

## Status

The project has crossed the **ready-to-implement first-pass** threshold. Documentation work should now deepen production detail in parallel with implementation instead of blocking programming.
