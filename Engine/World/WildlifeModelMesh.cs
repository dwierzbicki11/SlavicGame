using System.Numerics;
using SlavicGame.Engine.Assets;

namespace SlavicGame.Engine.World;

public static class WildlifeModelMesh
{
    public static void Append(
        WorldState world,
        IReadOnlyDictionary<string, GlbModel> models,
        double animationSeconds,
        Vector3 cameraPosition,
        float maxDistance,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(models);

        if (!double.IsFinite(animationSeconds) ||
            !float.IsFinite(maxDistance) ||
            maxDistance <= 0f)
        {
            return;
        }

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        Append(world, models, animationSeconds, cameraPosition, maxDistance, output, triangles);
        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    public static void Append(
        WorldState world,
        IReadOnlyDictionary<string, GlbModel> models,
        double animationSeconds,
        Vector3 cameraPosition,
        float maxDistance,
        List<TerrainVertex> output,
        List<uint> triangles)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(triangles);

        if (!double.IsFinite(animationSeconds) ||
            !float.IsFinite(maxDistance) ||
            maxDistance <= 0f)
        {
            return;
        }

        var maxDistanceSquared = maxDistance * maxDistance;
        var time = (float)Math.Max(0d, animationSeconds);

        foreach (var actor in world.Wildlife.Actors)
        {
            if (actor.Looted)
                continue;

            var delta = actor.Position - cameraPosition;
            if (delta.LengthSquared() > maxDistanceSquared)
                continue;

            var profile = WildlifeCatalog.For(actor.Species);
            if (!models.TryGetValue(profile.ModelFile, out var model))
            {
                throw new KeyNotFoundException(
                    $"Wildlife model '{profile.ModelFile}' for '{actor.Id}' is not loaded.");
            }

            var clip = SelectClip(actor);
            if (!model.AnimationNames.Contains(clip))
            {
                clip = actor.Behavior == WildlifeBehavior.Dead
                    ? "Idle"
                    : actor.Species == WildlifeSpecies.Raven
                        ? "Fly"
                        : "Walk";
            }

            if (!model.AnimationNames.Contains(clip))
                clip = model.AnimationNames.FirstOrDefault() ?? "";

            var corpseTilt =
                actor.Behavior == WildlifeBehavior.Dead
                    ? Matrix4x4.CreateRotationZ(-1.25f)
                    : Matrix4x4.Identity;

            var transform =
                Matrix4x4.CreateScale(profile.Scale) *
                corpseTilt *
                Matrix4x4.CreateRotationY(actor.YawRadians) *
                Matrix4x4.CreateTranslation(actor.Position);

            model.AppendTerrainMesh(
                transform,
                profile.Color,
                output,
                triangles,
                clip,
                time + StableAnimationOffset(actor.Id),
                sourceIsZUp: true);
        }
    }

    private static string SelectClip(WildlifeWorldActor actor) =>
        actor.Species switch
        {
            WildlifeSpecies.Raven => actor.Behavior is
                WildlifeBehavior.Fly or WildlifeBehavior.Spooked
                    ? "Fly"
                    : "Idle",

            _ => actor.Behavior switch
            {
                WildlifeBehavior.Dead => "Death",
                WildlifeBehavior.Flee => "Run",
                _ => actor.IsMoving
                    ? "Walk"
                    : "Idle"
            }
        };

    private static float StableAnimationOffset(string id)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in id)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (hash % 1000u) / 1000f * 2.5f;
        }
    }
}
