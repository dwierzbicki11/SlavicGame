using SlavicGame.Engine.Settings;
using SlavicGame.Engine.Windowing;
using Veldrid;

namespace SlavicGame.Engine.UI;

public enum FrontendAction
{
    None,
    StartGame,
    Exit,
    SettingsChanged
}

public sealed class FrontendController
{
    private static readonly string[] MainItems =
    [
        "GRAJ",
        "USTAWIENIA",
        "WYJSCIE"
    ];

    private int _mainSelection;
    private int _settingsSelection;

    public FrontendScreen Screen { get; private set; } = FrontendScreen.MainMenu;

    public bool IsPlaying => Screen == FrontendScreen.Playing;

    public FrontendAction HandleInput(GameWindow window, GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(settings);

        if (Screen == FrontendScreen.Playing)
            return FrontendAction.None;

        var up =
            window.ConsumeKeyPress(Key.Up) ||
            window.ConsumeKeyPress(Key.W);
        var down =
            window.ConsumeKeyPress(Key.Down) ||
            window.ConsumeKeyPress(Key.S);
        var left =
            window.ConsumeKeyPress(Key.Left) ||
            window.ConsumeKeyPress(Key.A);
        var right =
            window.ConsumeKeyPress(Key.Right) ||
            window.ConsumeKeyPress(Key.D);
        var confirm =
            window.ConsumeKeyPress(Key.Enter) ||
            window.ConsumeKeyPress(Key.Space);

        if (Screen == FrontendScreen.MainMenu)
        {
            if (up)
                _mainSelection = Wrap(_mainSelection - 1, MainItems.Length);
            if (down)
                _mainSelection = Wrap(_mainSelection + 1, MainItems.Length);

            if (!confirm)
                return FrontendAction.None;

            switch (_mainSelection)
            {
                case 0:
                    Screen = FrontendScreen.Playing;
                    return FrontendAction.StartGame;
                case 1:
                    Screen = FrontendScreen.Settings;
                    _settingsSelection = 0;
                    return FrontendAction.None;
                case 2:
                    return FrontendAction.Exit;
            }
        }

        if (Screen == FrontendScreen.Settings)
        {
            var count = SettingsCatalog.All.Count + 1;

            if (window.ConsumeKeyPress(Key.Escape))
            {
                Screen = FrontendScreen.MainMenu;
                return FrontendAction.None;
            }

            if (up)
                _settingsSelection = Wrap(_settingsSelection - 1, count);
            if (down)
                _settingsSelection = Wrap(_settingsSelection + 1, count);

            if (_settingsSelection == SettingsCatalog.All.Count)
            {
                if (confirm)
                {
                    Screen = FrontendScreen.MainMenu;
                    return FrontendAction.None;
                }

                return FrontendAction.None;
            }

            if (left || right || confirm)
            {
                var direction = left ? -1 : 1;
                SettingsCatalog.All[_settingsSelection].Change(settings, direction);
                settings.Normalize();
                return FrontendAction.SettingsChanged;
            }
        }

        return FrontendAction.None;
    }

    public void OpenMainMenu()
    {
        Screen = FrontendScreen.MainMenu;
        _mainSelection = 0;
    }

    public MenuView BuildView(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (Screen == FrontendScreen.MainMenu)
        {
            return new MenuView(
                "SLAVICGAME",
                "SLOWIANSKI ACTION RPG",
                [
                    new MenuPanelView(
                        string.Empty,
                        MainItems
                            .Select((label, index) =>
                                new MenuItemView(
                                    label,
                                    null,
                                    index == _mainSelection))
                            .ToArray())
                ],
                "STRZALKI / W S  -  ENTER");
        }

        var panels = Enum.GetValues<SettingCategory>()
            .Select(category =>
            {
                var indexed = SettingsCatalog.All
                    .Select((definition, index) => (definition, index))
                    .Where(entry => entry.definition.Category == category)
                    .Select(entry =>
                        new MenuItemView(
                            entry.definition.Label,
                            entry.definition.ValueText(settings),
                            entry.index == _settingsSelection))
                    .ToArray();

                return new MenuPanelView(CategoryName(category), indexed);
            })
            .ToArray();

        var backSelected = _settingsSelection == SettingsCatalog.All.Count;
        var footer =
            backSelected
                ? "> POWROT <    ENTER"
                : "STRZALKI / W S  WYBOR    LEWO PRAWO / ENTER  ZMIANA    * MSAA PO RESTARCIE    ESC  POWROT";

        return new MenuView(
            "USTAWIENIA",
            "KAZDA NOWA OPCJA GRAFIKI TRAFIA DO TEGO PANELU",
            panels,
            footer);
    }

    private static int Wrap(int value, int count)
    {
        if (count <= 0) return 0;
        var result = value % count;
        return result < 0 ? result + count : result;
    }

    private static string CategoryName(SettingCategory category) =>
        category switch
        {
            SettingCategory.Display => "EKRAN",
            SettingCategory.Controls => "STEROWANIE",
            SettingCategory.Graphics => "GRAFIKA",
            SettingCategory.PostProcessing => "POST FX",
            _ => category.ToString().ToUpperInvariant()
        };
}
