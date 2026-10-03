using System.Numerics;

namespace SlavicGame.Engine.World;

public enum CinematicSpace
{
    World,
    PlayerRelative
}

public sealed record CinematicShot(
    Vector3 From,
    Vector3 To,
    Vector3 Focus,
    double Seconds,
    string Subtitle,
    CinematicSpace Space = CinematicSpace.World);

public sealed record CinematicDefinition(string Id, IReadOnlyList<CinematicShot> Shots);

public sealed class CinematicPlayer
{
    // Compatibility aliases for the first two scenes. New quest code should use CinematicCatalog.
    public static CinematicDefinition Arrival => CinematicCatalog.Arrival;
    public static CinematicDefinition Shrine => CinematicCatalog.Shrine;

    private CinematicDefinition? _active;
    private int _shot;
    private double _elapsed;
    private Vector3 _playerAnchor;

    public bool IsPlaying => _active is not null;
    public string? ActiveId => _active?.Id;
    public string Subtitle => _active?.Shots[_shot].Subtitle ?? "";
    public Vector3 CameraPosition { get; private set; }
    public Vector3 CameraTarget { get; private set; }

    public bool TryStartById(WorldState world, string cinematicId) =>
        CinematicCatalog.TryGet(cinematicId, out var definition) &&
        TryStart(world, definition);

    public bool TryStart(WorldState world, CinematicDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(definition);

        if (IsPlaying || !world.Player.IsAlive || world.Magic.IsCasting ||
            world.Progress.HasFlag("cinematic.seen." + definition.Id))
            return false;

        if (world.Enemies.Any(e => e.IsAlive && Vector3.Distance(e.Position, world.PlayerPosition) < 25f))
            return false;

        if (definition.Shots.Count == 0 ||
            definition.Shots.Any(s => !double.IsFinite(s.Seconds) || s.Seconds <= 0))
            throw new ArgumentException("Cinematic requires positive-duration shots.", nameof(definition));

        _active = definition;
        _shot = 0;
        _elapsed = 0;
        _playerAnchor = new Vector3(world.PlayerPosition.X, 0, world.PlayerPosition.Z);
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
            if (++_shot == _active.Shots.Count)
            {
                Finish(world);
                return;
            }
        }

        UpdatePose(world.Terrain);
    }

    public void Finish(WorldState world)
    {
        if (_active is not null)
            world.Progress.SetFlag("cinematic.seen." + _active.Id);
        Reset();
    }

    public void Reset()
    {
        _active = null;
        _shot = 0;
        _elapsed = 0;
        _playerAnchor = Vector3.Zero;
    }

    private void UpdatePose(Terrain terrain)
    {
        if (_active is null) return;

        var shot = _active.Shots[_shot];
        var t = (float)(_elapsed / shot.Seconds);
        t = t * t * (3 - 2 * t);

        CameraPosition = AboveGround(Resolve(Vector3.Lerp(shot.From, shot.To, t), shot.Space));
        CameraTarget = AboveGround(Resolve(shot.Focus, shot.Space));

        Vector3 Resolve(Vector3 point, CinematicSpace space) =>
            space == CinematicSpace.PlayerRelative
                ? point + _playerAnchor
                : point;

        Vector3 AboveGround(Vector3 point) =>
            new(point.X, terrain.SampleHeight(point) + point.Y, point.Z);
    }
}
