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
VerticalSliceQuestInteractionRegression.Run(Check);
SwampPredatorEncounterRegression.Run(Check);
RiverInteractionRegression.Run(Check);
LootContainerUiRegression.Run(Check);

// Regression suites below exercise the rest of the engine. This marker is intentionally
// kept near the suite bootstrap; the existing test body follows in main.
