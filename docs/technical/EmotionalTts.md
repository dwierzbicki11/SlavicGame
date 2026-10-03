# Emotional TTS — first runtime integration

SlavicGame can synthesize cinematic narration and magic incantations at runtime with emotional acting direction.

## Provider

The first provider uses the OpenAI `/v1/audio/speech` endpoint with `gpt-4o-mini-tts`.

No API key is stored in the repository. TTS is automatically enabled only when `OPENAI_API_KEY` is present. Set `SLAVICGAME_TTS=0` to force-disable it.

Optional environment variables:

- `SLAVICGAME_TTS_MODEL` — default: `gpt-4o-mini-tts`;
- `SLAVICGAME_TTS_VOICE` — default: `cedar`.

Example Linux launch:

```bash
export OPENAI_API_KEY="..."
export SLAVICGAME_TTS_VOICE="cedar"
dotnet run -c Release
```

Do not commit API keys or put them in settings.json.

## Emotional direction

Voice lines carry `VoiceDirection` with:

- emotion;
- intensity 0..1;
- speaking speed;
- optional actor/persona direction.

Supported game-level emotions:

- neutral;
- calm;
- warm;
- uneasy;
- fearful;
- angry;
- sad;
- whisper;
- solemn;
- mystical;
- urgent.

The provider turns these values into acting instructions that explicitly request natural Polish, believable pauses, breath and emphasis, and forbid robotic/navigation-style delivery.

## Current hooks

- every current cinematic shot has explicit emotional direction;
- every magic incantation is spoken with a ritual/mystical direction;
- `DialogueNode` already accepts optional `VoiceDirection` for future playable NPC conversations.

## Playback

The API returns PCM16 mono at 24 kHz. The game queues it directly through SDL2, keeping the path cross-platform and avoiding a Windows-only audio library.

Synthesis is asynchronous and does not block the frame loop. Starting a newer voice line cancels the older pending request and clears queued speech.

Repeated identical lines are cached in memory for the current game session, which avoids repeated API calls for frequently cast spells.

## Production follow-ups

Before final voice production:

1. assign stable character voice IDs/personas;
2. add a persistent on-disk cache or pre-generation pipeline;
3. expose Voice volume and TTS enable/disable in Settings;
4. tune Polish pronunciation of fictional names and magic words;
5. author per-dialogue emotions instead of relying on neutral fallback;
6. consider recorded actor VO for critical scenes while retaining TTS for prototyping/dynamic content.
