namespace SlavicGame.Engine.Assets;

public readonly record struct AssetId(string Value)
{
    public override string ToString() => Value;

    public static AssetId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new AssetId(value.Trim());
    }
}

public sealed record AssetDescriptor(
    AssetId Id,
    string Path,
    string Kind);

public sealed class AssetCatalog
{
    private readonly Dictionary<AssetId, AssetDescriptor> _assets = [];

    public IReadOnlyCollection<AssetDescriptor> Assets => _assets.Values;

    public void Register(AssetDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (string.IsNullOrWhiteSpace(descriptor.Path) || string.IsNullOrWhiteSpace(descriptor.Kind))
        {
            throw new ArgumentException("Asset path and kind are required.", nameof(descriptor));
        }

        if (!_assets.TryAdd(descriptor.Id, descriptor))
        {
            throw new InvalidOperationException($"Asset '{descriptor.Id}' is already registered.");
        }
    }

    public AssetDescriptor Get(AssetId id) =>
        _assets.TryGetValue(id, out var descriptor)
            ? descriptor
            : throw new KeyNotFoundException($"Asset '{id}' is not registered.");
}
