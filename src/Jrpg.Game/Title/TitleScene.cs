using FontStashSharp;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
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

    // Legacy OverlayViewHost: the file list in a 1000 px column, #AA111111, over the dimmed title.
    private const int FilesWidth = 1000;
    private static readonly Rectangle _filesArea = new((VirtualScreen.Width - FilesWidth) / 2, 0, FilesWidth, VirtualScreen.Height);

    private readonly SaveSlots _slots;
    private readonly IReadOnlyList<SaveSlotEntry> _saves;
    private readonly TitleMenu _menu;
    private readonly SaveFilesPanel _files;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _entryFont;
    private readonly string _name;
    private readonly Vector2 _namePosition;
    private readonly string[] _labels;
    private readonly Vector2[] _labelCenters;
    private readonly Vector2[] _labelOrigins;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="slots">The save files, read once each time the title shows.</param>
    /// <param name="ui">The shared gradient textures the file list uses.</param>
    public TitleScene(ScreenText text, SaveSlots slots, UiTextures ui)
    {
        _slots = slots;
        _saves = slots.ReadAll();
        _menu = new TitleMenu(SaveSlots.MostRecent(_saves) is not null);
        _files = new SaveFilesPanel(slots, MessageIds.TitleLoad, text, ui);
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

    /// <summary>Whether the file list is open over the title, after Load.</summary>
    public bool IsChoosingFile { get; private set; }

    public SceneRequest? Update(TimeSpan delta, InputFrame input) => IsChoosingFile ? ChooseFile(input) : Choose(_menu.Update(input));

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Title), Color.White);
        batch.DrawString(_nameFont, _name, _namePosition + _shadowOffset, _shadow, effect: FontSystemEffect.Blurry, effectAmount: ShadowBlur);
        batch.DrawString(_nameFont, _name, _namePosition, Color.White);
        for (var index = 0; index < _labels.Length; index++)
        {
            DrawEntry(batch, index);
        }

        if (IsChoosingFile)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
            batch.Draw(textures.Pixel, _filesArea, UiTheme.FilesOverlay);
            _files.Draw(batch, textures, _filesArea, opacity: 1);
        }
    }

    private static string LabelIdOf(TitleEntry entry) => entry switch
    {
        TitleEntry.NewGame => MessageIds.TitleNewGame,
        TitleEntry.Continue => MessageIds.TitleContinue,
        TitleEntry.Load => MessageIds.TitleLoad,
        TitleEntry.Quit => MessageIds.TitleQuit,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    private SceneRequest? Choose(TitleEntry? entry)
    {
        switch (entry)
        {
            case TitleEntry.NewGame:
                return new NewGameRequested();
            case TitleEntry.Continue:
                return ContinueLatest();
            case TitleEntry.Load:
                _files.Open();
                IsChoosingFile = true;
                return null;
            case TitleEntry.Quit:
                return new QuitRequested();
            default:
                return null;
        }
    }

    // Continue is enabled only with a readable save; a file broken since the title read it is logged and changes nothing.
    private LoadRequested? ContinueLatest() =>
        SaveSlots.MostRecent(_saves) is { } latest && _slots.Load(latest.Number) is { } save ? new LoadRequested(save) : null;

    private LoadRequested? ChooseFile(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            IsChoosingFile = false;
            return null;
        }

        return _files.ChooseToLoad(input) is { } save ? new LoadRequested(save) : null;
    }

    // Drawn around its center, so the selected entry grows in place.
    private void DrawEntry(SpriteBatch batch, int index)
    {
        var entry = TitleMenu.Entries[index];
        if (!_menu.IsEnabled(entry))
        {
            batch.DrawString(_entryFont, _labels[index], _labelCenters[index], UiTheme.TextDisabled, origin: _labelOrigins[index]);
            return;
        }

        var isSelected = entry == _menu.Selected;
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
