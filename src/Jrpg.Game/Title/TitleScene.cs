using FontStashSharp;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Title;

/// <summary>
/// The screen the game opens on, as legacy <c>MainWindow</c>'s start screen (decision 82): the picture covering the
/// screen, the game's name at the top with a drop shadow, and the entries centered below it, each with a soft white glow
/// that brightens and grows on the selected one (legacy <c>StartMenuButtonStyle</c>'s hover).
/// </summary>
internal sealed class TitleScene : IScene
{
    // Legacy sizes were WPF units on a 1920 x 1080 window, so they carry over 1:1 (plan, "Port the legacy look").
    private const int NameTextPixels = 100;
    private const int NameTop = 60;
    private const int EntryTextPixels = 48;

    // Legacy entries were one line of text each, 20 px apart (the button template ignored its padding).
    private const int EntryGap = 20;

    // Legacy centered the entries in the space below the name, which ends about 180 px down.
    private const int EntriesCenterY = 630;

    // Legacy DropShadowEffect on the name: black at 70 %, blur 10, depth 4 at 320 degrees, which is about (3.1, 2.6).
    private const int ShadowBlur = 10;
    private static readonly Vector2 _shadowOffset = new(3, 3);
    private static readonly Color _shadow = Color.Black * 0.7f;

    // Legacy glow on each entry: white at 50 % with blur 8, and at full strength with blur 15 and 110 % size when hovered.
    private const int GlowBlur = 8;
    private const int SelectedGlowBlur = 15;
    private const float SelectedScale = 1.1f;
    private static readonly Color _glow = Color.White * 0.5f;

    private readonly TitleMenu _menu = new();
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _entryFont;
    private readonly string _name;
    private readonly Vector2 _namePosition;
    private readonly string[] _labels;
    private readonly Vector2[] _labelCenters;
    private readonly Vector2[] _labelOrigins;

    public TitleScene(ScreenText text)
    {
        _nameFont = text.Fonts.FontAt(NameTextPixels);
        _entryFont = text.Fonts.FontAt(EntryTextPixels);
        _name = text.Messages.Text(MessageIds.GameTitle);
        _namePosition = new Vector2((VirtualScreen.Width - _nameFont.MeasureString(_name).X) / 2f, NameTop);

        _labels = [.. TitleMenu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
        var pitch = _entryFont.LineHeight + EntryGap;
        var firstCenterY = EntriesCenterY - ((_labels.Length - 1) * pitch / 2f);
        _labelCenters = [.. _labels.Select((_, index) => new Vector2(VirtualScreen.Width / 2f, firstCenterY + (index * pitch)))];
        _labelOrigins = [.. _labels.Select(label => _entryFont.MeasureString(label) / 2)];
    }

    public SceneRequest? Update(TimeSpan delta, InputFrame input) => _menu.Update(input);

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.TitleBackground), Color.White);
        batch.DrawString(_nameFont, _name, _namePosition + _shadowOffset, _shadow, effect: FontSystemEffect.Blurry, effectAmount: ShadowBlur);
        batch.DrawString(_nameFont, _name, _namePosition, Color.White);
        for (var index = 0; index < _labels.Length; index++)
        {
            DrawEntry(batch, index);
        }
    }

    private static string LabelIdOf(TitleEntry entry) => entry switch
    {
        TitleEntry.NewGame => MessageIds.TitleNewGame,
        TitleEntry.Quit => MessageIds.TitleQuit,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    // Drawn around its center, so the selected entry grows in place.
    private void DrawEntry(SpriteBatch batch, int index)
    {
        var isSelected = TitleMenu.Entries[index] == _menu.Selected;
        var scale = new Vector2(isSelected ? SelectedScale : 1);
        var label = _labels[index];
        var center = _labelCenters[index];
        var origin = _labelOrigins[index];
        batch.DrawString(
            _entryFont,
            label,
            center,
            isSelected ? Color.White : _glow,
            origin: origin,
            scale: scale,
            effect: FontSystemEffect.Blurry,
            effectAmount: isSelected ? SelectedGlowBlur : GlowBlur);
        batch.DrawString(_entryFont, label, center, Color.White, origin: origin, scale: scale);
    }
}
