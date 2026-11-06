using System.Numerics;
using SlavicGame.Engine.Assets;

namespace SlavicGame.Engine.World;

public static class WorldItemModelMesh
{
    public static void Append(
        WorldState world,
        IReadOnlyDictionary<string, GlbModel> models,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(models);

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        Append(world, models, output, triangles);
        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    public static void Append(
        WorldState world,
        IReadOnlyDictionary<string, GlbModel> models,
        List<TerrainVertex> output,
        List<uint> triangles)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(triangles);

        foreach (var definition in WorldItemVisualCatalog.Definitions)
        {
            if (!WorldItemVisualCatalog.IsVisible(world, definition))
                continue;

            if (!models.TryGetValue(definition.AssetPath, out var model))
            {
                throw new InvalidOperationException(
                    $"World item visual model '{definition.AssetPath}' is not loaded.");
            }

            var position = definition.Position;
            position.Y =
                world.Terrain.SampleHeight(position) +
                definition.YOffset;

            var transform =
                Matrix4x4.CreateScale(definition.Scale) *
                Matrix4x4.CreateRotationY(definition.YawRadians) *
                Matrix4x4.CreateTranslation(position);

            model.AppendTerrainMesh(
                transform,
                definition.Color,
                output,
                triangles,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: true);
        }
    }

    private static void AppendGeometry(
        MeshGeometry geometry,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = checked((uint)vertices.Count);

        foreach (var position in geometry.Positions)
        {
            vertices.Add(
                new TerrainVertex(
                    position,
                    color));
        }

        foreach (var index in geometry.Indices)
            indices.Add(start + index);
    }
}
