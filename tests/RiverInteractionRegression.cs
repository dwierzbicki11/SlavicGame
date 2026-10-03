using System.Numerics;
using SlavicGame.Engine.Audio;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

internal static class RiverInteractionRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var z = 0f;
        var x = WaterLandscape.CenterX(z);

        world.SetPlayerPosition(new Vector3(x, 0f, z));
        world.WaterInteraction.Reset(world.PlayerPosition);
        world.WaterInteraction.Update(world, 0.1);

        check(world.WaterInteraction.IsInWater,
            "Player standing inside the carved channel is detected as in water");

        world.SetPlayerPosition(new Vector3(
            WaterLandscape.CenterX(z + 1f),
            0f,
            z + 1f));
        world.WaterInteraction.Update(world, 0.2);

        check(world.WaterInteraction.MovementIntensity > 0.1f,
            "Moving through the river raises ripple intensity");

        check(world.WaterInteraction.WaterDepth > 0.9f,
            "River center exposes deep ford water to gameplay");
        check(world.WaterInteraction.MovementSpeedMultiplier < 0.7f &&
              !WaterInteractionState.CanSprintAtDepth(
                  world.WaterInteraction.WaterDepth),
            "Deep ford substantially slows movement and blocks sprint");
        check(world.WaterInteraction.Wetness > 0f,
            "Standing and moving in the river wets the player");

        world.SetPlayerPosition(new Vector3(
            WaterLandscape.CenterX(z + 2.2f),
            0f,
            z + 2.2f));
        world.WaterInteraction.Update(world, 0.25);

        check(world.WaterInteraction.SplashPulse > 0.4f,
            "Wading stride produces an explicit splash pulse");

        TerrainVertex[] rippleVertices = [];
        uint[] rippleIndices = [];
        WaterInteractionMesh.Append(
            world,
            0.5f,
            ref rippleVertices,
            ref rippleIndices);

        check(rippleVertices.Length > 0 &&
              rippleIndices.Length > 0 &&
              rippleIndices.All(index => index < rippleVertices.Length),
            "Moving player produces a valid low-cost ripple mesh");

        world.SetPlayerPosition(Vector3.Zero);
        world.WaterInteraction.Update(world, 0.5);

        rippleVertices = [];
        rippleIndices = [];
        WaterInteractionMesh.Append(
            world,
            1.0f,
            ref rippleVertices,
            ref rippleIndices);

        check(!world.WaterInteraction.IsInWater &&
              rippleVertices.Length == 0 &&
              rippleIndices.Length == 0,
            "Leaving the river disables player ripple geometry");

        var wetnessAfterLeaving = world.WaterInteraction.Wetness;
        world.WaterInteraction.Update(world, 10.0);
        check(world.WaterInteraction.Wetness < wetnessAfterLeaving &&
              world.WaterInteraction.Wetness > 0f,
            "Wet clothing dries gradually instead of clearing instantly");

        check(MathF.Abs(RiverAmbienceSynthesizer.DistanceToRiver(
                new Vector3(WaterLandscape.CenterX(0f), 0f, 0f))) < 0.001f,
            "River ambience distance is zero on the water ribbon");

        var near = RiverAmbienceSynthesizer.Attenuation(0f);
        var mid = RiverAmbienceSynthesizer.Attenuation(35f);
        var far = RiverAmbienceSynthesizer.Attenuation(100f);

        check(near > mid && mid > far && far == 0f,
            "River ambience attenuates monotonically with listener distance");

        var pcm = RiverAmbienceSynthesizer.Generate(0.35f, 7).Validate();
        check(pcm.SampleRate == 24000 &&
              pcm.Channels == 1 &&
              pcm.BitsPerSample == 16 &&
              pcm.Data.Length ==
                  (int)Math.Round(
                      RiverAmbienceSynthesizer.SampleRate *
                      RiverAmbienceSynthesizer.SegmentSeconds) * 2,
            "Procedural river ambience emits PCM16 mono 24 kHz with stable duration");

        check(pcm.Data.Any(value => value != 0),
            "Procedural river ambience contains audible non-silent samples");

        var splashPcm = RiverAmbienceSynthesizer.Generate(
            0.35f,
            7,
            1f).Validate();
        check(!splashPcm.Data.SequenceEqual(pcm.Data),
            "Footstep splash pulse changes the generated river PCM");

        var dryVitals = new PlayerVitals();
        var wetVitals = new PlayerVitals();
        dryVitals.UpdateStamina(true, 1.0);
        wetVitals.UpdateStamina(
            true,
            1.0,
            WaterInteractionState.StaminaDrainMultiplierForDepth(0.55f),
            1f);
        check(wetVitals.Stamina < dryVitals.Stamina,
            "Sprinting through shallow water drains more stamina than dry sprinting");

        check(
            WaterInteractionState.MovementSpeedMultiplierForDepth(0f) == 1f &&
            WaterInteractionState.MovementSpeedMultiplierForDepth(0.35f) <
                WaterInteractionState.MovementSpeedMultiplierForDepth(0.08f) &&
            WaterInteractionState.MovementSpeedMultiplierForDepth(1.2f) <
                WaterInteractionState.MovementSpeedMultiplierForDepth(0.35f),
            "Wading movement penalty grows monotonically with water depth");

        check(
            WaterInteractionState.StaminaRecoveryMultiplier(0f, 0f) == 1f &&
            WaterInteractionState.StaminaRecoveryMultiplier(0.7f, 1f) < 0.5f,
            "Deep water plus soaked clothing slows stamina recovery");

        TerrainVertex[] waterAtA = [];
        uint[] waterIndicesA = [];
        WaterLandscape.AppendSurface(
            world.Terrain,
            0f,
            new Vector3(WaterLandscape.CenterX(0f), 0f, 0f),
            120f,
            ref waterAtA,
            ref waterIndicesA);

        TerrainVertex[] waterAtB = [];
        uint[] waterIndicesB = [];
        WaterLandscape.AppendSurface(
            world.Terrain,
            0.6f,
            new Vector3(WaterLandscape.CenterX(0f), 0f, 0f),
            120f,
            ref waterAtB,
            ref waterIndicesB);

        check(waterAtA.Length == waterAtB.Length &&
              waterAtA.Length > 100 &&
              waterAtA.Zip(waterAtB).Any(pair =>
                  Vector3.DistanceSquared(pair.First.Color, pair.Second.Color) > 0.000001f),
            "Water and foam presentation visibly changes over time");
    }
}
