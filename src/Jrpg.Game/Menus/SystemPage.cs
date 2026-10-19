using System.Collections.Immutable;
using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>The menu's System page (legacy System sub menu): back to the title, or quit. Neither asks first until G2.</summary>
internal sealed class SystemPage : IMenuPage
{
    private static readonly ImmutableArray<SystemEntry> _entries = [SystemEntry.TitleScreen, SystemEntry.Quit];

    private readonly ListCursor _cursor = new(_entries.Length, _ => true);
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;

    public SystemPage(ScreenText text)
    {
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _labels = [.. _entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
    }

    public string TitleMessageId => MessageIds.MenuSystem;

    public bool LeavesOnCancel => true;

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

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        var selected = _entries[_cursor.Selected];
        return selected switch
        {
            SystemEntry.TitleScreen => new TitleRequested(),
            SystemEntry.Quit => new QuitRequested(),
            _ => throw new InvalidOperationException($"The System page has no request for {selected}."),
        };
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        for (var index = 0; index < _labels.Length; index++)
        {
            var color = index == _cursor.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            var rowTop = area.Y + (index * UiTheme.MenuRowHeight);
            var position = new Vector2(area.X + UiTheme.MenuTextInset, rowTop + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f));
            batch.DrawString(_font, _labels[index], position, color * opacity);
        }
    }

    private static string LabelIdOf(SystemEntry entry) => entry switch
    {
        SystemEntry.TitleScreen => MessageIds.SystemTitleScreen,
        SystemEntry.Quit => MessageIds.SystemQuit,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };
}
