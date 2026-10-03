using System.Runtime.InteropServices;
using Veldrid.Sdl2;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Audio;

public sealed class SdlPcmPlayer : IDisposable
{
    private const uint SdlInitAudio = 0x00000010;
    private const ushort AudioS16Lsb = 0x8010;

    [StructLayout(LayoutKind.Sequential)]
    private struct SdlAudioSpec
    {
        public int Freq;
        public ushort Format;
        public byte Channels;
        public byte Silence;
        public ushort Samples;
        public ushort Padding;
        public uint Size;
        public IntPtr Callback;
        public IntPtr Userdata;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int InitSubSystemDelegate(uint flags);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void QuitSubSystemDelegate(uint flags);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint OpenAudioDeviceDelegate(
        IntPtr device,
        int isCapture,
        ref SdlAudioSpec desired,
        out SdlAudioSpec obtained,
        int allowedChanges);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void PauseAudioDeviceDelegate(uint device, int pauseOn);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int QueueAudioDelegate(uint device, IntPtr data, uint length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ClearQueuedAudioDelegate(uint device);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void CloseAudioDeviceDelegate(uint device);

    private static readonly InitSubSystemDelegate? InitSubSystem =
        Sdl2Native.LoadFunction<InitSubSystemDelegate>("SDL_InitSubSystem");
    private static readonly QuitSubSystemDelegate? QuitSubSystem =
        Sdl2Native.LoadFunction<QuitSubSystemDelegate>("SDL_QuitSubSystem");
    private static readonly OpenAudioDeviceDelegate? OpenAudioDevice =
        Sdl2Native.LoadFunction<OpenAudioDeviceDelegate>("SDL_OpenAudioDevice");
    private static readonly PauseAudioDeviceDelegate? PauseAudioDevice =
        Sdl2Native.LoadFunction<PauseAudioDeviceDelegate>("SDL_PauseAudioDevice");
    private static readonly QueueAudioDelegate? QueueAudio =
        Sdl2Native.LoadFunction<QueueAudioDelegate>("SDL_QueueAudio");
    private static readonly ClearQueuedAudioDelegate? ClearQueuedAudio =
        Sdl2Native.LoadFunction<ClearQueuedAudioDelegate>("SDL_ClearQueuedAudio");
    private static readonly CloseAudioDeviceDelegate? CloseAudioDevice =
        Sdl2Native.LoadFunction<CloseAudioDeviceDelegate>("SDL_CloseAudioDevice");

    private uint _device;
    private bool _audioSubsystemInitialized;
    private bool _disposed;

    private SdlPcmPlayer(uint device)
    {
        _device = device;
        _audioSubsystemInitialized = true;
    }

    public static SdlPcmPlayer? TryCreate()
    {
        if (InitSubSystem is null || QuitSubSystem is null || OpenAudioDevice is null ||
            PauseAudioDevice is null || QueueAudio is null || ClearQueuedAudio is null ||
            CloseAudioDevice is null)
        {
            EngineLog.Warn("TTS audio disabled: required SDL2 audio functions are unavailable.");
            return null;
        }

        if (InitSubSystem(SdlInitAudio) != 0)
        {
            EngineLog.Warn("TTS audio disabled: SDL audio subsystem could not initialize.");
            return null;
        }

        var desired = new SdlAudioSpec
        {
            Freq = 24000,
            Format = AudioS16Lsb,
            Channels = 1,
            Samples = 1024,
            Callback = IntPtr.Zero,
            Userdata = IntPtr.Zero
        };

        var device = OpenAudioDevice(IntPtr.Zero, 0, ref desired, out var obtained, 0);
        if (device == 0)
        {
            QuitSubSystem(SdlInitAudio);
            EngineLog.Warn("TTS audio disabled: SDL could not open the default playback device.");
            return null;
        }

        if (obtained.Freq != 24000 || obtained.Format != AudioS16Lsb || obtained.Channels != 1)
        {
            CloseAudioDevice(device);
            QuitSubSystem(SdlInitAudio);
            EngineLog.Warn(
                $"TTS audio disabled: unexpected SDL format {obtained.Freq} Hz / 0x{obtained.Format:X} / {obtained.Channels} ch.");
            return null;
        }

        PauseAudioDevice(device, 0);
        EngineLog.Info("TTS SDL playback ready: PCM16 mono 24 kHz.");
        return new SdlPcmPlayer(device);
    }

    public void Play(PcmAudio audio)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        audio.Validate();
        if (audio.SampleRate != 24000 || audio.Channels != 1 || audio.BitsPerSample != 16)
            throw new NotSupportedException("SDL TTS player expects PCM16 mono at 24 kHz.");

        Clear();
        var handle = GCHandle.Alloc(audio.Data, GCHandleType.Pinned);
        try
        {
            var result = QueueAudio!(_device, handle.AddrOfPinnedObject(), (uint)audio.Data.Length);
            if (result != 0)
                throw new InvalidOperationException($"SDL_QueueAudio returned {result}.");
        }
        finally
        {
            handle.Free();
        }
    }

    public void Clear()
    {
        if (_disposed || _device == 0) return;
        ClearQueuedAudio!(_device);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_device != 0)
        {
            ClearQueuedAudio!(_device);
            CloseAudioDevice!(_device);
            _device = 0;
        }

        if (_audioSubsystemInitialized)
        {
            QuitSubSystem!(SdlInitAudio);
            _audioSubsystemInitialized = false;
        }
    }
}
