using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record CinematicShot(Vector3 From, Vector3 To, Vector3 Focus, double Seconds, string Subtitle);
public sealed record CinematicDefinition(string Id, IReadOnlyList<CinematicShot> Shots);

public sealed class CinematicPlayer
{
    public static CinematicDefinition Arrival { get; } = new("arrival", Array.AsReadOnly(new[]
    {
        new CinematicShot(new(-22, 9, -20), new(-8, 5, -12), new(0, 2, 0), 4, "POGRANICZE ZARNOWCA. LAS PAMIETA KAZDY KROK."),
        new CinematicShot(new(12, 5, -8), new(5, 3, -4), new(0, 1.5f, 0), 4, "ZAR VEK. ZIVA DAR. VEDA NAW. SLOWA MAJA CENE.")
    }));
    public static CinematicDefinition Shrine { get; } = new("shrine", Array.AsReadOnly(new[]
    {
        new CinematicShot(new(-105, 9, 38), new(-99, 6, 46), new(-85, 2, 55), 4, "KAMIENNY KRAG. GRANICA JEST TUTAJ CIENSZA."),
        new CinematicShot(new(-72, 6, 68), new(-75, 4, 64), new(-85, 2, 55), 4, "VEDA NAW - ODSLON TO, CO POZOSTALO.")
    }));
    private CinematicDefinition? _active;
    private int _shot;
    private double _elapsed;
    public bool IsPlaying => _active is not null;
    public string Subtitle => _active?.Shots[_shot].Subtitle ?? "";
    public Vector3 CameraPosition { get; private set; }
    public Vector3 CameraTarget { get; private set; }

    public bool TryStart(WorldState world, CinematicDefinition definition)
    {
        if (IsPlaying || !world.Player.IsAlive || world.Magic.IsCasting || world.Progress.HasFlag("cinematic.seen." + definition.Id)) return false;
        if (world.Enemies.Any(e => e.IsAlive && Vector3.Distance(e.Position, world.PlayerPosition) < 25f)) return false;
        if (definition.Shots.Count == 0 || definition.Shots.Any(s => !double.IsFinite(s.Seconds) || s.Seconds <= 0))
            throw new ArgumentException("Cinematic requires positive-duration shots.", nameof(definition));
        _active = definition; _shot = 0; _elapsed = 0;
        UpdatePose(world.Terrain);
        return true;
    }
    public void Update(WorldState world, double dt)
    {
        if (!IsPlaying || !double.IsFinite(dt) || dt < 0) return;
        _elapsed += dt;
        while (_active is not null && _elapsed >= _active.Shots[_shot].Seconds)
        {
            _elapsed -= _active.Shots[_shot].Seconds;
            if (++_shot == _active.Shots.Count) { Finish(world); return; }
        }
        UpdatePose(world.Terrain);
    }
    public void Finish(WorldState world)
    {
        if (_active is not null) world.Progress.SetFlag("cinematic.seen." + _active.Id);
        Reset();
    }
    public void Reset() { _active = null; _shot = 0; _elapsed = 0; }
    private void UpdatePose(Terrain terrain)
    {
        if (_active is null) return;
        var shot = _active.Shots[_shot];
        var t = (float)(_elapsed / shot.Seconds); t = t * t * (3 - 2 * t);
        CameraPosition = AboveGround(Vector3.Lerp(shot.From, shot.To, t));
        CameraTarget = AboveGround(shot.Focus);
        Vector3 AboveGround(Vector3 p) => new(p.X, terrain.SampleHeight(p) + p.Y, p.Z);
    }
}
