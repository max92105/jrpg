using Jrpg.Core.Battle;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// A line of four: Ragnir, Teelle, Zanyr, Omer.
public sealed class FormationTests
{
    private readonly Combatant _ragnir = Member("ragnir");
    private readonly Combatant _teelle = Member("teelle");
    private readonly Combatant _zanyr = Member("zanyr");
    private readonly Combatant _omer = Member("omer");
    private readonly Formation _formation;

    public FormationTests() => _formation = new Formation([_ragnir, _teelle, _zanyr, _omer]);

    [Fact]
    public void Constructor_Party_LineIsActiveOrder()
    {
        Assert.Equal([_ragnir, _teelle, _zanyr, _omer], _formation.Line);
        Assert.Equal(2, _formation.PositionOf(_zanyr));
    }

    [Fact]
    public void Neighbors_Middle_ReturnsBothSides()
    {
        var neighbors = _formation.Neighbors(_teelle);

        Assert.Equal([_ragnir, _zanyr], neighbors);
    }

    [Fact]
    public void Neighbors_End_ReturnsOne()
    {
        var neighbors = _formation.Neighbors(_omer);

        Assert.Equal([_zanyr], neighbors);
    }

    [Fact]
    public void WithinRange_RangeOneInMiddle_ReturnsThree()
    {
        var reached = _formation.WithinRange(_teelle, range: 1);

        Assert.Equal([_ragnir, _teelle, _zanyr], reached);
    }

    [Fact]
    public void WithinRange_RangeTwoNearEnd_StopsAtEnd()
    {
        var reached = _formation.WithinRange(_zanyr, range: 2);

        Assert.Equal([_ragnir, _teelle, _zanyr, _omer], reached);
    }

    [Fact]
    public void Swap_Neighbors_ExchangesPositions()
    {
        _formation.Swap(_teelle, _zanyr);

        Assert.Equal([_ragnir, _zanyr, _teelle, _omer], _formation.Line);
    }

    [Fact]
    public void Swap_NotNeighbors_ThrowsAndKeepsTheLine()
    {
        Assert.Throws<ArgumentException>(() => _formation.Swap(_ragnir, _zanyr));

        Assert.Equal([_ragnir, _teelle, _zanyr, _omer], _formation.Line);
    }

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());
}
