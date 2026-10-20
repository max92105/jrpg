using FontStashSharp;
using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The command choice (legacy <c>CommandPanel</c>, leaning toward <c>Combat9</c>): a pointed header tab with the actor's
/// name, the rows in a thin double frame with their icons, the selected row lit with a pointer, and a one-line hint under
/// it. While a target is chosen, a pointer bobs over each selected target.
/// </summary>
internal sealed class CommandPanelView
{
    // Legacy CommandPanel and BattleCommandStyle, in 1920 x 1080 units.
    private const int HeaderInset = 30;
    private const int HeaderGap = 6;
    private const int FrameInset = 5;
    private const int RowHeight = 46;
    private const int RowPointerLeft = 8;
    private const int RowPointerSide = 12;
    private const int IconLeft = 44;
    private const int IconSize = 24;
    private const int IconGap = 12;
    private const int SelectionLineHeight = 2;
    private const float DisabledOpacity = 0.4f;
    private const int HintPaddingX = 16;
    private const int HintPaddingY = 10;
    private const int HeaderTextPixels = 20;
    private const int RowTextPixels = 20;
    private const int HintTextPixels = 16;

    // Decision 56: a 44 px pointer, drawn as a downward triangle (the lower half of a diamond), 6 px over the target.
    private const int TargetPointerSize = 44;
    private const int TargetPointerGap = 6;
    private static readonly DiamondShape _targetPointer = new(TargetPointerSize, 0, 1, Color.Transparent, Color.White);

    private static readonly string[] _icons =
        [UiArtFiles.Icons.Attack, UiArtFiles.Icons.Skills, UiArtFiles.Icons.Items, UiArtFiles.Icons.Defend, UiArtFiles.Icons.Run];

    private readonly CommandMenu _menu;
    private readonly BattlersView _battlers;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _headerFont;
    private readonly SpriteFontBase _rowFont;
    private readonly SpriteFontBase _hintFont;
    private readonly string[] _labels;
    private readonly string[] _hints;
    private readonly int _frameHeight;
    private MeasuredText _actorName = new(string.Empty, 0);
    private TimeSpan _pointerTime;

