using Jrpg.Core.Battle;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws every living battler at its spot in the formation, centered on it with its feet on the spot's bottom, the
/// actor stepped out toward the other side. Whoever falls, party member or enemy, is lit white, then fades out.
/// </summary>
internal sealed class BattlersView
{
    private readonly IReadOnlyList<Combatant> _combatants;
    private readonly Formation _formation;
    private Rectangle[] _spots;
    private Rectangle[] _spotsBefore;
    private readonly FormationSlide _slide = new();
    private IReadOnlyList<Combatant> _covered = [];
    private readonly BattlerArt[] _arts;
    // How strongly the white lies over a falling battler: bright, with its own colors still showing through.
    private const float FallenWhiteness = 0.6f;

    // Placeholder: the floor glow under a covered ally is a flat diamond, 1.4 times the spot's width and 24 px tall.
    private const float GlowWidthShare = 1.4f;
    private const int GlowHeight = 24;
    private const int GlowSize = 64;
    private static readonly DiamondShape _glow = new(GlowSize, 0, 1, Color.White, Color.White);

    private readonly BattleTurnSequence _sequence;
    private readonly UiTextures _ui;

    /// <param name="battle">Whose combatants it draws, and where the party stands in its formation.</param>
    /// <param name="sequence">Who is stepping out of the formation, and how far.</param>
    /// <param name="ui">Where painted battlers are shrunk, once.</param>
    public BattlersView(BattleState battle, BattleTurnSequence sequence, UiTextures ui)
    {
        var combatants = battle.Combatants;
        _combatants = combatants;
        _formation = battle.Formation;
        _sequence = sequence;
        _spots = BattleLayout.SpotsOf(combatants, _formation);
        _spotsBefore = _spots;
        _arts = [.. combatants.Select(BattlerArt.Of)];
        _ui = ui;
    }

    /// <summary>
    /// Places each party member by the formation now, called as a stage starts, not per frame: members whose place
    /// changed slide there from where they stand.
    /// </summary>
    public void RefreshSpots()
    {
        var spots = BattleLayout.SpotsOf(_combatants, _formation);
        if (spots.SequenceEqual(_spots))
        {
            return;
        }

        _spotsBefore = [.. _spotsBefore.Select((before, index) => _slide.SpotOf(before, _spots[index]))];
        _spots = spots;
        _slide.Restart();
    }

    public void Update(TimeSpan delta) => _slide.Update(delta);

    /// <summary>The allies a choice being aimed would cover all at once, such as an aura's; their spots glow.</summary>
    public void ShowCoverage(IReadOnlyList<Combatant> covered) => _covered = covered;

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawCoverage(batch, textures);
        for (var index = 0; index < _combatants.Count; index++)
        {
            var combatant = _combatants[index];
            var opacity = _sequence.OpacityOf(combatant);
            if (opacity > 0)
            {
                DrawBattler(batch, textures, index, opacity);
            }
        }
    }

    /// <summary>Where <paramref name="combatant"/>'s sprite starts at the top, for whatever points at them.</summary>
    /// <exception cref="ArgumentException">The combatant isn't in this battle.</exception>
    public int TopOf(TextureLibrary textures, Combatant combatant)
    {
        var index = IndexOf(combatant);
        return DrawnArea(textures, _arts[index], SpotOf(combatant)).Top;
    }

    /// <summary>Where <paramref name="combatant"/> stands now: their formation spot, stepped out while they act.</summary>
    /// <exception cref="ArgumentException">The combatant isn't in this battle.</exception>
    public Rectangle SpotOf(Combatant combatant) => SteppedSpot(combatant, SlidSpot(IndexOf(combatant)));

    private Rectangle SlidSpot(int index) => _slide.SpotOf(_spotsBefore[index], _spots[index]);

    // Only a choice that takes several allies at once glows: one target is already shown by the cursor, and enemies are
    // never covered by an aura.
    private void DrawCoverage(SpriteBatch batch, TextureLibrary textures)
    {
        if (_covered.Count < 2 || _covered[0].Side != BattleSide.Party)
        {
            return;
        }

        for (var index = 0; index < _covered.Count; index++)
        {
            var spot = SpotOf(_covered[index]);
            var width = (int)(spot.Width * GlowWidthShare);
            var glow = new Rectangle(spot.Center.X - (width / 2), spot.Bottom - (GlowHeight / 2), width, GlowHeight);
            batch.Draw(_ui.Diamond(textures, _glow), glow, UiTheme.AuraGlow);
        }
    }

    private int IndexOf(Combatant combatant)
    {
        for (var index = 0; index < _combatants.Count; index++)
        {
            if (_combatants[index] == combatant)
            {
                return index;
            }
        }

        throw new ArgumentException($"{combatant.Name} isn't in this battle.", nameof(combatant));
    }

    // Each side steps toward the other: the party left, the enemies right.
    private Rectangle SteppedSpot(Combatant combatant, Rectangle spot)
    {
        var direction = combatant.Side == BattleSide.Party ? -1 : 1;
        spot.Offset((int)(direction * BattlePacing.StepDistance * _sequence.StepShare(combatant)), 0);
        return spot;
    }

    // Centered on the spot, feet on its bottom.
    private static Rectangle DrawnArea(TextureLibrary textures, BattlerArt art, Rectangle spot)
    {
        var source = textures.Get(ArtFolders.Battlers, art.SpriteFile);
        var size = BattlerScaling.Of(new Point(source.Width, source.Height)).DrawnSize;
        return new Rectangle(spot.Center.X - (size.X / 2), spot.Bottom - size.Y, size.X, size.Y);
    }

    // A fallen battler, still fading, is lit white over its own colors.
    private void DrawBattler(SpriteBatch batch, TextureLibrary textures, int index, float opacity)
    {
        var (combatant, art) = (_combatants[index], _arts[index]);
        var area = DrawnArea(textures, art, SteppedSpot(combatant, SlidSpot(index)));
        var source = textures.Get(ArtFolders.Battlers, art.SpriteFile);
        var scaling = BattlerScaling.Of(new Point(source.Width, source.Height));
        var image = scaling.IsShrunkSmoothly ? _ui.Battler(textures, art.SpriteFile, scaling.DrawnSize.X) : source;
        var effects = art.IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.Draw(image, area, null, Color.White * opacity, 0, Vector2.Zero, effects, 0);
        if (!combatant.IsAlive)
        {
            batch.Draw(_ui.WhiteSilhouette(textures, image), area, null, Color.White * (opacity * FallenWhiteness), 0, Vector2.Zero, effects, 0);
        }
    }
}
