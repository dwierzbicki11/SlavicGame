namespace SlavicGame.Engine.Gameplay;

public sealed class PlayerProfile
{
    private readonly HashSet<string> _titles = new(StringComparer.Ordinal);

    public string Name { get; private set; } = "Hunter";
    public int Money { get; private set; }
    public IReadOnlyCollection<string> Titles => _titles;

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void ChangeMoney(int amount)
    {
        var next = checked(Money + amount);
        if (next < 0)
        {
            throw new InvalidOperationException("Player cannot spend more money than is available.");
        }
        Money = next;
    }

    public void AddTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        _titles.Add(title.Trim());
    }

    public void Restore(string name, int money, IEnumerable<string> titles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(titles);
        if (money < 0) throw new ArgumentOutOfRangeException(nameof(money));
        Name = name;
        Money = money;
        _titles.Clear();
        foreach (var title in titles)
        {
            if (!string.IsNullOrWhiteSpace(title)) _titles.Add(title);
        }
    }
}
