using System.Text;

namespace SlavicGame.Engine.Renderer;

// The same bitmap font is used by dialogue/HUD and menus. Keep the original
// seven-row ASCII body and baseline; accents extend above it, ogoneks below it.
internal readonly record struct BitmapGlyph(int[] Rows, int TopOffset = 0);

internal static class BitmapFont
{
    internal const int Width = 5;
    internal const int Height = 7;
    internal const int Advance = 6;

    internal static readonly IReadOnlyDictionary<char, BitmapGlyph> Glyphs = CreateGlyphs();

    // Authored text and decomposed Unicode input must have identical glyphs,
    // advance and alignment. Uppercase is the existing UI style, not a fallback
    // that removes Polish diacritics.
    internal static string Prepare(string text) =>
        text.Normalize(NormalizationForm.FormC).ToUpperInvariant();

    internal static float Measure(string text, float scale) =>
        Prepare(text).Length * Advance * scale;

    internal static (int Top, int Bottom) VerticalBounds(string preparedText)
    {
        var top = 0;
        var bottom = Height;
        foreach (var character in preparedText)
            if (Glyphs.TryGetValue(character, out var glyph))
            {
                top = Math.Min(top, glyph.TopOffset);
                bottom = Math.Max(bottom, glyph.TopOffset + glyph.Rows.Length);
            }
        return (top, bottom);
    }

