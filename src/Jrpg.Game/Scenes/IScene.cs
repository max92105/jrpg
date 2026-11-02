using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>
/// What the host updates and draws: one screen of the game (exploration, a battle, a menu), or the flow that stacks
/// them. Screens never call each other (ADR 0008).
/// </summary>
internal interface IScene
{
    /// <returns>What the scene asks the flow for, or null (almost always).</returns>
    SceneRequest? Update(TimeSpan delta, InputFrame input);

    /// <summary>
    /// The effect files the scene's last update asks for (ADR 0035). The flow reads the top scene's right after its
    /// update, before applying the request it returned, so a sound asked in the update that opens a screen or starts a
    /// fade is played once. A scene that makes none keeps this default.
    /// </summary>
    IReadOnlyList<string> Sounds => [];

    /// <summary>Draws in virtual-screen coordinates; the batch is already begun.</summary>
    void Draw(SpriteBatch batch, TextureLibrary textures);
}
