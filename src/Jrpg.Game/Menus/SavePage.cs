using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>Saving from the menu: Confirm writes the playthrough into the chosen file. Overwriting doesn't ask yet (S3).</summary>
/// <param name="slots">The files.</param>
/// <param name="session">What a save writes.</param>
/// <param name="text">Messages and fonts.</param>
/// <param name="ui">The shared gradient textures.</param>
internal sealed class SavePage(SaveSlots slots, GameSession session, ScreenText text, UiTextures ui) : ISaveFilesPage
{
    private readonly SaveFilesPanel _panel = new(slots, MessageIds.SystemSave, text, ui);

    /// <summary>The files as last read; empty until the page is opened.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries => _panel.Entries;

    public void Open() => _panel.Open();

    /// <returns>Null: saving asks the flow for nothing.</returns>
    public SceneRequest? Update(InputFrame input)
    {
        if (_panel.Update(input) is { } chosen)
        {
            slots.Write(chosen.Number, session);
            _panel.Refresh();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity) => _panel.Draw(batch, textures, area, opacity);
}
