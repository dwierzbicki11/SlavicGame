using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Dialogue;

public static class VerticalSliceDialogueCatalog
{
    private const string QuestId = VerticalSliceBootstrap.ContractQuestId;

    private static readonly IReadOnlyDictionary<string, DialogueGraph> Graphs =
        new Dictionary<string, DialogueGraph>(StringComparer.Ordinal)
        {
            ["missing-family"] = BuildMissingFamily(),
            ["crossing-keeper"] = BuildCrossingKeeper(),
            ["herbalist"] = BuildHerbalist(),
            ["community-guard"] = BuildGuard(),
            ["shrine-keeper"] = BuildShrineKeeper()
        };

    public static DialogueGraph GetGraph(string npcId) =>
        Graphs.TryGetValue(npcId, out var graph)
            ? graph
            : throw new KeyNotFoundException(
                $"No vertical-slice dialogue graph for NPC '{npcId}'.");

    public static string SelectStartNode(
        WorldState world,
        string npcId)
    {
        var quest = world.Progress.Quests.Get(QuestId);

        return npcId switch
        {
            "missing-family"
                when quest.Phase == QuestPhase.Offered =>
                "mf.offer",

            "missing-family"
                when world.Progress.Inventory.Contains("missing-person-keepsake") &&
                     !quest.Evidence.Any(entry =>
                         entry.Id == "light-over-swamp.keepsake-owner") =>
                "mf.keepsake",

            "missing-family"
                when quest.Phase is QuestPhase.Resolved or QuestPhase.TurnedIn =>
                "mf.result",

            "missing-family" => "mf.investigation",
            "crossing-keeper" => "ck.intro",
            "herbalist" => "hb.intro",
            "community-guard" => "cg.intro",
            "shrine-keeper" => "sk.intro",
            _ => GetGraph(npcId).StartNodeId
        };
    }

