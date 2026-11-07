using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Animation;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Characters;

public static class EnemyCharacterModel
{
    public static readonly Vector3 DefaultPredatorColor = new(0.30f, 0.20f, 0.13f);
    public static readonly Vector3 AttackWindupColor = new(0.72f, 0.38f, 0.08f);

    public static void Append(
        WorldState world,
        GlbModel enemyModel,
        float timeSeconds,
        List<TerrainVertex> vertexList,
        List<uint> indexList)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(enemyModel);
        ArgumentNullException.ThrowIfNull(vertexList);
        ArgumentNullException.ThrowIfNull(indexList);

        foreach (var enemy in world.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            var clip = EnemyAnimationPresenter.ClipFor(enemy);
            var clipTime = EnemyAnimationPresenter.ClipTimeFor(
                enemy,
                timeSeconds,
                enemy.IsHitReacting ? enemyModel.AnimationDuration("Hit") : 0f,
                enemy.State == EnemyState.Attack ? enemyModel.AnimationDuration("Attack") : 0f);

            // This quadruped is authored with its head along source/engine +X.
            var yaw = MathF.Atan2(-enemy.FacingDirection.Z, enemy.FacingDirection.X);
            var transform =
                Matrix4x4.CreateRotationY(yaw) *
                Matrix4x4.CreateTranslation(enemy.Position);

            enemyModel.AppendTerrainMesh(
                transform,
                enemy.IsAttackWindingUp ? AttackWindupColor : DefaultPredatorColor,
                vertexList,
                indexList,
                clip,
                clipTime,
                sourceIsZUp: true);
        }
    }
}
