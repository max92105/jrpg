using System.Globalization;
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
    internal const int FullScreenRow = 0;
    internal const int TextSpeedRow = 1;
    internal const int MusicRow = 2;
    internal const int EffectsRow = 3;
    internal const int VoicesRow = 4;
    private const int RowCount = 5;

    // Values sit in a column right of the labels, with a small diamond either side of the selected one: the reference
    // UI's sign that Left and Right change it (docs/ui-reference).
    private const int ValueColumn = 320;
    private const int ArrowGap = 16;
    private const int ArrowSide = 6;

    // Keeps a volume's number clear of its pips (decision 424).
    private const int PipsToNumberGap = 14;

    private readonly PlayerSettings _settings;
    private readonly UiTextures _ui;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly MeasuredText[] _onOff;
    private readonly MeasuredText[] _speeds;
    private readonly MeasuredText[] _levels;
    private readonly ListCursor _cursor = new(RowCount, _ => true);
    private readonly SelectionMotion _selection = new();
    private readonly PipRow _pips;
    private readonly AskedSounds _sounds = new();

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared textures: the selected row and the volumes' pips.</param>
    /// <param name="settings">What the page changes; the host and dialogue read it.</param>
    public OptionsPage(ScreenText text, UiTextures ui, PlayerSettings settings)
    {
        Hint = text.Messages.Text(MessageIds.MenuHintOptions);
        _settings = settings;
        _ui = ui;
        _pips = new PipRow(ui);
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        var messages = text.Messages;
        _labels =
        [
            messages.Text(MessageIds.OptionsFullScreen),
            messages.Text(MessageIds.OptionsTextSpeed),
            messages.Text(MessageIds.OptionsMusicVolume),
            messages.Text(MessageIds.OptionsEffectsVolume),
            messages.Text(MessageIds.OptionsVoiceVolume),
        ];
        _onOff = [MeasuredText.Of(_font, messages.Text(MessageIds.OptionsOff)), MeasuredText.Of(_font, messages.Text(MessageIds.OptionsOn))];

        // Indexed by TextSpeed, so each label follows its speed whatever the enum's order.
        _speeds = [.. Enum.GetValues<TextSpeed>().Select(speed => MeasuredText.Of(_font, messages.Text(LabelIdOf(speed))))];
        _levels = [.. Enumerable.Range(0, VolumeLevel.Max + 1).Select(level => MeasuredText.Of(_font, level.ToString(CultureInfo.InvariantCulture)))];
    }

    public string TitleMessageId => MessageIds.MenuOptions;

    public TabGlyph Glyph => TabGlyph.Options;

    public string Hint { get; }

    public IReadOnlyList<string> Sounds => _sounds.Files;

    public bool LeavesOnCancel => true;

    /// <summary>The row Left and Right change now.</summary>
    internal int SelectedRow => _cursor.Selected;

    /// <summary>Where the highlight is, in rows.</summary>
    internal float SelectionPosition => _selection.Position;

    // The values are read from the settings when drawing, so only the cursor goes back.
    public void Open()
    {
        _cursor.Select(0);
        _selection.JumpTo(_cursor);
    }

    public void Advance(TimeSpan delta) => _selection.Advance(delta);

    public SceneRequest? Update(InputFrame input)
    {
        _sounds.Clear();
        var before = _cursor.Selected;
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        _selection.Follow(_cursor);
        _sounds.AskMoveIfChanged(before, _cursor.Selected);
        if (input.IsPressed(GameButton.Right))
        {
            ChangeAndSound(1);
        }

        if (input.IsPressed(GameButton.Left))
        {
            ChangeAndSound(-1);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var column = MenuLayout.SingleColumn(area);
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        _ui.DrawGlidingSelection(batch, textures, _selection.RowIn(RowAt(column, 0)), opacity);
        for (var row = 0; row < RowCount; row++)
        {
            DrawRow(batch, textures, new RowPlace(row, RowAt(column, row)), opacity);
        }
    }

    private static Rectangle RowAt(Rectangle area, int row) =>
        new(area.X, area.Y + (row * UiTheme.MenuRowHeight), area.Width, UiTheme.MenuRowHeight);

    // A value at its end doesn't change, so it stays silent.
    private void ChangeAndSound(int step)
    {
        var shown = ValueOf(_cursor.Selected);
        Change(step);
        if (ValueOf(_cursor.Selected) != shown)
        {
            _sounds.Ask(UiSoundFiles.Effects.MenuMove);
        }
    }

    // Full screen flips either way; the text speed and the volumes step and stop at either end.
    private void Change(int step)
    {
        switch (_cursor.Selected)
        {
            case FullScreenRow:
                _settings.IsFullScreen = !_settings.IsFullScreen;
                break;
            case TextSpeedRow:
                _settings.TextSpeed = (TextSpeed)Math.Clamp((int)_settings.TextSpeed + step, (int)TextSpeed.Slow, (int)TextSpeed.Instant);
                break;
            case MusicRow:
                _settings.MusicVolume = Stepped(_settings.MusicVolume, step);
                break;
            case EffectsRow:
                _settings.EffectsVolume = Stepped(_settings.EffectsVolume, step);
                break;
            default:
                _settings.VoiceVolume = Stepped(_settings.VoiceVolume, step);
                break;
        }
    }

    private static int Stepped(int level, int step) => Math.Clamp(level + step, 0, VolumeLevel.Max);

    private MeasuredText ValueOf(int row)
    {
        if (LevelOf(row) is { } level)
        {
            return _levels[level];
        }

        return row == TextSpeedRow ? _speeds[(int)_settings.TextSpeed] : _onOff[_settings.IsFullScreen ? 1 : 0];
    }

    // The level a volume row draws as pips; null for the two rows that stay text.
    private int? LevelOf(int row) => row switch
    {
        MusicRow => _settings.MusicVolume,
        EffectsRow => _settings.EffectsVolume,
        VoicesRow => _settings.VoiceVolume,
        _ => null,
    };

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
        var tone = isSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
        var color = tone * opacity;
        var textTop = place.Area.Y + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f);
        var labelLeft = place.Area.X + UiTheme.MenuTextInset;
        batch.DrawString(_font, _labels[place.Index], new Vector2(labelLeft, textTop), color);

        var value = ValueOf(place.Index);
        var valueLeft = labelLeft + ValueColumn;
        var middle = textTop + (_font.LineHeight / 2f);
        var numberLeft = (float)valueLeft;
        if (LevelOf(place.Index) is { } level)
        {
            var pipsAt = new Point(valueLeft, (int)(middle - (PipRow.PipSize / 2f)));
            _pips.Draw(batch, textures, pipsAt, new PipRowLook(VolumeLevel.Max, level, tone, opacity, default));
            numberLeft += PipRow.WidthOf(VolumeLevel.Max) + PipsToNumberGap;
        }

        batch.DrawString(_font, value.Text, new Vector2(numberLeft, textTop), color);
        if (isSelected)
        {
            new Diamond(new Vector2(valueLeft - ArrowGap, middle), ArrowSide).Draw(batch, textures.Pixel, color);
            new Diamond(new Vector2(numberLeft + value.Width + ArrowGap, middle), ArrowSide).Draw(batch, textures.Pixel, color);
        }
    }

    // Which row is drawn, and where.
    private readonly record struct RowPlace(int Index, Rectangle Area);
}
