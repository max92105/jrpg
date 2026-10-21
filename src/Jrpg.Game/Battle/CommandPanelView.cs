using System.Globalization;
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
/// it. The actor's skills replace the commands, each with its MP cost and its description as the hint. While a target is
/// chosen, a pointer bobs over each selected target.
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
    private const int CostRight = 16;
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
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _headerFont;
    private readonly SpriteFontBase _rowFont;
    private readonly SpriteFontBase _hintFont;
    private readonly string[] _labels;
    private readonly string[] _hints;
    private readonly string _notEnoughMp;
    private MeasuredText _actorName = new(string.Empty, 0);
    private BattleSkillChoice? _shownSkills;
    private MeasuredText[] _skillCosts = [];
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
        _messages = text.Messages;
        _headerFont = text.Fonts.FontAt(HeaderTextPixels);
        _rowFont = text.Fonts.FontAt(RowTextPixels);
        _hintFont = text.Fonts.FontAt(HintTextPixels);
        _labels = [.. CommandMenu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
        _hints = [.. CommandMenu.Entries.Select(entry => text.Messages.Text(HintIdOf(entry)))];
        _notEnoughMp = text.Messages.Text(MessageIds.BattleNotEnoughMp);
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

        var frameTop = header.Bottom + HeaderGap;
        var frameBottom = _menu.Skills is { } skills
            ? DrawSkills(batch, textures, skills, new Point(topLeft.X, frameTop))
            : DrawCommands(batch, textures, new Point(topLeft.X, frameTop));
        DrawHint(batch, textures, new Point(topLeft.X + HeaderInset, frameBottom + HeaderGap));
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
    private static Rectangle DrawFrame(SpriteBatch batch, Texture2D pixel, Point topLeft, int rowCount)
    {
        var frame = new Rectangle(topLeft.X, topLeft.Y, CommandPanelPlacement.Width, (2 * FrameInset) + (rowCount * RowHeight));
        batch.Draw(pixel, frame, UiTheme.CommandPanelFill);
        Outline.Draw(batch, pixel, frame, UiTheme.BarLine);
        var inner = frame;
        inner.Inflate(1 - FrameInset, 1 - FrameInset);
        Outline.Draw(batch, pixel, inner, UiTheme.Stroke);
        return frame;
    }

    private static Rectangle RowIn(Rectangle frame, int row) =>
        new(frame.X + FrameInset, frame.Y + FrameInset + (row * RowHeight), frame.Width - (2 * FrameInset), RowHeight);

    /// <returns>The frame's bottom.</returns>
    private int DrawCommands(SpriteBatch batch, TextureLibrary textures, Point topLeft)
    {
        var frame = DrawFrame(batch, textures.Pixel, topLeft, CommandMenu.Entries.Length);
        DrawRowBacks(batch, textures, frame, CommandMenu.Entries.IndexOf(_menu.Selected));
        for (var index = 0; index < CommandMenu.Entries.Length; index++)
        {
            var entry = CommandMenu.Entries[index];
            var row = RowIn(frame, index);
            var opacity = _menu.IsEnabled(entry) ? 1 : DisabledOpacity;
            DrawRowLabel(batch, textures, row, new CommandRowLabel(_icons[index], _labels[index], opacity));
        }

        return frame.Bottom;
    }

    /// <returns>The frame's bottom.</returns>
    private int DrawSkills(SpriteBatch batch, TextureLibrary textures, BattleSkillChoice skills, Point topLeft)
    {
        if (!ReferenceEquals(skills, _shownSkills))
        {
            _shownSkills = skills;
            _skillCosts = [.. skills.Skills.Select(skill => MeasuredText.Of(_rowFont, _messages.Format(
                MessageIds.BattleSkillCost, new MessageArgument("cost", skill.MpCost.ToString(CultureInfo.InvariantCulture)))))];
        }

        var rowCount = Math.Min(skills.Skills.Count, BattleSkillChoice.VisibleRows);
        var frame = DrawFrame(batch, textures.Pixel, topLeft, rowCount);
        DrawRowBacks(batch, textures, frame, skills.SelectedIndex - skills.FirstVisible);
        for (var visible = 0; visible < rowCount; visible++)
        {
            var index = skills.FirstVisible + visible;
            var skill = skills.Skills[index];
            var row = RowIn(frame, visible);
            var opacity = skills.IsUsable(index) ? 1 : DisabledOpacity;
            DrawRowLabel(batch, textures, row, new CommandRowLabel(skill.IconFile, skill.Name, opacity));
            var cost = _skillCosts[index];
            var costLeft = row.Right - CostRight - cost.Width;
            batch.DrawString(_rowFont, cost.Text, new Vector2(costLeft, row.Center.Y - (_rowFont.LineHeight / 2)), UiTheme.TextPrimary * opacity);
        }

        return frame.Bottom;
    }

    // Every row of the frame gets the plain fill and separator, except the selected one, lit with the pointer.
    private void DrawRowBacks(SpriteBatch batch, TextureLibrary textures, Rectangle frame, int selectedRow)
    {
        var rowCount = (frame.Height - (2 * FrameInset)) / RowHeight;
        for (var index = 0; index < rowCount; index++)
        {
            var row = RowIn(frame, index);
            if (index != selectedRow)
            {
                batch.Draw(textures.Pixel, row, UiTheme.CommandRowFill);
                batch.Draw(textures.Pixel, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), UiTheme.Stroke);
                continue;
            }

            batch.Draw(_ui.Gradient(textures, UiTheme.Selection), row, Color.White);
            batch.Draw(_ui.Gradient(textures, UiTheme.SelectionLine), new Rectangle(row.X, row.Bottom - SelectionLineHeight, row.Width, SelectionLineHeight), Color.White);
            new Diamond(new Vector2(row.X + RowPointerLeft + RowPointerSide, row.Center.Y), RowPointerSide).Draw(batch, textures.Pixel, Color.White);
        }
    }

    // A skill with no icon file leaves the icon's place empty, so the names stay aligned.
    private void DrawRowLabel(SpriteBatch batch, TextureLibrary textures, Rectangle row, CommandRowLabel content)
    {
        var icon = new Rectangle(row.X + IconLeft, row.Center.Y - (IconSize / 2), IconSize, IconSize);
        if (content.IconFile.Length > 0)
        {
            batch.Draw(_ui.Icon(textures, content.IconFile, IconSize), icon, Color.White * content.Opacity);
        }

        var textTop = row.Center.Y - (_rowFont.LineHeight / 2);
        batch.DrawString(_rowFont, content.Label, new Vector2(icon.Right + IconGap, textTop), UiTheme.TextPrimary * content.Opacity);
    }

    private void DrawHint(SpriteBatch batch, TextureLibrary textures, Point topLeft)
    {
        var height = (int)_hintFont.LineHeight + (2 * HintPaddingY);
        var box = new Rectangle(topLeft.X, topLeft.Y, CommandPanelPlacement.Width - HeaderInset, height);
        batch.Draw(textures.Pixel, box, UiTheme.CommandHintFill);
        batch.Draw(_ui.Gradient(textures, UiTheme.HighlightLine), new Rectangle(box.X, box.Bottom - 1, box.Width, 1), Color.White);
        batch.DrawString(_hintFont, HintText(), new Vector2(box.X + HintPaddingX, box.Y + HintPaddingY), UiTheme.TextPrimary);
    }

    private string HintText() => _menu.Skills switch
    {
        { IsSayingNotEnoughMp: true } => _notEnoughMp,
        { } skills => skills.Selected.Description,
        null => _hints[CommandMenu.Entries.IndexOf(_menu.Selected)],
    };

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
