using System.Numerics;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Audio;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.Quest;
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
        check(VoiceUsage.ParseScope(null) == VoiceUsageScope.SpellsOnly &&
              VoiceUsage.ParseScope("spells") == VoiceUsageScope.SpellsOnly &&
              VoiceUsage.ParseScope("all") == VoiceUsageScope.All,
            "Local TTS defaults to protagonist spells and requires explicit all scope for cinematics");
        var calmStyle = ChatterboxProsody.FromDirection(new VoiceDirection(VoiceEmotion.Calm, 0.4f, 1f));
        var fearfulStyle = ChatterboxProsody.FromDirection(new VoiceDirection(VoiceEmotion.Fearful, 0.8f, 1f));
        check(calmStyle.EmotionKey == "calm" && fearfulStyle.EmotionKey == "fearful" &&
              fearfulStyle.Exaggeration > calmStyle.Exaggeration &&
              fearfulStyle.CfgWeight <= calmStyle.CfgWeight,
            "Free local TTS maps stronger emotions to more expressive Chatterbox prosody");
        check(CinematicCatalog.TryGet("ritual-preparation", out var ritualPreparation) &&
              ritualPreparation.Shots.All(shot => shot.Space == CinematicSpace.PlayerRelative),
            "Quest cinematics can use player-relative camera shots");
        check(world.Progress.Tracking.Tracks.Count(t => t.MagicSignature is not null) >= 2, "Playable supernatural traces exist");

        check(SpellLessons.All.Count == 3 &&
              SpellLessons.All.Select(lesson => lesson.SpellId).Distinct().Count() == 3,
            "Every playable spell has one learning lesson");
        check(SpellLessons.All.All(lesson => !SpellLessons.IsLearned(world, lesson.SpellId)),
            "New game starts with spell knowledge locked");
        check(!magic.TryStart(world, Vector3.UnitZ) && magic.Message.Contains("NIE ZNASZ", StringComparison.Ordinal),
            "Unknown spell cannot be cast");

        world.SetPlayerPosition(new Vector3(-85, 0, 55));
        world.SpellLearning.RefreshMessage(world);
        check(world.SpellLearning.TryLearnCurrent(world) &&
              SpellLessons.IsLearned(world, "spell.spark") &&
              magic.Current.Id == "spell.spark",
            "Reaching the shrine teaches Spark through the contextual lesson");

        world.SetPlayerPosition(new Vector3(0, 0, -85));
        world.Player.TakeDamage(35);
        world.SpellLearning.Update(world);
        var bandagesBeforeLesson = world.Progress.Inventory.Count("simple-bandage");
        check(world.SpellLearning.TryLearnCurrent(world) &&
              SpellLessons.IsLearned(world, "spell.mend") &&
              world.Progress.Inventory.Count("simple-bandage") == bandagesBeforeLesson - 1,
            "Mend requires a real wound and consumes one practice bandage");

        world.SetPlayerPosition(new Vector3(-85, 0, 55));
        check(!world.SpellLearning.TryLearnCurrent(world) &&
              !SpellLessons.IsLearned(world, "spell.reveal-trace"),
            "Reveal Trace remains locked without identity and anchor knowledge");

        var learningQuest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        learningQuest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.keepsake-owner",
            VerticalSliceBootstrap.ContractQuestId,
            KnowledgeKind.ConfirmedFact,
            "Pamiatka nalezala do zaginionego.",
            "missing-family"));
        world.Progress.SetFlag(VerticalSliceRituals.AnchorKnowledgeFlag);
        check(world.SpellLearning.TryLearnCurrent(world) &&
              SpellLessons.IsLearned(world, "spell.reveal-trace") &&
              magic.Current.Id == "spell.reveal-trace",
            "Reveal Trace unlocks only after confirmed identity and anchor knowledge");

        var learnedJson = SaveGameService.Serialize(world);
        var learnedRestored = WorldGenerator.Generate();
        SaveGameService.Restore(learnedRestored, learnedJson);
        check(SpellLessons.All.All(lesson => SpellLessons.IsLearned(learnedRestored, lesson.SpellId)),
            "Learned spells persist through save and load");

        world.Player.Restore();
        magic.SelectSpell("spell.spark");
        magic.SelectNext(world);
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
        magic.SelectNext(world); magic.SelectNext(world);
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

        var ritualWorld = WorldGenerator.Generate();
        var ritualRuntime = ritualWorld.Rituals;
        var ritualQuest = ritualWorld.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);

        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.NotLearned,
            "Release-bound-echo ritual is gated by learned knowledge");

        ritualWorld.Progress.SetFlag(VerticalSliceRituals.LearnedFlag);
        ritualWorld.Progress.SetFlag(VerticalSliceRituals.IdentitySignFlag);
        ritualWorld.Progress.SetFlag(VerticalSliceRituals.BoundarySignFlag);
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.WrongLocation,
            "Known ritual still requires the authored ritual location");

        ritualWorld.SetPlayerPosition(new Vector3(-85, 0, 55));
        ritualWorld.Time.SetTimeOfDay(12);
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.WrongTime,
            "Release-bound-echo refuses daylight without consuming anything");

        ritualWorld.Time.SetTimeOfDay(23);
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.MissingItem,
            "Ritual reports missing critical items");

        ritualWorld.Progress.Inventory.Add("missing-person-keepsake");
        ritualWorld.Progress.Inventory.Add("ritual-thread");
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.MissingEvidence,
            "Ritual requires confirmed identity evidence");

        ritualQuest.SetPhase(QuestPhase.Preparation);
        ritualQuest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.keepsake-owner",
            VerticalSliceBootstrap.ContractQuestId,
            KnowledgeKind.ConfirmedFact,
            "Pamiatka nalezala do zaginionego.",
            "missing-family"));
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.MissingKnowledge,
            "Ritual requires knowledge that the keepsake anchors the apparition");

        ritualWorld.Progress.SetFlag(VerticalSliceRituals.AnchorKnowledgeFlag);
        var firstRitualStart = ritualRuntime.TryStart(ritualWorld);
        check(firstRitualStart.Started && ritualRuntime.CurrentStep == RitualStep.DefineArea,
            "Validated release-bound-echo begins its seven-step sequence");

        TerrainVertex[] ritualVisualVertices = [];
        uint[] ritualVisualIndices = [];
        MagicEffectMesh.Append(ritualWorld, ref ritualVisualVertices, ref ritualVisualIndices);
        check(ritualVisualVertices.Length > 0 &&
              ritualVisualIndices.Length > 0 &&
              ritualVisualVertices.Length < 512,
            "Active ritual generates a bounded low-poly circle and staged glyph mesh");

        check(ritualWorld.Progress.Inventory.Contains("missing-person-keepsake") &&
              ritualWorld.Progress.Inventory.Contains("ritual-thread"),
            "Starting ritual does not consume critical quest items");

        ritualWorld.Player.TakeDamage(1);
        ritualRuntime.Update(ritualWorld, 0.25);
        check(!ritualRuntime.IsPerforming &&
              ritualWorld.Progress.Inventory.Contains("missing-person-keepsake") &&
              ritualWorld.Progress.Inventory.Contains("ritual-thread") &&
              !ritualWorld.Progress.HasFlag(VerticalSliceRituals.ReleasedFlag),
            "Damage interrupts ritual safely without consuming items or resolving apparition");

        ritualWorld.Player.Restore();
        check(ritualRuntime.TryStart(ritualWorld).Started, "Interrupted ritual can be retried");
        ritualRuntime.Update(ritualWorld, 2.1);
        check(ritualRuntime.IsPerforming && ritualRuntime.CurrentStep == RitualStep.RecognitionSign,
            "Ritual advances deterministically through authored steps");
        ritualRuntime.Update(ritualWorld, 10);
        check(!ritualRuntime.IsPerforming &&
              ritualWorld.Progress.HasFlag(VerticalSliceRituals.ReleasedFlag) &&
              ritualQuest.Resolution == QuestResolution.RitualClosure &&
              ritualWorld.Progress.Inventory.Count("missing-person-keepsake") == 0 &&
              ritualWorld.Progress.Inventory.Count("ritual-thread") == 0 &&
              ritualRuntime.ConsumeCompletionSignal(),
            "Completed ritual commits items once and persists the ritual-closure outcome");
        check(ritualRuntime.Validate(ritualWorld).Failure == RitualStartFailure.AlreadyResolved,
            "Resolved apparition cannot run release-bound-echo twice");

        ritualVisualVertices = [];
        ritualVisualIndices = [];
        MagicEffectMesh.Append(ritualWorld, ref ritualVisualVertices, ref ritualVisualIndices);
        check(ritualRuntime.CompletionGlowRemaining > 0 &&
              ritualVisualVertices.Length > 0 &&
              ritualVisualVertices.Length < 512,
            "Successful ritual leaves a short bounded completion glow");

        ritualRuntime.Update(ritualWorld, 3.0);
        ritualVisualVertices = [];
        ritualVisualIndices = [];
        MagicEffectMesh.Append(ritualWorld, ref ritualVisualVertices, ref ritualVisualIndices);
        check(ritualRuntime.CompletionGlowRemaining == 0 &&
              ritualVisualVertices.Length == 0 &&
              ritualVisualIndices.Length == 0,
            "Ritual completion geometry expires instead of accumulating in the world");

        var ritualRestored = WorldGenerator.Generate();
        SaveGameService.Restore(ritualRestored, SaveGameService.Serialize(ritualWorld));
        check(ritualRestored.Progress.HasFlag(VerticalSliceRituals.ReleasedFlag) &&
              ritualRestored.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId).Resolution ==
                  QuestResolution.RitualClosure &&
              ritualRestored.Progress.Inventory.Count("missing-person-keepsake") == 0,
            "Ritual closure outcome and consumed anchor survive save/load");
    }
}
