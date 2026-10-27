using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatNamesTests
{
    public static TheoryData<StatKind> EveryKind => [.. Enum.GetValues<StatKind>()];

    [Theory]
    [MemberData(nameof(EveryKind))]
    public void IdOf_EveryKind_IsInTheMessageTable(StatKind kind)
    {
        var name = RepositoryScreenText.Text.Messages.Text(StatNames.IdOf(kind));

        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void IdOf_EveryKind_IsItsOwnName()
    {
        var ids = Enum.GetValues<StatKind>().Select(StatNames.IdOf);

        Assert.Equal(Enum.GetValues<StatKind>().Length, ids.Distinct().Count());
    }
}
