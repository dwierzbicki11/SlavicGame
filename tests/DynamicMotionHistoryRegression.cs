using System.Numerics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

internal static class DynamicMotionHistoryRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var history = new DynamicMotionHistory();
        var firstFrame = new[]
        {
            new TerrainVertex(new Vector3(1f, 2f, 3f), Vector3.One),
            new TerrainVertex(new Vector3(4f, 5f, 6f), Vector3.One)
        };

        var first = history.Build(firstFrame, resetHistory: false);
        check(first.Length == 2 &&
              first.All(v => v.CurrentPosition == v.PreviousPosition),
            "Dynamic motion starts with zero object velocity");

        var secondFrame = new[]
        {
            new TerrainVertex(new Vector3(1.5f, 2f, 3f), Vector3.One),
            new TerrainVertex(new Vector3(4f, 5.25f, 6f), Vector3.One)
        };
        var second = history.Build(secondFrame, resetHistory: false);
        check(second[0].PreviousPosition == firstFrame[0].Position &&
              second[0].CurrentPosition == secondFrame[0].Position &&
              second[1].PreviousPosition == firstFrame[1].Position &&
              second[1].CurrentPosition == secondFrame[1].Position,
            "Dynamic motion preserves the exact prior actor vertex positions");

        var reusableHistory = new DynamicMotionHistory();
        var reusable = new DynamicMotionVertex[2];
        reusableHistory.BuildInto(firstFrame, resetHistory: false, reusable);
        reusableHistory.BuildInto(secondFrame, resetHistory: false, reusable);
        check(reusable[0].PreviousPosition == firstFrame[0].Position &&
              reusable[1].PreviousPosition == firstFrame[1].Position,
            "Dynamic motion supports caller-owned storage without changing temporal history");

        history.Reset();
        var afterReset = history.Build(secondFrame, resetHistory: false);
        check(afterReset.All(v => v.CurrentPosition == v.PreviousPosition),
            "Dynamic motion history reset prevents stale velocity after cuts/resizes");

        var topologyChange = history.Build(
            secondFrame.AsSpan(0, 1),
            resetHistory: false);
        check(topologyChange.Length == 1 &&
              topologyChange[0].CurrentPosition ==
              topologyChange[0].PreviousPosition,
            "Dynamic motion rejects mismatched topology instead of pairing unrelated vertices");

        check(DynamicMotionVertex.SizeInBytes == 24,
            "Dynamic motion vertex layout remains two float3 positions");
    }
}
