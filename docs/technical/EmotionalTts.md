# Emotional TTS — free local runtime

SlavicGame uses a local open-source TTS path primarily for the **player character speaking magic incantations**. Cinematic narration and future NPC dialogue remain optional extensions. The game does not require a paid speech API.

## Backend

The runtime backend is **Chatterbox Multilingual** by Resemble AI.

Reasons for this choice:
- open-source / MIT;
- Polish language support;
- local inference after model download;
- multilingual zero-shot voice cloning;
- controllable expression strength through `exaggeration`;
- suitable for reusable character voice references.

No OpenAI API key, ElevenLabs key or other paid TTS credential is read by the game.

## Install

The helper environment is deliberately separate from the C# runtime.

Linux:

```bash
sudo apt install python3.11 python3.11-venv
bash tools/tts/setup-local.sh
dotnet run -c Release
```

Windows PowerShell:

```powershell
./tools/tts/setup-local.ps1
dotnet run -c Release
```

The setup creates `.venv-tts` and installs `chatterbox-tts`. On the first synthesized line, Chatterbox downloads its open model files into the normal local model cache. After the files are present, synthesis itself does not require a paid service.

Use `SLAVICGAME_TTS=0` to force-disable speech.

By default the runtime uses `SpellsOnly` scope: only protagonist spell incantations request TTS. Set `SLAVICGAME_TTS_SCOPE=all` only when testing cinematic/NPC voice generation.

Optional overrides:
- `SLAVICGAME_TTS_PYTHON` — full path to the Python executable;
- `SLAVICGAME_TTS_DEVICE` — `auto`, `cpu`, `cuda` or `mps`;
- `SLAVICGAME_TTS_REFERENCE_DIR` — voice reference directory;
- `SLAVICGAME_TTS_CACHE_DIR` — generated PCM cache;
- `SLAVICGAME_TTS_SCRIPT` — sidecar path.

## Runtime architecture

C# starts one persistent local Python sidecar on the first requested line. The model is loaded once and subsequent requests reuse it.

Protocol:
1. C# sends one JSON request per line;
2. the Python process synthesizes Polish speech locally;
3. output is normalized to PCM16 mono 24 kHz;
4. the generated line is stored in a persistent local cache;
5. C# reads the PCM and queues it through SDL2.

The frame loop never waits synchronously for synthesis. A newer requested voice line cancels playback of an older pending line.

No generated voice cache is committed to Git.

## Emotion model

Game content continues to use:
- emotion category;
- intensity 0..1;
- speed metadata;
- optional persona;
- optional stable voice ID.

The local adapter maps intensity to Chatterbox `exaggeration` and `cfg_weight`. Strong fear, anger, urgency and mystical delivery receive more expression; calm and whisper use lower exaggeration.

Chatterbox's expression control changes intensity, but categorical acting such as "fear" versus "anger" is most reliable when the game also supplies an emotion-matched reference clip.

## Character and emotion reference clips

Optional references live in:

```text
assets/voice/reference/
```

For voice `hunter` with emotion `fearful`, the resolver tries:

1. `hunter_fearful.wav`
2. `hunter.wav`
3. `fearful.wav`
4. `default.wav`
5. Chatterbox built-in voice

This means one actor/reference voice can be reused for many lines, while selected emotional reference clips can make key scenes more convincing.

Use only recordings that the project has permission to use.

## Current hooks

- protagonist spell incantations are the default and primary TTS use;
- `ZAR VEK` uses a short, forceful/urgent casting direction;
- `ZIVA DAR` uses a slower solemn/restorative direction;
- `VEDA NAW` uses a lower, more mystical near-whisper direction;
- all spells use stable voice ID `protagonist`, overridable through `SLAVICGAME_PLAYER_VOICE`;
- cinematic shots still carry voice direction metadata, but do not synthesize by default;
- `DialogueNode` supports per-line `VoiceDirection` for future optional NPC voice;
- repeated identical lines persist in the local generated-audio cache.

## Performance

A GPU is optional but greatly improves first-generation latency. CPU inference is supported by the architecture but can be slow, especially on the first line after launch.

For a game build with many fixed lines, the preferred production workflow is to pre-generate/cache dialogue during development. Runtime generation remains useful for prototyping and dynamic content.

## Production follow-ups

1. assign stable voice IDs to important NPCs;
2. prepare legally usable neutral/emotional reference clips;
3. add a build-time voice-pack pre-generation command;
4. expose Voice volume and TTS on/off in game Settings;
5. tune fictional names and magic-word pronunciation;
6. keep recorded actor VO possible for major scenes without changing dialogue APIs.
