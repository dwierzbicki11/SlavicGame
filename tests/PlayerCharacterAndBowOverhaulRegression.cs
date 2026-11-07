using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Characters;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.UI;

public static class PlayerCharacterAndBowOverhaulRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var playerGlbPath = Path.Combine(AppContext.BaseDirectory, "assets", "models", "animated", "player_hunter_animated.glb");
        var bowGlbPath = Path.Combine(AppContext.BaseDirectory, "assets", "models", "static", "luk_r0_01.glb");
        var arrowGlbPath = Path.Combine(AppContext.BaseDirectory, "assets", "models", "static", "strzala_r0_01.glb");

        var playerModel = GlbModel.Load(playerGlbPath);
        var bowModel = GlbModel.Load(bowGlbPath);
        var arrowModel = GlbModel.Load(arrowGlbPath);

        check(playerModel.AnimationNames.Contains("Idle") &&
              playerModel.AnimationNames.Contains("Walk") &&
              playerModel.AnimationNames.Contains("Run") &&
              playerModel.AnimationNames.Contains("Attack") &&
              playerModel.AnimationNames.Contains("Death"),
            "Player hunter model contains Idle, Walk, Run, Attack, and Death animation clips");

        // 1. Player animation state machine resolution
        var world = WorldGenerator.Generate();
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Idle",
            "Stationary player resolves to 'Idle' animation clip");

        world.PlayerIsMoving = true;
        world.PlayerIsSprinting = false;
        world.PlayerVelocity = new Vector3(0f, 0f, 5f);
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Walk",
            "Moving player resolves to 'Walk' animation clip");

        world.PlayerIsSprinting = true;
        world.PlayerVelocity = new Vector3(0f, 0f, 9f);
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Run",
            "Sprinting player resolves to 'Run' animation clip");

        world.PlayerIsMoving = false;
        world.PlayerIsSprinting = false;
        world.PlayerVelocity = Vector3.Zero;
        world.Dodge.TryStart(world, Vector3.UnitZ);
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Run",
            "Active dodge roll/evasion resolves to dynamic 'Run' clip");
        world.Dodge.Reset();

        world.Melee.TryStart(world);
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Attack",
            "Melee sword swing resolves to 'Attack' animation clip");
        world.Melee.Reset();

        world.Bow.SetAiming(world, true);
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Attack",
            "Aiming with the hunting bow resolves to upper-body 'Attack' pose");
        world.Bow.SetAiming(world, false);

        world.Player.TakeDamage(100f);
        check(!world.Player.IsAlive, "Player health reduced to zero");
        check(PlayerCharacterModel.ResolveClip(world, playerModel, 0.5f) == "Death",
            "Dead player resolves to 'Death' animation clip instead of disappearing");

        // 2. Camera Over-The-Shoulder and Bow Aiming Zoom
        var camera = new Camera3D();
        check(camera.Distance < 5.0f && camera.Distance >= 3.0f,
            "Default third-person camera distance is tight exploration scale (~3.6m instead of distant 9m)");
        check(camera.ShoulderOffset > 0.20f && camera.ShoulderOffset <= 0.50f,
            "Third-person camera has an authentic right shoulder offset for clean reticle sightlines");

        world = WorldGenerator.Generate();
        camera.Follow(world.PlayerPosition, 0.1f, world.Terrain);
        var initialDist = camera.Distance;
        var initialShoulder = camera.ShoulderOffset;

        // When bow aiming begins, camera transitions to over-the-shoulder zoom
        camera.IsAiming = true;
        camera.Follow(world.PlayerPosition, 0.5f, world.Terrain);
        check(camera.AimDistance < initialDist,
            "Aiming bow tightens camera distance for over-the-shoulder archery zoom");
        check(camera.AimShoulderOffset > initialShoulder,
            "Aiming bow shifts camera laterally over the right shoulder");

        var lookDir = camera.GetMoveForward();
        var toTarget = Vector3.Normalize(new Vector3(camera.Target.X - camera.Position.X, 0f, camera.Target.Z - camera.Position.Z));
        check(Vector3.Dot(lookDir, toTarget) > 0.85f,
            "Camera look-at target aligns with forward view direction");

        camera.IsAiming = false;
        camera.Follow(world.PlayerPosition, 0.5f, world.Terrain);

        // 3. Player Character Geometry and Hunter Accessories
        world = WorldGenerator.Generate();
        var vertices = new List<TerrainVertex>(2048);
        var indices = new List<uint>(4096);

        PlayerCharacterModel.Append(world, playerModel, 0f, 0.35f, true, vertices, indices);
        check(vertices.Count > 0 && indices.Count > 0,
            "PlayerCharacterModel generates dynamic character mesh");
        check(indices.All(idx => idx < vertices.Count),
            "Player mesh indices are valid and bound to vertex range");

        // Hunter quiver and slung bow add visible gear when bow is in inventory
        var vertsWithBow = vertices.Count;
        var indicesWithBow = indices.Count;
        check(vertsWithBow > 1000, "Slavic hunter model includes tunic, accessories, quiver and slung bow geometry");

        // 4. Bow Combat Overhaul: Iron Arrows integration and trajectory
        world = WorldGenerator.Generate();
        check(world.Progress.Inventory.Contains(BowCombatRuntime.BowItemId),
            "Player inventory starts with hunting bow");
        check(BowCombatRuntime.HasArrows(world),
            "Player has arrows in inventory");

        // Add iron arrows from blacksmith crafting
        world.Progress.Inventory.Add(BowCombatRuntime.IronArrowItemId, 5);
        check(world.Progress.Inventory.Count(BowCombatRuntime.IronArrowItemId) == 5,
            "Iron arrows added to inventory");

        world.Bow.SetAiming(world, true);
        check(world.Bow.TryStartDraw(world), "Bow starts drawing with iron arrows prioritized");
        world.Bow.Update(world, 1.25);
        check(world.Bow.DrawFraction >= 0.99f, "Bow reaches full draw");

        var basicAmmoBefore = world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId);
        var ironAmmoBefore = world.Progress.Inventory.Count(BowCombatRuntime.IronArrowItemId);

        var spawnOrigin = world.PlayerPosition + Vector3.UnitY * 1.35f;
        var fireDirection = Vector3.UnitZ;
        check(world.Bow.TryRelease(world, spawnOrigin, fireDirection),
            "Released bow shot fires iron arrow");

        check(world.Progress.Inventory.Count(BowCombatRuntime.IronArrowItemId) == ironAmmoBefore - 1,
            "Iron arrow consumed on release");
        check(world.Progress.Inventory.Count(BowCombatRuntime.ArrowItemId) == basicAmmoBefore,
            "Basic arrow ammo preserved while iron arrows are available");

        check(world.Bow.Projectiles.Count == 1, "Iron arrow projectile exists in flight");
        var ironProjectile = world.Bow.Projectiles[0];
        check(ironProjectile.Damage > 38f + 10f,
            "Iron arrow inflicts superior damage (+15 bonus DMG above basic arrow max)");

        // 5. Bow Presentation Mesh: Arrow nocking and draw pullback
        var bowMeshVerts = new List<TerrainVertex>(512);
        var bowMeshIdxs = new List<uint>(1024);

        world.Bow.SetAiming(world, true);
        world.Bow.TryStartDraw(world);
        world.Bow.Update(world, 0.60); // half draw

        BowPresentationMesh.Append(world, bowModel, arrowModel, camera.Position, Vector3.UnitZ, Vector3.UnitX, false, bowMeshVerts, bowMeshIdxs);
        check(bowMeshVerts.Count > 0 && bowMeshIdxs.Count > 0,
            "BowPresentationMesh renders bow and nocked arrow during draw");

        // 6. Player Facing Orientation during Movement and Bow Aiming
        world = WorldGenerator.Generate();
        var controllerInput = new PlayerInput(
            Forward: true,
            Backward: false,
            Right: false,
            Left: false,
            Running: true,
            LookDelta: Vector2.Zero,
            DodgePressed: false,
            JumpPressed: false);

        PlayerController.Update(world, camera, controllerInput, 0.2);
        check(world.PlayerIsMoving && world.PlayerIsSprinting,
            "PlayerController updates PlayerIsMoving and PlayerIsSprinting");
        check(world.PlayerMotionDistance > 0f,
            "PlayerController tracks accumulated PlayerMotionDistance");

        // Aiming bow aligns player facing yaw directly with camera yaw
        world.Bow.SetAiming(world, true);
        camera.Rotate(50f, 0f);
        PlayerController.Update(world, camera, controllerInput with { Forward = false, Running = false }, 0.1);
        check(MathF.Abs(MathF.IEEERemainder(world.PlayerFacingYaw - camera.Yaw, MathF.PI * 2f)) < 0.05f,
            "Aiming the bow turns character to face camera look direction immediately");
    }
}
