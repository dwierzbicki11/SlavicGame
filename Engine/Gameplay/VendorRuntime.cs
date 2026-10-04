using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum VendorMode
{
    Buy,
    Sell
}

public enum VendorTransactionResult
{
    None,
    Completed,
    Closed,
    Unavailable,
    OutOfStock,
    MissingItem,
    NotEnoughMoney,
    Locked
}

public sealed record VendorOffer(
    string ItemId,
    string DisplayName,
    int BaseBuyPrice,
    int BaseSellPrice,
    int InitialStock,
    bool PlayerMaySell,
    string? RequiredFlag = null);

public sealed record VendorDefinition(
    string Id,
    string NpcId,
    string DisplayName,
    string RequiredActivity,
    IReadOnlyList<VendorOffer> Offers);

public sealed record VendorStockEntry(
    string ItemId,
    int Quantity);

public sealed record VendorStockSnapshot(
    string VendorId,
    VendorStockEntry[] Stock);

public sealed record VendorViewLine(
    string ItemId,
    string DisplayName,
    int Price,
    int Quantity,
    bool Available);

public sealed class VendorRuntime
{
    private static readonly VendorDefinition[] Definitions =
    [
        new(
            "vendor.r0.trader",
            "settler-trader-01",
            "HANDLARZ ZARNOWCA",
            "market-trade",
            [
                new("simple-bandage", "OPATRUNEK", 8, 3, 6, true),
                new("arrow-basic", "STRZALA", 2, 1, 24, true),
                new("forest-resin", "ZYWICA LESNA", 5, 2, 4, true)
            ]),
        new(
            "vendor.r0.herbalist",
            "herbalist",
            "ZIELARKA",
            "trade-and-prepare",
            [
                new("marsh-herb", "ZIELE MOKRADEL", 5, 2, 8, true),
                new("simple-bandage", "OPATRUNEK", 7, 3, 6, true),
                new(
                    "marsh-sight-tonic",
                    "NAPAR TROPICIELA",
                    18,
                    7,
                    2,
                    false,
                    "recipe.marsh-sight-tonic.learned")
            ])
    ];

    private static readonly HashSet<string> QuestProtectedItems =
        new(StringComparer.Ordinal)
        {
            "missing-person-keepsake",
            "ritual-thread"
        };

    private readonly Dictionary<string, Dictionary<string, int>> _stock =
        new(StringComparer.Ordinal);

    public bool IsOpen { get; private set; }
    public string? VendorId { get; private set; }
    public VendorMode Mode { get; private set; } = VendorMode.Buy;
    public int SelectedIndex { get; private set; }
    public string Message { get; private set; } = "";

    public string DisplayName =>
        CurrentDefinition?.DisplayName ?? "HANDEL";

    public static IReadOnlyList<VendorDefinition> Catalog => Definitions;

    public void Initialize()
    {
        _stock.Clear();
        foreach (var vendor in Definitions)
        {
            _stock[vendor.Id] = vendor.Offers.ToDictionary(
                offer => offer.ItemId,
                offer => offer.InitialStock,
                StringComparer.Ordinal);
        }

        Close();
    }

