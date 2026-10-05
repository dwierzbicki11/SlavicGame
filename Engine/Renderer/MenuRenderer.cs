using System.Numerics;
using SlavicGame.Engine.UI;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class MenuRenderer : IDisposable
{
    private readonly List<UiVertex> _vertices = [];

    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _screenBuffer;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private GraphicsDevice? _graphicsDevice;
    private uint _vertexCapacity = 4096;
    private uint _preparedVertexCount;
    private bool _preparedForFrame;
    private bool _disposed;

    internal ulong PrepareCount { get; private set; }
    internal ulong DrawCount { get; private set; }

    public void Initialize(
        GraphicsDevice graphicsDevice,
        OutputDescription outputDescription)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;

        _vertexBuffer = factory.CreateBuffer(new BufferDescription(
            UiVertex.SizeInBytes * _vertexCapacity,
            BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _screenBuffer = factory.CreateBuffer(new BufferDescription(
            16,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "ScreenSize",
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex)));

        _set = factory.CreateResourceSet(new ResourceSetDescription(
            _layout,
            _screenBuffer));

        _shaders = ShaderLibrary.LoadPair(factory, "hud");

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementSemantic.Position,
                VertexElementFormat.Float2),
            new VertexElementDescription(
                "Color",
                VertexElementSemantic.Color,
                VertexElementFormat.Float4));

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription([vertexLayout], _shaders),
            [_layout],
            outputDescription));
    }

    public void Render(
        CommandList commandList,
        uint width,
        uint height,
        MenuView? view)
    {
        Prepare(commandList, width, height, view);
        Draw(commandList);
    }

    /// <summary>
    /// Builds and uploads menu geometry once for a simulation frame. Frame
    /// Generation can then draw the exact same UI onto both the interpolated
    /// and rendered swapchain images without rebuilding or uploading it twice.
    /// </summary>
    internal void Prepare(
        CommandList commandList,
        uint width,
        uint height,
        MenuView? view)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_graphicsDevice is null ||
            _vertexBuffer is null ||
            _screenBuffer is null ||
            _set is null ||
            _pipeline is null)
        {
            throw new InvalidOperationException(
                "Menu renderer is not initialized.");
        }

        _preparedForFrame = true;
        _preparedVertexCount = 0;
        PrepareCount++;

        if (view is null)
            return;

        var screen = new Vector4(width, height, 0f, 0f);
        commandList.UpdateBuffer(_screenBuffer, 0, screen);

        Build(view, width, height);
        EnsureCapacity();
        _preparedVertexCount = checked((uint)_vertices.Count);
        if (_preparedVertexCount == 0)
            return;

        commandList.UpdateBuffer(
            _vertexBuffer,
            0,
            System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_vertices));
    }

    /// <summary>
    /// Draws the geometry uploaded by <see cref="Prepare"/>. Multiple draws are
    /// intentional for FG: generated and rendered images receive identical UI.
    /// </summary>
    internal void Draw(CommandList commandList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        if (!_preparedForFrame)
            throw new InvalidOperationException(
                "MenuRenderer.Draw requires Prepare for the current frame.");
        if (_preparedVertexCount == 0)
            return;

        commandList.SetPipeline(_pipeline!);
        commandList.SetGraphicsResourceSet(0, _set!);
        commandList.SetVertexBuffer(0, _vertexBuffer!);
        commandList.Draw(_preparedVertexCount);
        DrawCount++;
    }

    private void Build(MenuView view, uint width, uint height)
    {
        _vertices.Clear();

        var w = Math.Max(1f, width);
        var h = Math.Max(1f, height);

        AddQuad(
            0f,
            0f,
            w,
            h,
            new Vector4(0.018f, 0.014f, 0.011f, 0.82f));

        AddQuad(
            0f,
            0f,
            w,
            5f,
            new Vector4(0.55f, 0.38f, 0.17f, 0.95f));

        DrawText(
            view.Title,
            72f,
            58f,
            6.5f,
            new Vector4(0.92f, 0.82f, 0.58f, 1f));

        DrawText(
            view.Subtitle,
            76f,
            118f,
            2.2f,
            new Vector4(0.72f, 0.68f, 0.58f, 0.92f));

        if (view.Tabs.Count == 0)
        {
            DrawMainPanel(view.Panels[0], w, h);
        }
        else
        {
            DrawTabs(view.Tabs, w);
            DrawSettingsPage(view.Panels[0], w, h);
        }

        DrawText(
            view.Footer,
            72f,
            h - 42f,
            1.8f,
            new Vector4(0.68f, 0.66f, 0.58f, 0.92f));
    }

    private void DrawMainPanel(
        MenuPanelView panel,
        float width,
        float height)
    {
        var panelX = 72f;
        var panelY = 190f;
        var panelWidth = MathF.Min(520f, width - 144f);
        var rowHeight = 72f;

        AddQuad(
            panelX - 24f,
            panelY - 22f,
            panelWidth,
            panel.Items.Count * rowHeight + 44f,
            new Vector4(0.045f, 0.037f, 0.028f, 0.86f));

        for (var i = 0; i < panel.Items.Count; i++)
        {
            var item = panel.Items[i];
            var y = panelY + i * rowHeight;

            if (item.Selected)
            {
                AddQuad(
                    panelX - 10f,
                    y - 12f,
                    panelWidth - 28f,
                    54f,
                    new Vector4(0.30f, 0.20f, 0.08f, 0.92f));
                DrawText(
                    ">",
                    panelX,
                    y,
                    4.0f,
                    new Vector4(0.98f, 0.82f, 0.42f, 1f));
            }

            DrawText(
                item.Label,
                panelX + 42f,
                y,
                4.0f,
                item.Selected
                    ? new Vector4(1f, 0.91f, 0.67f, 1f)
                    : new Vector4(0.80f, 0.77f, 0.67f, 0.95f));
        }
    }

    private void DrawTabs(
        IReadOnlyList<MenuTabView> tabs,
        float width)
    {
        const float left = 72f;
        const float top = 154f;
        const float gap = 12f;
        var available = MathF.Max(320f, width - left * 2f);
        var tabWidth =
            (available - gap * Math.Max(0, tabs.Count - 1)) /
            Math.Max(1, tabs.Count);

        for (var i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var x = left + i * (tabWidth + gap);

            AddQuad(
                x,
                top,
                tabWidth,
                38f,
                tab.Active
                    ? new Vector4(0.30f, 0.20f, 0.08f, 0.96f)
                    : new Vector4(0.055f, 0.047f, 0.037f, 0.90f));

            if (tab.Active)
            {
                AddQuad(
                    x,
                    top + 34f,
                    tabWidth,
                    4f,
                    new Vector4(0.98f, 0.72f, 0.28f, 1f));
            }

            var scale = 1.75f;
            var labelWidth = Measure(tab.Label, scale);
            DrawText(
                tab.Label,
                x + MathF.Max(10f, (tabWidth - labelWidth) * 0.5f),
                top + 10f,
                scale,
                tab.Active
                    ? new Vector4(1f, 0.91f, 0.67f, 1f)
                    : new Vector4(0.70f, 0.68f, 0.61f, 0.92f));
        }
    }

    private void DrawSettingsPage(
        MenuPanelView panel,
        float width,
        float height)
    {
        const float left = 72f;
        const float top = 208f;
        var panelWidth = MathF.Max(360f, width - left * 2f);
        var availableRowsHeight = MathF.Max(220f, height - top - 78f);
        var rowSpacing = MathF.Min(
            34f,
            availableRowsHeight / Math.Max(1, panel.Items.Count));
        var textScale = rowSpacing < 20f
            ? 1.15f
            : rowSpacing < 26f
                ? 1.40f
                : 1.75f;
        var selectionHeight = MathF.Max(15f, rowSpacing - 2f);
        var panelHeight = MathF.Min(
            height - top - 58f,
            56f + panel.Items.Count * rowSpacing);

        AddQuad(
            left,
            top,
            panelWidth,
            panelHeight,
            new Vector4(0.042f, 0.035f, 0.027f, 0.90f));

        AddQuad(
            left,
            top,
            panelWidth,
            4f,
            new Vector4(0.46f, 0.32f, 0.14f, 0.92f));

        DrawText(
            panel.Title,
            left + 18f,
            top + 16f,
            2.25f,
            new Vector4(0.93f, 0.81f, 0.56f, 1f));

        for (var i = 0; i < panel.Items.Count; i++)
        {
            var item = panel.Items[i];
            var y = top + 48f + i * rowSpacing;

            if (item.Selected)
            {
                AddQuad(
                    left + 10f,
                    y - 6f,
                    panelWidth - 20f,
                    selectionHeight,
                    new Vector4(0.25f, 0.17f, 0.07f, 0.94f));
            }

            var displayLabel = item.RequiresRestart
                ? item.Label + " *"
                : item.Label;

            DrawText(
                displayLabel,
                left + 18f,
                y,
                textScale,
                item.Selected
                    ? new Vector4(1f, 0.90f, 0.62f, 1f)
                    : new Vector4(0.77f, 0.74f, 0.65f, 0.95f));

            if (!string.IsNullOrWhiteSpace(item.Value))
            {
                var value = item.Value!;
                var valueWidth = Measure(value, textScale);
                DrawText(
                    value,
                    left + panelWidth - valueWidth - 18f,
                    y,
                    textScale,
                    item.Selected
                        ? new Vector4(1f, 0.78f, 0.35f, 1f)
                        : new Vector4(0.64f, 0.62f, 0.56f, 0.95f));
            }
        }
    }

    private void DrawText(
        string text,
        float x,
        float y,
        float scale,
        Vector4 color)
    {
        var cursor = x;
        foreach (var rawCharacter in text.ToUpperInvariant())
        {
            var character = rawCharacter switch
            {
                'Ą' => 'A',
                'Ć' => 'C',
                'Ę' => 'E',
                'Ł' => 'L',
                'Ń' => 'N',
                'Ó' => 'O',
                'Ś' => 'S',
                'Ż' or 'Ź' => 'Z',
                _ => rawCharacter
            };

            if (!Font.TryGetValue(character, out var glyph))
            {
                cursor += 6f * scale;
                continue;
            }

            for (var row = 0; row < 7; row++)
            for (var col = 0; col < 5; col++)
            {
                if ((glyph[row] & (1 << (4 - col))) == 0)
                    continue;

                AddQuad(
                    cursor + col * scale,
                    y + row * scale,
                    MathF.Max(1f, scale - 0.35f),
                    MathF.Max(1f, scale - 0.35f),
                    color);
            }

            cursor += 6f * scale;
        }
    }

    private static float Measure(string text, float scale) =>
        text.Length * 6f * scale;

    private void AddQuad(
        float x,
        float y,
        float width,
        float height,
        Vector4 color)
    {
        var x1 = x + width;
        var y1 = y + height;

        _vertices.Add(new UiVertex(new Vector2(x, y), color));
        _vertices.Add(new UiVertex(new Vector2(x1, y), color));
        _vertices.Add(new UiVertex(new Vector2(x1, y1), color));

        _vertices.Add(new UiVertex(new Vector2(x, y), color));
        _vertices.Add(new UiVertex(new Vector2(x1, y1), color));
        _vertices.Add(new UiVertex(new Vector2(x, y1), color));
    }

    private void EnsureCapacity()
    {
        if (_graphicsDevice is null ||
            _vertexBuffer is null ||
            _vertices.Count <= _vertexCapacity)
        {
            return;
        }

        _vertexCapacity = Math.Max(
            checked((uint)_vertices.Count),
            _vertexCapacity * 2);

        _graphicsDevice.WaitForIdle();
        _vertexBuffer.Dispose();
        _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(
                UiVertex.SizeInBytes * _vertexCapacity,
                BufferUsage.VertexBuffer | BufferUsage.Dynamic));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _pipeline?.Dispose();
        _set?.Dispose();
        _layout?.Dispose();
        _vertexBuffer?.Dispose();
        _screenBuffer?.Dispose();

        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _preparedForFrame = false;
        _preparedVertexCount = 0;
        _set = null;
        _layout = null;
        _vertexBuffer = null;
        _screenBuffer = null;
        _shaders = null;
        _graphicsDevice = null;
    }

    private readonly struct UiVertex
    {
        public const uint SizeInBytes = 24;

        public readonly Vector2 Position;
        public readonly Vector4 Color;

        public UiVertex(Vector2 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }

    internal static readonly Dictionary<char, int[]> Font = new()
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
        ['*'] = [0,0b10101,0b01110,0b11111,0b01110,0b10101,0]
    };
}