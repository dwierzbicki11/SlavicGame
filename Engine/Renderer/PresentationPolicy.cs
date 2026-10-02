using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Renderer;

public static class PresentationPolicy
{
    public const string MesaPresentModeVariable = "MESA_VK_WSI_PRESENT_MODE";

    public static void Apply(bool vsync)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var requestedMode = vsync ? "fifo" : "immediate";
        Environment.SetEnvironmentVariable(MesaPresentModeVariable, requestedMode);

        EngineLog.Info(
            $"Linux Vulkan presentation policy: {MesaPresentModeVariable}={requestedMode} " +
            $"(VSync requested={vsync}).");
    }
}
