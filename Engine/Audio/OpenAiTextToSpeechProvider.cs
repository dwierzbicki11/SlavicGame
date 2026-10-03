using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SlavicGame.Engine.Audio;

public sealed record TtsRuntimeConfig(
    bool Enabled,
    string? ApiKey,
    string Model = "gpt-4o-mini-tts",
    string Voice = "cedar")
{
    public static TtsRuntimeConfig FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var explicitToggle = Environment.GetEnvironmentVariable("SLAVICGAME_TTS");
        var enabled = !string.Equals(explicitToggle, "0", StringComparison.OrdinalIgnoreCase) &&
                      !string.IsNullOrWhiteSpace(apiKey);
        var model = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_MODEL");
        var voice = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_VOICE");

        return new TtsRuntimeConfig(
            enabled,
            apiKey,
            string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini-tts" : model.Trim(),
            string.IsNullOrWhiteSpace(voice) ? "cedar" : voice.Trim());
    }
}

public sealed class OpenAiTextToSpeechProvider : ITextToSpeechProvider, IDisposable
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _defaultVoice;
    private bool _disposed;

    public OpenAiTextToSpeechProvider(TtsRuntimeConfig config, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!config.Enabled || string.IsNullOrWhiteSpace(config.ApiKey))
            throw new ArgumentException("TTS configuration is not enabled.", nameof(config));

        _apiKey = config.ApiKey;
        _model = config.Model;
        _defaultVoice = config.Voice;
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    }

    public async Task<PcmAudio> SynthesizeAsync(
        VoiceRequest request,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        request.Validate();

        var body = new
        {
            model = _model,
            input = request.Text,
            voice = string.IsNullOrWhiteSpace(request.Voice) ? _defaultVoice : request.Voice,
            instructions = VoiceDirectionPrompts.Build(request.Direction),
            response_format = "pcm",
            speed = Math.Clamp(request.Direction.Speed, 0.5f, 1.5f)
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        message.Content = JsonContent.Create(body);

        using var response = await _http.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (error.Length > 400) error = error[..400];
            throw new HttpRequestException(
                $"TTS request failed ({(int)response.StatusCode} {response.ReasonPhrase}): {error}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return new PcmAudio(bytes, 24000, 1, 16).Validate();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _http.Dispose();
    }
}