    /// <param name="menu">The choice it shows.</param>
    /// <param name="battlers">Where each combatant stands and where their sprite starts.</param>
    /// <param name="text">The labels, hints and fonts.</param>
    /// <param name="ui">Where the header, gradients, icons and pointer are baked, once.</param>
    public CommandPanelView(CommandMenu menu, BattlersView battlers, ScreenText text, UiTextures ui)
    {
        _menu = menu;
        _battlers = battlers;
        _ui = ui;
        _headerFont = text.Fonts.FontAt(HeaderTextPixels);
        _rowFont = text.Fonts.FontAt(RowTextPixels);
        _hintFont = text.Fonts.FontAt(HintTextPixels);
        _labels = [.. CommandMenu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
        _hints = [.. CommandMenu.Entries.Select(entry => text.Messages.Text(HintIdOf(entry)))];
        _frameHeight = (2 * FrameInset) + (CommandMenu.Entries.Length * RowHeight);
    }

    /// <summary>Moves the target pointer's bob on.</summary>
    public void Advance(TimeSpan delta) => _pointerTime += delta;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the art come from.</param>
    /// <param name="actor">The party member choosing.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Combatant actor)
    {
        var topLeft = CommandPanelPlacement.Place(_battlers.SpotOf(actor));
        var header = new Rectangle(topLeft.X + HeaderInset, topLeft.Y, CommandHeaderShape.Width, CommandHeaderShape.Height);
        batch.Draw(_ui.CommandHeader(textures), header, Color.White);
        if (!ReferenceEquals(_actorName.Text, actor.Name))
        {
            _actorName = MeasuredText.Of(_headerFont, actor.Name);
        }

        var nameTop = header.Center.Y - (_headerFont.LineHeight / 2);
        batch.DrawString(_headerFont, _actorName.Text, new Vector2(header.Center.X - (_actorName.Width / 2), nameTop), UiTheme.TextPrimary);

        var frame = new Rectangle(topLeft.X, header.Bottom + HeaderGap, CommandPanelPlacement.Width, _frameHeight);
        DrawFrame(batch, textures.Pixel, frame);
        for (var index = 0; index < _labels.Length; index++)
        {
            var row = new Rectangle(frame.X + FrameInset, frame.Y + FrameInset + (index * RowHeight), frame.Width - (2 * FrameInset), RowHeight);
            DrawRow(batch, textures, index, row);
        }

        DrawHint(batch, textures, new Point(topLeft.X + HeaderInset, frame.Bottom + HeaderGap));
        if (_menu.Targets is { } targets)
        {
            DrawTargetPointers(batch, textures, targets.Selected);
        }
    }

    private static string LabelIdOf(CommandEntry entry) => entry switch
    {
        CommandEntry.Attack => MessageIds.BattleAttack,
        CommandEntry.Skills => MessageIds.BattleSkills,
        CommandEntry.Items => MessageIds.BattleItems,
        CommandEntry.Defend => MessageIds.BattleDefend,
        CommandEntry.Run => MessageIds.BattleRun,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    private static string HintIdOf(CommandEntry entry) => entry switch
    {
        CommandEntry.Attack => MessageIds.BattleAttackHint,
        CommandEntry.Skills => MessageIds.BattleSkillsHint,
        CommandEntry.Items => MessageIds.BattleItemsHint,
        CommandEntry.Defend => MessageIds.BattleDefendHint,
        CommandEntry.Run => MessageIds.BattleRunHint,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    // A thin double frame, like the reference: a strong outer line, then a faint inner one 3 px in.
    private static void DrawFrame(SpriteBatch batch, Texture2D pixel, Rectangle frame)
    {
        batch.Draw(pixel, frame, UiTheme.CommandPanelFill);
        Outline.Draw(batch, pixel, frame, UiTheme.BarLine);
        var inner = frame;
        inner.Inflate(1 - FrameInset, 1 - FrameInset);
        Outline.Draw(batch, pixel, inner, UiTheme.Stroke);
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, int index, Rectangle row)
    {
        var entry = CommandMenu.Entries[index];
        var isSelected = entry == _menu.Selected;
        if (isSelected)
        {
            batch.Draw(_ui.Gradient(textures, UiTheme.Selection), row, Color.White);
            batch.Draw(_ui.Gradient(textures, UiTheme.SelectionLine), new Rectangle(row.X, row.Bottom - SelectionLineHeight, row.Width, SelectionLineHeight), Color.White);
            new Diamond(new Vector2(row.X + RowPointerLeft + RowPointerSide, row.Center.Y), RowPointerSide).Draw(batch, textures.Pixel, Color.White);
        }
        else
        {
            batch.Draw(textures.Pixel, row, UiTheme.CommandRowFill);
            batch.Draw(textures.Pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke);
        }

        var opacity = CommandMenu.IsEnabled(entry) ? 1 : DisabledOpacity;
        var icon = new Rectangle(row.X + IconLeft, row.Center.Y - (IconSize / 2), IconSize, IconSize);
        batch.Draw(_ui.Icon(textures, _icons[index], IconSize), icon, Color.White * opacity);
        var textTop = row.Center.Y - (_rowFont.LineHeight / 2);
        batch.DrawString(_rowFont, _labels[index], new Vector2(icon.Right + IconGap, textTop), UiTheme.TextPrimary * opacity);
    }

    private void DrawHint(SpriteBatch batch, TextureLibrary textures, Point topLeft)
    {
        var height = (int)_hintFont.LineHeight + (2 * HintPaddingY);
        var box = new Rectangle(topLeft.X, topLeft.Y, CommandPanelPlacement.Width - HeaderInset, height);
        batch.Draw(textures.Pixel, box, UiTheme.CommandHintFill);
        batch.Draw(_ui.Gradient(textures, UiTheme.HighlightLine), new Rectangle(box.X, box.Bottom - 1, box.Width, 1), Color.White);
        var hint = _hints[CommandMenu.Entries.IndexOf(_menu.Selected)];
        batch.DrawString(_hintFont, hint, new Vector2(box.X + HintPaddingX, box.Y + HintPaddingY), UiTheme.TextPrimary);
    }

    private void DrawTargetPointers(SpriteBatch batch, TextureLibrary textures, IReadOnlyList<Combatant> targets)
    {
        var bob = (int)PointerBob.OffsetAt(_pointerTime);
        var pointer = _ui.Diamond(textures, _targetPointer);
        for (var index = 0; index < targets.Count; index++)
        {
            var target = targets[index];
            var bottom = _battlers.TopOf(textures, target) - TargetPointerGap + bob;
            var left = _battlers.SpotOf(target).Center.X - (TargetPointerSize / 2);
            batch.Draw(pointer, new Rectangle(left, bottom - TargetPointerSize, TargetPointerSize, TargetPointerSize), Color.White);
        }
    }
}
