using System.Numerics;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Input;
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

// Independent intersection oracle using the triangles submitted to the renderer.
float? MeshGroundHit(Terrain testTerrain, Vector3 start, Vector3 end, float clearance)
{
    TerrainMesh.Build(testTerrain, out var meshVertices, out var meshIndices);
    var direction = end - start;
    float? firstHit = null;
    for (var i = 0; i < meshIndices.Length; i += 3)
    {
        var a = meshVertices[meshIndices[i]].Position + Vector3.UnitY * clearance;
        var b = meshVertices[meshIndices[i + 1]].Position + Vector3.UnitY * clearance;
        var c = meshVertices[meshIndices[i + 2]].Position + Vector3.UnitY * clearance;
        var edge1 = b - a;
        var edge2 = c - a;
        var p = Vector3.Cross(direction, edge2);
        var determinant = Vector3.Dot(edge1, p);
        if (MathF.Abs(determinant) < 0.000001f) continue;
        var inverse = 1f / determinant;
        var offset = start - a;
        var u = Vector3.Dot(offset, p) * inverse;
        var q = Vector3.Cross(offset, edge1);
        var v = Vector3.Dot(direction, q) * inverse;
        var t = Vector3.Dot(edge2, q) * inverse;
        if (u < 0f || v < 0f || u + v > 1f || t < 0f || t > 1f) continue;
        if (firstHit is null || t < firstHit) firstHit = t;
    }
    return firstHit;
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
Check(world.Obstacles.Count == 9, "Idempotent obstacle initialization");
Check(world.Enemies.Count == 1 && world.Enemies[0].Id == "swamp-predator",
    "World initializes one vertical-slice predator");
StaticWorldMesh.Build(world, out var worldVertices, out var worldIndices);
Check(worldVertices.Length == world.Terrain.Width * world.Terrain.Depth + world.Obstacles.Count * 8,
    "Static world mesh includes obstacle vertices");
Check(worldIndices.Length == (world.Terrain.Width - 1) * (world.Terrain.Depth - 1) * 6 + world.Obstacles.Count * 36,
    "Static world mesh includes obstacle indices");
Check(worldIndices.Max() == worldVertices.Length - 1, "Static world indices address the final obstacle vertex");
world.SetPlayerPosition(new Vector3(0, 999, -85));
Check(world.CurrentRegion == "old-village", "Village detection");
world.SetPlayerPosition(new Vector3(10000, 0, -10000));
Near(world.PlayerPosition.X, 127, "East boundary");
Near(world.PlayerPosition.Z, -127, "South boundary");
Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition), "Ground following");
Reject(() => world.SetPlayerPosition(new Vector3(float.PositiveInfinity, 0, 0)), "Invalid player position");
Reject(() => new WorldObstacle("invalid", Vector3.Zero, Vector2.Zero, 1f, Vector3.One),
    "Zero obstacle size rejected");
var villageHut = world.Obstacles.First(obstacle => obstacle.Id == "village-hut-a");
world.SetPlayerPosition(villageHut.Position);
var resolvedPlayer = new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z);
Check(!villageHut.IntersectsCircle(resolvedPlayer, world.PlayerRadius),
    "Player is resolved out of static obstacle collision");
var invalidHorizontalRejected = false;
try { world.ResolveHorizontalPosition(new Vector2(float.NaN, 0f), 0.5f); }
catch (ArgumentOutOfRangeException) { invalidHorizontalRejected = true; checks++; }
if (!invalidHorizontalRejected) throw new Exception("Invalid horizontal collision input rejected");

var collisionWorld = WorldGenerator.Generate();
var collisionCamera = new Camera3D();
collisionWorld.SetPlayerPosition(new Vector3(-10f, 0f, -80f));
for (var frame = 0; frame < 120; frame++)
{
    PlayerController.Update(collisionWorld, collisionCamera,
        new PlayerInput(true, false, false, false, true, Vector2.Zero), 1.0 / 60.0);
    var horizontalPlayer = new Vector2(collisionWorld.PlayerPosition.X, collisionWorld.PlayerPosition.Z);
    Check(collisionWorld.Obstacles.All(obstacle =>
        !obstacle.IntersectsCircle(horizontalPlayer, collisionWorld.PlayerRadius)),
        "Controller movement never leaves player inside an obstacle");
}
var aiWorld = WorldGenerator.Generate();
var predator = aiWorld.Enemies.Single();
ActorMesh.Build(aiWorld.Enemies, out var actorVertices, out var actorIndices);
Check(actorVertices.Length == 8 && actorIndices.Length == 36,
    "Living enemy produces one dynamic actor box");
