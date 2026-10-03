using System.Diagnostics;
using System.Text.Json;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Audio;

public sealed record LocalTtsRuntimeConfig(
    bool Enabled,
    string PythonExecutable,
    string ScriptPath,
    string Device,
    string ReferenceDirectory,
    string CacheDirectory)
{
    public static LocalTtsRuntimeConfig FromEnvironment()
    {
        var explicitToggle = Environment.GetEnvironmentVariable("SLAVICGAME_TTS");
        if (string.Equals(explicitToggle, "0", StringComparison.OrdinalIgnoreCase))
            return Disabled();

        var root = FindProjectRoot();
        var script = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_SCRIPT");
        if (string.IsNullOrWhiteSpace(script))
            script = Path.Combine(root, "tools", "tts", "chatterbox_server.py");

        var python = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_PYTHON");
        if (string.IsNullOrWhiteSpace(python))
        {
            var unixVenv = Path.Combine(root, ".venv-tts", "bin", "python");
            var windowsVenv = Path.Combine(root, ".venv-tts", "Scripts", "python.exe");
            python = File.Exists(unixVenv)
                ? unixVenv
                : File.Exists(windowsVenv)
                    ? windowsVenv
                    : "";
        }

        var enabled =
            !string.IsNullOrWhiteSpace(python) &&
            File.Exists(python) &&
            File.Exists(script);

        if (string.Equals(explicitToggle, "1", StringComparison.OrdinalIgnoreCase) &&
            !enabled)
        {
            EngineLog.Warn(
                "Local TTS was explicitly enabled but its Python environment is missing. " +
                "Run tools/tts/setup-local.sh first.");
        }

        var referenceDirectory = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_REFERENCE_DIR");
        if (string.IsNullOrWhiteSpace(referenceDirectory))
            referenceDirectory = Path.Combine(root, "assets", "voice", "reference");

        var cacheDirectory = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_CACHE_DIR");
        if (string.IsNullOrWhiteSpace(cacheDirectory))
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(local))
                local = Path.GetTempPath();
            cacheDirectory = Path.Combine(local, "SlavicGame", "tts-cache");
        }

        var device = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_DEVICE");
        if (string.IsNullOrWhiteSpace(device))
            device = "auto";

        return new LocalTtsRuntimeConfig(
            enabled,
            python,
            script,
            device,
            referenceDirectory,
            cacheDirectory);
    }

    private static LocalTtsRuntimeConfig Disabled() =>
        new(false, "", "", "auto", "", "");

    private static string FindProjectRoot()
    {
        var candidates = new[]
        {
            Environment.CurrentDirectory,
            AppContext.BaseDirectory,
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."))
        };

        foreach (var candidate in candidates)
        {
            var full = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(full, "SlavicGame.csproj")))
                return full;
            if (File.Exists(Path.Combine(full, "tools", "tts", "chatterbox_server.py")))
                return full;
        }

        return Environment.CurrentDirectory;
    }
}

public sealed record LocalVoiceStyle(
    string EmotionKey,
    float Exaggeration,
    float CfgWeight);

public static class ChatterboxProsody
{
    public static LocalVoiceStyle FromDirection(VoiceDirection direction)
    {
        direction.Validate();

        var exaggeration = direction.Emotion switch
        {
            VoiceEmotion.Whisper => 0.28f + direction.Intensity * 0.22f,
            VoiceEmotion.Calm => 0.34f + direction.Intensity * 0.22f,
            VoiceEmotion.Warm => 0.42f + direction.Intensity * 0.30f,
            VoiceEmotion.Sad => 0.40f + direction.Intensity * 0.36f,
            VoiceEmotion.Solemn => 0.45f + direction.Intensity * 0.36f,
            VoiceEmotion.Mystical => 0.48f + direction.Intensity * 0.42f,
            VoiceEmotion.Uneasy => 0.50f + direction.Intensity * 0.42f,
            VoiceEmotion.Fearful => 0.55f + direction.Intensity * 0.42f,
            VoiceEmotion.Angry => 0.58f + direction.Intensity * 0.42f,
            VoiceEmotion.Urgent => 0.60f + direction.Intensity * 0.40f,
            _ => 0.40f + direction.Intensity * 0.28f
        };

        exaggeration = Math.Clamp(exaggeration, 0.25f, 1.0f);
        var cfgWeight = exaggeration >= 0.72f ? 0.30f : exaggeration >= 0.58f ? 0.38f : 0.50f;

        return new LocalVoiceStyle(
            direction.Emotion.ToString().ToLowerInvariant(),
            exaggeration,
            cfgWeight);
    }
}

