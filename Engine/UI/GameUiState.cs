namespace SlavicGame.Engine.UI;

public enum UiScreen
{
    None,
    Inventory,
    Journal,
    Dialogue,
    Map,
    Character,
    Pause
}

public sealed class GameUiState
{
    public UiScreen ActiveScreen { get; private set; }
    public string? InteractionPrompt { get; private set; }
    public string? Notification { get; private set; }

    public bool BlocksMovement =>
        ActiveScreen is UiScreen.Inventory or UiScreen.Journal or UiScreen.Dialogue or
        UiScreen.Map or UiScreen.Character or UiScreen.Pause;

    public void Open(UiScreen screen) => ActiveScreen = screen;

    public void Close() => ActiveScreen = UiScreen.None;

    public void SetInteractionPrompt(string? text) =>
        InteractionPrompt = string.IsNullOrWhiteSpace(text) ? null : text;

    public void SetNotification(string? text) =>
        Notification = string.IsNullOrWhiteSpace(text) ? null : text;
}
