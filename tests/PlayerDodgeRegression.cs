using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class PlayerDodgeRegression
{
    private static readonly PlayerInput Idle = new(false, false, false, false, false, Vector2.Zero);

    public static void Run(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        var camera = new Camera3D();
        var start = world.PlayerPosition;
        PlayerController.Update(world, camera, Idle with { Forward = true, DodgePressed = true }, 0.1);
        check(world.Dodge.IsActive && Near(Travel(world, start), 1.2f) && world.Player.Stamina == 80f,
            "Alt starts a directional dodge and pays stamina once without walking or sprinting on top");
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == 80f,
            "Repeated dodge requests during movement cannot charge stamina again");
        PlayerController.Update(world, camera, Idle, 0.15);
        check(!world.Dodge.IsActive && Near(Travel(world, start), 3f) && Near((float)world.Dodge.CooldownRemaining, 0.55f),
            "A dodge travels three metres and enters recovery at the movement boundary");
        PlayerController.Update(world, camera, Idle, 0.54);
        check(!world.Dodge.TryStart(world, Vector3.UnitX), "Recovery prevents an early second dodge");
        PlayerController.Update(world, camera, Idle, 0.01);
        check(world.Dodge.TryStart(world, Vector3.UnitX), "A fresh dodge is available after the complete cooldown");

        world = WorldGenerator.Generate(); camera = new Camera3D(); start = world.PlayerPosition;
        PlayerController.Update(world, camera, Idle with { DodgePressed = true }, 0.25);
        check(Near(Travel(world, start), 3f) &&
              Vector3.Dot(HorizontalDirection(world.PlayerPosition - start), camera.GetMoveForward()) < -0.99f,
            "Alt without movement performs a backward dodge relative to the camera");

        world = WorldGenerator.Generate(); camera = new Camera3D(); start = world.PlayerPosition;
        PlayerController.Update(world, camera,
            Idle with { Forward = true, Right = true, LookDelta = new Vector2(90f, 0f), DodgePressed = true }, 0.1);
        var committed = HorizontalDirection(world.PlayerPosition - start);
        var expected = Vector3.Normalize(camera.GetMoveForward() + camera.GetMoveRight());
        check(Vector3.Dot(committed, expected) > 0.999f && Near(Travel(world, start), 1.2f),
            "Diagonal dodge is normalized and uses the current frame's mouse look");
        var yaw = camera.Yaw;
        PlayerController.Update(world, camera, Idle with { Left = true, LookDelta = new Vector2(100f, 0f) }, 0.15);
        check(camera.Yaw != yaw && Near(Travel(world, start), 3f) &&
              Vector3.Dot(HorizontalDirection(world.PlayerPosition - start), committed) > 0.999f,
            "Mouse look remains responsive while the dodge direction stays committed");

        var slow = TimedMovement(0.1, 0.4, running: true);
        var fast = TimedMovement(1.0 / 120.0, 0.4, running: true);
        check(Vector3.Distance(slow.Position, fast.Position) < 0.002f && Near(slow.Travel, 4.35f) &&
              Near(slow.Stamina, 76.4f) && Near(fast.Stamina, slow.Stamina) && Near(slow.Cooldown, fast.Cooldown),
            "10 and 120 FPS preserve dodge distance, leftover sprint time, stamina and recovery");
        slow = TimedMovement(0.1, 1.0, running: false);
        fast = TimedMovement(1.0 / 120.0, 1.0, running: false);
        check(Near(slow.Stamina, 84.5f) && Near(fast.Stamina, slow.Stamina) &&
              slow.Cooldown == 0f && fast.Cooldown == 0f,
            "Post-dodge stamina delay and regeneration agree across frame rates");

        world = WorldGenerator.Generate(); camera = new Camera3D(); start = world.PlayerPosition;
        world.Player.SetState(100f, 19f);
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == 19f &&
              world.Dodge.HudText(world).Contains("ZA MALO STAMINY", StringComparison.Ordinal),
            "Insufficient stamina rejects the dodge and gives HUD feedback");
        PlayerController.Update(world, camera, Idle with { Forward = true, DodgePressed = true }, 0.1);
        check(!world.Dodge.IsActive && Near(Travel(world, start), 0.5f),
            "A failed dodge leaves ordinary walking available");
        world.Player.SetState(100f, 20f);
        check(world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == 0f,
            "Exactly the dodge cost is sufficient and cannot produce negative stamina");

        world = WorldGenerator.Generate(); start = world.PlayerPosition;
        check(!world.Dodge.TryStart(world, Vector3.Zero) &&
              !world.Dodge.TryStart(world, new Vector3(float.NaN, 0f, 1f)) && world.Player.Stamina == 100f,
            "Invalid dodge directions do not spend stamina");
        PlayerController.Update(world, new Camera3D(), Idle with { DodgePressed = true }, double.NaN);
        PlayerController.Update(world, new Camera3D(), Idle with { DodgePressed = true }, -1d);
        check(!world.Dodge.IsActive && world.Player.Stamina == 100f && world.PlayerPosition == start,
            "Invalid frame time cannot start or move a dodge");

        world = WorldGenerator.Generate();
        var wall = world.Obstacles.Single(obstacle => obstacle.Id == "village-north-00-collision");
        var alongX = wall.HalfSize.X < wall.HalfSize.Y;
        var direction = alongX ? Vector3.UnitX : Vector3.UnitZ;
        var halfSize = alongX ? wall.HalfSize.X : wall.HalfSize.Y;
        start = wall.Position - direction * (halfSize + world.PlayerRadius + 0.2f);
        world.SetPlayerPosition(start);
        start = world.PlayerPosition;
        check(world.Dodge.TryStart(world, direction), "Dodge starts in front of a real thin world obstacle");
        world.Dodge.Update(world, 0.25);
        check(Vector3.Dot(world.PlayerPosition - wall.Position, direction) <= -halfSize - world.PlayerRadius &&
              !wall.IntersectsCircle(new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z), world.PlayerRadius),
            "One long frame cannot dodge through a thin world wall");
        check(Near(world.PlayerPosition.Y, world.Terrain.SampleHeight(world.PlayerPosition)),
            "Collision-constrained dodge remains grounded on the terrain");

        world = WorldGenerator.Generate();
        var halfWidth = (world.Terrain.Width - 1) * world.Terrain.CellSize * 0.5f;
        world.SetPlayerPosition(new Vector3(halfWidth - 0.5f, 0f, 0f));
        world.Dodge.TryStart(world, Vector3.UnitX); world.Dodge.Update(world, 0.25);
        check(world.PlayerPosition.X <= halfWidth, "Dodge respects the existing map bounds");

        world = WorldGenerator.Generate();
        world.SetPlayerPosition(new Vector3(WaterLandscape.CenterX(0f), 0f, 0f));
        check(!world.Dodge.TryStart(world, Vector3.UnitZ) && world.Player.Stamina == 100f,
            "Deep river water blocks dodge without charging stamina");

        world = WorldGenerator.Generate();
        var ammo = world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);
        world.Bow.SetAiming(world, true); world.Bow.TryStartDraw(world); world.Bow.Update(world, 0.5);
        check(world.Dodge.TryStart(world, Vector3.UnitX) && !world.Bow.IsAiming && !world.Bow.IsDrawing,
            "Dodge cancels an active bow aim and draw");
        world.Bow.SetAiming(world, true);
        check(!world.Bow.IsAiming && !world.Bow.TryRelease(world, world.PlayerPosition, Vector3.UnitZ) &&
              world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == ammo && world.Bow.Projectiles.Count == 0,
            "Held aim and a simultaneous release cannot fire or consume an arrow during dodge");
        check(!world.Melee.TryStart(world) && world.Melee.Controller.State == MeleeAttackState.Free,
            "Dodge movement cannot overlap a new melee attack");
        world.Progress.SetFlag(SpellLessons.LearnedFlag("spell.spark"));
        check(!world.Magic.TryStart(world, Vector3.UnitZ) &&
              world.Rituals.Validate(world).Failure == RitualStartFailure.PlayerUnavailable && world.Player.Stamina == 80f,
            "Dodge movement blocks casting and rituals without an extra stamina charge");
        world.Dodge.Update(world, 0.25); world.Bow.SetAiming(world, true);
        check(world.Bow.IsAiming && world.Bow.TryStartDraw(world), "Bow aim can resume during dodge recovery");

        world = WorldGenerator.Generate();
        check(world.Melee.TryStart(world), "Melee fixture starts a windup");
        var stamina = world.Player.Stamina;
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == stamina,
            "An existing melee attack blocks dodge without charging its cost");
        world = WorldGenerator.Generate(); world.Progress.SetFlag(SpellLessons.LearnedFlag("spell.spark"));
        check(world.Magic.TryStart(world, Vector3.UnitZ), "Magic fixture starts a known spell");
        stamina = world.Player.Stamina;
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == stamina,
            "A running incantation blocks dodge without charging its cost");

        world = WorldGenerator.Generate();
        check(world.Cinematics.TryStart(world, CinematicPlayer.Arrival), "Cinematic fixture starts arrival");
        check(!world.Dodge.TryStart(world, Vector3.UnitX) && world.Player.Stamina == 100f,
            "Cinematics block dodge without consuming stamina");
        world.Cinematics.Reset(); world.Dodge.TryStart(world, Vector3.UnitX); start = world.PlayerPosition;
        world.Cinematics.TryStart(world, CinematicPlayer.Arrival); world.Dodge.Update(world, 0.1);
        world.Cinematics.Reset(); world.Dodge.Update(world, 0.1);
        check(!world.Dodge.IsActive && world.PlayerPosition == start,
            "An automatically started cinematic interrupts dodge instead of resuming it afterward");

        world = WorldGenerator.Generate();
        world.SetPlayerPosition(LootContainerRuntime.Position(world)!.Value);
        check(world.Loot.TryOpenNearest(world), "Dodge UI fixture opens the market chest");
        start = world.PlayerPosition;
        PlayerController.Update(world, new Camera3D(), Idle with { Right = true, DodgePressed = true }, 0.1);
        check(!world.Dodge.IsActive && world.PlayerPosition == start && world.Player.Stamina == 100f,
            "An open storage screen blocks both direct movement and dodge input");
        world.Loot.Close(); world.Dodge.TryStart(world, Vector3.UnitX); world.Loot.TryOpenNearest(world);
        world.Dodge.Update(world, 0.1); world.Loot.Close(); world.Dodge.Update(world, 0.1);
        check(!world.Dodge.IsActive && world.PlayerPosition == start,
            "Opening UI interrupts a transient dodge so it cannot resume after closing");

        world = WorldGenerator.Generate(); world.Dodge.TryStart(world, Vector3.UnitX); world.Dodge.Update(world, 0.1);
        var saved = SaveGameService.Serialize(world);
        SaveGameService.Restore(world, saved); start = world.PlayerPosition;
        PlayerController.Update(world, new Camera3D(), Idle, 0.2);
        check(!world.Dodge.IsActive && world.Dodge.CooldownRemaining == 0d && world.PlayerPosition == start,
            "Loading into the same world discards transient dodge direction and cooldown");
        world.Dodge.TryStart(world, Vector3.UnitX); start = world.PlayerPosition; world.Player.TakeDamage(100f);
        PlayerController.Update(world, new Camera3D(), Idle, 0.2);
        check(!world.Dodge.IsActive && world.PlayerPosition == start,
            "Death stops a pending dodge without a final displacement");

        var (fightWorld, enemy) = Fight();
        enemy.Update(fightWorld, 0.1);
        PlayerController.Update(fightWorld, new Camera3D(), Idle with { Right = true, DodgePressed = true }, 0.25);
        enemy.Update(fightWorld, 0.31);
        check(fightWorld.Player.Health == 100f && enemy.Attack.Phase == EnemyAttackPhase.Recovery,
            "Dodge physically escapes the predator's telegraphed melee contact");
        (fightWorld, enemy) = Fight();
        fightWorld.Dodge.TryStart(fightWorld, Vector3.UnitZ); fightWorld.Dodge.Update(fightWorld, 0.01);
        enemy.Update(fightWorld, 0.41);
        check(fightWorld.Dodge.IsActive && fightWorld.Player.Health == 92f,
            "A dodge still inside strike reach takes damage: avoidance uses position, without invulnerability");
    }

    private static (Vector3 Position, float Travel, float Stamina, float Cooldown) TimedMovement(double step, double duration, bool running)
    {
        var world = WorldGenerator.Generate(); var camera = new Camera3D(); var start = world.PlayerPosition;
        var remaining = duration; var first = true;
        while (remaining > 0.000000001)
        {
            var dt = Math.Min(step, remaining);
            PlayerController.Update(world, camera, Idle with { Forward = true, Running = running, DodgePressed = first }, dt);
            first = false; remaining -= dt;
        }
        return (world.PlayerPosition, Travel(world, start), world.Player.Stamina, (float)world.Dodge.CooldownRemaining);
    }

    private static float Travel(WorldState world, Vector3 start) =>
        new Vector2(world.PlayerPosition.X - start.X, world.PlayerPosition.Z - start.Z).Length();
    private static Vector3 HorizontalDirection(Vector3 direction) => Vector3.Normalize(new Vector3(direction.X, 0f, direction.Z));
    private static bool Near(float actual, float expected) => MathF.Abs(actual - expected) < 0.002f;

    private static (WorldState, EnemyAgent) Fight()
    {
        var world = WorldGenerator.Generate();
        var position = new Vector3(0f, world.Terrain.SampleHeight(Vector3.Zero), 0f);
        var enemy = new EnemyAgent("dodge-target", position);
        world.SetPlayerPosition(position + Vector3.UnitX * 1.2f);
        enemy.Restore(new EnemySnapshot(enemy.Id, position, enemy.MaxHealth, EnemyState.Chase));
        enemy.Update(world, 0d);
        return (world, enemy);
    }
}
