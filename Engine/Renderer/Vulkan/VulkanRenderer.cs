using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Renderer.Vulkan;

public sealed class VulkanRenderer : IDisposable
{
    private readonly VulkanContext _context = new();

    public VulkanContext Context => _context;

    public void Initialize()
    {
        EngineLog.Info("Initializing Vulkan renderer.");
        _context.Initialize();
    }

    public void Dispose() => _context.Dispose();
}
