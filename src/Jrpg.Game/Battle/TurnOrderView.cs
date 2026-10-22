using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The turn order at the top left: the acting combatant in a big diamond with a bright second frame, then "NEXT" over
/// the upcoming turns in small diamonds that touch, like beads on a thin line.
/// </summary>
internal sealed class TurnOrderView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    private const int Left = 24;
    private const int Top = 20;
    private const int CurrentSize = 140;
    private const int NextSize = 66;
    private const int NextOverlap = 6;
    private const int NextColumnShift = -8;
    private const int LabelInset = 20;
    private const int LabelGap = 2;
    private const int LabelPixels = 14;
    private const int LineLeft = 110;
    private const int LineWidth = 760;

    private readonly TurnOrderStrip _strip;
    private readonly UiTextures _ui;
    private readonly TurnDiamondView _current;
    private readonly TurnDiamondView _next;
    private readonly OutlinedFont _labelFont;
    private readonly string _label;

    /// <param name="strip">Who the bar shows and who is lit.</param>
    /// <param name="combatants">Every combatant of the battle, for their portraits.</param>
    /// <param name="text">The "NEXT" label and its font.</param>
    /// <param name="ui">Where the diamonds, portraits and the line are baked, once.</param>
    public TurnOrderView(TurnOrderStrip strip, IReadOnlyList<Combatant> combatants, ScreenText text, UiTextures ui)
    {
        _strip = strip;
        _ui = ui;
        var arts = combatants.ToDictionary(combatant => combatant, BattlerArt.Of);
        _current = new TurnDiamondView(new TurnDiamondStyle(CurrentSize, UiTheme.SelectionStroke, UiTheme.SelectionStroke), ui, strip, arts);
        _next = new TurnDiamondView(new TurnDiamondStyle(NextSize, UiTheme.BarLine, null), ui, strip, arts);
        _labelFont = new OutlinedFont(text.Fonts.FontAt(LabelPixels));
        _label = text.Messages.Text(MessageIds.BattleNext);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var shown = _strip.Shown;
        if (shown.Count == 0)
        {
            return;
        }

        var middle = Top + (CurrentSize / 2);
        batch.Draw(_ui.Gradient(textures, UiTheme.SectionLine), new Rectangle(Left + LineLeft, middle, LineWidth, 1), Color.White);
        _current.Draw(batch, textures, new Point(Left, Top), shown[0]);

        var columnLeft = Left + CurrentSize + NextColumnShift;
        var labelHeight = (int)_labelFont.Font.LineHeight;
        var columnTop = middle - ((labelHeight + LabelGap + NextSize) / 2);
        _labelFont.Draw(batch, _label, new Vector2(columnLeft + LabelInset, columnTop), UiTheme.TextSecondary);
        var rowTop = columnTop + labelHeight + LabelGap;
        for (var index = 1; index < shown.Count; index++)
        {
            _next.Draw(batch, textures, new Point(columnLeft + ((index - 1) * (NextSize - NextOverlap)), rowTop), shown[index]);
        }
    }
}
