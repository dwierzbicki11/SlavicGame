using System.Numerics;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.World;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Animation;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Audio;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Dialogue;
using SlavicGame.Engine.Entity;
using SlavicGame.Engine.Gods;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Inventory;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.Physics;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.Scene;
using SlavicGame.Engine.UI;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    checks++;
}
MagicCinematicRegression.Run(Check);
FidelityFxStartupRegression.Run(Check);
FidelityFxFenceRegression.Run(Check);
VerticalSliceQuestInteractionRegression.Run(Check);
SwampPredatorEncounterRegression.Run(Check);
SwampApparitionRegression.Run(Check);
WorldItemVisualRegression.Run(Check);
RiverInteractionRegression.Run(Check);
WeatherVisualRegression.Run(Check);
CampfireInteractionRegression.Run(Check);
FootprintTrailRegression.Run(Check);
NpcDialogueRegression.Run(Check);
NpcWeatherBehaviorRegression.Run(Check);
NpcCrowdSteeringRegression.Run(Check);
NpcReactionRegression.Run(Check);
NpcWorkstationRegression.Run(Check);
VendorRegression.Run(Check);
CraftingRegression.Run(Check);
MissingToolsSideQuestRegression.Run(Check);
R0FordSideQuestRegression.Run(Check);
WildlifeRegression.Run(Check);
WildlifeHuntingRegression.Run(Check);
WildlifeTrackingRegression.Run(Check);
BowCombatRegression.Run(Check);
LootContainerDepositRegression.Run(Check);
EnemyRenderedHitRegression.Run(Check);
EnemyAttackTelegraphRegression.Run(Check);
PlayerDodgeRegression.Run(Check);
PlayerJumpRegression.Run(Check);

// River terrain and water must form one sloped channel; animation has to read downstream.
var riverTerrain = new Terrain(513, 513, 4f);
var riverZ = 0f;
var riverCenterX = WaterLandscape.CenterX(riverZ);
var riverPoint = new Vector3(riverCenterX, 0f, riverZ);
var riverWaterLevel = WaterLandscape.WaterLevel(riverZ);

Check(riverTerrain.SampleHeight(riverPoint) <= riverWaterLevel - 0.9f,
    "river center is carved well below its local water surface");
Check(WaterLandscape.ShapeHeight(
        riverCenterX,
        riverZ,
        2f) <= riverWaterLevel - WaterLandscape.BedDepth + 0.001f,
    "river profile contains a real central bed");
Check(WaterLandscape.ShapeHeight(
        riverCenterX + WaterLandscape.SurfaceHalfWidth(riverZ) + 4f,
        riverZ,
        2f) > riverWaterLevel,
    "dry bank rises above the water instead of being forced underneath it");
Check(WaterLandscape.ShapeHeight(0f, 0f, 4f) == 4f,
    "river preserves distant spawn terrain");
Check(WaterLandscape.WaterLevel(500f) < WaterLandscape.WaterLevel(-500f),
    "river water level falls in the downstream +Z direction");
Check(WaterLandscape.FlowDirection(0f).Y > 0.8f,
    "river flow vector points downstream");

TerrainVertex[] waterVertices = [];
uint[] waterIndices = [];
WaterLandscape.AppendSurface(
    riverTerrain,
    0f,
    riverPoint,
    100f,
    ref waterVertices,
    ref waterIndices);
Check(waterVertices.Length > 0 && waterIndices.Length > 0,
    "near river rendered");
Check(waterVertices.All(v =>
        MathF.Abs(v.Position.Y - WaterLandscape.WaterLevel(v.Position.Z)) < 0.07f),
    "water and foam presentation follow the local downstream level inside the terrain channel");
Check(waterIndices.All(index => index < waterVertices.Length),
    "water indices valid");

TerrainVertex[] animatedWaterVertices = [];
uint[] animatedWaterIndices = [];
WaterLandscape.AppendSurface(
    riverTerrain,
    0.75f,
    riverPoint,
    100f,
    ref animatedWaterVertices,
    ref animatedWaterIndices);
