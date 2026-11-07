using System.Numerics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;

internal static class MinimapRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var camera = new Camera3D();

        // 1. POI Catalog Checks
        var pois = MinimapRenderer.Pois;
        check(pois.Count >= 10, "Minimap POI catalog defines comprehensive landmarks across regions");
        check(pois.Any(p => p.Name == "ŻARNOWIEC" && p.ShortTag == "Ż"), "Minimap includes Żarnowiec POI");
        check(pois.Any(p => p.Name == "GRÓD BORZYWOJ" && p.ShortTag == "G"), "Minimap includes Gród Borzywoj POI");
        check(pois.Any(p => p.Name == "PRZYSTAŃ RYBACKA" && p.ShortTag == "R"), "Minimap includes Przystań Rybacka POI");
        check(pois.Any(p => p.Name == "MOST RZECZNY" && p.ShortTag == "="), "Minimap includes Most Rzeczny POI");
        check(pois.Any(p => p.Name == "OSADA SMOLNIKÓW" && p.ShortTag == "S"), "Minimap includes Osada Smolników POI");
        check(pois.Any(p => p.Name == "OBÓZ KARAWANY" && p.ShortTag == "O"), "Minimap includes Obóz Karawany POI");
        check(pois.Any(p => p.Name == "KAMIENNY KRĄG" && p.ShortTag == "K"), "Minimap includes Kamienny Krąg POI");
        check(pois.Any(p => p.Name == "CHRAM PERUNA" && p.ShortTag == "P"), "Minimap includes Chram Peruna POI");
        check(pois.Any(p => p.Name == "UROCZYSKO WELESA" && p.ShortTag == "W"), "Minimap includes Uroczysko Welesa POI");
        check(pois.Any(p => p.Name == "ŚWIĘTY GAJ MOKOSZY" && p.ShortTag == "M"), "Minimap includes Święty Gaj Mokoszy POI");

        check(pois.All(p => float.IsFinite(p.Position.X) && float.IsFinite(p.Position.Y)),
            "All minimap POI coordinates are finite");
        check(pois.All(p => p.Color.W > 0.5f), "All minimap POIs have visible alpha colors");

        // 2. Geometry Emission Test at Origin
        var quads = new List<(float X, float Y, float W, float H, Vector4 Color)>();
        var triangles = new List<(Vector2 A, Vector2 B, Vector2 C, Vector4 Color)>();
        var texts = new List<(string Text, float X, float Y, float Scale, Vector4 Color)>();

        void AddQuad(float x, float y, float w, float h, Vector4 c) => quads.Add((x, y, w, h, c));
        void AddTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 cCol) => triangles.Add((a, b, c, cCol));
        void AddText(string t, float x, float y, float s, Vector4 c, bool bg, Vector4? bgCol) => texts.Add((t, x, y, s, c));

        world.SetPlayerPosition(new Vector3(0f, 0f, 0f));
        MinimapRenderer.Render(world, camera, 1280f, 720f, AddQuad, AddTriangle, AddText);

        check(quads.Count > 30, "Minimap at origin emits frame, background, trails, and POI quads");
        check(triangles.Count >= 6, "Minimap emits frame bevels, player heading arrow and view cone");
        check(texts.Any(t => t.Text == "N") && texts.Any(t => t.Text == "S") &&
              texts.Any(t => t.Text == "W") && texts.Any(t => t.Text == "Z"),
            "Minimap emits all four cardinal compass directions (N, S, W, Z)");
        check(texts.Any(t => t.Text.Contains("PUSZCZA", StringComparison.OrdinalIgnoreCase) ||
                             t.Text.Contains("ŻYWIA", StringComparison.OrdinalIgnoreCase)),
            "Minimap displays current region banner");
        check(!texts.Any(t => t.Text.StartsWith("X:")),
            "Minimap avoids raw debug coordinate clutter");

        // 3. Gród Borzywoj Rendering & POI in-range diamond vs edge clamp
        quads.Clear();
        triangles.Clear();
        texts.Clear();
        world.SetPlayerPosition(new Vector3(-320f, 0f, 220f));
        MinimapRenderer.Render(world, camera, 1280f, 720f, AddQuad, AddTriangle, AddText);

        check(texts.Any(t => t.Text == "G"),
            "Gród Borzywoj POI renders in-range landmark diamond and tag when player is present");
        check(texts.Any(t => t.Text.Contains("BORZYWOJ", StringComparison.OrdinalIgnoreCase)),
            "Minimap region title reflects Gród Borzywoj when player enters the stronghold");
        check(triangles.Count > 10,
            "Distant settlements outside 155m range emit directional edge radar arrows");

        // 4. Przystań Rybacka & River Bridge Rendering
        quads.Clear();
        triangles.Clear();
        texts.Clear();
        world.SetPlayerPosition(new Vector3(148f, 0f, -180f));
        MinimapRenderer.Render(world, camera, 1280f, 720f, AddQuad, AddTriangle, AddText);

        check(texts.Any(t => t.Text == "R"),
            "Przystań Rybacka POI renders in-range landmark diamond when player is at the harbor");
        check(quads.Any(q => q.W >= 10f && q.Color.X > 0.6f && q.Color.Y > 0.5f),
            "Most Rzeczny wooden bridge plank marker is rendered within minimap radius");
        check(quads.Count(q => q.Color.Z > 0.65f) >= 15,
            "River water ribbon quads are rendered across the minimap at the river bank");

        // 5. GameSettings & SettingsCatalog integration
        var settings = new GameSettings();
        check(settings.ShowMinimap, "Minimap is enabled by default in GameSettings");
        var catalog = SettingsCatalog.All;
        check(catalog.Any(d => d.Id == "minimap"), "SettingsCatalog exposes the minimap toggle option");
    }
}
