using System.Numerics;

namespace SlavicGame.Engine.Audio;

public static class RiverAmbienceSynthesizer
{
    public const int SampleRate = 24000;
    public const double SegmentSeconds = 1.8;

    public static float DistanceToRiver(Vector3 listener)
    {
        var horizontal = MathF.Abs(
            listener.X -
            SlavicGame.Engine.World.WaterLandscape.CenterX(listener.Z));
        return MathF.Max(
            0f,
            horizontal -
            SlavicGame.Engine.World.WaterLandscape.SurfaceHalfWidth(listener.Z));
    }

    public static float Attenuation(float distance)
    {
        if (!float.IsFinite(distance))
            return 0f;

        distance = MathF.Max(0f, distance);
        if (distance <= 4f)
            return 1f;
        if (distance >= 78f)
            return 0f;

        var t = (distance - 4f) / 74f;
        t = Math.Clamp(t, 0f, 1f);
        var smooth = t * t * (3f - 2f * t);
        return 1f - smooth;
    }

    public static PcmAudio Generate(float volume, int segmentIndex = 0)
    {
        volume = Math.Clamp(volume, 0f, 1f);

        var frames = (int)Math.Round(SampleRate * SegmentSeconds);
        var data = new byte[frames * sizeof(short)];
        uint state = unchecked((uint)(0x71A3B5D9 + segmentIndex * 0x45D9F3B));
        float low = 0f;
        float slow = 0f;

        for (var i = 0; i < frames; i++)
        {
            state = state * 1664525u + 1013904223u;
            var noise = ((state >> 8) / 8388607.5f) - 1f;

            low += (noise - low) * 0.085f;
            slow += (low - slow) * 0.018f;
            var waterNoise =
                slow * 0.78f +
                (low - slow) * 0.48f +
                noise * 0.055f;

            var t = i / (float)SampleRate;
            var rippleTone =
                MathF.Sin(t * MathF.Tau * 91f + segmentIndex * 0.37f) * 0.035f +
                MathF.Sin(t * MathF.Tau * 173f + segmentIndex * 0.19f) * 0.022f;

            var fadeFrames = (int)(SampleRate * 0.05f);
            var edgeFade = 1f;
            if (i < fadeFrames)
                edgeFade = i / (float)fadeFrames;
            else if (i > frames - fadeFrames)
                edgeFade = (frames - i) / (float)fadeFrames;

            var sample = Math.Clamp(
                (waterNoise + rippleTone) *
                volume *
                0.42f *
                Math.Clamp(edgeFade, 0f, 1f),
                -1f,
                1f);

            var pcm = (short)MathF.Round(sample * short.MaxValue);
            data[i * 2] = (byte)(pcm & 0xff);
            data[i * 2 + 1] = (byte)((pcm >> 8) & 0xff);
        }

        return new PcmAudio(data, SampleRate, 1, 16);
    }
}
