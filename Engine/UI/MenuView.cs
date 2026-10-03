namespace SlavicGame.Engine.UI;

public enum FrontendScreen
{
    MainMenu,
    Settings,
    Playing
}

public sealed record MenuItemView(
    string Label,
    string? Value,
    bool Selected,
    bool RequiresRestart = false);

public sealed record MenuPanelView(
    string Title,
    IReadOnlyList<MenuItemView> Items);

public sealed record MenuTabView(
    string Label,
    bool Active);

public sealed record MenuView(
    string Title,
    string Subtitle,
    IReadOnlyList<MenuTabView> Tabs,
    IReadOnlyList<MenuPanelView> Panels,
    string Footer);
