using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Title;

/// <summary>
/// The screen the game opens on: its entries as plain centered text, the selected one marked. The legacy look comes
/// in T2.
/// </summary>
internal sealed class TitleScene : IScene
{
    private const int EntryTextPixels = 48;
    private const int EntrySpacing = 80;
    private const int MarkerGap = 24;
    private const string Marker = ">";

    private static readonly Color _unselected = Color.White * 0.6f;

    private readonly TitleMenu _menu = new();
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly Vector2[] _positions;
    private readonly float _markerWidth;

    public TitleScene(ScreenText text)
    {
        _font = text.Fonts.FontAt(EntryTextPixels);
        _labels = [.. TitleMenu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
        _positions = [.. _labels.Select(CenteredPosition)];
        _markerWidth = _font.MeasureString(Marker).X;
    }

    public SceneRequest? Update(TimeSpan delta, InputFrame input) => _menu.Update(input);

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        for (var index = 0; index < _labels.Length; index++)
        {
            var isSelected = TitleMenu.Entries[index] == _menu.Selected;
            batch.DrawString(_font, _labels[index], _positions[index], isSelected ? Color.White : _unselected);
            if (isSelected)
            {
                batch.DrawString(_font, Marker, _positions[index] - new Vector2(MarkerGap + _markerWidth, 0), Color.White);
            }
        }
    }

    private static string LabelIdOf(TitleEntry entry) => entry switch
    {
        TitleEntry.NewGame => MessageIds.TitleNewGame,
        TitleEntry.Quit => MessageIds.TitleQuit,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    // The entries stack as one block centered on the screen.
    private Vector2 CenteredPosition(string label, int index)
    {
        var blockTop = (VirtualScreen.Height - (_labels.Length * EntrySpacing)) / 2f;
        return new Vector2((VirtualScreen.Width - _font.MeasureString(label).X) / 2f, blockTop + (index * EntrySpacing));
    }
}
