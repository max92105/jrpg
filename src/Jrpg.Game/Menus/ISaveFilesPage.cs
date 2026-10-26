using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>Saving or loading: the save files, shown in the System page's place until Cancel.</summary>
internal interface ISaveFilesPage
{
    /// <summary>False while something in the page, such as a question, answers Cancel itself.</summary>
    bool LeavesOnCancel { get; }

    /// <summary>The footer's line while these files show; see <see cref="IMenuPage.Hint"/>.</summary>
    string Hint { get; }

    /// <summary>Reads the files; called each time the System page opens this one.</summary>
    void Open();

    /// <summary>Moves the list's selection on by one update's time, while these files show.</summary>
    void Advance(TimeSpan delta);

    /// <returns>What choosing a file asks the flow for, or null.</returns>
    SceneRequest? Update(InputFrame input);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and gradients come from.</param>
    /// <param name="area">The menu's page area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity);
}
