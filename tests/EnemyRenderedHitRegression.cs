using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Animation;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class EnemyRenderedHitRegression
{
    public static void Run(Action<bool, string> check)
    {
        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets", "models", "animated");
        var model = GlbModel.Load(Path.Combine(assetsRoot, "swamp_predator_animated.glb"));
        var player = GlbModel.Load(Path.Combine(assetsRoot, "player_hunter_animated.glb"));
        var world = WorldGenerator.Generate();
        var npcs = world.NpcWorld.Actors.Select(actor => NpcVisualCatalog.ModelFile(actor.Id, actor.Role))
            .Distinct(StringComparer.Ordinal).ToDictionary(file => file, file => GlbModel.Load(Path.Combine(assetsRoot, file)), StringComparer.Ordinal);
        var enemy = world.Enemies.Single();
        world.SetPlayerPosition(enemy.Position + Vector3.UnitZ * 5f);
        enemy.Restore(new EnemySnapshot(enemy.Id, enemy.Position, enemy.MaxHealth, EnemyState.Chase));
        enemy.TakeDamage(10f);
        enemy.Update(world, 0.09);
        check(enemy.State == EnemyState.Chase && enemy.IsHitReacting,
            "Rendered hit remains a presentation override of live Chase engagement");
        var duration = model.AnimationDuration("Hit");
        check(duration > 0f && MathF.Abs(enemy.HitReactionProgress - 0.5f) < 0.01f,
            "Hit animation progress follows the active reaction window");
        var expected = model.BuildMesh(Matrix4x4.CreateTranslation(enemy.Position), "Hit", duration * enemy.HitReactionProgress, true);
        var rendered = Render(123.45);
        var suffix = rendered.Skip(rendered.Length - expected.Positions.Length).Select(vertex => vertex.Position).ToArray();
        check(suffix.SequenceEqual(expected.Positions),
            "Production actor mesh samples the real Hit GLB clip after damage");
        var atOtherWorldTime = Render(999.0);
        var otherSuffix = atOtherWorldTime.Skip(atOtherWorldTime.Length - expected.Positions.Length).Select(vertex => vertex.Position);
        check(otherSuffix.SequenceEqual(suffix),
            "Hit pose is anchored to impact time instead of global looping animation time");
        var running = model.BuildMesh(Matrix4x4.CreateTranslation(enemy.Position), "Run", 123.45f, true);
        check(!suffix.SequenceEqual(running.Positions) && suffix.All(position =>
                float.IsFinite(position.X) && float.IsFinite(position.Y) && float.IsFinite(position.Z)),
            "Hit renders a visibly different finite pose from the Run clip");

        enemy.TakeDamage(1f);
        check(enemy.HitReactionProgress == 0f && enemy.State == EnemyState.Chase,
            "A repeated hit restarts the rendered reaction while preserving Chase");
        enemy.Update(world, 0.181);
        rendered = Render(123.45);
        running = model.BuildMesh(Matrix4x4.CreateTranslation(enemy.Position), "Run", 123.45f, true);
        check(!enemy.IsHitReacting && EnemyAnimationPresenter.ClipFor(enemy) == "Run" &&
              rendered.Skip(rendered.Length - running.Positions.Length).Select(vertex => vertex.Position).SequenceEqual(running.Positions),
            "Production mesh resumes Run after the hit reaction expires");

        enemy.Restore(new EnemySnapshot(enemy.Id, enemy.Position, enemy.Health, EnemyState.Attack));
        enemy.TakeDamage(1f);
        check(enemy.State == EnemyState.Attack && EnemyAnimationPresenter.ClipFor(enemy) == "Hit",
            "Hit clip overrides Attack without replacing combat engagement");
        SaveGameService.Restore(world, SaveGameService.Serialize(world));
        check(!enemy.IsHitReacting && EnemyAnimationPresenter.ClipFor(enemy) == "Attack",
            "Save/load clears transient recoil and resumes the saved AI animation");

        var livingCount = Render(123.45).Length;
        enemy.TakeDamage(enemy.MaxHealth);
        check(EnemyAnimationPresenter.StateFor(enemy) == EnemyAnimationPresenter.Dead &&
              Render(123.45).Length == livingCount - running.Positions.Length,
            "Death takes priority and the production mesh stops appending the defeated enemy");

        TerrainVertex[] Render(double time)
        {
            ActorModelMesh.Build(world, player, npcs, model, world.PlayerPosition, time, 0f, false,
                out var vertices, out var indices);
            check(indices.All(index => index < vertices.Length), "Rendered actor indices remain valid during hit and death states");
            return vertices;
        }
    }
}
