using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Magic;

public enum SpellEffect { Spark, Mend, Reveal }

public sealed record CastSpell(
    string Id,
    string Name,
    IncantationPhrase Phrase,
    SpellEffect Effect,
    float Cost,
    double Cooldown,
    Vector3 Color)
{
    public string Incantation => Phrase.Text;
}

public sealed record MagicSnapshot(int Selected, double Cooldown);

public sealed class SpellCasting
{
    public static IReadOnlyList<CastSpell> Spells { get; } = Array.AsReadOnly(new[]
    {
        new CastSpell("spell.spark", "ISKRA", MagicLanguage.Spark, SpellEffect.Spark,
            24f, 2.0, new Vector3(1f, 0.38f, 0.06f)),
        new CastSpell("spell.mend", "SZEPT ZYCIA", MagicLanguage.Mend, SpellEffect.Mend,
            35f, 8.0, new Vector3(0.2f, 0.9f, 0.4f)),
        new CastSpell("spell.reveal-trace", "ODSLONIECIE SLADU", MagicLanguage.RevealTrace, SpellEffect.Reveal,
            18f, 5.0, new Vector3(0.25f, 0.7f, 1f))
    });

    public int Selected { get; private set; }
    public CastSpell Current => Spells[Selected];
    public double Cooldown { get; private set; }
    public double CastingRemaining { get; private set; }
    public double RevealRemaining { get; private set; }
    public double FlashRemaining { get; private set; }
    public Vector3 FlashPosition { get; private set; }
    public Vector3 FlashColor { get; private set; }
    public string Message { get; private set; } = "Q ZMIANA CZARU / F RZUC / C SCENKA";
    public bool IsCasting => CastingRemaining > 0;
    public double CastingProgress =>
        !IsCasting || _castingDuration <= 0
            ? 0
            : Math.Clamp(1.0 - CastingRemaining / _castingDuration, 0.0, 1.0);

    private Vector3 _direction;
    private float _healthAtStart;
    private double _castingDuration;

    public void SelectNext(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (IsCasting) return;

        for (var offset = 1; offset <= Spells.Count; offset++)
        {
            var candidate = (Selected + offset) % Spells.Count;
            if (SpellLessons.IsLearned(world, Spells[candidate].Id))
            {
                Selected = candidate;
                Message = $"WYBRANO: {Current.Name}";
                return;
            }
        }

        Message = "NIE ZNASZ JESZCZE ZADNEGO CZARU / L NAUKA";
    }

