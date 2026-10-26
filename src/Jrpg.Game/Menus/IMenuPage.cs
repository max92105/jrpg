using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>One page of the game menu, entered from its tab; the menu draws the tabs and frames the page.</summary>
internal interface IMenuPage
{
    /// <summary>The message id of the page's name, shown on its tab.</summary>
    string TitleMessageId { get; }

    /// <summary>The placeholder icon before the page's name on its tab (decision 254).</summary>
    TabGlyph Glyph { get; }

    /// <summary>
    /// The page's current line of help for the menu's footer, already formatted. It changes only when the page's state does,
    /// so the menu reads it every frame without allocating.
    /// </summary>
    string Hint { get; }

    /// <summary>
    /// Whether the menu shows the party sidebar beside the page, which then gets a narrower area. False on a page that
    /// shows the party itself, or while it shows its own side panel.
    /// </summary>
    bool ShowsPartySidebar { get; }

    /// <summary>
    /// Whether Cancel now goes back to the tabs. False while something inside the page, such as a confirmation, answers
    /// Cancel itself.
    /// </summary>
    bool LeavesOnCancel { get; }

    /// <summary>
    /// Called each time the player enters the page from its tab: rebuilds what it shows from Core and starts on the first
    /// entry.
    /// </summary>
    void Open();

    /// <summary>
    /// The time since the last update, given before <see cref="Update"/> so a page can move over time (glide its
    /// selection, bob its pointer). A page with nothing moving keeps this empty default.
    /// </summary>
    void Advance(TimeSpan delta)
    {
    }

    /// <returns>What the page asks the flow for, or null.</returns>
    SceneRequest? Update(InputFrame input);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">The images the page draws.</param>
    /// <param name="area">Where the page goes, in virtual-screen pixels.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity);
}