    public bool CanOpenNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        return FindNearestAvailable(world) is not null;
    }

    public bool TryOpenNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var definition = FindNearestAvailable(world);
        if (definition is null)
        {
            Message = "BRAK DOSTEPNEGO HANDLARZA W POBLIZU";
            return false;
        }

        EnsureVendorStock(definition);
        VendorId = definition.Id;
        Mode = VendorMode.Buy;
        SelectedIndex = 0;
        IsOpen = true;
        Message = "";
        ClampSelection(world);
        return true;
    }

    public void Close()
    {
        IsOpen = false;
        VendorId = null;
        SelectedIndex = 0;
        Mode = VendorMode.Buy;
        Message = "";
    }

    public void MoveSelection(WorldState world, int direction)
    {
        if (!IsOpen || direction == 0)
            return;

        var lines = BuildLines(world);
        if (lines.Count == 0)
        {
            SelectedIndex = 0;
            return;
        }

        SelectedIndex =
            (SelectedIndex + Math.Sign(direction) + lines.Count) %
            lines.Count;
    }

    public void SetMode(WorldState world, VendorMode mode)
    {
        if (!IsOpen)
            return;

        Mode = mode;
        SelectedIndex = 0;
        Message = "";
        ClampSelection(world);
    }

    public void ToggleMode(WorldState world, int direction)
    {
        if (!IsOpen || direction == 0)
            return;

        SetMode(
            world,
            Mode == VendorMode.Buy
                ? VendorMode.Sell
                : VendorMode.Buy);
    }

    public VendorTransactionResult Confirm(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsOpen || CurrentDefinition is not { } vendor)
            return VendorTransactionResult.None;

        if (!IsVendorAvailable(world, vendor, requireNearby: true))
        {
            Message = "HANDLARZ NIE JEST JUZ DOSTEPNY";
            Close();
            return VendorTransactionResult.Unavailable;
        }

        var lines = BuildLines(world);
        if (lines.Count == 0)
        {
            Message = Mode == VendorMode.Buy
                ? "BRAK TOWARU"
                : "NIE MASZ NIC DO SPRZEDANIA";
            return VendorTransactionResult.None;
        }

        SelectedIndex = Math.Clamp(SelectedIndex, 0, lines.Count - 1);
        var line = lines[SelectedIndex];
        var offer = vendor.Offers.First(item =>
            string.Equals(item.ItemId, line.ItemId, StringComparison.Ordinal));

        return Mode == VendorMode.Buy
            ? Buy(world, vendor, offer)
            : Sell(world, vendor, offer);
    }

    public IReadOnlyList<VendorViewLine> BuildLines(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsOpen || CurrentDefinition is not { } vendor)
            return [];

        EnsureVendorStock(vendor);
        var result = new List<VendorViewLine>();

        foreach (var offer in vendor.Offers)
        {
            if (!IsUnlocked(world, offer))
                continue;

            if (Mode == VendorMode.Buy)
            {
                var stock = StockOf(vendor.Id, offer.ItemId);
                result.Add(new VendorViewLine(
                    offer.ItemId,
                    offer.DisplayName,
                    BuyPrice(world, offer),
                    stock,
                    stock > 0));
            }
            else if (offer.PlayerMaySell &&
                     !QuestProtectedItems.Contains(offer.ItemId))
            {
                var owned = world.Progress.Inventory.Count(offer.ItemId);
                if (owned <= 0)
                    continue;

                result.Add(new VendorViewLine(
                    offer.ItemId,
                    offer.DisplayName,
                    SellPrice(world, offer),
                    owned,
                    true));
            }
        }

        return result;
    }

    public int BuyPrice(WorldState world, VendorOffer offer)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(offer);

        var reputation = world.Progress.Reputation.Get(
            ReputationScope.Village,
            "old-village");
        var factor = 1f - reputation * 0.001f;
        return Math.Max(
            1,
            (int)MathF.Ceiling(offer.BaseBuyPrice * factor));
    }

    public int SellPrice(WorldState world, VendorOffer offer)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(offer);

        var reputation = world.Progress.Reputation.Get(
            ReputationScope.Village,
            "old-village");
        var factor = 1f + reputation * 0.0005f;
        return Math.Max(
            1,
            (int)MathF.Floor(offer.BaseSellPrice * factor));
    }

    public VendorStockSnapshot[] Capture()
    {
        return Definitions
            .Select(vendor =>
            {
                EnsureVendorStock(vendor);
                return new VendorStockSnapshot(
                    vendor.Id,
                    _stock[vendor.Id]
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair =>
                            new VendorStockEntry(pair.Key, pair.Value))
                        .ToArray());
            })
            .ToArray();
    }

    public void Restore(IEnumerable<VendorStockSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        Initialize();

        foreach (var snapshot in snapshots)
        {
            var definition = Definitions.FirstOrDefault(vendor =>
                string.Equals(
                    vendor.Id,
                    snapshot.VendorId,
                    StringComparison.Ordinal));
            if (definition is null)
                continue;

            var stock = _stock[definition.Id];
            foreach (var entry in snapshot.Stock ?? [])
            {
                if (entry.Quantity < 0)
                    throw new ArgumentException(
                        "Vendor stock cannot be negative.",
                        nameof(snapshots));

                if (definition.Offers.Any(offer =>
                    string.Equals(
                        offer.ItemId,
                        entry.ItemId,
                        StringComparison.Ordinal)))
                {
                    stock[entry.ItemId] = entry.Quantity;
                }
            }
        }

        Close();
    }

    public int StockOf(string vendorId, string itemId)
    {
        if (!_stock.TryGetValue(vendorId, out var stock))
            return 0;
        return stock.GetValueOrDefault(itemId);
    }

    private VendorTransactionResult Buy(
        WorldState world,
        VendorDefinition vendor,
        VendorOffer offer)
    {
        if (!IsUnlocked(world, offer))
        {
            Message = "TOWAR JESZCZE NIEDOSTEPNY";
            return VendorTransactionResult.Locked;
        }

        var stock = StockOf(vendor.Id, offer.ItemId);
        if (stock <= 0)
        {
            Message = "BRAK W MAGAZYNIE";
            return VendorTransactionResult.OutOfStock;
        }

        var price = BuyPrice(world, offer);
        if (world.Progress.Profile.Money < price)
        {
            Message = $"BRAK PIENIEDZY / POTRZEBA {price}";
            return VendorTransactionResult.NotEnoughMoney;
        }

        world.Progress.Profile.ChangeMoney(-price);
        world.Progress.Inventory.Add(offer.ItemId);
        _stock[vendor.Id][offer.ItemId] = stock - 1;
        Message = $"KUPIONO {offer.DisplayName} / -{price}";
        ClampSelection(world);
        return VendorTransactionResult.Completed;
    }

    private VendorTransactionResult Sell(
        WorldState world,
        VendorDefinition vendor,
        VendorOffer offer)
    {
        if (!offer.PlayerMaySell ||
            QuestProtectedItems.Contains(offer.ItemId))
        {
            Message = "TEGO PRZEDMIOTU NIE MOZNA SPRZEDAC";
            return VendorTransactionResult.Locked;
        }

        if (!world.Progress.Inventory.Contains(offer.ItemId))
        {
            Message = "NIE MASZ TEGO PRZEDMIOTU";
            return VendorTransactionResult.MissingItem;
        }

        var price = SellPrice(world, offer);
        if (!world.Progress.Inventory.Remove(offer.ItemId))
        {
            Message = "SPRZEDAZ NIEUDANA";
            return VendorTransactionResult.MissingItem;
        }

        world.Progress.Profile.ChangeMoney(price);
        _stock[vendor.Id][offer.ItemId] =
            checked(StockOf(vendor.Id, offer.ItemId) + 1);
        Message = $"SPRZEDANO {offer.DisplayName} / +{price}";
        ClampSelection(world);
        return VendorTransactionResult.Completed;
    }

    private VendorDefinition? FindNearestAvailable(WorldState world)
    {
        var candidates = Definitions
            .Where(vendor =>
                IsVendorAvailable(world, vendor, requireNearby: true))
            .Select(vendor =>
            {
                var actor = world.NpcWorld.Find(vendor.NpcId)!;
                var delta = actor.Position - world.PlayerPosition;
                delta.Y = 0f;
                return (Vendor: vendor, DistanceSquared: delta.LengthSquared());
            })
            .OrderBy(item => item.DistanceSquared)
            .ToArray();

        return candidates.FirstOrDefault().Vendor;
    }

    private static bool IsVendorAvailable(
        WorldState world,
        VendorDefinition vendor,
        bool requireNearby)
    {
        var actor = world.NpcWorld.Find(vendor.NpcId);
        if (actor is null ||
            !string.Equals(
                actor.Activity,
                vendor.RequiredActivity,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!requireNearby)
            return true;

        var delta = actor.Position - world.PlayerPosition;
        delta.Y = 0f;
        return delta.LengthSquared() <= 4.25f * 4.25f;
    }

    private static bool IsUnlocked(
        WorldState world,
        VendorOffer offer) =>
        string.IsNullOrWhiteSpace(offer.RequiredFlag) ||
        world.Progress.HasFlag(offer.RequiredFlag);

    private VendorDefinition? CurrentDefinition =>
        Definitions.FirstOrDefault(vendor =>
            string.Equals(
                vendor.Id,
                VendorId,
                StringComparison.Ordinal));

    private void EnsureVendorStock(VendorDefinition vendor)
    {
        if (_stock.ContainsKey(vendor.Id))
            return;

        _stock[vendor.Id] = vendor.Offers.ToDictionary(
            offer => offer.ItemId,
            offer => offer.InitialStock,
            StringComparer.Ordinal);
    }

    private void ClampSelection(WorldState world)
    {
        var count = BuildLines(world).Count;
        SelectedIndex = count == 0
            ? 0
            : Math.Clamp(SelectedIndex, 0, count - 1);
    }
}