aiWorld.SetPlayerPosition(predator.HomePosition + new Vector3(3f, 0f, 0f));
predator.Update(aiWorld, 0.01);
Check(predator.State == EnemyState.Alert, "Enemy notices player inside detection range");
predator.Update(aiWorld, 0.60);
Check(predator.State == EnemyState.Chase, "Alert transitions into chase");
for (var frame = 0; frame < 120 && predator.State != EnemyState.Attack; frame++)
{
    predator.Update(aiWorld, 1.0 / 60.0);
}
Check(predator.State == EnemyState.Attack, "Chasing enemy reaches attack state");
var healthBeforeAttack = aiWorld.Player.Health;
predator.Update(aiWorld, 0.01);
Check(aiWorld.Player.Health < healthBeforeAttack, "Enemy attack damages player");
aiWorld.SetPlayerPosition(Vector3.Zero);
predator.Update(aiWorld, 0.01);
Check(predator.State == EnemyState.Return, "Enemy disengages from distant player");
for (var frame = 0; frame < 600 && predator.State != EnemyState.Patrol; frame++)
{
    predator.Update(aiWorld, 1.0 / 60.0);
}
Check(predator.State == EnemyState.Patrol, "Enemy returns to its home patrol");
predator.TakeDamage(1000f);
Check(!predator.IsAlive && predator.State == EnemyState.Dead, "Enemy health reaches dead state");
ActorMesh.Build(aiWorld.Enemies, out actorVertices, out actorIndices);
Check(actorVertices.Length == 0 && actorIndices.Length == 0,
    "Dead enemy is removed from dynamic actor mesh");
Reject(() => predator.TakeDamage(float.NaN), "NaN enemy damage rejected");

var clock = new WorldTime();
clock.Update(450);
Check(clock.TimeOfDayHours == 20 && clock.IsNight, "Night boundary");
clock.Update(900 * 5 + 375);
Check(clock.TimeOfDayHours == 6 && clock.IsDay, "Multi-day wrapping");
clock.Update(double.NaN);
clock.Update(-10);
Check(clock.TimeOfDayHours == 6, "Invalid time does not corrupt clock");

var vitals = new PlayerVitals();
vitals.UpdateStamina(true, 5);
Check(vitals.Stamina == 0f && !vitals.CanSprint, "Sprinting can exhaust stamina");
vitals.UpdateStamina(false, 0.5);
Check(vitals.Stamina == 0f, "Stamina waits for recovery delay");
vitals.UpdateStamina(false, 0.5);
Check(vitals.Stamina > 0f && vitals.CanSprint, "Stamina recovers after delay");
vitals.TakeDamage(35f);
Check(vitals.Health == 65f, "Damage reduces health");
vitals.Heal(10f);
Check(vitals.Health == 75f, "Healing restores health");
vitals.TakeDamage(1000f);
Check(vitals.Health == 0f && !vitals.IsAlive, "Health clamps at zero");
Reject(() => vitals.TakeDamage(float.NaN), "NaN damage rejected");

var weather = new WeatherSystem(7);
weather.SetCondition(WeatherKind.Fog, true);
Check(weather.Condition == WeatherKind.Fog && weather.FogDensity >= 0.03f,
    "Fog preset raises fog density");
weather.SetCondition(WeatherKind.Rain, true);
Check(weather.RainIntensity > 0.5f && weather.Cloudiness > 0.7f,
    "Rain preset affects rain and cloudiness");
var scheduledWeather = new WeatherSystem(7);
scheduledWeather.Update(46, WorldRegionType.Swamp);
Check(scheduledWeather.SecondsUntilChange > 0 && scheduledWeather.SecondsUntilChange != 45,
    "Weather schedules a regional transition");