    private static DialogueGraph BuildMissingFamily() =>
        new(
            "dialogue.missing-family",
            "mf.offer",
            [
                Node(
                    "mf.offer",
                    "missing-family",
                    "Nie wrócił z przeprawy. Ludzie mówią o świetle na mokradłach.",
                    Choice("mf.ask-when", "Kiedy wyszedł?", "mf.when"),
                    Choice(
                        "mf.ask-item",
                        "Co miał przy sobie?",
                        "mf.item",
                        effects:
                        [
                            Flag("light-over-swamp.keepsake-identification-cue")
                        ]),
                    Choice(
                        "mf.accept",
                        "Przyjmuję zlecenie.",
                        null,
                        effects:
                        [
                            SetQuestPhaseEffect(QuestPhase.Active),
                            Flag(VerticalSliceQuestInteractions.ContractAcceptedFlag),
                            Evidence(
                                "light-over-swamp.witness-light",
                                KnowledgeKind.Rumor,
                                "Rodzina mówi o świetle widywanym po zmierzchu.",
                                "missing-family"),
                            Evidence(
                                "light-over-swamp.last-route",
                                KnowledgeKind.ConfirmedFact,
                                "Zaginiony miał przejść przez mokradła.",
                                "missing-family")
                        ]),
                    Choice("mf.leave", "Wrócę później.", null)),

                Node(
                    "mf.when",
                    "missing-family",
                    "Wyszedł jeszcze za dnia. Miał wrócić przed zmierzchem.",
                    Choice("mf.when.back", "Rozumiem.", "mf.offer")),

                Node(
                    "mf.item",
                    "missing-family",
                    "Miał przy sobie drobną pamiątkę. Poznam ją, jeśli ją znajdziesz.",
                    Choice("mf.item.back", "Będę jej szukał.", "mf.offer")),

                Node(
                    "mf.investigation",
                    "missing-family",
                    "Masz już coś pewnego?",
                    Choice(
                        "mf.predator",
                        "Znalazłem ślady dużej istoty.",
                        "mf.predator-answer",
                        requirements:
                        [
                            FlagReq(
                                VerticalSliceQuestInteractions.PredatorTracksFlag)
                        ]),
                    Choice("mf.not-yet", "Jeszcze nie.", null)),

                Node(
                    "mf.predator-answer",
                    "missing-family",
                    "Więc to coś go zabiło?",
                    Choice(
                        "mf.predator-careful",
                        "Jeszcze tego nie wiem.",
                        null,
                        effects:
                        [
                            Relationship(
                                "missing-family",
                                RelationshipKind.Trust,
                                2)
                        ]),
                    Choice(
                        "mf.predator-claim",
                        "Na pewno.",
                        null,
                        effects:
                        [
                            Flag("missing-family.player-claimed-predator-cause")
                        ])),

                Node(
                    "mf.keepsake",
                    "missing-family",
                    "To... pokaż mi ten przedmiot.",
                    Choice(
                        "mf.show-keepsake",
                        "Pokaż pamiątkę.",
                        null,
                        requirements:
                        [
                            ItemReq("missing-person-keepsake")
                        ],
                        effects:
                        [
                            Evidence(
                                "light-over-swamp.keepsake-owner",
                                KnowledgeKind.ConfirmedFact,
                                "Rodzina potwierdziła, że pamiątka należała do zaginionego.",
                                "missing-family"),
                            Relationship(
                                "missing-family",
                                RelationshipKind.Trust,
                                4),
                            Flag(VerticalSliceQuestInteractions.OwnerConfirmedFlag)
                        ]),
                    Choice("mf.hide-keepsake", "Jeszcze nie.", null)),

                Node(
                    "mf.result",
                    "missing-family",
                    "Czy to już koniec?",
                    Choice(
                        "mf.result.truth",
                        "Zrobiłem tyle, by mógł odejść.",
                        null,
                        requirements:
                        [
                            FlagReq(VerticalSliceRituals.ReleasedFlag)
                        ],
                        effects:
                        [
                            Flag("missing-family.received-truth"),
                            Relationship(
                                "missing-family",
                                RelationshipKind.Trust,
                                3)
                        ]),
                    Choice("mf.result.leave", "Wrócę później.", null))
            ]);

    private static DialogueGraph BuildCrossingKeeper() =>
        new(
            "dialogue.crossing-keeper",
            "ck.intro",
            [
                Node(
                    "ck.intro",
                    "crossing-keeper",
                    "Przeprawa dostała mocno. Nie wszystko zrobiła woda.",
                    Choice(
                        "ck.damage",
                        "Co uszkodziło kładkę?",
                        "ck.damage-answer"),
                    Choice(
                        "ck.predator",
                        "Znalazłem ślady dużej istoty.",
                        "ck.predator-answer",
                        requirements:
                        [
                            FlagReq(
                                VerticalSliceQuestInteractions.PredatorTracksFlag)
                        ]),
                    Choice("ck.leave", "To wszystko.", null)),

                Node(
                    "ck.damage-answer",
                    "crossing-keeper",
                    "Deski pękły od ciężaru albo uderzenia. Światło desek nie łamie.",
                    Choice("ck.damage.back", "Rozumiem.", "ck.intro")),

                Node(
                    "ck.predator-answer",
                    "crossing-keeper",
                    "Nie widziałem światła i bestii w jednej chwili. To mogą być dwie różne sprawy.",
                    Choice(
                        "ck.two-causes",
                        "To pasuje do śladów.",
                        null,
                        effects:
                        [
                            Evidence(
                                "light-over-swamp.two-causes",
                                KnowledgeKind.Interpretation,
                                "Światło i fizyczny drapieżnik mogą być dwoma oddzielnymi źródłami zagrożenia.",
                                "crossing-keeper"),
                            Flag("light-over-swamp.two-causes-known")
                        ]))
            ]);

