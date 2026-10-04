using System.Text;

namespace SlavicGame.Engine.Audio;

public static class WavPcmLoader
{
    public static PcmAudio LoadAsset(string relativePath) =>
        Load(ResolveAssetPath(relativePath));

    public static PcmAudio Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: false);

        if (ReadFourCc(reader) != "RIFF")
            throw new InvalidDataException($"Audio asset '{path}' is not RIFF.");

        _ = reader.ReadUInt32();
        if (ReadFourCc(reader) != "WAVE")
            throw new InvalidDataException($"Audio asset '{path}' is not WAVE.");

        ushort format = 0;
        ushort channels = 0;
        uint sampleRate = 0;
        ushort bitsPerSample = 0;
        byte[]? data = null;

        while (stream.Position + 8 <= stream.Length)
        {
            var chunkId = ReadFourCc(reader);
            var chunkSize = reader.ReadUInt32();
            var chunkStart = stream.Position;

            if (chunkStart + chunkSize > stream.Length)
                throw new InvalidDataException(
                    $"Audio asset '{path}' has a truncated '{chunkId}' chunk.");

            switch (chunkId)
            {
                case "fmt ":
                    if (chunkSize < 16)
                        throw new InvalidDataException(
                            $"Audio asset '{path}' has an invalid fmt chunk.");

                    format = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    sampleRate = reader.ReadUInt32();
                    _ = reader.ReadUInt32();
                    _ = reader.ReadUInt16();
                    bitsPerSample = reader.ReadUInt16();
                    break;

                case "data":
                    data = reader.ReadBytes(checked((int)chunkSize));
                    break;
            }

            stream.Position = chunkStart + chunkSize + (chunkSize & 1u);
        }

        if (format != 1 || channels != 1 ||
            sampleRate != 24000 || bitsPerSample != 16)
        {
            throw new NotSupportedException(
                $"Audio asset '{path}' must be PCM16 mono 24 kHz; " +
                $"got format={format}, channels={channels}, " +
                $"rate={sampleRate}, bits={bitsPerSample}.");
        }

        if (data is null || data.Length == 0 || (data.Length & 1) != 0)
            throw new InvalidDataException(
                $"Audio asset '{path}' has no valid PCM payload.");

        return new PcmAudio(
            data,
            checked((int)sampleRate),
            channels,
            bitsPerSample).Validate();
    }

    public static PcmAudio ApplyGain(PcmAudio audio, float gain)
    {
        ArgumentNullException.ThrowIfNull(audio);
        audio.Validate();
        gain = Math.Clamp(gain, 0f, 1f);

        if (Math.Abs(gain - 1f) <= 0.0001f)
            return audio;

        var scaled = new byte[audio.Data.Length];
        for (var i = 0; i < audio.Data.Length; i += 2)
        {
            var sample = (short)(audio.Data[i] | (audio.Data[i + 1] << 8));
            var value = Math.Clamp(
                (int)MathF.Round(sample * gain),
                short.MinValue,
                short.MaxValue);

            scaled[i] = (byte)(value & 0xff);
            scaled[i + 1] = (byte)((value >> 8) & 0xff);
        }

        return new PcmAudio(
            scaled,
            audio.SampleRate,
            audio.Channels,
            audio.BitsPerSample);
    }

    public static double DurationSeconds(PcmAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);
        audio.Validate();

        var bytesPerFrame =
            audio.Channels * (audio.BitsPerSample / 8);

        return audio.Data.Length /
               (double)(audio.SampleRate * bytesPerFrame);
    }

    private static string ResolveAssetPath(string relativePath)
    {
        var normalized = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, normalized),
            Path.Combine(Directory.GetCurrentDirectory(), normalized)
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException(
            $"Audio asset '{relativePath}' was not found. " +
            $"Checked output and working directories.");
    }

    private static string ReadFourCc(BinaryReader reader) =>
        Encoding.ASCII.GetString(reader.ReadBytes(4));
}
