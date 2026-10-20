using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Loading from the menu: Confirm on a readable file asks the flow to continue from it. An empty or unreadable file does
/// nothing; one that turned unreadable since the list opened is shown so (the reason is logged), and the list stays.
/// </summary>
/// <param name="slots">The files.</param>
/// <param name="text">Messages and fonts.</param>
/// <param name="ui">The shared gradient textures.</param>
internal sealed class LoadPage(SaveSlots slots, ScreenText text, UiTextures ui) : ISaveFilesPage
{
    private readonly SaveFilesPanel _panel = new(slots, MessageIds.SystemLoad, text, ui);

    /// <summary>The files as last read; empty until the page is opened.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries => _panel.Entries;

    public void Open() => _panel.Open();

    public SceneRequest? Update(InputFrame input)
    {
        return _panel.ChooseToLoad(input) is { } save ? new LoadRequested(save) : null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity) => _panel.Draw(batch, textures, area, opacity);
}
