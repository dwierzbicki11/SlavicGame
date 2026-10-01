using System.Numerics;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.World;
using SlavicGame.Engine.Renderer;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    checks++;
}
void Near(float actual, float expected, string name) => Check(MathF.Abs(actual - expected) < 0.0001f, name);
void Reject(Action action, string name)
{
    try { action(); } catch (ArgumentOutOfRangeException) { checks++; return; }
    throw new Exception(name);
}

Reject(() => new Terrain(1, 2), "Degenerate width");
Reject(() => new Terrain(2, 1), "Degenerate depth");
Reject(() => new Terrain(2, 2, 0), "Zero cell size");
Reject(() => new Terrain(2, 2, float.NaN), "NaN cell size");
var terrain = new Terrain(7, 9, 2);
TerrainMesh.Build(terrain, out var vertices, out var indices);
Check(vertices.Length == 63 && indices.Length == 288, "Mesh dimensions");
foreach (var vertex in vertices)
    Near(terrain.SampleHeight(vertex.Position), vertex.Position.Y, "Grid height");
for (var i = 0; i < indices.Length; i += 3)
{
    var a = vertices[indices[i]].Position;
    var b = vertices[indices[i + 1]].Position;
    var c = vertices[indices[i + 2]].Position;
    var sample = a * 0.2f + b * 0.3f + c * 0.5f;
    Near(terrain.SampleHeight(sample), sample.Y, "Height on rendered triangle");
}
TerrainMesh.Build(new Terrain(257, 257), out var largeVertices, out var largeIndices);
Check(largeIndices.Max() == largeVertices.Length - 1, "32-bit terrain indices");
Reject(() => terrain.SampleHeight(new Vector3(float.NaN, 0, 0)), "Invalid sample");
var world = WorldGenerator.Generate();
world.Initialize();
Check(world.Regions.Count == 4, "Idempotent world initialization");
world.SetPlayerPosition(new Vector3(0, 999, -85));
Check(world.CurrentRegion == "old-village", "Village detection");
world.SetPlayerPosition(new Vector3(10000, 0, -10000));
Near(world.PlayerPosition.X, 127, "East boundary");
Near(world.PlayerPosition.Z, -127, "South boundary");
Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition), "Ground following");
Reject(() => world.SetPlayerPosition(new Vector3(float.PositiveInfinity, 0, 0)), "Invalid player position");
var clock = new WorldTime();
clock.Update(450);
Check(clock.TimeOfDayHours == 20 && clock.IsNight, "Night boundary");
clock.Update(900 * 5 + 375);
Check(clock.TimeOfDayHours == 6 && clock.IsDay, "Multi-day wrapping");
clock.Update(double.NaN);
clock.Update(-10);
Check(clock.TimeOfDayHours == 6, "Invalid time does not corrupt clock");
var time = new GameTime();
time.Advance(5);
Check(time.DeltaSeconds == 0.25 && time.TotalSeconds == 0.25, "Frame delta clamp");
time.Advance(double.NaN);
Check(time.TotalSeconds == 0.25, "Invalid frame delta");
var camera = new Camera3D();
Check(Vector3.Dot(camera.GetMoveForward(), camera.GetMoveRight()) < 0.0001, "Camera movement axes");
camera.Update(new Vector3(10, 0, 10), 0, 0, 1);
var expected = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY)
    * Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, 1.6f, camera.NearPlane, camera.FarPlane);
Check(camera.GetViewProjection(1.6f) == expected, "System.Numerics row-vector matrix order");
Console.WriteLine($"PASS: {checks} regression checks.");