    public bool SelectSpell(string spellId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spellId);
        var index = Spells.ToList().FindIndex(spell => string.Equals(spell.Id, spellId, StringComparison.Ordinal));
        if (index < 0) return false;
        Selected = index;
        return true;
    }

    public void NormalizeSelection(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (SpellLessons.IsLearned(world, Current.Id)) return;

        for (var i = 0; i < Spells.Count; i++)
            if (SpellLessons.IsLearned(world, Spells[i].Id))
            {
                Selected = i;
                return;
            }

        Selected = 0;
    }

    public bool TryStart(WorldState world, Vector3 direction)
    {
        if (!world.Player.IsAlive || world.Dodge.IsActive || world.Cinematics.IsPlaying || world.Rituals.IsPerforming || IsCasting) return false;
        if (!SpellLessons.IsLearned(world, Current.Id))
        {
            Message = $"NIE ZNASZ: {Current.Name} / L NAUKA";
            return false;
        }
        if (Cooldown > 0) { Message = "CZAR SIE ODNAWIA"; return false; }
        if (!float.IsFinite(direction.X) || !float.IsFinite(direction.Y) || !float.IsFinite(direction.Z) ||
            direction.LengthSquared() < 0.001f) return false;
        if (Current.Effect == SpellEffect.Mend && world.Player.Health >= world.Player.MaxHealth)
        { Message = "ZDROWIE JEST PELNE"; return false; }
        if (!world.Player.TrySpendStamina(Current.Cost)) { Message = "BRAK STAMINY"; return false; }

        _direction = Vector3.Normalize(direction);
        _healthAtStart = world.Player.Health;
        _castingDuration = Current.Phrase.DurationSeconds;
        CastingRemaining = _castingDuration;
        Cooldown = Current.Cooldown + CastingRemaining;
        Message = Current.Phrase.CueAt(0);
        return true;
    }

    public void Update(WorldState world, double dt)
    {
        if (!double.IsFinite(dt) || dt < 0) return;
        Cooldown = Math.Max(0, Cooldown - dt);
        RevealRemaining = Math.Max(0, RevealRemaining - dt);
        FlashRemaining = Math.Max(0, FlashRemaining - dt);
        if (!IsCasting) return;

        if (!world.Player.IsAlive || world.Player.Health < _healthAtStart)
        {
            Cancel();
            Message = "INKANTACJA PRZERWANA";
            return;
        }

        CastingRemaining = Math.Max(0, CastingRemaining - dt);
        var elapsed = Math.Max(0, _castingDuration - CastingRemaining);
        Message = Current.Phrase.CueAt(elapsed);
        if (IsCasting) return;

        var origin = world.PlayerPosition + Vector3.UnitY * 1.1f;
        FlashColor = Current.Color;
        FlashRemaining = 0.6;
        FlashPosition = origin;

        switch (Current.Effect)
        {
            case SpellEffect.Mend:
                world.Player.Heal(25f);
                Message = $"{Current.Incantation} - ODZYSKANO 25 ZDROWIA";
                break;

            case SpellEffect.Reveal:
                RevealRemaining = 12;
                Message = $"{Current.Incantation} - SZUKAJ BLEKITNYCH SLADOW";
                break;

            case SpellEffect.Spark:
                var target = world.Enemies.Where(e => e.IsAlive)
                    .Select(e => (Enemy: e, Point: e.Position + Vector3.UnitY * 0.85f))
                    .Where(e => Vector3.DistanceSquared(origin, e.Point) <= 18f * 18f &&
                        Vector3.DistanceSquared(origin, e.Point) > 0.001f &&
                        Vector3.Dot(Vector3.Normalize(e.Point - origin), _direction) >= 0.90f &&
                        HasLineOfSight(world, origin, e.Point))
                    .OrderBy(e => Vector3.DistanceSquared(origin, e.Point))
                    .FirstOrDefault();

                if (target.Enemy is not null)
                {
                    target.Enemy.TakeDamage(22f);
                    FlashPosition = target.Point;
                    Message = $"{Current.Incantation} - TRAFIENIE";
                }
                else
                {
                    FlashPosition = origin + _direction * 3f;
                    Message = $"{Current.Incantation} - BRAK CELU W ZASIEGU";
                }
                break;
        }
    }

    public static bool HasLineOfSight(WorldState world, Vector3 from, Vector3 to)
    {
        if (world.Terrain.IntersectGroundSegment(from, to, 0.05f) is not null) return false;

        foreach (var obstacle in world.Obstacles)
        {
            var min = obstacle.Position - new Vector3(obstacle.HalfSize.X, 0, obstacle.HalfSize.Y);
            var max = obstacle.Position + new Vector3(obstacle.HalfSize.X, obstacle.Height, obstacle.HalfSize.Y);
            var delta = to - from;
            float enter = 0, exit = 1;
            if (Slab(from.X, delta.X, min.X, max.X, ref enter, ref exit) &&
                Slab(from.Y, delta.Y, min.Y, max.Y, ref enter, ref exit) &&
                Slab(from.Z, delta.Z, min.Z, max.Z, ref enter, ref exit))
                return false;
        }
        return true;
    }

    private static bool Slab(float origin, float delta, float min, float max, ref float enter, ref float exit)
    {
        if (MathF.Abs(delta) < 0.000001f) return origin >= min && origin <= max;
        var a = (min - origin) / delta;
        var b = (max - origin) / delta;
        enter = MathF.Max(enter, MathF.Min(a, b));
        exit = MathF.Min(exit, MathF.Max(a, b));
        return enter <= exit;
    }

    public void Cancel()
    {
        CastingRemaining = 0;
        _castingDuration = 0;
    }

    public MagicSnapshot Capture() => new(Selected, Cooldown);

    public void Restore(MagicSnapshot? state)
    {
        Selected = state is not null ? Math.Clamp(state.Selected, 0, Spells.Count - 1) : 0;
        Cooldown = state is not null && double.IsFinite(state.Cooldown)
            ? Math.Clamp(state.Cooldown, 0, 9)
            : 0;
        CastingRemaining = RevealRemaining = FlashRemaining = _castingDuration = 0;
        Message = "Q ZMIANA CZARU / F RZUC / C SCENKA";
    }
}
