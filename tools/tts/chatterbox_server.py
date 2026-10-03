#!/usr/bin/env python3
import argparse
import hashlib
import json
import os
import sys
from importlib.metadata import version as package_version
from pathlib import Path

import numpy as np
import torch
import torchaudio
from chatterbox.mtl_tts import ChatterboxMultilingualTTS


def choose_device(requested: str) -> str:
    if requested and requested != "auto":
        return requested
    if torch.cuda.is_available():
        return "cuda"
    if hasattr(torch.backends, "mps") and torch.backends.mps.is_available():
        return "mps"
    return "cpu"


def select_reference(reference_dir: Path, voice, emotion):
    candidates = []
    if voice:
        candidates.extend([
            reference_dir / f"{voice}_{emotion}.wav",
            reference_dir / f"{voice}.wav",
        ])
    candidates.extend([
        reference_dir / f"{emotion}.wav",
        reference_dir / "default.wav",
    ])
    for path in candidates:
        if path.is_file():
            return str(path)
    return None


def key_for(payload: dict, prompt_path: str | None) -> str:
    data = {
        "text": payload.get("text", ""),
        "voice": payload.get("voice"),
        "emotion": payload.get("emotion", "neutral"),
        "exaggeration": round(float(payload.get("exaggeration", 0.5)), 3),
        "cfg_weight": round(float(payload.get("cfg_weight", 0.5)), 3),
        "speed": round(float(payload.get("speed", 1.0)), 3),
        "prompt": prompt_path or "",
        "model": "chatterbox-multilingual-v3",
        "language": "pl",
    }
    return hashlib.sha256(
        json.dumps(data, sort_keys=True, ensure_ascii=False).encode("utf-8")
    ).hexdigest()


def to_pcm16(wav: torch.Tensor, source_rate: int, target_rate: int = 24000) -> bytes:
    wav = wav.detach().cpu().float()
    if wav.ndim == 1:
        wav = wav.unsqueeze(0)
    if wav.shape[0] > 1:
        wav = wav.mean(dim=0, keepdim=True)
    if source_rate != target_rate:
        wav = torchaudio.functional.resample(wav, source_rate, target_rate)
    wav = torch.clamp(wav.squeeze(0), -1.0, 1.0)
    pcm = (wav.numpy() * 32767.0).astype(np.int16)
    return pcm.tobytes()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--device", default="auto")
    parser.add_argument("--reference-dir", required=True)
    parser.add_argument("--cache-dir", required=True)
    args = parser.parse_args()

    device = choose_device(args.device)
    reference_dir = Path(args.reference_dir)
    cache_dir = Path(args.cache_dir)
    cache_dir.mkdir(parents=True, exist_ok=True)

    try:
        model = ChatterboxMultilingualTTS.from_pretrained(device=device, t3_model="v3")
    except Exception as exc:
        print(json.dumps({"ready": False, "error": str(exc)}), flush=True)
        return 1

    print(json.dumps({
        "ready": True,
        "device": device,
        "sample_rate": 24000,
        "backend": "chatterbox-multilingual-v3",
        "python": sys.version.split()[0],
        "chatterbox": package_version("chatterbox-tts"),
        "torch": torch.__version__
    }), flush=True)

    for raw in sys.stdin:
        raw = raw.strip()
        if not raw:
            continue
        try:
            payload = json.loads(raw)
            if payload.get("shutdown"):
                break

            text = str(payload.get("text", "")).strip()
            if not text:
                raise ValueError("TTS text is empty")

            emotion = str(payload.get("emotion", "neutral")).lower()
            prompt_path = select_reference(
                reference_dir,
                payload.get("voice"),
                emotion,
            )
            cache_key = key_for(payload, prompt_path)
            output = cache_dir / f"{cache_key}.pcm"

            if not output.exists():
                kwargs = {
                    "language_id": "pl",
                    "exaggeration": float(payload.get("exaggeration", 0.5)),
                    "cfg_weight": float(payload.get("cfg_weight", 0.5)),
                }
                if prompt_path:
                    kwargs["audio_prompt_path"] = prompt_path

                wav = model.generate(text, **kwargs)
                output.write_bytes(to_pcm16(wav, int(model.sr), 24000))

            print(json.dumps({
                "ok": True,
                "path": str(output.resolve()),
                "sample_rate": 24000,
                "channels": 1,
            }, ensure_ascii=False), flush=True)
        except Exception as exc:
            print(json.dumps({
                "ok": False,
                "error": f"{type(exc).__name__}: {exc}"
            }, ensure_ascii=False), flush=True)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
