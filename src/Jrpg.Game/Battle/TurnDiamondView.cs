using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant in a turn order diamond (drawn on a 48 unit grid): a dark backdrop, red in its lower half for an
/// enemy, the portrait cut to a diamond, and a hairline frame. Dimmed when the strip doesn't light it.
/// </summary>
internal sealed class TurnDiamondView
{
    // The diamond's grid (placeholder): backdrop 22 of 24 units, portrait 40 of 48, frames 1 unit thick.
    private const float BackdropReach = 22 / 24f;
    private const float FrameThickness = 1 / 24f;
    private const float PortraitShare = 40 / 48f;
    private const float DimmedOpacity = 0.35f;

    private readonly UiTextures _ui;
    private readonly TurnOrderStrip _strip;
    private readonly IReadOnlyDictionary<Combatant, BattlerArt> _arts;
    private readonly int _size;
    private readonly int _portraitSize;
    private readonly DiamondShape _partyBackdrop;
    private readonly DiamondShape _enemyBackdrop;
    private readonly DiamondShape _frame;
    private readonly DiamondShape? _outerFrame;

    /// <param name="style">The diamond's size and frame colors.</param>
    /// <param name="ui">Where the shapes and portraits are baked, once.</param>
    /// <param name="strip">Who is lit.</param>
    /// <param name="arts">Each combatant's art, for its portrait.</param>
    public TurnDiamondView(TurnDiamondStyle style, UiTextures ui, TurnOrderStrip strip, IReadOnlyDictionary<Combatant, BattlerArt> arts)
    {
        _ui = ui;
        _strip = strip;
        _arts = arts;
        _size = style.Size;
        _portraitSize = (int)(style.Size * PortraitShare);
        _partyBackdrop = new DiamondShape(style.Size, 0, BackdropReach, UiTheme.TurnBackdrop, UiTheme.TurnBackdrop);
        _enemyBackdrop = _partyBackdrop with { Lower = UiTheme.TurnEnemyHalf };
        _frame = new DiamondShape(style.Size, BackdropReach - FrameThickness, BackdropReach, style.Frame, style.Frame);
        _outerFrame = style.OuterFrame is { } outer ? new DiamondShape(style.Size, 1 - FrameThickness, 1, outer, outer) : null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Point topLeft, Combatant combatant)
    {
        var tint = Color.White * (_strip.IsLit(combatant) ? 1 : DimmedOpacity);
        var area = new Rectangle(topLeft.X, topLeft.Y, _size, _size);
        var backdrop = combatant.Side == BattleSide.Enemies ? _enemyBackdrop : _partyBackdrop;
        batch.Draw(_ui.Diamond(textures, backdrop), area, tint);
        var inset = (_size - _portraitSize) / 2;
        var portrait = _ui.DiamondPortrait(textures, _arts[combatant].PortraitFile, _portraitSize);
        batch.Draw(portrait, new Rectangle(topLeft.X + inset, topLeft.Y + inset, _portraitSize, _portraitSize), tint);
        batch.Draw(_ui.Diamond(textures, _frame), area, tint);
        if (_outerFrame is not null)
        {
            batch.Draw(_ui.Diamond(textures, _outerFrame), area, tint);
        }
    }
}
