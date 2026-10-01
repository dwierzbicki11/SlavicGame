using SlavicGame.Engine.Entity;

namespace SlavicGame.Engine.Scene;

public sealed class GameScene
{
    private readonly Dictionary<string, GameEntity> _entities = new(StringComparer.Ordinal);

    public string Id { get; }
    public IReadOnlyCollection<GameEntity> Entities => _entities.Values;

    public GameScene(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public void Add(GameEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_entities.TryAdd(entity.Id, entity))
        {
            throw new InvalidOperationException($"Entity '{entity.Id}' already exists in scene '{Id}'.");
        }
    }

    public bool Remove(string entityId) => _entities.Remove(entityId);

    public GameEntity? Find(string entityId) =>
        _entities.GetValueOrDefault(entityId);
}

public sealed class SceneManager
{
    private readonly Dictionary<string, GameScene> _scenes = new(StringComparer.Ordinal);

    public GameScene? Current { get; private set; }
    public IReadOnlyCollection<GameScene> Scenes => _scenes.Values;

    public void Register(GameScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (!_scenes.TryAdd(scene.Id, scene))
        {
            throw new InvalidOperationException($"Scene '{scene.Id}' is already registered.");
        }
    }

    public GameScene Load(string sceneId)
    {
        if (!_scenes.TryGetValue(sceneId, out var scene))
        {
            throw new KeyNotFoundException($"Scene '{sceneId}' is not registered.");
        }

        Current = scene;
        return scene;
    }
}
