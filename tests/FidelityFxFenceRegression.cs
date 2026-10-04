using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Renderer.FidelityFx;

internal static class FidelityFxFenceRegression
{
    // Inject Vulkan delegates into the real command ring without loading a GPU
    // driver. An unsignaled wait fails immediately instead of hanging the test.
    public static void Run(Action<bool, string> check)
    {
        var ring = (FidelityFxVulkanCommands)RuntimeHelpers.GetUninitializedObject(
            typeof(FidelityFxVulkanCommands));
        var waits = 0;
        var resets = 0;
        var submitted = false;
        var failSubmit = false;
        var failEnd = false;
        Set(ring, "_commandBuffers", new nint[] { 1, 2, 3 });
        Set(ring, "_fences", new ulong[] { 11, 12, 13 });
        Set(ring, "_pending", new bool[3]);
        Hook(ring, "_waitForFences", () =>
        {
            if (!submitted) throw new Exception("Wait on a fence without submitted work");
            waits++;
            return 0;
        });
        Hook(ring, "_resetFences", () => { resets++; return 0; });
        Hook(ring, "_resetCommandBuffer", () => 0);
        Hook(ring, "_beginCommandBuffer", () => 0);
        Hook(ring, "_endCommandBuffer", () => failEnd ? -4 : 0);
        Hook(ring, "_queueSubmit", () =>
        {
            if (failSubmit) return -4;
            submitted = true;
            return 0;
        });
        Hook(ring, "_destroyFence", () => 0);

        var buffer = ring.Begin();
        ring.WaitAll();
        check(waits == 0 && resets == 0,
            "Abandoned recording does not reset or wait for an unsubmitted fence");

        failEnd = true;
        ExpectFailure(() => ring.EndAndSubmit(buffer));
        ring.WaitAll();
        check(waits == 0 && resets == 0,
            "Failed command recording leaves no pending fence");

        failEnd = false;
        failSubmit = true;
        buffer = ring.Begin();
        ExpectFailure(() => ring.EndAndSubmit(buffer));
        ring.WaitAll();
        check(waits == 0 && resets == 1,
            "Failed queue submission never waits on its reset fence");

        failSubmit = false;
        buffer = ring.Begin();
        ring.EndAndSubmit(buffer);
        ring.WaitAll();
        ring.WaitAll();
        check(waits == 1 && resets == 2,
            "Successful GPU submission is waited exactly once by WaitAll");

        ring.Begin(); // Leave another slot recording, then exercise cleanup.
        submitted = false;
        ring.Dispose();
        check(waits == 1, "Cleanup ignores abandoned and already completed slots");
    }

    private static void ExpectFailure(Action action)
    {
        try { action(); }
        catch (InvalidOperationException) { return; }
        throw new Exception("Injected Vulkan failure was not reported");
    }

    private static FieldInfo Field(string name) =>
        typeof(FidelityFxVulkanCommands).GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static void Set(object target, string name, object value) =>
        Field(name).SetValue(target, value);

    private static void Hook(object target, string name, Func<int> callback)
    {
        var field = Field(name);
        var invoke = field.FieldType.GetMethod("Invoke")!;
        var parameters = invoke.GetParameters()
            .Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        Expression body = Expression.Invoke(Expression.Constant(callback));
        if (invoke.ReturnType == typeof(void))
            body = Expression.Block(body, Expression.Empty());
        field.SetValue(target, Expression.Lambda(field.FieldType, body, parameters).Compile());
    }
}
