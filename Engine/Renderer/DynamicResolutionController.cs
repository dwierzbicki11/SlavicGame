using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.Renderer;

public sealed class DynamicResolutionController
{
    public const float MinScale = 0.50f;
    public const float MaxScale = 1.00f;
    public const float StepDown = 0.08f;
    public const float StepUp = 0.04f;
    public const int MinCooldownFrames = 60;

    private readonly float[] _frametimeHistory = new float[30];
    private int _historyIndex;
    private int _historyCount;
    private int _cooldownFrames;
    private float _currentScale = 1.0f;

    public float CurrentScale => _currentScale;
    public bool IsActive => _currentScale < 0.999f;

    public void Reset()
    {
        _currentScale = 1.0f;
        _historyIndex = 0;
        _historyCount = 0;
        _cooldownFrames = MinCooldownFrames;
    }

    public bool Update(
        float deltaSeconds,
        DynamicResolutionMode mode,
        ResolutionSize baseResolution,
        out ResolutionSize newResolution)
    {
        newResolution = baseResolution;
        if (mode == DynamicResolutionMode.Off || deltaSeconds <= 0.0001f)
        {
            if (_currentScale < 0.999f)
            {
                _currentScale = 1.0f;
                return true;
            }
            return false;
        }

        float targetFrametime = mode switch
        {
            DynamicResolutionMode.Target30 => 1f / 30f,
            DynamicResolutionMode.Target60 => 1f / 60f,
            _ => 1f / 60f
        };

        _frametimeHistory[_historyIndex] = deltaSeconds;
        _historyIndex = (_historyIndex + 1) % _frametimeHistory.Length;
        if (_historyCount < _frametimeHistory.Length)
            _historyCount++;

        if (_cooldownFrames > 0)
        {
            _cooldownFrames--;
            return false;
        }

        if (_historyCount < 15)
            return false;

        float total = 0f;
        for (int i = 0; i < _historyCount; i++)
            total += _frametimeHistory[i];
        float avgFrametime = total / _historyCount;

        float dropThreshold = targetFrametime * 1.06f; // Drop if average frametime exceeds target by 6%
        float raiseThreshold = targetFrametime * 0.82f; // Raise only if average frametime has >= 18% headroom

        float previousScale = _currentScale;

        if (avgFrametime > dropThreshold && _currentScale > MinScale)
        {
            _currentScale = MathF.Max(MinScale, _currentScale - StepDown);
            _cooldownFrames = MinCooldownFrames;
        }
        else if (avgFrametime < raiseThreshold && _currentScale < MaxScale)
        {
            _currentScale = MathF.Min(MaxScale, _currentScale + StepUp);
            _cooldownFrames = MinCooldownFrames * 2; // Slower to raise to avoid oscillation
        }

        if (MathF.Abs(_currentScale - previousScale) > 0.015f)
        {
            uint width = (uint)MathF.Round(baseResolution.Width * _currentScale);
            uint height = (uint)MathF.Round(baseResolution.Height * _currentScale);

            // Keep even dimensions for hardware alignment and temporal/FSR upscaling
            width = (width / 2) * 2;
            height = (height / 2) * 2;
            width = Math.Clamp(width, 480u, (uint)baseResolution.Width);
            height = Math.Clamp(height, 270u, (uint)baseResolution.Height);

            newResolution = new ResolutionSize((int)width, (int)height);
            EngineLog.Info($"DRS adjusted render scale to {_currentScale:P0} ({newResolution.Width}x{newResolution.Height}, avg frametime={avgFrametime * 1000f:F1}ms).");
            return true;
        }

        return false;
    }
}
