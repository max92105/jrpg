using Jrpg.Core.Battle;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws every living battler at its spot in the formation, centered on it with its feet on the spot's bottom. The
/// fallen are gone, as legacy <c>AnimateDeath</c> faded out any combatant that fell.
/// </summary>
internal sealed class BattlersView
{
    private readonly IReadOnlyList<Combatant> _combatants;
    private readonly Rectangle[] _spots;
    private readonly BattlerArt[] _arts;
    private readonly UiTextures _ui;

    /// <param name="combatants">The battle's combatants, in its order.</param>
    /// <param name="ui">Where painted battlers are shrunk, once.</param>
    public BattlersView(IReadOnlyList<Combatant> combatants, UiTextures ui)
    {
        _combatants = combatants;
        _spots = BattleLayout.SpotsOf(combatants);
        _arts = [.. combatants.Select(BattlerArt.Of)];
        _ui = ui;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        for (var index = 0; index < _combatants.Count; index++)
        {
            if (_combatants[index].IsAlive)
            {
                DrawBattler(batch, textures, _arts[index], _spots[index]);
            }
        }
    }

    private void DrawBattler(SpriteBatch batch, TextureLibrary textures, BattlerArt art, Rectangle spot)
    {
        var source = textures.Get(ArtFolders.Battlers, art.SpriteFile);
        var scaling = BattlerScaling.Of(new Point(source.Width, source.Height));
        var image = scaling.IsShrunkSmoothly ? _ui.Battler(textures, art.SpriteFile, scaling.DrawnSize.X) : source;
        var size = scaling.DrawnSize;
        var destination = new Rectangle(spot.Center.X - (size.X / 2), spot.Bottom - size.Y, size.X, size.Y);
        var effects = art.IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.Draw(image, destination, null, Color.White, 0, Vector2.Zero, effects, 0);
    }
}
