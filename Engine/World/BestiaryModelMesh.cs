using System.Numerics;
using SlavicGame.Engine.Assets;

namespace SlavicGame.Engine.World;

public static class BestiaryModelMesh
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

        foreach (var actor in world.Bestiary.Actors)
        {
            if (actor.Looted)
                continue;

            var delta = actor.Position - cameraPosition;
            if (delta.LengthSquared() > maxDistanceSquared)
                continue;

            var profile = BestiaryVisualCatalog.For(actor.Species);
            if (!models.TryGetValue(profile.ModelFile, out var model))
            {
                throw new KeyNotFoundException(
                    $"Bestiary model '{profile.ModelFile}' for '{actor.Id}' is not loaded.");
            }

            var clip = actor.AnimationClip;
            if (!model.AnimationNames.Contains(clip))
            {
                clip = actor.IsAlive ? "Idle" : "Death";
            }

            if (!model.AnimationNames.Contains(clip))
                clip = model.AnimationNames.FirstOrDefault() ?? "";

            var corpseTilt = !actor.IsAlive
                ? Matrix4x4.CreateRotationZ(-1.3f)
                : Matrix4x4.Identity;

            var transform =
                Matrix4x4.CreateScale(profile.Scale) *
                corpseTilt *
                Matrix4x4.CreateRotationY(actor.YawRadians) *
                Matrix4x4.CreateTranslation(actor.Position);

            model.AppendTerrainMesh(
                transform,
                profile.BaseColor,
                output,
                triangles,
                clip,
                actor.ClipTime,
                sourceIsZUp: true);
        }
    }
}