    private static IReadOnlyDictionary<char, BitmapGlyph> CreateGlyphs()
    {
        var ascii = new Dictionary<char, int[]>
        {
            ['A'] = [0b01110,0b10001,0b10001,0b11111,0b10001,0b10001,0b10001],
            ['B'] = [0b11110,0b10001,0b10001,0b11110,0b10001,0b10001,0b11110],
            ['C'] = [0b01111,0b10000,0b10000,0b10000,0b10000,0b10000,0b01111],
            ['D'] = [0b11110,0b10001,0b10001,0b10001,0b10001,0b10001,0b11110],
            ['E'] = [0b11111,0b10000,0b10000,0b11110,0b10000,0b10000,0b11111],
            ['F'] = [0b11111,0b10000,0b10000,0b11110,0b10000,0b10000,0b10000],
            ['G'] = [0b01111,0b10000,0b10000,0b10111,0b10001,0b10001,0b01111],
            ['H'] = [0b10001,0b10001,0b10001,0b11111,0b10001,0b10001,0b10001],
            ['I'] = [0b11111,0b00100,0b00100,0b00100,0b00100,0b00100,0b11111],
            ['J'] = [0b00111,0b00010,0b00010,0b00010,0b10010,0b10010,0b01100],
            ['K'] = [0b10001,0b10010,0b10100,0b11000,0b10100,0b10010,0b10001],
            ['L'] = [0b10000,0b10000,0b10000,0b10000,0b10000,0b10000,0b11111],
            ['M'] = [0b10001,0b11011,0b10101,0b10101,0b10001,0b10001,0b10001],
            ['N'] = [0b10001,0b11001,0b10101,0b10011,0b10001,0b10001,0b10001],
            ['O'] = [0b01110,0b10001,0b10001,0b10001,0b10001,0b10001,0b01110],
            ['P'] = [0b11110,0b10001,0b10001,0b11110,0b10000,0b10000,0b10000],
            ['Q'] = [0b01110,0b10001,0b10001,0b10001,0b10101,0b10010,0b01101],
            ['R'] = [0b11110,0b10001,0b10001,0b11110,0b10100,0b10010,0b10001],
            ['S'] = [0b01111,0b10000,0b10000,0b01110,0b00001,0b00001,0b11110],
            ['T'] = [0b11111,0b00100,0b00100,0b00100,0b00100,0b00100,0b00100],
            ['U'] = [0b10001,0b10001,0b10001,0b10001,0b10001,0b10001,0b01110],
            ['V'] = [0b10001,0b10001,0b10001,0b10001,0b10001,0b01010,0b00100],
            ['W'] = [0b10001,0b10001,0b10001,0b10101,0b10101,0b10101,0b01010],
            ['X'] = [0b10001,0b10001,0b01010,0b00100,0b01010,0b10001,0b10001],
            ['Y'] = [0b10001,0b10001,0b01010,0b00100,0b00100,0b00100,0b00100],
            ['Z'] = [0b11111,0b00001,0b00010,0b00100,0b01000,0b10000,0b11111],
            ['0'] = [0b01110,0b10001,0b10011,0b10101,0b11001,0b10001,0b01110],
            ['1'] = [0b00100,0b01100,0b00100,0b00100,0b00100,0b00100,0b01110],
            ['2'] = [0b01110,0b10001,0b00001,0b00010,0b00100,0b01000,0b11111],
            ['3'] = [0b11110,0b00001,0b00001,0b01110,0b00001,0b00001,0b11110],
            ['4'] = [0b00010,0b00110,0b01010,0b10010,0b11111,0b00010,0b00010],
            ['5'] = [0b11111,0b10000,0b10000,0b11110,0b00001,0b00001,0b11110],
            ['6'] = [0b01110,0b10000,0b10000,0b11110,0b10001,0b10001,0b01110],
            ['7'] = [0b11111,0b00001,0b00010,0b00100,0b01000,0b01000,0b01000],
            ['8'] = [0b01110,0b10001,0b10001,0b01110,0b10001,0b10001,0b01110],
            ['9'] = [0b01110,0b10001,0b10001,0b01111,0b00001,0b00001,0b01110],
            [' '] = [0,0,0,0,0,0,0],
            ['-'] = [0,0,0,0b11111,0,0,0],
            ['/'] = [0b00001,0b00010,0b00100,0b01000,0b10000,0,0],
            ['.'] = [0,0,0,0,0,0b00110,0b00110],
            [':'] = [0,0b00110,0b00110,0,0b00110,0b00110,0],
            ['>'] = [0b10000,0b01000,0b00100,0b00010,0b00100,0b01000,0b10000],
            ['<'] = [0b00001,0b00010,0b00100,0b01000,0b00100,0b00010,0b00001],
            ['*'] = [0,0b10101,0b01110,0b11111,0b01110,0b10101,0],
            ['?'] = [0b01110,0b10001,0b00001,0b00010,0b00100,0,0b00100],
            ['!'] = [0b00100,0b00100,0b00100,0b00100,0b00100,0,0b00100],
            [','] = [0,0,0,0,0,0b00110,0b00100],
            [';'] = [0,0b00110,0b00110,0,0b00110,0b00100,0b01000],
            ['"'] = [0b01010,0b01010,0b01010,0,0,0,0],
            ['\''] = [0b00100,0b00100,0b01000,0,0,0,0],
            ['('] = [0b00010,0b00100,0b01000,0b01000,0b01000,0b00100,0b00010],
            [')'] = [0b01000,0b00100,0b00010,0b00010,0b00010,0b00100,0b01000],
            ['['] = [0b01110,0b01000,0b01000,0b01000,0b01000,0b01000,0b01110],
            [']'] = [0b01110,0b00010,0b00010,0b00010,0b00010,0b00010,0b01110]
        };
        var glyphs = ascii.ToDictionary(pair => pair.Key, pair => new BitmapGlyph(pair.Value));

        foreach (var (letter, body) in new[] { ('Ć', 'C'), ('Ń', 'N'), ('Ó', 'O'), ('Ś', 'S'), ('Ź', 'Z') })
            glyphs[letter] = new BitmapGlyph([0b00010, 0b00100, .. ascii[body]], -2);
        glyphs['Ż'] = new BitmapGlyph([0b00100, 0, .. ascii['Z']], -2);
        glyphs['Ą'] = new BitmapGlyph([.. ascii['A'], 0b00010, 0b00011]);
        glyphs['Ę'] = new BitmapGlyph([.. ascii['E'], 0b00010, 0b00011]);
        glyphs['Ł'] = new BitmapGlyph([0b10000,0b10000,0b10100,0b11000,0b10000,0b10000,0b11111]);
        return glyphs;
    }
}
