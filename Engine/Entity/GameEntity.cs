namespace SlavicGame.Engine.Entity;

public interface IGameComponent
{
}

public sealed class GameEntity
{
    private readonly Dictionary<Type, IGameComponent> _components = [];

    public string Id { get; }
    public bool Active { get; set; } = true;
    public IReadOnlyCollection<IGameComponent> Components => _components.Values;

    public GameEntity(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public void Add<T>(T component) where T : class, IGameComponent
    {
        ArgumentNullException.ThrowIfNull(component);
        _components[typeof(T)] = component;
    }

    public bool Remove<T>() where T : class, IGameComponent =>
        _components.Remove(typeof(T));

    public bool TryGet<T>(out T? component) where T : class, IGameComponent
    {
        if (_components.TryGetValue(typeof(T), out var stored))
        {
            component = (T)stored;
            return true;
        }

        component = null;
        return false;
    }

    public T GetRequired<T>() where T : class, IGameComponent =>
        TryGet<T>(out var component)
            ? component!
            : throw new KeyNotFoundException($"Entity '{Id}' does not contain component {typeof(T).Name}.");
}
