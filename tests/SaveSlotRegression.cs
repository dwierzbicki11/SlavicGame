using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class SaveSlotRegression
{
    [ModuleInitializer]
    internal static void Initialize() => Run();

    private static void Run()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "SlavicGame-SaveSlotRegression-" + Guid.NewGuid().ToString("N"));

        try
        {
            var service = new SaveSlotService(root);
            var world = WorldGenerator.Generate();

            var originalPosition = new Vector3(11f, 2f, -7f);
            world.SetPlayerPosition(originalPosition);
            var first = service.SaveAutosave(world, 60.0);

            world.SetPlayerPosition(new Vector3(21f, 2f, -17f));
            service.SaveAutosave(world, 120.0);

            world.SetPlayerPosition(new Vector3(31f, 2f, -27f));
            service.SaveAutosave(world, 180.0);

            world.SetPlayerPosition(new Vector3(41f, 2f, -37f));
            service.SaveAutosave(world, 240.0);

            var listed = service.ListAutosaves();
            var primary = listed
                .Where(item => !item.IsBackup)
                .ToArray();

            Check(primary.Length == SaveSlotService.AutosaveSlotCount,
                "autosave rotation keeps exactly three primary slots");
            Check(listed.Any(item => item.IsBackup),
                "overwriting an autosave preserves a backup");

            foreach (var item in primary)
                File.WriteAllText(item.Path, "{ definitely-not-valid-json");

            world.SetPlayerPosition(Vector3.Zero);
            Check(service.TryLoadLatest(world, out var loaded),
                "loader falls back when all primary autosaves are corrupt");
            Check(loaded is not null && loaded.UsedBackup,
                "loader explicitly reports backup recovery");
            Check(Vector3.Distance(world.PlayerPosition, originalPosition) < 0.001f,
                "backup recovery restores the previous valid world snapshot");

            // Invalid files are preserved for diagnostics/recovery and are not
            // silently deleted by metadata discovery.
            Check(primary.All(item => File.Exists(item.Path)),
                "corrupt save files are preserved instead of deleted");
            Check(File.Exists(first.Path + ".bak"),
                "previous primary remains available as backup");
        }
        finally
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Temp cleanup should not make the regression itself flaky.
            }
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
