using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;

internal static class EnemyDebugViewRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var enemy = new EnemyAgent("debug-predator", new Vector3(1.25f, 2f, -3.5f));
        var description = EnemyDebugView.Describe(enemy);

        Require(description.Contains("debug-predator state=Patrol hp=60.0/60.0", StringComparison.Ordinal),
            "Debug snapshot should identify the actor, AI state and health.");
        Require(description.Contains("attack=Ready hitReact=False", StringComparison.Ordinal),
            "Debug snapshot should expose attack and hit-reaction state.");
        Require(description.Contains("pos=(1.25,2.00,-3.50)", StringComparison.Ordinal),
            "Debug snapshot should expose a stable invariant position.");

        enemy.TakeDamage(20f);
        description = EnemyDebugView.Describe(enemy);
        Require(description.Contains("state=Alert hp=40.0/60.0", StringComparison.Ordinal) &&
                description.Contains("hitReact=True", StringComparison.Ordinal),
            "Debug snapshot should immediately reflect combat state transitions.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