public sealed class LocalChatterboxTtsProvider : ITextToSpeechProvider, IDisposable
{
    private readonly LocalTtsRuntimeConfig _config;
    private readonly SemaphoreSlim _startLock = new(1, 1);
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private Process? _process;
    private StreamWriter? _stdin;
    private StreamReader? _stdout;
    private Task? _stderrPump;
    private bool _disposed;

    public LocalChatterboxTtsProvider(LocalTtsRuntimeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!config.Enabled)
            throw new ArgumentException("Local TTS configuration is not enabled.", nameof(config));
        _config = config;
    }

    public async Task<PcmAudio> SynthesizeAsync(
        VoiceRequest request,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        request.Validate();
        await EnsureStartedAsync(cancellationToken).ConfigureAwait(false);

        await _requestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var style = ChatterboxProsody.FromDirection(request.Direction);
            var message = JsonSerializer.Serialize(new
            {
                id = request.Id,
                text = request.Text,
                voice = request.Voice,
                emotion = style.EmotionKey,
                exaggeration = style.Exaggeration,
                cfg_weight = style.CfgWeight,
                speed = request.Direction.Speed
            });

            cancellationToken.ThrowIfCancellationRequested();

            // Once a request is written, always drain exactly one response before
            // releasing the request lock. Chatterbox inference itself is not
            // safely cancellable through this line protocol.
            await _stdin!.WriteLineAsync(message).ConfigureAwait(false);
            await _stdin.FlushAsync().ConfigureAwait(false);

            var line = await _stdout!.ReadLineAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(line))
                throw new InvalidOperationException("Local TTS process closed without a response.");

            using var json = JsonDocument.Parse(line);
            var root = json.RootElement;
            if (!root.GetProperty("ok").GetBoolean())
            {
                var error = root.TryGetProperty("error", out var errorNode)
                    ? errorNode.GetString()
                    : "unknown local TTS error";
                throw new InvalidOperationException(error);
            }

            var path = root.GetProperty("path").GetString()
                ?? throw new InvalidOperationException("Local TTS response has no output path.");
            var sampleRate = root.GetProperty("sample_rate").GetInt32();
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            return new PcmAudio(bytes, sampleRate, 1, 16).Validate();
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private async Task EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (_process is { HasExited: false })
            return;

        await _startLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_process is { HasExited: false })
                return;

            Directory.CreateDirectory(_config.CacheDirectory);
            var startInfo = new ProcessStartInfo
            {
                FileName = _config.PythonExecutable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(_config.ScriptPath);
            startInfo.ArgumentList.Add("--device");
            startInfo.ArgumentList.Add(_config.Device);
            startInfo.ArgumentList.Add("--reference-dir");
            startInfo.ArgumentList.Add(_config.ReferenceDirectory);
            startInfo.ArgumentList.Add("--cache-dir");
            startInfo.ArgumentList.Add(_config.CacheDirectory);

            _process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start local Chatterbox TTS.");
            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;
            _stderrPump = PumpStderrAsync(_process.StandardError);

            var readyLine = await _stdout.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(readyLine))
                throw new InvalidOperationException("Local Chatterbox TTS failed during startup.");

            using var ready = JsonDocument.Parse(readyLine);
            if (!ready.RootElement.TryGetProperty("ready", out var readyNode) || !readyNode.GetBoolean())
            {
                var error = ready.RootElement.TryGetProperty("error", out var errorNode)
                    ? errorNode.GetString()
                    : "unknown startup error";
                throw new InvalidOperationException($"Local Chatterbox TTS failed: {error}");
            }

            EngineLog.Info(
                $"Free local TTS ready: Chatterbox Multilingual on {ready.RootElement.GetProperty("device").GetString()}.");
        }
        finally
        {
            _startLock.Release();
        }
    }

    private static async Task PumpStderrAsync(StreamReader reader)
    {
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                EngineLog.Info($"TTS local: {line}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_process is { HasExited: false })
            {
                try { _stdin?.WriteLine("{\"shutdown\":true}"); _stdin?.Flush(); }
                catch { }
                if (!_process.WaitForExit(1000))
                    _process.Kill(entireProcessTree: true);
            }
        }
        finally
        {
            _stdin?.Dispose();
            _stdout?.Dispose();
            _process?.Dispose();
            _startLock.Dispose();
            _requestLock.Dispose();
        }
    }
}
