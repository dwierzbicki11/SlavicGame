using SlavicGame.Engine.Audio;

internal static class AudioAssetRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var music = WavPcmLoader.LoadAsset(
            "assets/audio/music/r0_day_woodland.wav");

        check(
            music.SampleRate == 24000 &&
            music.Channels == 1 &&
            music.BitsPerSample == 16,
            "Music assets use the SDL PCM16 mono 24 kHz runtime format");

        check(
            WavPcmLoader.DurationSeconds(music) >= 7.5,
            "Exploration music contains a real loop-length payload");

        var quiet = WavPcmLoader.ApplyGain(music, 0.25f);
        check(
            quiet.Data.Length == music.Data.Length &&
            Peak(quiet.Data) < Peak(music.Data),
            "Audio bus gain scales PCM amplitude without changing duration");

        foreach (var relative in new[]
                 {
                     "assets/audio/music/r0_night_marsh.wav",
                     "assets/audio/music/combat_predator.wav",
                     "assets/audio/music/ritual_threshold.wav",
                     "assets/audio/sfx/melee_swing.wav",
                     "assets/audio/sfx/bow_release.wav",
                     "assets/audio/sfx/magic_cast.wav",
                     "assets/audio/sfx/ritual_start.wav",
                     "assets/audio/sfx/water_splash.wav"
                 })
        {
            var audio = WavPcmLoader.LoadAsset(relative);
            check(
                audio.Data.Length > 1000,
                $"Audio asset '{relative}' is present and non-empty");
        }
    }

    private static int Peak(byte[] data)
    {
        var peak = 0;
        for (var i = 0; i + 1 < data.Length; i += 2)
        {
            var value = Math.Abs(
                (int)(short)(data[i] | (data[i + 1] << 8)));
            peak = Math.Max(peak, value);
        }
        return peak;
    }
}
