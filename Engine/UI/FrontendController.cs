using SlavicGame.Engine.Settings;
using SlavicGame.Engine.Windowing;
using Veldrid;

namespace SlavicGame.Engine.UI;

public enum FrontendAction
{
    None,
    StartGame,
    ContinueGame,
    Exit,
    SettingsChanged
}

public sealed class FrontendController
{
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
    private bool _restartRequired;
    private bool _continueAvailable;
    private string? _continueSummary;
    private bool _sessionStarted;

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
            var mainItems = BuildMainItems();

            if (up)
                _mainSelection = Wrap(_mainSelection - 1, mainItems.Length);
            if (down)
                _mainSelection = Wrap(_mainSelection + 1, mainItems.Length);

            if (!confirm)
                return FrontendAction.None;

            switch (mainItems[_mainSelection])
            {
                case "NOWA GRA":
                case "WZNOW":
                    _sessionStarted = true;
                    Screen = FrontendScreen.Playing;
                    return FrontendAction.StartGame;

                case "KONTYNUUJ":
                    _sessionStarted = true;
                    Screen = FrontendScreen.Playing;
                    return FrontendAction.ContinueGame;

                case "USTAWIENIA":
                    Screen = FrontendScreen.Settings;
                    _settingsSelection = 0;
                    _settingsTabIndex = 0;
                    return FrontendAction.None;

                case "WYJSCIE":
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
                var definition = definitions[_settingsSelection];
                definition.Change(settings, direction);
                _restartRequired |= definition.RequiresRestart;
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

    public void SetContinueInfo(
        bool available,
        string? summary = null)
    {
        _continueAvailable = available;
        _continueSummary = available ? summary : null;
        _mainSelection = 0;
    }

    public void CancelLoadedSession()
    {
        _sessionStarted = false;
        OpenMainMenu();
    }

    public MenuView BuildView(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (Screen == FrontendScreen.MainMenu)
        {
            var mainItems = BuildMainItems();
            var subtitle = _sessionStarted
                ? "GRA WSTRZYMANA"
                : !string.IsNullOrWhiteSpace(_continueSummary)
                    ? _continueSummary!
                    : "SLOWIANSKI ACTION RPG";

            return new MenuView(
                "SLAVICGAME",
                subtitle,
                Array.Empty<MenuTabView>(),
                [
                    new MenuPanelView(
                        string.Empty,
                        mainItems
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
                    index == _settingsSelection,
                    definition.RequiresRestart))
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

        var footer = _restartRequired
            ? "RESTART WYMAGANY DLA MSAA / PELNEJ JAKOSCI TEKSTUR"
            : "TAB ZAKLADKA   W/S WYBOR   A/D LUB ENTER ZMIANA   ESC POWROT";

        return new MenuView(
            "USTAWIENIA",
            "TAB  ZMIANA ZAKLADKI",
            tabs,
            [
                new MenuPanelView(
                    CategoryName(activeCategory),
                    items)
            ],
            footer);
    }

    private string[] BuildMainItems()
    {
        if (_sessionStarted)
        {
            return
            [
                "WZNOW",
                "USTAWIENIA",
                "WYJSCIE"
            ];
        }

        return _continueAvailable
            ?
            [
                "NOWA GRA",
                "KONTYNUUJ",
                "USTAWIENIA",
                "WYJSCIE"
            ]
            :
            [
                "NOWA GRA",
                "USTAWIENIA",
                "WYJSCIE"
            ];
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
