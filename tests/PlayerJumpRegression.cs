using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.Physics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class PlayerJumpRegression
{
    private static readonly PlayerInput Idle = new(false, false, false, false, false, Vector2.Zero);

    public static void Run(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate(); var camera = new Camera3D();
        var start = world.PlayerPosition;
        Frame(world, camera, Idle with { JumpPressed = true }, 0.1);
        check(world.Jump.IsAirborne && Near(world.PlayerPosition.Y - start.Y, 0.57f) &&
              !world.Dodge.IsActive && world.Player.Stamina == 100f,
            "Space launches a real jump with gravity, without starting dodge or spending stamina");
        var velocity = world.Jump.VerticalVelocity;
        check(!world.Jump.TryStart(world) && Near(world.Jump.VerticalVelocity, velocity),
            "A second Space press cannot add a double-jump impulse in the air");
        Frame(world, camera, Idle, 0.1);
        check(Near(world.PlayerPosition.Y - start.Y, 0.98f) && Near(world.Jump.VerticalVelocity, 3.3f),
            "World ticking preserves airborne height instead of snapping the player to terrain");
        Frame(world, camera, Idle, 0.20625);
        check(Near(world.PlayerPosition.Y - start.Y, 1.3203125f) && Near(world.Jump.VerticalVelocity, 0f),
            "Jump apex agrees with the independent ballistic height and time");
        Frame(world, camera, Idle, 0.15);
        check(world.Jump.IsAirborne && world.Jump.VerticalVelocity < 0f,
            "Gravity changes ascent into descent after the apex");
        Frame(world, camera, Idle, 0.4);
        check(!world.Jump.IsAirborne && world.Jump.VerticalVelocity == 0f && Near(world.PlayerPosition.Y, start.Y),
            "Landing ends the flight at the terrain height and clears vertical velocity");
        check(world.Jump.TryStart(world), "Landing permits the next deliberate jump");

        world = WorldGenerator.Generate(); camera = new Camera3D();
        Frame(world, camera, Idle with { DodgePressed = true }, 0.1);
        check(world.Dodge.IsActive && !world.Jump.IsAirborne && world.Player.Stamina == 80f,
            "Alt retains the stamina-based dodge without starting a jump");
        check(!world.Jump.TryStart(world), "Jump cannot overlap active dodge movement");
        world = WorldGenerator.Generate(); camera = new Camera3D();
        Frame(world, camera, Idle with { JumpPressed = true, DodgePressed = true }, 0.1);
        check(world.Jump.IsAirborne && !world.Dodge.IsActive && world.Player.Stamina == 100f,
            "Simultaneous Space and Alt give jump priority without charging a failed dodge");
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == 100f,
            "Dodge is unavailable while airborne without spending its stamina cost");
        Frame(world, camera, Idle, 1d);
        check(world.Dodge.TryStart(world, Vector3.UnitX), "Dodge becomes available after landing");

        var slow = Simulate(0.1, 0.2); var fast = Simulate(1.0 / 120.0, 0.2);
        check(Vector3.Distance(slow.Position, fast.Position) < 0.002f &&
              Near(slow.HeightAboveStart, 0.98f) && Near(slow.Travel, 1f) &&
              Near(slow.Velocity, fast.Velocity) && slow.Stamina == 100f && fast.Stamina == 100f,
            "10 and 120 FPS preserve horizontal movement, vertical trajectory and jump stamina");
        slow = Simulate(0.1, 1.2); fast = Simulate(1.0 / 120.0, 1.2);
        check(!slow.Airborne && !fast.Airborne && Vector3.Distance(slow.Position, fast.Position) < 0.002f,
            "Walking jump lands consistently on sloped generated terrain at 10 and 120 FPS");

        world = WorldGenerator.Generate(); camera = new Camera3D();
        Frame(world, camera, Idle with { JumpPressed = true, Forward = true, LookDelta = new Vector2(90f, 0f) }, 0.2);
        check(Near(new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z).Length(), 1f) &&
              Vector3.Dot(Vector3.Normalize(new Vector3(world.PlayerPosition.X, 0f, world.PlayerPosition.Z)), camera.GetMoveForward()) > 0.999f,
            "Jump movement uses the current mouse look while WASD remains responsive");
        check(Near(camera.Target.Y, world.PlayerPosition.Y + camera.TargetHeight),
            "Third-person camera follows the actual elevated player position");
        world = WorldGenerator.Generate(); camera = new Camera3D { Mode = CameraMode.FirstPerson };
        Frame(world, camera, Idle with { JumpPressed = true }, 0.2);
        check(Near(camera.Position.Y, world.PlayerPosition.Y + camera.EyeHeight),
            "First-person eye position rises with the jump instead of staying on the ground");

        world = WorldGenerator.Generate();
        var wall = world.Obstacles.Single(obstacle => obstacle.Id == "village-north-00-collision");
        world.SetPlayerPosition(wall.Position - Vector3.UnitZ * (wall.HalfSize.Y + world.PlayerRadius + 0.2f));
        camera = new Camera3D();
        Frame(world, camera, Idle with { JumpPressed = true, Backward = true }, 0.5);
        check(world.PlayerPosition.Z <= wall.Position.Z - wall.HalfSize.Y - world.PlayerRadius &&
              !wall.IntersectsCircle(new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z), world.PlayerRadius),
            "A long jumping frame cannot pass through the village's thin palisade");
        Frame(world, camera, Idle, 1d);
        check(!world.Jump.IsAirborne && Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition)),
            "A wall-constrained jump lands on the local generated terrain");

        world = WorldGenerator.Generate();
        world.SetPlayerPosition(new Vector3(WaterLandscape.CenterX(0f), 0f, 0f));
        check(!world.Jump.TryStart(world), "Deep river water blocks jumping");
        world = WorldGenerator.Generate(); world.Player.TakeDamage(100f);
        check(!world.Jump.TryStart(world), "A dead player cannot start a jump");
        world = WorldGenerator.Generate(); world.Melee.TryStart(world);
        check(!world.Jump.TryStart(world), "Melee windup blocks a new jump");
        world = WorldGenerator.Generate(); world.Progress.SetFlag(SpellLessons.LearnedFlag("spell.spark"));
        check(world.Magic.TryStart(world, Vector3.UnitZ) && !world.Jump.TryStart(world),
            "An active incantation blocks a new jump");
        world = WorldGenerator.Generate(); world.Cinematics.TryStart(world, CinematicPlayer.Arrival);
        check(!world.Jump.TryStart(world), "Cinematic Space remains reserved for skipping the scene");

        world = WorldGenerator.Generate();
        world.SetPlayerPosition(LootContainerRuntime.Position(world)!.Value);
        check(world.Loot.TryOpenNearest(world) && !world.Jump.TryStart(world),
            "Storage UI blocks a new Space jump");
        world.Loot.Close(); world.Jump.TryStart(world); world.Jump.Update(world, 0.1);
        var beforeUiFall = world.PlayerPosition.Y;
        check(world.Loot.TryOpenNearest(world), "Airborne UI fixture opens the nearby chest");
        world.Jump.Update(world, 0.1);
        check(world.PlayerPosition.Y > beforeUiFall && world.Jump.IsAirborne,
            "Existing airborne physics continues even when a gameplay UI opens");
        world.Jump.Update(world, 1d);
        check(!world.Jump.IsAirborne && Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition)),
            "An already airborne player can finish landing while UI is open");

        world = WorldGenerator.Generate(); camera = new Camera3D();
        world.Bow.SetAiming(world, true); world.Bow.TryStartDraw(world); world.Bow.Update(world, 0.3);
        var ammo = world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);
        Frame(world, camera, Idle with { JumpPressed = true }, 0.1);
        check(world.Jump.IsAirborne && world.Bow.IsAiming && world.Bow.IsDrawing &&
              world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammo,
            "Space jump preserves bow aiming and draw without using an arrow");
        check(world.Bow.TryRelease(world, camera.Position, camera.GetLookDirection()) &&
              world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammo - 1,
            "A bow drawn before jumping can release one arrow while airborne");

        world = WorldGenerator.Generate(); camera = new Camera3D();
        Frame(world, camera, Idle with { JumpPressed = true }, 0.2);
        var saved = SaveGameService.Serialize(world);
        SaveGameService.Restore(world, saved);
        check(!world.Jump.IsAirborne && world.Jump.VerticalVelocity == 0f &&
              Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition)),
            "Loading an airborne save safely grounds the player and removes pending vertical velocity");
        start = world.PlayerPosition; Frame(world, camera, Idle, 0.2);
        check(world.PlayerPosition == start, "A loaded jump cannot resume or inject a delayed vertical impulse");
        world.Jump.TryStart(world); world.Jump.Update(world, 0.1);
        world.SetPlayerPosition(new Vector3(4f, 0f, 0f));
        check(!world.Jump.IsAirborne && world.Jump.VerticalVelocity == 0f,
            "Explicit teleport discards the old airborne trajectory");

        world = WorldGenerator.Generate(); world.Jump.TryStart(world); world.Jump.Update(world, 0.1);
        start = world.PlayerPosition; velocity = world.Jump.VerticalVelocity;
        world.Jump.Update(world, double.NaN); world.Jump.Update(world, -1d);
        check(world.PlayerPosition == start && world.Jump.VerticalVelocity == velocity,
            "Invalid jump frame time does not corrupt height or velocity");
        world.Player.TakeDamage(100f); PlayerController.Update(world, new Camera3D(), Idle, 1d);
        check(!world.Jump.IsAirborne && Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition)),
            "Death prevents movement input but lets an existing jump settle to the terrain");

        world = WorldGenerator.Generate(); camera = new Camera3D();
        world.Weather.SetCondition(WeatherKind.Clear, true);
        var bank = Enumerable.Range(0, 81).Select(z =>
            {
                var point = new Vector3(WaterLandscape.CenterX(z) + WaterLandscape.SurfaceHalfWidth(z) + 2.4f, 0f, z);
                point.Y = world.Terrain.SampleHeight(point);
                return point;
            })
            .First(point => FootprintTrailState.TrackabilityAt(world, point) >= 0.3f);
        world.SetPlayerPosition(bank); world.Footprints.Reset(world.PlayerPosition);
        Frame(world, camera, Idle with { Backward = true }, 0.2);
        check(world.Footprints.Footprints.Count > 0, "Jump trail fixture has trackable ground footprints");
        world.Footprints.Reset(world.PlayerPosition);
        var launchPosition = world.PlayerPosition;
        check(world.Jump.TryStart(world), "Trackable-bank fixture permits a normal jump launch");
        Frame(world, camera, Idle with { Backward = true, JumpPressed = true }, 0.2);
        check(world.Jump.IsAirborne && world.Footprints.Footprints.Count == 0,
            $"Airborne movement does not paint walking footprints onto the ground (launch {launchPosition}, end {world.PlayerPosition}, velocity {world.Jump.VerticalVelocity}, footprints {world.Footprints.Footprints.Count})");

        RenderedJump(check);
    }

    private static void RenderedJump(Action<bool, string> check)
    {
        var root = Path.Combine(AppContext.BaseDirectory, "assets", "models", "animated");
        var player = GlbModel.Load(Path.Combine(root, "player_hunter_animated.glb"));
        var predator = GlbModel.Load(Path.Combine(root, "swamp_predator_animated.glb"));
        var world = WorldGenerator.Generate();
        var npcs = world.NpcWorld.Actors.Select(actor => NpcVisualCatalog.ModelFile(actor.Id, actor.Role))
            .Distinct(StringComparer.Ordinal).ToDictionary(file => file, file => GlbModel.Load(Path.Combine(root, file)), StringComparer.Ordinal);
        var playerVertexCount = player.BuildMesh(Matrix4x4.Identity, "Idle", 0f, true).Positions.Length;
        var grounded = Render(); var baseHeight = world.PlayerPosition.Y;
        Frame(world, new Camera3D(), Idle with { JumpPressed = true }, 0.2);
        var airborne = Render(); var rise = world.PlayerPosition.Y - baseHeight;
        check(rise > 0.9f && Enumerable.Range(0, playerVertexCount).All(index =>
                Vector3.Distance(airborne[index].Position - grounded[index].Position, Vector3.UnitY * rise) < 0.002f),
            "Production actor rendering raises the real player GLB mesh by the actual jump height");

        TerrainVertex[] Render()
        {
            ActorModelMesh.Build(world, player, npcs, predator, world.PlayerPosition, 0d, 0f, true,
                out var vertices, out var indices);
            check(indices.All(index => index < vertices.Length), "Player jump rendering keeps valid actor mesh indices");
            return vertices;
        }
    }

    private static (Vector3 Position, float HeightAboveStart, float Travel, float Velocity, float Stamina, bool Airborne) Simulate(double step, double duration)
    {
        var world = WorldGenerator.Generate(); var camera = new Camera3D(); var start = world.PlayerPosition;
        var remaining = duration; var first = true;
        while (remaining > 0.000000001)
        {
            var dt = Math.Min(step, remaining);
            Frame(world, camera, Idle with { Forward = true, JumpPressed = first }, dt);
            first = false; remaining -= dt;
        }
        return (world.PlayerPosition, world.PlayerPosition.Y - start.Y,
            new Vector2(world.PlayerPosition.X - start.X, world.PlayerPosition.Z - start.Z).Length(),
            world.Jump.VerticalVelocity, world.Player.Stamina, world.Jump.IsAirborne);
    }

    private static void Frame(WorldState world, Camera3D camera, PlayerInput input, double deltaSeconds)
    {
        PlayerController.Update(world, camera, input, deltaSeconds);
        world.Update(deltaSeconds);
    }

    private static bool Near(float actual, float expected) => MathF.Abs(actual - expected) < 0.002f;
}
