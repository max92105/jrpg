using FontStashSharp;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Options page: the settings that exist, one per row, each changed with Left and Right and applied at once.
/// Kept for this run only (question 89).
/// </summary>
internal sealed class OptionsPage : IMenuPage
{
    private const int FullScreenRow = 0;
    private const int TextSpeedRow = 1;
    private const int RowCount = 2;

    // Values sit in a column right of the labels, with a small diamond either side of the selected one: the reference
    // UI's sign that Left and Right change it (docs/ui-reference).
    private const int ValueColumn = 320;
    private const int ArrowGap = 16;
    private const int ArrowSide = 6;

    private readonly PlayerSettings _settings;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly MeasuredText[] _onOff;
    private readonly MeasuredText[] _speeds;
    private readonly ListCursor _cursor = new(RowCount, _ => true);

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: the selected row.</param>
    /// <param name="settings">What the page changes; the host and dialogue read it.</param>
    public OptionsPage(ScreenText text, UiTextures ui, PlayerSettings settings)
    {
        Hint = text.Messages.Text(MessageIds.MenuHintOptions);
        _settings = settings;
        _ui = ui;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        var messages = text.Messages;
        _labels = [messages.Text(MessageIds.OptionsFullScreen), messages.Text(MessageIds.OptionsTextSpeed)];
        _onOff = [MeasuredText.Of(_font, messages.Text(MessageIds.OptionsOff)), MeasuredText.Of(_font, messages.Text(MessageIds.OptionsOn))];

        // Indexed by TextSpeed, so each label follows its speed whatever the enum's order.
        _speeds = [.. Enum.GetValues<TextSpeed>().Select(speed => MeasuredText.Of(_font, messages.Text(LabelIdOf(speed))))];
    }

    public string TitleMessageId => MessageIds.MenuOptions;

    public string Hint { get; }

    public bool ShowsPartySidebar => true;

    public bool LeavesOnCancel => true;

    // The values are read from the settings when drawing, so only the cursor goes back.
    public void Open() => _cursor.Select(0);

    public SceneRequest? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        if (input.IsPressed(GameButton.Right))
        {
            Change(1);
        }

        if (input.IsPressed(GameButton.Left))
        {
            Change(-1);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        for (var row = 0; row < RowCount; row++)
        {
            var rowArea = new Rectangle(area.X, area.Y + (row * UiTheme.MenuRowHeight), area.Width, UiTheme.MenuRowHeight);
            DrawRow(batch, textures, new RowPlace(row, rowArea), opacity);
        }
    }

    // Full screen flips either way; the text speed steps through the list and stops at either end.
    private void Change(int step)
    {
        if (_cursor.Selected == FullScreenRow)
        {
            _settings.IsFullScreen = !_settings.IsFullScreen;
            return;
        }

        var speed = Math.Clamp((int)_settings.TextSpeed + step, (int)TextSpeed.Slow, (int)TextSpeed.Instant);
        _settings.TextSpeed = (TextSpeed)speed;
    }

    private MeasuredText ValueOf(int row) => row == TextSpeedRow
        ? _speeds[(int)_settings.TextSpeed]
        : _onOff[_settings.IsFullScreen ? 1 : 0];

    private static string LabelIdOf(TextSpeed speed) => speed switch
    {
        TextSpeed.Slow => MessageIds.TextSpeedSlow,
        TextSpeed.Normal => MessageIds.TextSpeedNormal,
        TextSpeed.Fast => MessageIds.TextSpeedFast,
        TextSpeed.Instant => MessageIds.TextSpeedInstant,
        _ => throw new ArgumentOutOfRangeException(nameof(speed), speed, null),
    };

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, RowPlace place, float opacity)
    {
        var isSelected = place.Index == _cursor.Selected;
        if (isSelected)
        {
            _ui.DrawSelectedRow(batch, textures, place.Area, opacity);
        }

        var color = (isSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary) * opacity;
        var textTop = place.Area.Y + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f);
        var labelLeft = place.Area.X + UiTheme.MenuTextInset;
        batch.DrawString(_font, _labels[place.Index], new Vector2(labelLeft, textTop), color);

        var value = ValueOf(place.Index);
        var valueLeft = labelLeft + ValueColumn;
        batch.DrawString(_font, value.Text, new Vector2(valueLeft, textTop), color);
        if (isSelected)
        {
            var middle = textTop + (_font.LineHeight / 2f);
            new Diamond(new Vector2(valueLeft - ArrowGap, middle), ArrowSide).Draw(batch, textures.Pixel, color);
            new Diamond(new Vector2(valueLeft + value.Width + ArrowGap, middle), ArrowSide).Draw(batch, textures.Pixel, color);
        }
    }

    // Which row is drawn, and where.
    private readonly record struct RowPlace(int Index, Rectangle Area);
}
