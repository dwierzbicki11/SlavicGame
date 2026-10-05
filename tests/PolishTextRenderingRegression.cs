using System.Text;
using SlavicGame.Engine.Dialogue;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

internal static class PolishTextRenderingRegression
{
    public static void Run(Action<bool, string> check)
    {
        const string lower = "ąćęłńóśźż";
        const string upper = "ĄĆĘŁŃÓŚŹŻ";
        check(BitmapFont.Prepare(lower) == upper, "Polish lowercase retains every diacritic in uppercase UI");
        check(BitmapFont.Prepare(lower.Normalize(NormalizationForm.FormD)) == upper,
            "Decomposed Polish Unicode resolves to the same nine glyphs");
        check(BitmapFont.Measure(lower, 2) == 108 &&
              BitmapFont.Measure(lower.Normalize(NormalizationForm.FormD), 2) == 108,
            "Combining accents neither widen nor shift text alignment");

        foreach (var (letter, body) in new[] {
            ('Ą', 'A'), ('Ć', 'C'), ('Ę', 'E'), ('Ł', 'L'), ('Ń', 'N'),
            ('Ó', 'O'), ('Ś', 'S'), ('Ź', 'Z'), ('Ż', 'Z') })
        {
            check(BitmapFont.Glyphs.ContainsKey(letter), $"Font contains Polish glyph {letter}");
            var glyph = BitmapFont.Glyphs[letter];
            check(glyph.Rows.Any(row => row != 0) && glyph.Rows.All(row => row is >= 0 and < 32),
                $"Polish glyph {letter} has visible five-column pixels");
            check(glyph.TopOffset != 0 || !glyph.Rows.SequenceEqual(BitmapFont.Glyphs[body].Rows),
                $"Polish glyph {letter} is not transliterated to {body}");
        }
        foreach (var letter in "ĆŃÓŚŹ")
        {
            var glyph = BitmapFont.Glyphs[letter];
            check(glyph.TopOffset == -2 && glyph.Rows[0] == 0b00010 && glyph.Rows[1] == 0b00100,
                $"Acute accent on {letter} is drawn above the original baseline");
        }
        foreach (var (letter, body) in new[] {
            ('Ą', 'A'), ('Ć', 'C'), ('Ę', 'E'), ('Ń', 'N'), ('Ó', 'O'),
            ('Ś', 'S'), ('Ź', 'Z'), ('Ż', 'Z') })
        {
            var glyph = BitmapFont.Glyphs[letter];
            check(glyph.Rows.Skip(-glyph.TopOffset).Take(7).SequenceEqual(BitmapFont.Glyphs[body].Rows),
                $"Polish glyph {letter} retains the readable original seven-row body");
        }
        foreach (var letter in "ĄĘ")
        {
            var glyph = BitmapFont.Glyphs[letter];
            check(glyph.TopOffset == 0 && glyph.Rows.Length == 9 &&
                  glyph.Rows[7] == 0b00010 && glyph.Rows[8] == 0b00011,
                $"Ogonek on {letter} is drawn below the original body");
        }
        check(BitmapFont.Glyphs['Ż'].Rows[0] == 0b00100 && BitmapFont.Glyphs['Ż'].Rows[1] == 0,
            "Dotted Ż remains distinguishable from acute Ź");
        check(BitmapFont.Glyphs['Ł'].Rows[2] == 0b10100 && BitmapFont.Glyphs['Ł'].Rows[3] == 0b11000,
            "Ł has a visible diagonal stroke rather than plain L");
        check(BitmapFont.VerticalBounds(upper) == (-2, 9) && BitmapFont.VerticalBounds("ASCII") == (0, 7),
            "Dialogue backdrop includes accents and ogoneks without changing ASCII height");

        // Exercise real authored dialogue, including every choice reachable
        // from its initial node, not just an isolated pangram fixture.
        var world = new WorldState();
        world.Initialize();
        foreach (var npc in world.Npcs)
        {
            var graph = VerticalSliceDialogueCatalog.GetGraph(npc.Id);
            var pending = new Queue<string>();
            var visited = new HashSet<string>();
            pending.Enqueue(graph.StartNodeId);
            while (pending.TryDequeue(out var id))
            {
                if (!visited.Add(id)) continue;
                var node = graph.GetNode(id);
                foreach (var text in new[] { node.Text }.Concat(node.Choices.Select(choice => choice.Text)))
                    check(BitmapFont.Prepare(text).All(BitmapFont.Glyphs.ContainsKey),
                        $"Dialogue {npc.Id}/{id} has a glyph for every authored character: {text}");
                foreach (var choice in node.Choices)
                    if (choice.NextNodeId is not null) pending.Enqueue(choice.NextNodeId);
            }
        }
    }
}