var fogBeforeInvalidUpdate = weather.FogDensity;
weather.Update(double.NaN, WorldRegionType.Forest);
Check(weather.FogDensity == fogBeforeInvalidUpdate, "Invalid weather delta is ignored");
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
// Exercise the actual controller across held W/W+D/A/S and running input.
foreach (var running in new[] { false, true })
foreach (var movement in new[] { (true, false, false, false), (true, false, true, false), (false, true, false, false), (false, false, false, true) })
{
    var movingWorld = WorldGenerator.Generate();
    var movingCamera = new Camera3D();
    for (var frame = 0; frame < 30; frame++)
    {
        var previousPosition = movingWorld.PlayerPosition;
        var previousYaw = movingCamera.Yaw;
        var previousPitch = movingCamera.Pitch;
        PlayerController.Update(movingWorld, movingCamera,
            new PlayerInput(movement.Item1, movement.Item2, movement.Item3, movement.Item4,
                running, new Vector2(3, 1)), 1.0 / 60.0);
        var horizontalStep = new Vector2(movingWorld.PlayerPosition.X - previousPosition.X,
            movingWorld.PlayerPosition.Z - previousPosition.Z).Length();
        Near(horizontalStep, (running ? 9f : 5f) / 60f, "Move on every frame while looking");
        Check(movingCamera.Yaw != previousYaw && movingCamera.Pitch != previousPitch,
            "Yaw and pitch change on every frame while movement is held");
    }
}
var stationaryWorld = WorldGenerator.Generate();
var stationaryCamera = new Camera3D();
PlayerController.Update(stationaryWorld, stationaryCamera,
    new PlayerInput(true, true, true, true, false, new Vector2(10, 10)), 0.1);
Check(stationaryWorld.PlayerPosition.X == 0 && stationaryWorld.PlayerPosition.Z == 0,
    "Opposite movement keys cancel");
Check(stationaryCamera.Yaw != MathF.PI && stationaryCamera.Pitch != -0.25f,
    "Looking still works when movement cancels");
var mouseEvent = new Vector2(12, -4);
Check(MouseMotion.Select(mouseEvent, Vector2.Zero) == mouseEvent,
    "Delivered mouse event survives a drained SDL accumulator");
Check(MouseMotion.Select(mouseEvent, mouseEvent) == mouseEvent,
    "Mouse event and polled motion are not counted twice");
Check(MouseMotion.Select(Vector2.Zero, mouseEvent) == mouseEvent,
    "Polled motion covers a missing mouse event");
Check(MouseMotion.Select(Vector2.Zero, Vector2.Zero) == Vector2.Zero,
    "Idle mouse produces no look movement");
var eventOnlyWorld = WorldGenerator.Generate();
var eventOnlyCamera = new Camera3D();
var eventOnlyYaw = eventOnlyCamera.Yaw;
var eventOnlyPosition = eventOnlyWorld.PlayerPosition;
PlayerController.Update(eventOnlyWorld, eventOnlyCamera,
    new PlayerInput(true, false, false, false, true, MouseMotion.Select(mouseEvent, Vector2.Zero)), 0.1);
Check(eventOnlyWorld.PlayerPosition != eventOnlyPosition && eventOnlyCamera.Yaw != eventOnlyYaw,
    "Running and turning work when only SDL events contain motion");

var tiredWorld = WorldGenerator.Generate();
tiredWorld.Player.UpdateStamina(true, 10);
var tiredCamera = new Camera3D();
var tiredStart = tiredWorld.PlayerPosition;
PlayerController.Update(tiredWorld, tiredCamera,
    new PlayerInput(true, false, false, false, true, Vector2.Zero), 1.0 / 60.0);
var tiredStep = new Vector2(
    tiredWorld.PlayerPosition.X - tiredStart.X,
    tiredWorld.PlayerPosition.Z - tiredStart.Z).Length();
Near(tiredStep, 5f / 60f, "Exhausted sprint falls back to walking speed");

var deadWorld = WorldGenerator.Generate();
deadWorld.Player.TakeDamage(1000f);
var deadCamera = new Camera3D();
var deadStart = deadWorld.PlayerPosition;
PlayerController.Update(deadWorld, deadCamera,
    new PlayerInput(true, false, false, false, true, Vector2.Zero), 0.1);
