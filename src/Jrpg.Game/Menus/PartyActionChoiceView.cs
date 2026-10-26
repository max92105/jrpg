using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Party page's action choice (a translucent panel, one row per action) over the right of the chosen card,
/// with "To reserve" greyed when Core refuses it.
/// </summary>
internal sealed class PartyActionChoiceView
{
    /// <summary>The entry that starts swapping places; the other one sends the member to the reserve.</summary>
    public const int ChangeOrderEntry = 0;

    /// <summary>How many actions the choice offers.</summary>
    public const int EntryCount = 2;

    // A 300 px translucent panel of 52 px rows (the menu lists' row height), 16 px in from the card's corner.
    private const int Width = 300;
    private const int RowHeight = 52;
    private const int Inset = 16;
    private const int BodyPixels = 26;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;

    /// <param name="text">The action labels and their font.</param>
    /// <param name="ui">The shared selected-row texture.</param>
    public PartyActionChoiceView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _font = text.Fonts.FontAt(BodyPixels);
        _labels = [text.Messages.Text(MessageIds.PartyChangeOrder), text.Messages.Text(MessageIds.PartyToReserve)];
    }

    // The choice only opens once the menu has faded in, so it draws at full opacity.
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place)
    {
        var card = place.Card;
        var panel = new Rectangle(card.Right - Width - Inset, card.Y + Inset, Width, EntryCount * RowHeight);
        batch.Draw(textures.Pixel, panel, UiTheme.Surface);
        Outline.Draw(batch, textures.Pixel, panel, UiTheme.Stroke);
        for (var index = 0; index < EntryCount; index++)
        {
            var row = new Rectangle(panel.X, panel.Y + (index * RowHeight), panel.Width, RowHeight);
            var isSelected = index == place.Choice.Selected;
            if (isSelected)
            {
                _ui.DrawSelectedRow(batch, textures, row, 1);
            }

            var textTop = row.Y + ((RowHeight - _font.LineHeight) / 2);
            batch.DrawString(_font, _labels[index], new Vector2(row.X + Inset, textTop), ColorOf(index, place));
        }
    }

    private static Color ColorOf(int index, Place place)
    {
        if (index != ChangeOrderEntry && !place.IsToReserveAllowed)
        {
            return UiTheme.TextDisabled;
        }

        return index == place.Choice.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
    }

    /// <summary>What to draw this frame, and over which card.</summary>
    /// <param name="Choice">Which action is highlighted.</param>
    /// <param name="Card">The chosen member's card.</param>
    /// <param name="IsToReserveAllowed">Whether Core lets the member leave, worked out when the choice opened.</param>
    public readonly record struct Place(ListCursor Choice, Rectangle Card, bool IsToReserveAllowed);
}
