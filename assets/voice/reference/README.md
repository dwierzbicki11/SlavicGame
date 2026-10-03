# Voice reference clips

This directory is optional. SlavicGame's free local Chatterbox TTS can run without reference audio, but reference clips improve character identity and emotion.

Use only recordings you own or are licensed to use.

Resolution order for a request with voice `hunter` and emotion `fearful`:

1. `hunter_fearful.wav`
2. `hunter.wav`
3. `fearful.wav`
4. `default.wav`
5. no reference clip — Chatterbox's built-in voice is used.

Recommended reference clips are clean mono speech, roughly 5–15 seconds, without music or effects.

Supported emotion keys:

- neutral
- calm
- warm
- uneasy
- fearful
- angry
- sad
- whisper
- solemn
- mystical
- urgent

A character can therefore have one neutral reference plus selected emotional variants instead of recording every dialogue line.

## Protagonist spell voice

Spell casting uses the stable voice ID `protagonist` by default.

Recommended first references:

- `protagonist.wav` — neutral identity of the player character's voice;
- `protagonist_urgent.wav` — forceful combat casting such as `ZAR VEK`;
- `protagonist_solemn.wav` — restorative casting such as `ZIVA DAR`;
- `protagonist_mystical.wav` — quiet ritual casting such as `VEDA NAW`.

The player-character voice ID can later come from character creation. For development it can already be overridden with `SLAVICGAME_PLAYER_VOICE`.
