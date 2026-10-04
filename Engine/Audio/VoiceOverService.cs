using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Audio;

public sealed class VoiceOverService : IDisposable
{
    private readonly ITextToSpeechProvider? _provider;
    private readonly SdlPcmPlayer? _player;
    private readonly object _gate = new();
    private readonly Dictionary<string, PcmAudio> _memoryCache = new(StringComparer.Ordinal);
    private CancellationTokenSource? _requestCancellation;
    private int _generation;
    private bool _disposed;

    public bool IsEnabled => _provider is not null && _player is not null;
    public float Gain { get; set; } = 1f;

    private VoiceOverService(ITextToSpeechProvider? provider, SdlPcmPlayer? player)
    {
        _provider = provider;
        _player = player;
    }

    public static VoiceOverService CreateFromEnvironment()
    {
        var config = LocalTtsRuntimeConfig.FromEnvironment();
        if (!config.Enabled)
        {
            EngineLog.Info(
                "Free local emotional TTS is not installed. Run tools/tts/setup-local.sh (Linux/macOS) " +
                "or tools/tts/setup-local.ps1 (Windows).");
            return new VoiceOverService(null, null);
        }

        var player = SdlPcmPlayer.TryCreate("TTS");
        if (player is null)
            return new VoiceOverService(null, null);

        try
        {
            var provider = new LocalChatterboxTtsProvider(config);
            EngineLog.Info("Emotional TTS enabled with free local Chatterbox Multilingual.");
            return new VoiceOverService(provider, player);
        }
        catch
        {
            player.Dispose();
            throw;
        }
    }

    public void Speak(VoiceRequest request)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        request.Validate();
        if (!IsEnabled) return;

        CancellationTokenSource cancellation;
        int generation;

        lock (_gate)
        {
            _requestCancellation?.Cancel();
            _requestCancellation?.Dispose();
            _requestCancellation = new CancellationTokenSource();
            cancellation = _requestCancellation;
            generation = ++_generation;
            _player!.Clear();
        }

        _ = GenerateAndPlayAsync(request, generation, cancellation.Token);
    }

    public void Stop()
    {
        if (_disposed) return;

        lock (_gate)
        {
            _generation++;
            _requestCancellation?.Cancel();
            _requestCancellation?.Dispose();
            _requestCancellation = null;
            _player?.Clear();
        }
    }

    private async Task GenerateAndPlayAsync(
        VoiceRequest request,
        int generation,
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = CacheKey(request);
            PcmAudio? audio;
            lock (_gate)
                _memoryCache.TryGetValue(cacheKey, out audio);

            if (audio is null)
            {
                audio = await _provider!.SynthesizeAsync(request, cancellationToken).ConfigureAwait(false);
                lock (_gate)
                    _memoryCache[cacheKey] = audio;
            }

            lock (_gate)
            {
                if (_disposed || generation != _generation || cancellationToken.IsCancellationRequested)
                    return;
                _player!.Play(
                    WavPcmLoader.ApplyGain(
                        audio,
                        Math.Clamp(Gain, 0f, 1f)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            EngineLog.Warn($"TTS '{request.Id}' failed: {exception.Message}");
        }
    }

    private static string CacheKey(VoiceRequest request) =>
        string.Join("|",
            request.Id,
            request.Text,
            request.Direction.Emotion,
            request.Direction.Intensity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            request.Direction.Speed.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            request.Direction.Persona ?? "",
            request.Voice ?? "");

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_gate)
        {
            _generation++;
            _requestCancellation?.Cancel();
            _requestCancellation?.Dispose();
            _requestCancellation = null;
        }

        _player?.Dispose();
        if (_provider is IDisposable disposable)
            disposable.Dispose();
    }
}