Check(animatedWaterVertices.Length == waterVertices.Length &&
      animatedWaterVertices.Zip(waterVertices).Any(pair =>
          Vector3.DistanceSquared(pair.First.Position, pair.Second.Position) > 0.000001f ||
          Vector3.DistanceSquared(pair.First.Color, pair.Second.Color) > 0.000001f),
    "river surface visibly animates downstream over time");

waterVertices = [];
waterIndices = [];
WaterLandscape.AppendSurface(
    riverTerrain,
    0f,
    new Vector3(-900f, 0f, 0f),
    100f,
    ref waterVertices,
    ref waterIndices);
Check(waterVertices.Length == 0, "distant river culled");
void Near(float actual, float expected, string name) => Check(MathF.Abs(actual - expected) < 0.0001f, name);
void Reject(Action action, string name)
{
    try { action(); } catch (ArgumentOutOfRangeException) { checks++; return; }
    throw new Exception(name);
}

// Independent intersection oracle using the triangles submitted to the renderer.
float? MeshGroundHit(Terrain testTerrain, Vector3 start, Vector3 end, float clearance)
{
    TerrainMesh.Build(testTerrain, out var meshVertices, out var meshIndices);
    var direction = end - start;
    float? firstHit = null;
    for (var i = 0; i < meshIndices.Length; i += 3)
    {
        var a = meshVertices[meshIndices[i]].Position + Vector3.UnitY * clearance;
        var b = meshVertices[meshIndices[i + 1]].Position + Vector3.UnitY * clearance;
        var c = meshVertices[meshIndices[i + 2]].Position + Vector3.UnitY * clearance;
        var edge1 = b - a;
        var edge2 = c - a;
        var p = Vector3.Cross(direction, edge2);
        var determinant = Vector3.Dot(edge1, p);
        if (MathF.Abs(determinant) < 0.000001f) continue;
        var inverse = 1f / determinant;
        var offset = start - a;
        var u = Vector3.Dot(offset, p) * inverse;
        var q = Vector3.Cross(offset, edge1);
        var v = Vector3.Dot(direction, q) * inverse;
        var t = Vector3.Dot(edge2, q) * inverse;
        if (u < 0f || v < 0f || u + v > 1f || t < 0f || t > 1f) continue;
        if (firstHit is null || t < firstHit) firstHit = t;
    }
    return firstHit;
}


// Architecture foundations used by the documented RPG systems.
var entity = new GameEntity("test-entity");
var animationComponent = new TestComponent("component");
entity.Add(animationComponent);
Check(entity.GetRequired<TestComponent>().Value == "component", "Entity component storage");
var scene = new GameScene("jawia-test");
scene.Add(entity);
var sceneManager = new SceneManager();
sceneManager.Register(scene);
Check(sceneManager.Load("jawia-test").Find("test-entity") == entity, "Scene registration and lookup");

var catalog = new AssetCatalog();
var assetId = AssetId.Parse("models/test");
catalog.Register(new AssetDescriptor(assetId, "Assets/test.glb", "model"));
Check(catalog.Get(assetId).Path == "Assets/test.glb", "Asset catalog lookup");
AssetIntegrationRegression.Run(Check);
PresentationPolicyRegression.Run(Check);
CelestialLightingRegression.Run(Check);
TemporalFrameRegression.Run(Check);
DynamicMotionHistoryRegression.Run(Check);
AudioAssetRegression.Run(Check);
FrontendSettingsRegression.Run(Check);
SettingsPersistenceRegression.Run(Check);
CameraFrustumRegression.Run(Check);

var animation = new AnimationStateMachine();
animation.Register("idle");
animation.Play("idle");
animation.Advance(0.5, 1.0);
Near(animation.NormalizedTime, 0.5f, "Animation state advances deterministically");

var audioSettings = new AudioSettings();
audioSettings.SetVolume(AudioBus.Music, 0.4f);
Near(audioSettings.GetVolume(AudioBus.Music), 0.4f, "Audio bus settings");

var uiState = new GameUiState();
uiState.Open(UiScreen.Inventory);
Check(uiState.BlocksMovement, "Inventory screen blocks movement");
uiState.Close();

var nearestInteraction = InteractionSystem.FindNearest(
    Vector3.Zero,
    [
        new InteractionTarget("far", new Vector3(5, 0, 0), InteractionKind.Inspect, "Inspect"),
        new InteractionTarget("near", new Vector3(1, 0, 0), InteractionKind.Talk, "Talk")
    ],
    3f);
