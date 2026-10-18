using Jrpg.Game.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>
/// What the host updates and draws: one screen of the game (exploration, a battle, a menu), or the flow that stacks
/// them. Screens never call each other (ADR 0008).
/// </summary>
internal interface IScene
{
    void Update(TimeSpan delta, InputFrame input);

    /// <summary>Draws in virtual-screen coordinates; the batch is already begun.</summary>
    void Draw(SpriteBatch batch);
}
