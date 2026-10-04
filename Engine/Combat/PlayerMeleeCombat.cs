using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Combat;

public sealed class PlayerMeleeCombat
{
    public static AttackDefinition LightAttack { get; } = new AttackDefinition(
        "melee.light",
        Damage: 20f,
        StaminaCost: 12f,
        Range: 2.1f,
        WindupSeconds: 0.18,
        RecoverySeconds: 0.32,
        DamageType.Physical,
        AttackDirection.Right).Validate();

    private const float HalfAngleDegrees = 52f;
    private const double MaxSimulationStep = 0.03;

    public MeleeAttackController Controller { get; } = new();
    public string Message { get; private set; } = "LPM ATAK";

    public bool TryStart(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Cinematics.IsPlaying ||
            world.Rituals.IsPerforming ||
            world.Magic.IsCasting ||
            !world.Player.IsAlive)
        {
            Message = "ATAK TERAZ NIEDOSTEPNY";
            return false;
        }

        if (!Controller.TryStart(LightAttack, world.Player))
        {
            Message = world.Player.Stamina < LightAttack.StaminaCost
                ? "ZA MALO STAMINY"
                : "ATAK JESZCZE TRWA";
            return false;
        }

        Message = "ATAK";
        return true;
    }

    public void Update(WorldState world, Vector3 forward, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
            return;

        var remaining = deltaSeconds;
        if (remaining <= 0)
        {
            ResolveHits(world, forward);
            return;
        }

        while (remaining > 0)
        {
            var step = Math.Min(MaxSimulationStep, remaining);
            Controller.Update(world.Player, step);
            ResolveHits(world, forward);
            remaining -= step;
        }

        if (Controller.State == MeleeAttackState.Free &&
            Message == "ATAK")
        {
            Message = "LPM ATAK";
        }
    }

    private void ResolveHits(WorldState world, Vector3 forward)
    {
        if (!Controller.CanDealDamage || Controller.CurrentAttack is not { } attack)
            return;

        var candidates = world.Enemies
            .Where(enemy => enemy.IsAlive)
            .Select(enemy => new MeleeHitCandidate(
                enemy.Id,
                enemy.Position,
                enemy.Radius))
            .Concat(
                world.Wildlife.Actors
                    .Where(actor =>
                        actor.Behavior != WildlifeBehavior.Dead &&
                        actor.Species != WildlifeSpecies.Raven)
                    .Select(actor =>
                    {
                        var profile =
                            WildlifeCatalog.For(actor.Species);
                        return new MeleeHitCandidate(
                            actor.Id,
                            actor.Position,
                            profile.BodyRadius);
                    }))
            .ToArray();

        var hits = MeleeHitResolver.ResolveActiveHits(
            Controller,
            world.PlayerPosition,
            forward,
            HalfAngleDegrees,
            candidates);

        foreach (var hit in hits)
        {
            var enemy = world.Enemies.FirstOrDefault(item =>
                string.Equals(
                    item.Id,
                    hit.TargetId,
                    StringComparison.Ordinal));

            if (enemy is not null)
            {
                enemy.TakeDamage(attack.Damage);

                if (!enemy.IsAlive)
                {
                    if (string.Equals(
                            enemy.Id,
                            SwampPredatorEncounter.Id,
                            StringComparison.Ordinal))
                    {
                        SwampPredatorEncounter.MarkKilled(world);
                    }

                    Message =
                        $"POKONANO: {enemy.Id.ToUpperInvariant()}";
                }
                else
                {
                    Message =
                        $"TRAFIENIE: {enemy.Id.ToUpperInvariant()} / HP {enemy.Health:0}/{enemy.MaxHealth:0}";
                }

                continue;
            }

            var wildlifeBefore =
                world.Wildlife.Actors.FirstOrDefault(actor =>
                    string.Equals(
                        actor.Id,
                        hit.TargetId,
                        StringComparison.Ordinal));

            if (wildlifeBefore is null ||
                !world.Wildlife.TryDamage(
                    world,
                    hit.TargetId,
                    attack.Damage))
            {
                continue;
            }

            var wildlifeAfter =
                world.Wildlife.Actors.First(actor =>
                    string.Equals(
                        actor.Id,
                        hit.TargetId,
                        StringComparison.Ordinal));

            Message =
                wildlifeAfter.Behavior == WildlifeBehavior.Dead
                    ? $"UPOLWIONO: {WildlifeDisplayName(wildlifeAfter.Species)} / E ZBIERZ LUP"
                    : $"ZRANIONO: {WildlifeDisplayName(wildlifeAfter.Species)} / HP {wildlifeAfter.Health:0}/{wildlifeAfter.MaxHealth:0}";
        }
    }
    private static string WildlifeDisplayName(
        WildlifeSpecies species) => species switch
    {
        WildlifeSpecies.Deer => "JELEN",
        WildlifeSpecies.Boar => "DZIK",
        WildlifeSpecies.Wolf => "WILK",
        WildlifeSpecies.Raven => "KRUK",
        _ => species.ToString().ToUpperInvariant()
    };

}