Check(nearestInteraction?.Id == "near", "Nearest interaction target selected");

var inventory = new InventoryState();
inventory.Add("herb", 3);
Check(inventory.Remove("herb", 2) && inventory.Count("herb") == 1, "Inventory add and remove");
var inventorySnapshot = inventory.Capture();
inventory.Add("herb", 4);
inventory.Restore(inventorySnapshot);
Check(inventory.Count("herb") == 1, "Inventory snapshot restore");

var pickupState = new PickupInteractionState();
var pickupTarget = new InteractionTarget("pickup-marsh-herb-01", new Vector3(1, 0, 0), InteractionKind.Take, "Podnies ziolo");
var pickup = new PickupDefinition(pickupTarget.Id, "marsh-herb", 2, "quest:marsh-herb-collected");
string? emittedQuestEvent = null;
Check(pickupState.TryCollect(pickupTarget, pickup, inventory, id => emittedQuestEvent = id) == PickupResult.Collected,
    "Take interaction moves world pickup into inventory");
Check(inventory.Count("marsh-herb") == 2 && emittedQuestEvent == "quest:marsh-herb-collected",
    "Pickup emits quest integration event after collection");
Check(pickupState.TryCollect(pickupTarget, pickup, inventory) == PickupResult.AlreadyCollected && inventory.Count("marsh-herb") == 2,
    "World pickup cannot be collected twice");
var pickupSnapshot = pickupState.Capture();
var restoredPickupState = new PickupInteractionState();
restoredPickupState.Restore(pickupSnapshot);
Check(restoredPickupState.IsCollected(pickupTarget.Id), "Pickup consumed state survives save restore");
Check(restoredPickupState.TryCollect(pickupTarget with { Id = "other" }, pickup, inventory) == PickupResult.InvalidTarget,
    "Pickup definition cannot collect a mismatched interaction target");

var lootState = new LootContainerInteractionState();
var lootTarget = new InteractionTarget("chest-old-village-01", new Vector3(1, 0, 0), InteractionKind.Inspect, "Przeszukaj skrzynie");
var lootDefinition = new LootContainerDefinition(lootTarget.Id,
    [new LootStack("marsh-herb", 3), new LootStack("old-coin", 2)],
    "quest:old-chest-looted");
string? lootQuestEvent = null;
Check(lootState.LootAll(lootTarget, lootDefinition, inventory, id => lootQuestEvent = id) == LootContainerResult.Looted,
    "Loot container transfers all stacks into inventory");
Check(inventory.Count("marsh-herb") == 5 && inventory.Count("old-coin") == 2 && lootQuestEvent == "quest:old-chest-looted",
    "Loot container updates inventory and quest integration event");
Check(lootState.LootAll(lootTarget, lootDefinition, inventory) == LootContainerResult.Empty && inventory.Count("old-coin") == 2,
    "Loot container cannot duplicate consumed contents");
var lootSnapshot = lootState.Capture();
var restoredLootState = new LootContainerInteractionState();
restoredLootState.Restore(lootSnapshot);
Check(restoredLootState.LootAll(lootTarget, lootDefinition, inventory) == LootContainerResult.Empty,
    "Empty loot container state survives save restore");
Check(restoredLootState.LootAll(lootTarget with { Id = "other" }, lootDefinition, inventory) == LootContainerResult.InvalidTarget,
    "Loot container definition rejects mismatched interaction target");

var questJournal = new QuestJournal();
var contract = questJournal.Get(VerticalSliceBootstrap.ContractQuestId);
contract.SetPhase(QuestPhase.Investigation);
Check(contract.AddEvidence(new EvidenceEntry(
    "witness-light",
    contract.Id,
    KnowledgeKind.Rumor,
    "A witness saw a light above the marsh.",
    "missing-family")), "Quest accepts unique rumor evidence");
Check(contract.AddEvidence(new EvidenceEntry(
    "broken-crossing",
    contract.Id,
    KnowledgeKind.Observation,
    "The crossing is physically damaged.")), "Quest accepts observation evidence");