    private static DialogueGraph BuildHerbalist() =>
        new(
            "dialogue.herbalist",
            "hb.intro",
            [
                Node(
                    "hb.intro",
                    "herbalist",
                    "Mokradło nie jest jedną chorobą. Najpierw ustal, co naprawdę widzisz.",
                    Choice(
                        "hb.teacher",
                        "Możesz mnie nauczyć opatrywać rany magią?",
                        "hb.teacher-answer",
                        effects:
                        [
                            Flag(DialogueRuntime.TeacherReadyFlag("herbalist"))
                        ]),
                    Choice(
                        "hb.recipe",
                        "Masz coś na lepsze widzenie śladów?",
                        "hb.recipe-answer",
                        effects:
                        [
                            Flag("recipe.marsh-sight-tonic.learned")
                        ]),
                    Choice("hb.leave", "Wrócę później.", null)),

                Node(
                    "hb.teacher-answer",
                    "herbalist",
                    "Najpierw poznaj własny ból i naucz się zwykłego opatrunku. Potem pokażę ci, jak prowadzić siłę.",
                    Choice("hb.teacher.end", "Zapamiętam.", null)),

                Node(
                    "hb.recipe-answer",
                    "herbalist",
                    "Zioła z mokradła i żywica pozwolą wyostrzyć wzrok. Nie zastąpi to dowodu.",
                    Choice("hb.recipe.end", "Dobrze.", null))
            ]);

    private static DialogueGraph BuildGuard() =>
        new(
            "dialogue.community-guard",
            "cg.intro",
            [
                Node(
                    "cg.intro",
                    "community-guard",
                    "Po zmierzchu nie chodzimy przez mokradła bez powodu.",
                    Choice(
                        "cg.rumor",
                        "Kto widział światło?",
                        "cg.rumor-answer"),
                    Choice(
                        "cg.predator",
                        "Na mokradle jest drapieżnik.",
                        "cg.predator-answer",
                        requirements:
                        [
                            FlagReq(
                                SwampPredatorEncounter.IdentifiedFlag)
                        ]),
                    Choice("cg.leave", "Rozumiem.", null)),

                Node(
                    "cg.rumor-answer",
                    "community-guard",
                    "Kilku ludzi. Każdy opowiada trochę inaczej. Traktuj to jak plotkę, dopóki sam nie sprawdzisz.",
                    Choice(
                        "cg.rumor.note",
                        "Zapiszę to jako relację świadków.",
                        null,
                        effects:
                        [
                            Evidence(
                                "light-over-swamp.witness-light",
                                KnowledgeKind.Rumor,
                                "Kilku mieszkańców widziało nocne światło, ale relacje nie są zgodne.",
                                "community-guard")
                        ])),

                Node(
                    "cg.predator-answer",
                    "community-guard",
                    "To zmienia wartę. Światło może straszyć, ale zęby zabijają naprawdę.",
                    Choice("cg.predator.end", "Będę uważał.", null))
            ]);

