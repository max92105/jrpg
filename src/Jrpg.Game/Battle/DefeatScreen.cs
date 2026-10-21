using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The lost battle's choices over the frozen battle (legacy <c>DefeatScreen</c>, decision 65): load the latest save, pick
/// one from the save list, or end the session for the title. A plain layout until the defeat look (plan
/// 2026-10-20-battle, B8b2).
/// </summary>
internal sealed class DefeatScreen
{
    private const int Left = 104;
    private const int TitleTop = 112;
    private const int SubtitleTop = 246;
    private const int EntriesTop = 320;
    private const int EntryPitch = 78;
    private const int TitleTextPixels = 96;
    private const int SubtitleTextPixels = 24;
    private const int EntryTextPixels = 28;
    private const int FilesWidth = 1000;
    private static readonly Rectangle _filesArea = new((VirtualScreen.Width - FilesWidth) / 2, 0, FilesWidth, VirtualScreen.Height);

    private readonly SaveSlots _slots;
    private readonly IReadOnlyList<SaveSlotEntry> _saves;
    private readonly DefeatMenu _menu;
    private readonly SaveFilesPanel _files;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _subtitleFont;
    private readonly SpriteFontBase _entryFont;
    private readonly string _title;
    private readonly string _subtitle;
    private readonly string[] _labels;
    private bool _hasLeft;

    /// <param name="slots">The save files, read once when the battle is lost.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures the file list uses.</param>
    public DefeatScreen(SaveSlots slots, ScreenText text, UiTextures ui)
    {
        _slots = slots;
        _saves = slots.ReadAll();
        _menu = new DefeatMenu(SaveSlots.MostRecent(_saves) is not null);
        _files = new SaveFilesPanel(slots, MessageIds.TitleLoad, text, ui);
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _subtitleFont = text.Fonts.FontAt(SubtitleTextPixels);
        _entryFont = text.Fonts.FontAt(EntryTextPixels);
        _title = text.Messages.Text(MessageIds.BattleDefeat);
        _subtitle = text.Messages.Text(MessageIds.BattlePartyFallen);
        _labels = [.. DefeatMenu.Entries.Select(entry => text.Messages.Text(LabelIdOf(entry)))];
    }

    /// <summary>Whether the save list is open over the choices, after Load.</summary>
    public bool IsChoosingFile { get; private set; }

    /// <returns><see cref="LoadRequested"/> or <see cref="TitleRequested"/> once, on the update a choice is made; null otherwise.</returns>
    public SceneRequest? Update(InputFrame input)
    {
        if (_hasLeft)
        {
            return null;
        }

        var request = IsChoosingFile ? ChooseFile(input) : Choose(_menu.Update(input));
        _hasLeft = request is not null;
        return request;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_titleFont, _title, new Vector2(Left, TitleTop), UiTheme.Danger);
        batch.DrawString(_subtitleFont, _subtitle, new Vector2(Left, SubtitleTop), UiTheme.TextSecondary);
        for (var index = 0; index < _labels.Length; index++)
        {
            var entry = DefeatMenu.Entries[index];
            var color = !_menu.IsEnabled(entry) ? UiTheme.TextDisabled
                : entry == _menu.Selected ? UiTheme.Accent
                : UiTheme.TextPrimary;
            batch.DrawString(_entryFont, _labels[index], new Vector2(Left, EntriesTop + (index * EntryPitch)), color);
        }

        if (IsChoosingFile)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
            batch.Draw(textures.Pixel, _filesArea, UiTheme.FilesOverlay);
            _files.Draw(batch, textures, _filesArea, opacity: 1);
        }
    }

    private static string LabelIdOf(DefeatEntry entry) => entry switch
    {
        DefeatEntry.LoadLastSave => MessageIds.BattleLoadLastSave,
        DefeatEntry.Load => MessageIds.TitleLoad,
        DefeatEntry.MainMenu => MessageIds.BattleMainMenu,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    private SceneRequest? Choose(DefeatEntry? entry)
    {
        switch (entry)
        {
            case DefeatEntry.LoadLastSave:
                return LoadLatest();
            case DefeatEntry.Load:
                _files.Open();
                IsChoosingFile = true;
                return null;
            case DefeatEntry.MainMenu:
                return new TitleRequested();
            default:
                return null;
        }
    }

    // As the title's Continue: a file broken since the screen read it is logged and changes nothing.
    private LoadRequested? LoadLatest() =>
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
}
