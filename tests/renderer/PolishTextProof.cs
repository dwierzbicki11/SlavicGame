using System.Numerics;
using System.Text;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.UI;
using SlavicGame.Engine.World;
using Veldrid;

internal static class PolishTextProof
{
    public static int Run(GraphicsDevice device)
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            checks++;
            Console.WriteLine("PASS: " + message);
        }

        var world = WorldGenerator.Generate();
        var settings = new GameSettings();
        GraphicsPresetCatalog.Apply(settings, GraphicsPreset.LowEnd);
        settings.Upscaler = UpscalerMode.Bilinear;
        settings.AntiAliasing = AntiAliasingMode.Off;
        settings.ShowFps = false;
        var family = world.NpcWorld.Find("missing-family") ?? throw new Exception("Missing dialogue NPC");
        world.SetPlayerPosition(family.Position);
        Check(world.Dialogue.Start(world, family.Id), "GPU text proof opens the real family dialogue");
        var camera = new Camera3D();
        camera.SetCinematicPose(new Vector3(0, 8, -66), new Vector3(0, 2, -87));
        using var target = new Target(device, 640, 360);
        using var renderer = new VeldridRenderer();
        renderer.InitializeOffscreen(device, target.Framebuffer, world, TextureQuality.Low,
            MsaaQuality.Off, UpscalerMode.Bilinear);
        void Render(MenuView? menu = null) => renderer.Render(world, camera, 60, 0, 1.0 / 60, settings, menu);
        bool HasColor(Rgba16[] image, float x, float y, Vector3 color)
        {
            var pixel = image[(int)MathF.Floor(y) * 640 + (int)MathF.Floor(x)];
            return MathF.Abs((float)pixel.R - color.X) < 0.01f &&
                   MathF.Abs((float)pixel.G - color.Y) < 0.01f &&
                   MathF.Abs((float)pixel.B - color.Z) < 0.01f;
        }
        void CheckTextPixel(Rgba16[] image, string text, char letter, int col, int row,
            float x, float y, float scale, Vector3 color, string message)
        {
            var index = BitmapFont.Prepare(text).IndexOf(letter);
            Check(index >= 0 && HasColor(image,
                x + (index * 6 + col + 0.5f) * scale, y + (row + 0.5f) * scale, color), message);
        }

        var dialogueColor = new Vector3(0.9f, 0.93f, 1f);
        void CheckDialogue(string message, params (char Letter, int Col, int Row)[] marks)
        {
            Render();
            var node = world.Dialogue.CurrentNode!;
            var choices = world.Dialogue.AvailableChoices(world);
            var panelTop = 360 - MathF.Min(320f, MathF.Max(210f, 145f + choices.Count * 34f));
            var scale = Math.Clamp(592f / (BitmapFont.Prepare(node.Text).Length * 6), 0.8f, 2f);
            var image = Read(device, target.Color);
            foreach (var (letter, col, row) in marks)
                CheckTextPixel(image, node.Text, letter, col, row, 24, panelTop + 50,
                    scale, dialogueColor, message + ": " + letter);
            if (choices[0].Text.Contains('?'))
            {
                var text = "> " + choices[0].Text;
                scale = Math.Clamp(568f / (BitmapFont.Prepare(text).Length * 6), 0.8f, 2f);
                CheckTextPixel(image, text, '?', 2, 6, 36, panelTop + 92, scale,
                    dialogueColor, "Dialogue choice question mark has visible GPU pixels");
            }
        }
        CheckDialogue("Authored dialogue retains accent/stroke/ogonek pixels",
            ('Ó', 3, -2), ('Ł', 2, 2), ('Ą', 3, 7), ('Ś', 3, -2));
        Check(world.Dialogue.ChooseById(world, "mf.ask-item"), "GPU text proof selects a real dialogue choice");
        CheckDialogue("Next dialogue node retains its ogoneks", ('Ą', 3, 7), ('Ę', 3, 7));
        world.Dialogue.Close();

        const string title = "ąćęłńóśźż";
        MenuView Menu(string text) => new(text, "", [], [new MenuPanelView("", [])], "");
        Render(Menu(title));
        var composed = Read(device, target.Color);
        var titleColor = new Vector3(0.92f, 0.82f, 0.58f);
        foreach (var (letter, col, row) in new[] {
            ('Ą', 3, 7), ('Ć', 3, -2), ('Ę', 3, 7), ('Ł', 2, 2), ('Ń', 3, -2),
            ('Ó', 3, -2), ('Ś', 3, -2), ('Ź', 3, -2), ('Ż', 2, -2) })
            CheckTextPixel(composed, title, letter, col, row, 72, 58, 6.5f, titleColor,
                "Menu draws real Polish glyph pixels rather than ASCII substitution: " + letter);
        Check(!HasColor(composed, 72 + (8 * 6 + 3.5f) * 6.5f, 58 - 1.5f * 6.5f, titleColor),
            "Menu dotted Ż does not turn into acute Ź");
        Render(Menu(title.Normalize(NormalizationForm.FormD)));
        var decomposed = Read(device, target.Color);
        Check(composed.Zip(decomposed).All(pair => pair.First.R == pair.Second.R &&
              pair.First.G == pair.Second.G && pair.First.B == pair.Second.B),
            "Composed and decomposed Polish titles produce identical real renderer images");
        Console.WriteLine($"Polish text GPU proof: {checks} checks passed; authored dialogue, choices, menu diacritics and Unicode normalization.");
        return checks;
    }

    private static Rgba16[] Read(GraphicsDevice device, Texture texture)
    {
        using var staging = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            texture.Width, texture.Height, 1, 1, texture.Format, TextureUsage.Staging));
        using var commands = device.ResourceFactory.CreateCommandList();
        using var fence = device.ResourceFactory.CreateFence(false);
        commands.Begin();
        commands.CopyTexture(texture, staging);
        commands.End();
        device.SubmitCommands(commands, fence);
        device.WaitForFence(fence);
        var map = device.Map<Rgba16>(staging, MapMode.Read);
        try
        {
            var pixels = new Rgba16[texture.Width * texture.Height];
            for (uint y = 0; y < texture.Height; y++) for (uint x = 0; x < texture.Width; x++)
                pixels[y * texture.Width + x] = map[x, y];
            return pixels;
        }
        finally { device.Unmap(staging); }
    }
}
