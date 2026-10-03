using System.Numerics;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Audio;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

public static class MagicCinematicRegression
{
    public static void Run(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        var magic = world.Magic;
        check(MagicLanguage.Words.Count == 6 && MagicLanguage.Words.Select(word => word.Id).Distinct().Count() == 6,
            "Incantation lexicon has six unique fictional words");
        check(SpellCasting.Spells.All(spell => spell.Phrase.Words.Count == 2 && spell.Incantation == spell.Phrase.Text),
            "Playable spells use structured two-word incantations");
        check(CinematicCatalog.All.Count == 8 && CinematicCatalog.All.Select(scene => scene.Id).Distinct().Count() == 8,
            "Eight unique cinematic definitions are runtime-ready");
        check(CinematicCatalog.All.SelectMany(scene => scene.Shots).All(shot => shot.Voice is not null),
            "Every cinematic shot has explicit emotional voice direction");
        var fearfulPrompt = VoiceDirectionPrompts.Build(new VoiceDirection(VoiceEmotion.Fearful, 0.8f, 1f));
        check(fearfulPrompt.Contains("natural Polish", StringComparison.Ordinal) &&
              fearfulPrompt.Contains("fearful", StringComparison.OrdinalIgnoreCase) &&
              fearfulPrompt.Contains("never like a navigation system", StringComparison.Ordinal),
            "TTS prompt requests natural emotional delivery instead of robotic narration");
        check(CinematicCatalog.TryGet("ritual-preparation", out var ritualPreparation) &&
              ritualPreparation.Shots.All(shot => shot.Space == CinematicSpace.PlayerRelative),
            "Quest cinematics can use player-relative camera shots");
        check(world.Progress.Tracking.Tracks.Count(t => t.MagicSignature is not null) >= 2, "Playable supernatural traces exist");
        magic.SelectNext();
        check(!magic.TryStart(world, Vector3.UnitZ) && world.Player.Stamina == 100, "Full-health heal costs nothing");
        world.Player.TakeDamage(40);
        check(magic.TryStart(world, Vector3.UnitZ), "Heal starts");
        check(magic.Message == "ZIVA", "Incantation begins with its first spoken word");
        check(world.Player.Stamina == 65 && world.Player.Health == 60, "Cost paid once before cast resolves");
        check(!magic.TryStart(world, Vector3.UnitZ), "Casting cannot stack");
        magic.Update(world, 0.5);
        check(magic.Message == "ZIVA DAR" && magic.CastingProgress > 0.5, "Incantation reveals the next word during windup");
        check(world.Player.Health == 60, "Windup does not heal early");
        magic.Update(world, 0.5);
        check(world.Player.Health == 85 && !magic.IsCasting, "Completed incantation heals");
        check(!magic.TryStart(world, Vector3.UnitZ), "Cooldown blocks repeats");
        var legacy = SaveGameService.Capture(world) with { Magic = null, Tracks = [] };
        var oldWorld = WorldGenerator.Generate();
        SaveGameService.Restore(oldWorld, legacy);
        check(oldWorld.Magic.Selected == 0 && oldWorld.Progress.Tracking.Tracks.Count >= 2,
            "Existing version-three saves gain magic defaults and trace locations");
        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);
        check(restored.Magic.Selected == 1 && restored.Magic.Cooldown > 0, "Save preserves selected spell and cooldown");
        magic.Update(world, 10);
        world.Player.Restore(); world.Player.TakeDamage(30);
        check(magic.TryStart(world, Vector3.UnitZ), "Second heal starts after cooldown");
        world.Player.TakeDamage(1);
        magic.Update(world, 1);
        check(world.Player.Health == 69 && !magic.IsCasting, "Damage interrupts magic without healing");
        magic.Restore(null); world.Player.SetState(100, 0);
        check(!magic.TryStart(world, Vector3.UnitZ), "Insufficient stamina rejects casting");
        world.Player.Restore();
        magic.SelectNext(); magic.SelectNext();
        check(magic.TryStart(world, Vector3.UnitZ), "Reveal starts");
        magic.Update(world, 1);
        check(magic.RevealRemaining > 0, "Reveal creates timed visibility window");
        check(world.Progress.Tracking.Tracks.All(t => !t.Discovered), "Reveal does not award evidence or discover tracks");
        magic.Update(world, 13);
        check(magic.RevealRemaining == 0, "Reveal expires");
        var hut = world.Obstacles.First(o => o.Id == "village-hut-a");
        var center = hut.Position + Vector3.UnitY * 2;
        check(!SpellCasting.HasLineOfSight(world, center - Vector3.UnitX * 12, center + Vector3.UnitX * 12), "Spells cannot pass through hut collider");