Check(deadWorld.PlayerPosition == deadStart, "Dead player cannot move");

Reject(() => camera.TerrainClearance = -0.1f, "Negative camera clearance");
Reject(() => camera.TerrainClearance = float.NaN, "NaN camera clearance");
Reject(() => terrain.IntersectGroundSegment(new Vector3(0, float.NaN, 0), Vector3.One),
    "Invalid ground segment origin");
Reject(() => terrain.IntersectGroundSegment(Vector3.One, new Vector3(0, float.PositiveInfinity, 0)),
    "Invalid ground segment endpoint");
Reject(() => terrain.IntersectGroundSegment(Vector3.One, Vector3.Zero, -1f), "Negative segment clearance");

var steepTerrain = new Terrain(7, 9, 0.25f);
var random = new Random(20261001);
var ridgeHits = 0;
for (var ray = 0; ray < 100; ray++)
{
    var start = new Vector3((float)random.NextDouble() * 1.4f - 0.7f, 0f,
        (float)random.NextDouble() * 1.9f - 0.95f);
    var end = new Vector3((float)random.NextDouble() * 1.4f - 0.7f, 0f,
        (float)random.NextDouble() * 1.9f - 0.95f);
    const float clearance = 0.5f;
    start.Y = steepTerrain.SampleHeight(start) + clearance + 0.2f;
    end.Y = steepTerrain.SampleHeight(end) + clearance + (ray % 2 == 0 ? 0.2f : -1f);
    var expectedHit = MeshGroundHit(steepTerrain, start, end, clearance);
    var actualHit = steepTerrain.IntersectGroundSegment(start, end, clearance);
    Check(actualHit.HasValue == expectedHit.HasValue, "Ground segment agrees with rendered mesh");
    if (expectedHit is not null)
    {
        Near(actualHit!.Value, expectedHit.Value, "First ground contact matches triangle intersection");
        if (ray % 2 == 0) ridgeHits++;
    }
}
Check(ridgeHits > 0, "Test includes ridges between two clear endpoints");
var verticalStart = new Vector3(0.13f, 0f, -0.21f);
verticalStart.Y = steepTerrain.SampleHeight(verticalStart) + 2f;
var verticalEnd = verticalStart - Vector3.UnitY * 4f;
Near(steepTerrain.IntersectGroundSegment(verticalStart, verticalEnd, 0.5f)!.Value, 0.375f,
    "Vertical ray crosses the correct offset surface");
Check(steepTerrain.IntersectGroundSegment(verticalEnd, verticalStart, 0.5f) == 0f,
    "Segment starting inside ground contacts immediately");
Check(steepTerrain.IntersectGroundSegment(verticalStart, verticalStart, 0.5f) is null,
    "Stationary safe segment has no contact");

// Previously an upward look put the third-person camera below the player's feet.
var groundWorld = WorldGenerator.Generate();
var groundCamera = new Camera3D();
for (var frame = 0; frame < 6; frame++)
{
    PlayerController.Update(groundWorld, groundCamera,
        new PlayerInput(false, false, false, false, false, new Vector2(0, -150)), 1.0 / 60.0);
    Check(groundCamera.Position.Y >= groundWorld.Terrain.SampleHeight(groundCamera.Position) + groundCamera.TerrainClearance,
        "Looking up keeps the camera above terrain");
}
Near(groundCamera.Pitch, 0.85f, "Terrain correction preserves requested pitch");
var unsafeOrbit = groundWorld.PlayerPosition + Vector3.UnitY * groundCamera.TargetHeight
    - groundCamera.GetLookDirection() * groundCamera.Distance + Vector3.UnitY * groundCamera.HeightOffset;
Check(unsafeOrbit.Y < groundWorld.Terrain.SampleHeight(unsafeOrbit),
    "Regression exercises an orbit that would enter the ground");

