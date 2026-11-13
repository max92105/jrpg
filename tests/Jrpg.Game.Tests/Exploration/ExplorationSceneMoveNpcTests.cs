using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

// The scene moves the cutscene walks on every update (ADR 0044): the villager at (12, 4) walks two cells left while the
// event waits, and the dialogue after the move shows once it arrived.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ExplorationSceneMoveNpcTests
{
    private static readonly MoveNpcStep _walkLeft = new("villager_1", [Facing.Left, Facing.Left]);
    private static readonly DialogueStep _greeting = new(new Speech(["Over here."]));

    [Fact]
    public void Update_DuringMove_AdvancesTheWalk()
    {
        var (exploration, scene) = Start();
        Assert.True(exploration.TryInteract());

        for (var update = 0; update < 10; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
        }

        var pose = exploration.NpcPoseOf(Villager(exploration));
        Assert.True(pose.IsWalking);
        Assert.InRange(pose.Position.X, 10, 11.99);
        Assert.Same(_walkLeft, exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_UntilArrived_ContinuesToTheDialogueAfterTheMove()
    {
        var (exploration, scene) = Start();
        Assert.True(exploration.TryInteract());

        for (var update = 0; update < 600 && exploration.CurrentEventStep is MoveNpcStep; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.Same(_greeting, exploration.CurrentEventStep);
        Assert.Equal(new Point2D(10, 4), exploration.NpcPoseOf(Villager(exploration)).Position);
    }

    private static PlacedObject Villager(ExplorationState exploration) =>
        exploration.CurrentMap.Layout.Objects.Single(placed => placed.Uid == "villager_1");

    private static (ExplorationState Exploration, ExplorationScene Scene) Start()
    {
        var catalog = StartMapScenarios.WithVillagerSaying(StartMapScenarios.BelowVillager, _walkLeft, _greeting);
        var exploration = StartMapScenarios.StartExploring(catalog);
        return (exploration, StartMapScenarios.SceneOf(exploration, catalog));
    }
}