        magic.Restore(null);
        var enemy = world.Enemies[0];
        // Find a clear nearby point, then aim at the enemy from that position.
        bool hitTested = false;
        for (var angle = 0; angle < 16 && !hitTested; angle++)
        {
            var offset = new Vector3(MathF.Cos(angle * MathF.Tau / 16), 0, MathF.Sin(angle * MathF.Tau / 16)) * 4;
            world.SetPlayerPosition(enemy.Position + offset);
            var from = world.PlayerPosition + Vector3.UnitY * 1.1f;
            var to = enemy.Position + Vector3.UnitY * 0.85f;
            if (!SpellCasting.HasLineOfSight(world, from, to)) continue;
            check(magic.TryStart(world, to - from), "Spark starts with nearby target");
            magic.Update(world, 1);
            check(enemy.Health == 38, "Spark applies damage exactly once");
            magic.Update(world, 1);
            check(enemy.Health == 38, "Resolved spark cannot hit repeatedly");
            hitTested = true;
        }
        check(hitTested, "Spark target scenario exercised");
        world.SetPlayerPosition(Vector3.Zero);
        magic.Restore(null);
        var scenes = world.Cinematics;
        check(scenes.TryStart(world, CinematicPlayer.Arrival), "Arrival cinematic starts");
        check(!magic.TryStart(world, Vector3.UnitZ), "Cinematic blocks casting");
        check(!scenes.TryStart(world, CinematicPlayer.Shrine), "Cinematics cannot overlap");
        scenes.Update(world, 2);
        check(scenes.IsPlaying && scenes.CameraPosition.Y > world.Terrain.SampleHeight(scenes.CameraPosition), "Cinematic camera stays above terrain");
        scenes.Finish(world);
        check(!scenes.IsPlaying && world.Progress.HasFlag("cinematic.seen.arrival"), "Skip returns control and records seen state");
        check(!scenes.TryStart(world, CinematicPlayer.Arrival), "Seen cinematic does not replay");
        check(scenes.TryStart(world, CinematicPlayer.Shrine), "Second cinematic starts");
        scenes.Update(world, 100);
        check(!scenes.IsPlaying && world.Progress.HasFlag("cinematic.seen.shrine"), "Large delta completes every shot safely");
        SaveGameService.Restore(restored, SaveGameService.Serialize(world));
        check(!restored.Cinematics.TryStart(restored, CinematicPlayer.Arrival), "Seen state survives save/load");

        var relativeWorld = WorldGenerator.Generate();
        foreach (var livingEnemy in relativeWorld.Enemies)
            livingEnemy.TakeDamage(10000);
        relativeWorld.SetPlayerPosition(new Vector3(40, 0, -40));
        check(relativeWorld.Cinematics.TryStartById(relativeWorld, "ritual-preparation"),
            "Catalog cinematic starts by stable ID");
        relativeWorld.Cinematics.Update(relativeWorld, 0.25);
        check(relativeWorld.Cinematics.ActiveId == "ritual-preparation" &&
              MathF.Abs(relativeWorld.Cinematics.CameraPosition.X - 40) < 12 &&
              MathF.Abs(relativeWorld.Cinematics.CameraPosition.Z + 40) < 12 &&
              relativeWorld.Cinematics.CameraPosition.Y >
                  relativeWorld.Terrain.SampleHeight(relativeWorld.Cinematics.CameraPosition),
            "Player-relative cinematic camera follows the captured player anchor");
    }
}
