using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Under each living enemy, its shown name and a thin red HP bar, on the left edge of its spot, and its status tags
/// under the bar. A fallen enemy's tag fades out with it.
/// </summary>
internal sealed class EnemyTagView
{
    // Placeholder layout: the tag's bottom sits 38 px below the sprite's.
    private const int TagWidth = 160;
    private const int NamePixels = 16;
    private const int NameGap = 3;
    private const int BarHeight = 4;
    private const int GapBelowSprite = 38;
    private const int StatusTagsGap = 2;

    private readonly BattleTurnSequence _sequence;
    private readonly OutlinedFont _font;
    private readonly Combatant[] _enemies;
    private readonly string[] _labels;
    private readonly StatBar[] _bars;
    private readonly Rectangle[] _tagAreas;
    private readonly StatusTagsText[] _statusTags;

    /// <param name="combatants">The battle's combatants, in its order.</param>
    /// <param name="sequence">Whose fall is playing, so a fallen enemy's tag fades with it.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the bars' gradient is made, once.</param>
    public EnemyTagView(IReadOnlyList<Combatant> combatants, BattleTurnSequence sequence, ScreenText text, UiTextures ui)
    {
        _sequence = sequence;
        _font = new OutlinedFont(text.Fonts.FontAt(NamePixels));
        var tagHeight = (int)_font.Font.LineHeight + NameGap + BarHeight;
        var spots = BattleLayout.SpotsOf(combatants);
        var enemies = combatants
            .Select((combatant, index) => (Combatant: combatant, Spot: spots[index]))
            .Where(enemy => enemy.Combatant.Side == BattleSide.Enemies)
            .ToList();
        _enemies = [.. enemies.Select(enemy => enemy.Combatant)];
        _tagAreas = [.. enemies.Select(enemy => new Rectangle(enemy.Spot.X, enemy.Spot.Bottom + GapBelowSprite - tagHeight, TagWidth, tagHeight))];
        _labels = [.. EnemyLabels.Of([.. _enemies.Select(enemy => enemy.Name)], text.Messages)];
        _bars = [.. _enemies.Select(_ => new StatBar(ui, UiTheme.EnemyHpBar, UiTheme.BattleBarTrack))];
        _statusTags = [.. _enemies.Select(enemy => new StatusTagsText(enemy.Statuses, text.Messages))];
        Refresh();
    }

    /// <summary>Fills each bar to its enemy's HP now, and writes its statuses.</summary>
    public void Refresh()
    {
        for (var index = 0; index < _enemies.Length; index++)
        {
            _bars[index].Show(_enemies[index].Vitals.Hp, _enemies[index].Vitals.MaxHp);
            _statusTags[index].Refresh();
        }
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        for (var index = 0; index < _enemies.Length; index++)
        {
            var enemy = _enemies[index];
            var opacity = _sequence.OpacityOf(enemy);
            if (opacity > 0)
            {
                var area = _tagAreas[index];
                _font.Draw(batch, _labels[index], new Vector2(area.X, area.Y), UiTheme.TextPrimary * opacity);
                _bars[index].Draw(batch, textures, new Rectangle(area.X, area.Bottom - BarHeight, area.Width, BarHeight), opacity);
                _font.Draw(batch, _statusTags[index].Line, new Vector2(area.X, area.Bottom + StatusTagsGap), UiTheme.TextSecondary * opacity);
            }
        }
    }
}
