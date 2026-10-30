using Jrpg.Core.Items;

namespace Jrpg.Game.Ui;

/// <summary>
/// A piece's name as the player reads it in a question: the base name, with its rarity first when it rolled ("Magic
/// Bronze Sword"), so the question tells it from a plain one of the same base. The Forge's salvage and the Shop's sale
/// ask with it.
/// </summary>
internal static class PieceName
{
    public static string Of(GearPiece piece, UiMessages messages) =>
        piece.Rarity == Rarity.Common && piece.Affixes.Count == 0
            ? piece.Base.Name
            : messages.Format(MessageIds.RolledPieceName, new MessageArgument("rarity", messages.Text(RarityNames.IdOf(piece.Rarity))), new MessageArgument("name", piece.Base.Name));
}
