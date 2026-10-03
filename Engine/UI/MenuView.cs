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
    bool Selected);

public sealed record MenuPanelView(
    string Title,
    IReadOnlyList<MenuItemView> Items);

public sealed record MenuView(
    string Title,
    string Subtitle,
    IReadOnlyList<MenuPanelView> Panels,
    string Footer);
