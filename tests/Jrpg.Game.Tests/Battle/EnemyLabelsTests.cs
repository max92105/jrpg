using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class EnemyLabelsTests
{
    [Fact]
    public void Of_ThreeGoblins_LettersInFormationOrder()
    {
        var labels = EnemyLabels.Of(["Goblin", "Goblin", "Goblin"], RepositoryScreenText.Text.Messages);

        Assert.Equal(["Goblin A", "Goblin B", "Goblin C"], labels);
    }

    [Fact]
    public void Of_UniqueNames_NoLetters()
    {
        var labels = EnemyLabels.Of(["Goblin", "Slime"], RepositoryScreenText.Text.Messages);

        Assert.Equal(["Goblin", "Slime"], labels);
    }

    [Fact]
    public void Of_MixedEncounter_LettersOnlyRepeatedNames()
    {
        var labels = EnemyLabels.Of(["Goblin", "Slime", "Goblin"], RepositoryScreenText.Text.Messages);

        Assert.Equal(["Goblin A", "Slime", "Goblin B"], labels);
    }
}
