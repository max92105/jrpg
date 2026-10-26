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
/// The screen the game opens on (decision 82): the picture covering the screen, the game's name at the top with a drop
/// shadow, the entries centered below it with the selected one in the shared selection look (decision 252), and a
/// footer line. It comes in from black (<see cref="TitleEntrance"/>), and takes input from the start.
/// </summary>
internal sealed class TitleScene : IScene
{
    // Placeholder sizes on the 1920 x 1080 virtual screen (decision 82).
    private const int NameTextPixels = 120;
    private const int NameTop = 60;
    private const int EntryTextPixels = 34;

    // Entries are 56 px rows, 6 px apart; the selected band is as wide as the longest entry plus 90 px each side.
    private const int EntryRowHeight = 56;
    private const int EntryGap = 6;
    private const int BandPadding = 90;

    // The footer line, centered 40 px above the bottom.
    private const int FooterTextPixels = 22;
    private const int FooterBottomMargin = 40;

    // The entries are centered in the space below the name, which ends about 210 px down.
    private const int EntriesCenterY = 630;

    // The name's drop shadow: black at 70 %, blur 10, offset 4 px at 320 degrees, which is about (3.1, 2.6).
    private const int ShadowBlur = 10;
    private static readonly Vector2 _shadowOffset = new(3, 3);
    private static readonly Color _shadow = Color.Black * 0.7f;

    // The file list in a 1000 px column, #AA111111, over the dimmed title.
    private const int FilesWidth = 1000;
    private static readonly Rectangle _filesArea = new((VirtualScreen.Width - FilesWidth) / 2, 0, FilesWidth, VirtualScreen.Height);

    private readonly SaveSlots _slots;
    private readonly IReadOnlyList<SaveSlotEntry> _saves;
    private readonly TitleMenu _menu;
    private readonly SaveFilesPanel _files;
    private readonly UiTextures _ui;
    private readonly TitleEntrance _entrance = new();
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _entryFont;
    private readonly string _name;
    private readonly Vector2 _namePosition;
    private readonly string[] _labels;
    private readonly Vector2[] _labelPositions;
    private readonly Rectangle[] _bands;
    private readonly SpriteFontBase _footerFont;
    private readonly string _footer;
    private readonly Vector2 _footerPosition;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="slots">The save files, read once each time the title shows.</param>
    /// <param name="ui">The shared textures: the selected entry's band, and the file list's gradients.</param>
    public TitleScene(ScreenText text, SaveSlots slots, UiTextures ui)
    {
        _slots = slots;
        _saves = slots.ReadAll();
        _menu = new TitleMenu(SaveSlots.MostRecent(_saves) is not null);
        _files = new SaveFilesPanel(slots, MessageIds.TitleLoad, text, ui);
        _ui = ui;
        _nameFont = text.Fonts.TitleFontAt(NameTextPixels);
        _entryFont = text.Fonts.FontAt(EntryTextPixels);
        _name = text.Messages.Text(MessageIds.GameTitle);
        _namePosition = new Vector2((VirtualScreen.Width - _nameFont.MeasureString(_name).X) / 2f, NameTop);

        _labels = [.. _menu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
        _bands = EntryBands(_labels.Max(label => _entryFont.MeasureString(label).X), _labels.Length);
        _labelPositions = [.. _labels.Select((label, index) => CenteredIn(_bands[index], _entryFont.MeasureString(label)))];
        _footerFont = text.Fonts.FontAt(FooterTextPixels);
        _footer = text.Messages.Text(MessageIds.TitleFooter);
        var footerTop = VirtualScreen.Height - FooterBottomMargin - _footerFont.LineHeight;
        _footerPosition = new Vector2((VirtualScreen.Width - _footerFont.MeasureString(_footer).X) / 2f, footerTop);
    }

    /// <summary>Whether the file list is open over the title, after Load.</summary>
    public bool IsChoosingFile { get; private set; }

    /// <summary>The entry the cursor is on, so tests can pick entries by what they are.</summary>
    internal TitleEntry SelectedEntry => _menu.Selected;

    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _entrance.Advance(delta);
        return IsChoosingFile ? ChooseFile(input) : Choose(_menu.Update(input));
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _entrance.MarkDrawn();
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Backgrounds, UiArtFiles.Backgrounds.Title), Color.White * _entrance.PictureOpacity);
        var nameOpacity = _entrance.NameOpacity;
        batch.DrawString(_nameFont, _name, _namePosition + _shadowOffset, _shadow * nameOpacity, effect: FontSystemEffect.Blurry, effectAmount: ShadowBlur);
        batch.DrawString(_nameFont, _name, _namePosition, Color.White * nameOpacity);
        var entriesOpacity = _entrance.EntriesOpacity;
        DrawEntries(batch, textures, entriesOpacity);
        batch.DrawString(_footerFont, _footer, _footerPosition, UiTheme.TextSecondary * entriesOpacity);

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

    // Continue is listed only with a readable save; a file broken since the title read it is logged and changes nothing.
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

    // One band per entry, all as wide as the longest, stacked and centered in the space below the name.
    private static Rectangle[] EntryBands(float longestEntry, int count)
    {
        var width = (int)longestEntry + (2 * BandPadding);
        var pitch = EntryRowHeight + EntryGap;
        var firstTop = EntriesCenterY - (((count * pitch) - EntryGap) / 2);
        return [.. Enumerable.Range(0, count).Select(index => new Rectangle((VirtualScreen.Width - width) / 2, firstTop + (index * pitch), width, EntryRowHeight))];
    }

    private static Vector2 CenteredIn(Rectangle band, Vector2 size) =>
        new(band.Center.X - (size.X / 2f), band.Center.Y - (size.Y / 2f));

    // Plain white entries, the selected one in the shared band with the pointer.
    private void DrawEntries(SpriteBatch batch, TextureLibrary textures, float opacity)
    {
        for (var index = 0; index < _labels.Length; index++)
        {
            if (_menu.Entries[index] == _menu.Selected)
            {
                var band = _bands[index];
                _ui.DrawSelectedRow(batch, textures, band, opacity);
                MenuPointer.Draw(batch, textures.Pixel, new Vector2(band.X, band.Center.Y), opacity);
            }

            batch.DrawString(_entryFont, _labels[index], _labelPositions[index], Color.White * opacity);
        }
    }
}
