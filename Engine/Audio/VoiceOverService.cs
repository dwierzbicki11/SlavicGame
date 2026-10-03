using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Audio;

public sealed class VoiceOverService : IDisposable
{
    private readonly ITextToSpeechProvider? _provider;
    private readonly SdlPcmPlayer? _player;
    private readonly object _gate = new();
    private CancellationTokenSource? _requestCancellation;
    private int _generation;
    private bool _disposed;

    public bool IsEnabled => _provider is not null && _player is not null;

    private VoiceOverService(ITextToSpeechProvider? provider, SdlPcmPlayer? player)
    {
        _provider = provider;
        _player = player;
    }

    public static VoiceOverService CreateFromEnvironment()
    {
        var config = TtsRuntimeConfig.FromEnvironment();
        if (!config.Enabled)
        {
            EngineLog.Info("Emotional TTS disabled: set OPENAI_API_KEY to enable.");
            return new VoiceOverService(null, null);
        }

        var player = SdlPcmPlayer.TryCreate();
        if (player is null)
            return new VoiceOverService(null, null);

        try
        {
            var provider = new OpenAiTextToSpeechProvider(config);
            EngineLog.Info($"Emotional TTS enabled: model={config.Model}, voice={config.Voice}.");
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
            var audio = await _provider!.SynthesizeAsync(request, cancellationToken).ConfigureAwait(false);
            lock (_gate)
            {
                if (_disposed || generation != _generation || cancellationToken.IsCancellationRequested)
                    return;
                _player!.Play(audio);
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