    private static DialogueGraph BuildShrineKeeper() =>
        new(
            "dialogue.shrine-keeper",
            "sk.intro",
            [
                Node(
                    "sk.intro",
                    "shrine-keeper",
                    "Miejsce reaguje na granicę. Reakcja nie mówi jeszcze, kto stoi po drugiej stronie.",
                    Choice(
                        "sk.place",
                        "Co to za miejsce?",
                        "sk.place-answer"),
                    Choice(
                        "sk.light",
                        "Co wiesz o świetle?",
                        "sk.light-answer"),
                    Choice(
                        "sk.teacher",
                        "Możesz nauczyć mnie znaków i magii?",
                        "sk.teacher-answer",
                        effects:
                        [
                            Flag(DialogueRuntime.TeacherReadyFlag("shrine-keeper"))
                        ]),
                    Choice(
                        "sk.ritual",
                        "Jak zamknąć więź?",
                        "sk.ritual-answer",
                        requirements:
                        [
                            EvidenceReq(
                                "light-over-swamp.keepsake-owner",
                                KnowledgeKind.ConfirmedFact)
                        ],
                        effects:
                        [
                            Flag(VerticalSliceRituals.AnchorKnowledgeFlag),
                            Flag(VerticalSliceRituals.LearnedFlag),
                            Flag(VerticalSliceRituals.IdentitySignFlag),
                            Flag(VerticalSliceRituals.BoundarySignFlag),
                            Flag(VerticalSliceQuestInteractions.ShrineLessonFlag)
                        ]),
                    Choice("sk.leave", "Na razie wystarczy.", null)),

                Node(
                    "sk.place-answer",
                    "shrine-keeper",
                    "Tu wyznaczano granice i składano zobowiązania. Starszych rzeczy nie będę zgadywał.",
                    Choice("sk.place.back", "Rozumiem.", "sk.intro")),

                Node(
                    "sk.light-answer",
                    "shrine-keeper",
                    "Światło może być śladem. Ślad może należeć do zmarłego albo tylko go przypominać.",
                    Choice(
                        "sk.light.note",
                        "Czyli sama zjawa nie potwierdza tożsamości.",
                        "sk.intro",
                        effects:
                        [
                            Evidence(
                                "light-over-swamp.identity-uncertain",
                                KnowledgeKind.Interpretation,
                                "Sama reakcja zjawiska nie potwierdza tożsamości zmarłego.",
                                "shrine-keeper")
                        ])),

                Node(
                    "sk.teacher-answer",
                    "shrine-keeper",
                    "Mogę pokazać ci zasady. Ćwiczenie zrobisz tutaj, przy mnie, gdy spełnisz warunki lekcji.",
                    Choice("sk.teacher.end", "Jestem gotów się uczyć.", null)),

                Node(
                    "sk.ritual-answer",
                    "shrine-keeper",
                    "Rozpoznaj osobę, wyznacz granicę, przygotuj kotwicę i nic obrzędową. Nie zamykaj czegoś, czego nie rozpoznałeś.",
                    Choice("sk.ritual.end", "Zapamiętam kolejność.", null))
            ]);

    private static DialogueNode Node(
        string id,
        string speaker,
        string text,
        params DialogueChoice[] choices) =>
        new(id, speaker, text, choices);

    private static DialogueChoice Choice(
        string id,
        string text,
        string? nextNodeId,
        DialogueRequirement[]? requirements = null,
        DialogueEffect[]? effects = null) =>
        new(
            id,
            text,
            nextNodeId,
            requirements ?? [],
            effects ?? []);

    private static DialogueRequirement FlagReq(
        string flag,
        bool expected = true) =>
        new(
            DialogueRequirementKind.WorldFlag,
            flag,
            expected ? "true" : "false");

    private static DialogueRequirement ItemReq(
        string item,
        int quantity = 1) =>
        new(
            DialogueRequirementKind.Item,
            item,
            quantity.ToString());

    private static DialogueRequirement EvidenceReq(
        string evidence,
        KnowledgeKind minimum) =>
        new(
            DialogueRequirementKind.Evidence,
            evidence,
            minimum.ToString());

    private static DialogueEffect Flag(
        string flag,
        bool enabled = true) =>
        new(
            DialogueEffectKind.SetWorldFlag,
            flag,
            enabled ? "true" : "false");

    private static DialogueEffect SetQuestPhaseEffect(
        QuestPhase phase) =>
        new(
            DialogueEffectKind.AdvanceQuest,
            QuestId,
            phase.ToString());

    private static DialogueEffect Evidence(
        string evidenceId,
        KnowledgeKind kind,
        string text,
        string sourceId) =>
        new(
            DialogueEffectKind.AddEvidence,
            QuestId,
            $"{evidenceId}|{kind}|{text}|{sourceId}");

    private static DialogueEffect Relationship(
        string npcId,
        RelationshipKind kind,
        int amount) =>
        new(
            DialogueEffectKind.ChangeRelationship,
            npcId,
            kind.ToString(),
            amount);
}
