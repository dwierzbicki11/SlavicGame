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

            var geometry = model.BuildMesh(
                transform,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: true);

            AppendGeometry(
                geometry,
                definition.Color,
                output,
                triangles);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();
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
