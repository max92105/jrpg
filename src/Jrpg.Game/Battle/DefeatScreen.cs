using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The lost battle's choices over the frozen battle (decision 65): load the latest save, pick one from the save list,
/// or end the session for the title. The choices take input once they have come in. <see cref="DefeatScreenView"/>
/// draws it.
/// </summary>
internal sealed class DefeatScreen
{
    private readonly SaveSlots _slots;
    private readonly IReadOnlyList<SaveSlotEntry> _saves;
    private readonly AskedSounds _sounds = new();
    private bool _hasLeft;

    /// <param name="slots">The save files, read once when the battle is lost.</param>
    /// <param name="text">Messages and fonts, for the save list.</param>
    /// <param name="ui">The shared gradient textures the save list uses.</param>
    public DefeatScreen(SaveSlots slots, ScreenText text, UiTextures ui)
    {
        _slots = slots;
        _saves = slots.ReadAll();
        Menu = new DefeatMenu(SaveSlots.MostRecent(_saves) is not null);
        Files = new SaveFilesPanel(slots, MessageIds.TitleLoad, text, ui);
    }

    public DefeatMenu Menu { get; }

    /// <summary>The save list Load opens.</summary>
    public SaveFilesPanel Files { get; }

    public DefeatEntrance Entrance { get; } = new();

    /// <summary>What the last update asks to hear, from the choices or the save list.</summary>
    public IReadOnlyList<string> Sounds => _sounds.Files;

    /// <summary>Whether the save list is open over the choices, after Load.</summary>
    public bool IsChoosingFile { get; private set; }

    /// <returns><see cref="LoadRequested"/> or <see cref="TitleRequested"/> once, on the update a choice is made; null otherwise.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        Entrance.Update(delta);
        Files.Advance(delta);
        if (_hasLeft || !Entrance.IsComplete)
        {
            return null;
        }

        var request = IsChoosingFile ? ChooseFile(input) : ChooseFromMenu(input);
        _hasLeft = request is not null;
        return request;
    }

    private SceneRequest? ChooseFromMenu(InputFrame input)
    {
        var entry = Menu.Update(input);
        _sounds.AskAll(Menu.Sounds);
        var request = Choose(entry);

        // A load whose file broke since the screen read it changes nothing, so it is silent (decision 421).
        if (entry is DefeatEntry.LoadLastSave ? request is not null : entry is not null)
        {
            _sounds.Ask(UiSoundFiles.Effects.MenuConfirm);
        }

        return request;
    }

    private SceneRequest? Choose(DefeatEntry? entry)
    {
        switch (entry)
        {
            case DefeatEntry.LoadLastSave:
                return LoadLatest();
            case DefeatEntry.Load:
                Files.Open();
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
            _sounds.Ask(UiSoundFiles.Effects.MenuCancel);
            return null;
        }

        var save = Files.ChooseToLoad(input);
        _sounds.AskAll(Files.Sounds);
        return save is null ? null : new LoadRequested(save);
    }
}