contract.Resolve(QuestResolution.RitualClosure);
Check(contract.ClaimReward(), "Resolved quest can be turned in");
Check(!contract.ClaimReward(), "Quest reward can only be claimed once");

var reputation = new ReputationSystem();
Check(reputation.Change(ReputationScope.Village, "old-village", 15) == 15,
    "Village reputation changes independently");
var divine = new DivineRelationshipSystem();
divine.Get("perun").SetPatron(true);
divine.Get("veles").SetPatron(true);
Check(divine.Relationships.Count(r => r.IsPatron) == 2,
    "Multiple divine patrons are structurally allowed");
Check(reputation.Get(ReputationScope.ReligiousInstitution, "perun-shrine") == 0,
    "Religious institution reputation remains separate from divine favor");

var relationships = new RelationshipSystem();
relationships.Change("herbalist", RelationshipKind.Trust, 12);
Check(relationships.Get("herbalist", RelationshipKind.Trust) == 12,
    "Character relationship state");

var nightSchedule = new NpcScheduleSlot(19, 6, "old-village", "night-watch");
Check(nightSchedule.Contains(23) && nightSchedule.Contains(2) && !nightSchedule.Contains(12),
    "NPC schedule supports overnight ranges");

var ritual = new RitualDefinition(
    "first-ritual",
    "First ritual",
    MagicSource.SoulsAndNawia,
    "old-shrine",
    20,
    6,
    [new ItemCost("anchor-item", 1)],
    ["broken-crossing"],
    new MagicCost(Stamina: 20));
Check(RitualRules.IsActiveAt(ritual, 23) && RitualRules.IsActiveAt(ritual, 4),
    "Ritual time window supports night crossing midnight");

var attack = new AttackDefinition(
    "basic-sword",
    20,
    12,
    1.8f,
    0.2,
    0.4,
    DamageType.Physical,
    AttackDirection.Right).Validate();
Near(attack.Range, 1.8f, "Combat attack definition validates");

var sphereEvent = new BoundaryPhenomenon(
    "test-leak",
    "black-swamp",
    BoundaryState.Leaking,
    20,
    6);
Check(sphereEvent.IsActive(23) && !sphereEvent.IsActive(12),
    "Boundary phenomenon respects active night window");
sphereEvent.Resolve();
Check(!sphereEvent.IsActive(23), "Resolved boundary phenomenon becomes inactive");

var sphereCollider = new SphereCollider(Vector3.Zero, 0.5f, CollisionLayer.Player, CollisionLayer.World);
Check(sphereCollider.IsValid, "Physics collider contracts validate geometry");

Reject(() => new Terrain(1, 2), "Degenerate width");
Reject(() => new Terrain(2, 1), "Degenerate depth");
Reject(() => new Terrain(2, 2, 0), "Zero cell size");
Reject(() => new Terrain(2, 2, float.NaN), "NaN cell size");
var terrain = new Terrain(7, 9, 2);
TerrainMesh.Build(terrain, out var vertices, out var indices);
Check(vertices.Length == 63 && indices.Length == 288, "Mesh dimensions");
foreach (var vertex in vertices)
    Near(terrain.SampleHeight(vertex.Position), vertex.Position.Y, "Grid height");
for (var i = 0; i < indices.Length; i += 3)
{
    var a = vertices[indices[i]].Position;
    var b = vertices[indices[i + 1]].Position;
    var c = vertices[indices[i + 2]].Position;
    var sample = a * 0.2f + b * 0.3f + c * 0.5f;
    Near(terrain.SampleHeight(sample), sample.Y, "Height on rendered triangle");
}
TerrainMesh.Build(new Terrain(257, 257), out var largeVertices, out var largeIndices);
Check(largeIndices.Max() == largeVertices.Length - 1, "32-bit terrain indices");
TerrainMesh.Build(new Terrain(257, 257), 4, out var lowLodVertices, out var lowLodIndices);
Check(lowLodVertices.Length < largeVertices.Length / 8 &&
      lowLodIndices.Length < largeIndices.Length / 8,
    "Low terrain geometry LOD substantially reduces mesh complexity");

Console.WriteLine($"All {checks} checks passed.");

sealed class TestComponent(string value) : IGameComponent
{
    public string Value { get; } = value;
}
