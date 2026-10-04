#!/usr/bin/env python3
"""Regenerate SlavicGame's original procedural audio pack.

No external samples and no third-party Python packages are required.
Output is PCM16 mono 24 kHz WAV, matching Engine/Audio/SdlPcmPlayer.cs.
"""

from __future__ import annotations
import math
import random
import wave
from array import array
from pathlib import Path

SR = 24_000
ROOT = Path(__file__).resolve().parents[2] / "assets" / "audio"

def midi(note: int) -> float:
    return 440.0 * 2.0 ** ((note - 69) / 12.0)

def tone(freq: float, seconds: float, gain: float = 0.2) -> list[float]:
    count = int(seconds * SR)
    out = []
    for i in range(count):
        t = i / SR
        fade = min(1.0, t / 0.03, (seconds - t) / 0.10)
        out.append(gain * max(0.0, fade) * math.sin(2 * math.pi * freq * t))
    return out

def mix(buffers: list[list[float]]) -> list[float]:
    size = max(map(len, buffers))
    out = [0.0] * size
    for source in buffers:
        for i, value in enumerate(source):
            out[i] += value
    peak = max(1e-9, max(abs(v) for v in out))
    scale = min(1.0, 0.82 / peak)
    return [v * scale for v in out]

def write(path: Path, samples: list[float]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    pcm = array("h", (int(max(-1.0, min(1.0, v)) * 32767) for v in samples))
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(1)
        wav.setsampwidth(2)
        wav.setframerate(SR)
        wav.writeframes(pcm.tobytes())

def drone(seconds: float, notes: tuple[int, ...], gain: float) -> list[float]:
    out = [0.0] * int(seconds * SR)
    for i in range(len(out)):
        t = i / SR
        out[i] = gain * sum(
            math.sin(2 * math.pi * midi(n) * t) +
            0.2 * math.sin(2 * math.pi * midi(n + 12) * t)
            for n in notes
        ) / len(notes)
    return out

def melody(seconds: float, events: list[tuple[float, int]]) -> list[float]:
    out = [0.0] * int(seconds * SR)
    for start, note in events:
        src = tone(midi(note), 1.25, 0.14)
        offset = int(start * SR)
        for i, value in enumerate(src):
            if offset + i < len(out):
                out[offset + i] += value
    return out

def noise_hit(seconds: float, seed: int, gain: float = 0.3) -> list[float]:
    rng = random.Random(seed)
    out = []
    for i in range(int(seconds * SR)):
        t = i / SR
        out.append(gain * math.exp(-16 * t) * (rng.random() * 2 - 1))
    return out

def main() -> None:
    write(ROOT / "music/r0_day_woodland.wav",
          mix([drone(8, (38, 45), .075),
               melody(8, [(0, 50), (2, 53), (4, 55), (6, 57)])]))
    write(ROOT / "music/r0_night_marsh.wav",
          mix([drone(8, (34, 41), .09),
               melody(8, [(.7, 46), (2.5, 49), (4.4, 51), (6.2, 49)])]))
    write(ROOT / "music/combat_predator.wav",
          mix([drone(6, (36, 43), .07),
               melody(6, [(0, 48), (1.5, 51), (3, 53), (4.5, 46)])]))
    write(ROOT / "music/ritual_threshold.wav",
          mix([drone(7, (33, 40, 45), .095),
               melody(7, [(0, 45), (1.8, 48), (3.6, 52), (5.2, 48)])]))

    write(ROOT / "sfx/ui_confirm.wav", tone(midi(76), .16, .24))
    write(ROOT / "sfx/ui_back.wav", tone(midi(64), .18, .22))
    write(ROOT / "sfx/melee_swing.wav", noise_hit(.28, 1, .38))
    write(ROOT / "sfx/bow_draw.wav", tone(110, .42, .16))
    write(ROOT / "sfx/bow_release.wav", tone(220, .22, .35))
    write(ROOT / "sfx/magic_cast.wav",
          mix([tone(230, .65, .15), tone(690, .65, .10)]))
    write(ROOT / "sfx/ritual_start.wav",
          mix([tone(midi(45), 1.0, .20), tone(midi(52), .8, .10)]))
    write(ROOT / "sfx/interaction_pickup.wav",
          mix([tone(midi(69), .32, .16), tone(midi(76), .24, .12)]))
    write(ROOT / "sfx/water_splash.wav", noise_hit(.42, 9, .25))
    write(ROOT / "sfx/impact_hit.wav", noise_hit(.30, 12, .42))
    write(ROOT / "sfx/jump.wav", tone(150, .24, .22))

    print(f"Generated audio pack in {ROOT}")

if __name__ == "__main__":
    main()
