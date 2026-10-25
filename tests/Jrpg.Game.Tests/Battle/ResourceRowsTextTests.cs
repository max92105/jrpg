using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.Resources;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

// Omer, a mage, acts first with 3 mana of 10; defending ends the turn, which gains 2.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ResourceRowsTextTests
{
    private readonly BattleState _battle =
        BattleScenarios.MemberAgainstGoblinTrio("omer", new Inventory(), new SeededRandom(BattleScenarios.SarxianFirstSeed));

    [Fact]
    public void Refresh_AmountChanged_Reformats()
    {
        var text = new ResourceRowsText(Omer.Resources, RepositoryScreenText.Text.Messages);
        text.Refresh();
        var before = text.AmountAt(0);
        _battle.Execute(new DefendCommand());

        text.Refresh();

        Assert.Equal(("3", "5", "/10"), (before, text.AmountAt(0), text.MaxAt(0)));
    }

    [Fact]
    public void Refresh_AmountUnchanged_KeepsSameString()
    {
        var text = new ResourceRowsText(Omer.Resources, RepositoryScreenText.Text.Messages);
        text.Refresh();
        _battle.Execute(new DefendCommand());
        text.Refresh();
        var afterChange = text.AmountAt(0);

        text.Refresh();

        Assert.Equal("5", afterChange);
        Assert.Same(afterChange, text.AmountAt(0));
    }

    [Fact]
    public void Refresh_TwoResources_FormatsBoth()
    {
        var meters = new ResourceMeters(
            [new ResourceDefinition("growth", "Growth", Max: 10, StartsAt: 0), new ResourceDefinition("decay", "Decay", Max: 8, StartsAt: 2)]);
        var text = new ResourceRowsText(meters, RepositoryScreenText.Text.Messages);

        text.Refresh();

        Assert.Equal(("0", "/10", "2", "/8"), (text.AmountAt(0), text.MaxAt(0), text.AmountAt(1), text.MaxAt(1)));
    }

    private Combatant Omer => _battle.Combatants[0];
}
