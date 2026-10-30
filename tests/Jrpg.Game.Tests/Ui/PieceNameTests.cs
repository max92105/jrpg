using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

// A rolled piece is named with its rarity first, so a question tells it from a plain one of the same base.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class PieceNameTests
{
    private static readonly ItemDefinition _sword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void Of_RolledPiece_PutsTheRarityFirst()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 2)]);

        var name = PieceName.Of(piece, _messages);

        var rarity = _messages.Text(RarityNames.IdOf(Rarity.Magic));
        Assert.Equal(_messages.Format(MessageIds.RolledPieceName, new MessageArgument("rarity", rarity), new MessageArgument("name", _sword.Name)), name);
        Assert.StartsWith(rarity, name);
    }

    [Fact]
    public void Of_PlainPiece_IsTheBaseName()
    {
        Assert.Equal(_sword.Name, PieceName.Of(GearPiece.Plain(_sword), _messages));
    }
}
