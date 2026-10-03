using Newtonsoft.Json;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Save;

public sealed record SaveSlotEnvelope(
    int EnvelopeVersion,
    string SlotId,
    DateTime SavedAtUtc,
    double PlayTimeSeconds,
    GameSaveSnapshot Snapshot);

public sealed record SaveSlotInfo(
    string SlotId,
    DateTime SavedAtUtc,
    double PlayTimeSeconds,
    string PlayerName,
    double WorldTimeHours,
    int SaveVersion,
    bool IsBackup,
    string Path);

public sealed record SaveLoadResult(
    SaveSlotInfo Slot,
    bool UsedBackup);

public sealed class SaveSlotService
{
    public const int EnvelopeVersion = 1;
    public const int AutosaveSlotCount = 3;

    private readonly string _rootDirectory;

    public SaveSlotService(string? rootDirectory = null)
    {
        _rootDirectory = rootDirectory ?? GetDefaultRootDirectory();
    }

    public IReadOnlyList<SaveSlotInfo> ListAutosaves()
    {
        if (!Directory.Exists(_rootDirectory))
            return [];

        var candidates = new List<SaveSlotInfo>();

        for (var slot = 1; slot <= AutosaveSlotCount; slot++)
        {
            AddIfValid(SlotPath(slot), isBackup: false, candidates);
            AddIfValid(BackupPath(slot), isBackup: true, candidates);
        }

        return candidates
            .OrderByDescending(item => item.SavedAtUtc)
            .ToArray();
    }

    public bool HasLoadableAutosave() => ListAutosaves().Count > 0;

    public SaveSlotInfo SaveAutosave(
        WorldState world,
        double playTimeSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        Directory.CreateDirectory(_rootDirectory);

        var slot = SelectOverwriteSlot();
        var path = SlotPath(slot);
        var backupPath = BackupPath(slot);

        if (File.Exists(path))
        {
            File.Copy(path, backupPath, overwrite: true);
        }

        var envelope = new SaveSlotEnvelope(
            EnvelopeVersion,
            $"autosave-{slot}",
            DateTime.UtcNow,
            Math.Max(0.0, playTimeSeconds),
            SaveGameService.Capture(world));

        var tempPath = path + ".tmp";
        try
        {
            var json = JsonConvert.SerializeObject(
                envelope,
                Formatting.Indented);

            File.WriteAllText(tempPath, json);
            File.Move(tempPath, path, overwrite: true);

            var info = ToInfo(envelope, path, isBackup: false);
            EngineLog.Info(
                $"Autosave '{info.SlotId}' written at {info.SavedAtUtc:O}.");
            return info;
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }

    public bool TryLoadLatest(
        WorldState world,
        out SaveLoadResult? result)
    {
        ArgumentNullException.ThrowIfNull(world);

        foreach (var info in ListAutosaves())
        {
            try
            {
                var envelope = ReadEnvelope(info.Path);
                SaveGameService.Restore(world, envelope.Snapshot);
                result = new SaveLoadResult(
                    ToInfo(envelope, info.Path, info.IsBackup),
                    info.IsBackup);

                EngineLog.Info(
                    $"Loaded '{result.Slot.SlotId}' from " +
                    $"{(result.UsedBackup ? "backup" : "primary")} save.");
                return true;
            }
            catch (Exception exception)
            {
                EngineLog.Warn(
                    $"Skipping invalid save '{info.Path}': {exception.Message}");
            }
        }

        result = null;
        return false;
    }

    private int SelectOverwriteSlot()
    {
        var primarySlots = new List<(int Slot, DateTime SavedAtUtc)>();

        for (var slot = 1; slot <= AutosaveSlotCount; slot++)
        {
            var path = SlotPath(slot);
            if (!File.Exists(path))
                return slot;

            try
            {
                var envelope = ReadEnvelope(path);
                primarySlots.Add((slot, envelope.SavedAtUtc));
            }
            catch
            {
                // Replace an invalid primary first, while its backup remains
                // available to the recovery path.
                return slot;
            }
        }

        return primarySlots
            .OrderBy(item => item.SavedAtUtc)
            .First()
            .Slot;
    }

    private void AddIfValid(
        string path,
        bool isBackup,
        ICollection<SaveSlotInfo> destination)
    {
        if (!File.Exists(path))
            return;

        try
        {
            var envelope = ReadEnvelope(path);
            destination.Add(ToInfo(envelope, path, isBackup));
        }
        catch (Exception exception)
        {
            EngineLog.Warn(
                $"Save metadata read failed for '{path}': {exception.Message}");
        }
    }

    private static SaveSlotEnvelope ReadEnvelope(string path)
    {
        var json = File.ReadAllText(path);
        var envelope = JsonConvert.DeserializeObject<SaveSlotEnvelope>(json)
            ?? throw new InvalidOperationException(
                "Save slot could not be deserialized.");

        if (envelope.EnvelopeVersion != EnvelopeVersion)
        {
            throw new NotSupportedException(
                $"Save envelope version {envelope.EnvelopeVersion} is not supported; " +
                $"expected {EnvelopeVersion}.");
        }

        if (envelope.Snapshot is null)
            throw new InvalidOperationException("Save slot has no game snapshot.");

        if (envelope.Snapshot.Version != SaveGameService.CurrentVersion)
        {
            throw new NotSupportedException(
                $"Save version {envelope.Snapshot.Version} is not supported; " +
                $"expected {SaveGameService.CurrentVersion}.");
        }

        return envelope;
    }

    private static SaveSlotInfo ToInfo(
        SaveSlotEnvelope envelope,
        string path,
        bool isBackup) =>
        new(
            envelope.SlotId,
            envelope.SavedAtUtc,
            envelope.PlayTimeSeconds,
            envelope.Snapshot.PlayerName,
            envelope.Snapshot.TimeOfDayHours,
            envelope.Snapshot.Version,
            isBackup,
            path);

    private string SlotPath(int slot) =>
        System.IO.Path.Combine(
            _rootDirectory,
            $"autosave-{slot}.json");

    private string BackupPath(int slot) =>
        SlotPath(slot) + ".bak";

    private static string GetDefaultRootDirectory()
    {
        var root = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(root))
            root = AppContext.BaseDirectory;

        return System.IO.Path.Combine(
            root,
            "SlavicGame",
            "saves");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best effort cleanup only; never destroy an older save because a
            // temporary file could not be removed.
        }
    }
}
