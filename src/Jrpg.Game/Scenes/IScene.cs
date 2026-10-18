using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>One screen of the game: exploration, a battle, a menu. Screens never call each other (ADR 0008).</summary>
internal interface IScene
{
    void Update(TimeSpan delta);

    /// <summary>Draws in virtual-screen coordinates; the batch is already begun.</summary>
    void Draw(SpriteBatch batch);
}
