using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Menus;

// A piece for sale is compared in the first slot its kind fits (decision 383): a ring against the first ring slot.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentComparisonTests
{
    [Fact]
    public void ShowPiece_Ring_ComparesInTheFirstRingSlot()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var leader = session.Party.Leader;
        var ring = StartMapScenarios.BelowClosedDoor.Items["power_ring"];
        var worn = GearPiece.Rolled(ring, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 5)]);
        session.Inventory.AddGear(worn);
        Assert.True(session.Equipment.TryEquip(leader, worn, EquipmentSlot.FirstRing));
        var comparison = new EquipmentComparison(RepositoryScreenText.Text.Messages);

        comparison.ShowPiece(leader, GearPiece.Rolled(ring, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 2)]));

        var power = Assert.Single(comparison.Shown!.Changes, change => change.Kind == StatKind.Power);
        Assert.Equal(power.Before - 3, power.After);
    }

    [Fact]
    public void Clear_AfterAPiece_ShowsNothing()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var comparison = new EquipmentComparison(RepositoryScreenText.Text.Messages);
        comparison.ShowPiece(session.Party.Leader, GearPiece.Plain(StartMapScenarios.BelowClosedDoor.Items["bronze_sword"]));

        comparison.Clear();

        Assert.Null(comparison.Shown);
        Assert.Null(comparison.Text);
    }
}
