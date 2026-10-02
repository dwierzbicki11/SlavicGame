using SlavicGame.Engine.Renderer;

internal static class PresentationPolicyRegression
{
    public static void Run(Action<bool, string> check)
    {
        if (!OperatingSystem.IsLinux())
        {
            check(true, "Linux immediate-present policy skipped on non-Linux runner");
            return;
        }

        PresentationPolicy.Apply(vsync: false);
        check(
            Environment.GetEnvironmentVariable(PresentationPolicy.MesaPresentModeVariable) == "immediate",
            "VSync OFF forces Mesa Vulkan immediate present mode");

        PresentationPolicy.Apply(vsync: true);
        check(
            Environment.GetEnvironmentVariable(PresentationPolicy.MesaPresentModeVariable) == "fifo",
            "VSync ON forces Mesa Vulkan FIFO present mode");

        PresentationPolicy.Apply(vsync: false);
    }
}
