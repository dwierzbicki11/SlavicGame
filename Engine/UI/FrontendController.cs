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

    private static readonly SettingCategory[] SettingsTabs =
    [
        SettingCategory.Display,
        SettingCategory.Graphics,
        SettingCategory.PostProcessing,
        SettingCategory.Controls
    ];

    private int _mainSelection;
    private int _settingsSelection;
    private int _settingsTabIndex;

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
                    _settingsTabIndex = 0;
                    return FrontendAction.None;
                case 2:
                    return FrontendAction.Exit;
            }
        }

        if (Screen == FrontendScreen.Settings)
        {
            if (window.ConsumeKeyPress(Key.Escape))
            {
                Screen = FrontendScreen.MainMenu;
                return FrontendAction.None;
            }

            if (window.ConsumeKeyPress(Key.Tab))
            {
                _settingsTabIndex = Wrap(
                    _settingsTabIndex + 1,
                    SettingsTabs.Length);
                _settingsSelection = 0;
                return FrontendAction.None;
            }

            var category = SettingsTabs[_settingsTabIndex];
            var definitions = SettingsCatalog.All
                .Where(item => item.Category == category)
                .ToArray();
            var count = definitions.Length + 1;

            if (up)
                _settingsSelection = Wrap(_settingsSelection - 1, count);
            if (down)
                _settingsSelection = Wrap(_settingsSelection + 1, count);

            if (_settingsSelection == definitions.Length)
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
                definitions[_settingsSelection].Change(settings, direction);
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
                Array.Empty<MenuTabView>(),
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

        var activeCategory = SettingsTabs[_settingsTabIndex];
        var definitions = SettingsCatalog.All
            .Where(item => item.Category == activeCategory)
            .ToArray();

        var items = definitions
            .Select((definition, index) =>
                new MenuItemView(
                    definition.Label,
                    definition.ValueText(settings),
                    index == _settingsSelection))
            .ToList();

        items.Add(new MenuItemView(
            "POWROT",
            null,
            _settingsSelection == definitions.Length));

        var tabs = SettingsTabs
            .Select((category, index) =>
                new MenuTabView(
                    CategoryName(category),
                    index == _settingsTabIndex))
            .ToArray();

        return new MenuView(
            "USTAWIENIA",
            "TAB  ZMIANA ZAKLADKI",
            tabs,
            [
                new MenuPanelView(
                    CategoryName(activeCategory),
                    items)
            ],
            "TAB  ZAKLADKA    STRZALKI / W S  WYBOR    LEWO PRAWO / ENTER  ZMIANA    ESC  POWROT");
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
            SettingCategory.PostProcessing => "EFEKTY",
            _ => category.ToString().ToUpperInvariant()
        };
}