foreach (var smoothing in new[] { 0.2f, 14f })
{
    var movingGroundWorld = WorldGenerator.Generate();
    var movingGroundCamera = new Camera3D { PositionSmoothing = smoothing };
    movingGroundWorld.SetPlayerPosition(new Vector3(-110f, 0f, -110f));
    for (var frame = 0; frame < 360; frame++)
    {
        var phase = frame % 120;
        PlayerController.Update(movingGroundWorld, movingGroundCamera,
            new PlayerInput(true, false, phase < 60, phase >= 60, true,
                new Vector2(8f, phase < 60 ? -8f : 8f)), frame % 90 == 0 ? 0.25 : 1.0 / 60.0);
        Check(movingGroundCamera.Position.Y >= movingGroundWorld.Terrain.SampleHeight(movingGroundCamera.Position)
            + movingGroundCamera.TerrainClearance, "Camera follows terrain while running and turning");
        Check(movingGroundWorld.Terrain.IntersectGroundSegment(movingGroundCamera.Target,
            movingGroundCamera.Position, movingGroundCamera.TerrainClearance) is null,
            "Smoothed camera boom remains clear of the ground");
    }
}

// Compressed terrain creates steep faces and small ridges; test the camera against
// the renderer's triangles rather than only against its own collision query.
var steepCamera = new Camera3D { Distance = 1.5f, PositionSmoothing = 0.2f };
for (var frame = 0; frame < 120; frame++)
{
    var player = new Vector3(MathF.Sin(frame * 0.11f) * 0.65f, 0f, MathF.Cos(frame * 0.11f) * 0.85f);
    player.Y = steepTerrain.SampleHeight(player);
    steepCamera.Update(player, 30f, frame < 60 ? -15f : 15f, frame % 30 == 0 ? 0f : 1f / 30f, steepTerrain);
    Check(steepCamera.Position.Y >= steepTerrain.SampleHeight(steepCamera.Position) + steepCamera.TerrainClearance,
        "Steep slopes do not swallow the smoothed camera");
    Check(MeshGroundHit(steepTerrain, steepCamera.Target, steepCamera.Position, steepCamera.TerrainClearance) is null,
        "Camera line of sight does not cross rendered terrain");
    var matrix = steepCamera.GetViewProjection(16f / 9f);
    Check(float.IsFinite(matrix.M11) && float.IsFinite(matrix.M22) && float.IsFinite(matrix.M33)
        && float.IsFinite(matrix.M41) && float.IsFinite(matrix.M42) && float.IsFinite(matrix.M43),
        "Collision resolution leaves a usable view matrix");
}

// Find an independently confirmed obstructed orbit, then require the camera to
// shorten it. A fixed distance threshold would not establish an obstruction.
TerrainMesh.Build(steepTerrain, out var ridgeVertices, out _);
var shortenedBooms = 0;
foreach (var vertex in ridgeVertices)
for (var direction = 0; direction < 24; direction++)
{
    var ridgeCamera = new Camera3D { Distance = 1.5f, HeightOffset = 0f };
    ridgeCamera.Rotate(0f, -100f); // Horizontal orbit.
    for (var turn = 0; turn < direction; turn++) ridgeCamera.Rotate(150f, 0f);
    var focus = vertex.Position + Vector3.UnitY * ridgeCamera.TargetHeight;
    var requested = focus - ridgeCamera.GetLookDirection() * ridgeCamera.Distance;
    requested.Y = MathF.Max(requested.Y, steepTerrain.SampleHeight(requested) + ridgeCamera.TerrainClearance + 0.01f);
    if (MeshGroundHit(steepTerrain, focus, requested, ridgeCamera.TerrainClearance) is null) continue;
    ridgeCamera.Follow(vertex.Position, 0f, steepTerrain);
    Check(Vector3.Distance(ridgeCamera.Target, ridgeCamera.Position) < Vector3.Distance(focus, requested),
        "Confirmed ridge shortens the requested camera boom");
    Check(MeshGroundHit(steepTerrain, ridgeCamera.Target, ridgeCamera.Position, ridgeCamera.TerrainClearance) is null,
        "Shortened boom stays in front of the confirmed ridge");
    shortenedBooms++;
}
Check(shortenedBooms > 0, "Test includes an obstructed third-person orbit");

Console.WriteLine($"PASS: {checks} regression checks.");
