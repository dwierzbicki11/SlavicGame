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
        var maxDistanceSquared = maxDistance * maxDistance;
        var time = (float)Math.Max(0d, animationSeconds);

        foreach (var actor in world.Wildlife.Actors)
        {
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
                clip = actor.Species == WildlifeSpecies.Raven
                    ? "Fly"
                    : "Walk";
            }

            if (!model.AnimationNames.Contains(clip))
                clip = model.AnimationNames.FirstOrDefault() ?? "";

            var transform =
                Matrix4x4.CreateScale(profile.Scale) *
                Matrix4x4.CreateRotationY(actor.YawRadians) *
                Matrix4x4.CreateTranslation(actor.Position);

            var mesh = model.BuildMesh(
                transform,
                clip,
                time + StableAnimationOffset(actor.Id),
                sourceIsZUp: true);

            var start = checked((uint)output.Count);
            foreach (var position in mesh.Positions)
            {
                output.Add(new TerrainVertex(
                    position,
                    profile.Color));
            }

            foreach (var index in mesh.Indices)
                triangles.Add(start + index);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    private static string SelectClip(WildlifeWorldActor actor) =>
        actor.Species switch
        {
            WildlifeSpecies.Raven => actor.Behavior is
                WildlifeBehavior.Fly or WildlifeBehavior.Spooked
                    ? "Fly"
                    : "Idle",

            _ => actor.Behavior == WildlifeBehavior.Flee
                ? "Run"
                : actor.IsMoving
                    ? "Walk"
                    : "Idle"
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
