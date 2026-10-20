using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The command choice drawn plainly until its panel look (plan 2026-10-20-battle, B4c): the list to the left of the acting
/// member, and a diamond marker over each selected target while one is aimed at.
/// </summary>
internal sealed class CommandListView
{
    // Placeholders until B4c's panel: 32 px text, 40 px rows, the list's right edge 40 px left of the actor.
    private const int TextPixels = 32;
    private const int RowHeight = 40;
    private const int ActorGap = 40;
    private const int MarkerSide = 18;
    private const int MarkerGap = 16;

    private readonly CommandMenu _menu;
    private readonly OutlinedFont _font;
    private readonly MeasuredText[] _labels;
    private readonly Dictionary<Combatant, Rectangle> _spots;

    /// <param name="menu">The choice it shows.</param>
    /// <param name="combatants">The battle's combatants, for where each stands.</param>
    /// <param name="text">The labels and their font.</param>
    public CommandListView(CommandMenu menu, IReadOnlyList<Combatant> combatants, ScreenText text)
    {
        _menu = menu;
        _font = new OutlinedFont(text.Fonts.FontAt(TextPixels));
        _labels = [.. CommandMenu.Entries.Select(entry => MeasuredText.Of(_font.Font, text.Messages.Text(LabelIdOf(entry))))];
        var spots = BattleLayout.SpotsOf(combatants);
        _spots = combatants.Select((combatant, index) => (combatant, spots[index])).ToDictionary();
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="actor">The party member choosing.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Combatant actor)
    {
        if (_menu.Targets is { } targets)
        {
            DrawMarkers(batch, textures, targets.Selected);
            return;
        }

        var spot = _spots[actor];
        var right = spot.X - ActorGap;
        var top = spot.Center.Y - (CommandMenu.Entries.Length * RowHeight / 2);
        for (var index = 0; index < _labels.Length; index++)
        {
            var label = _labels[index];
            var entry = CommandMenu.Entries[index];
            var color = !CommandMenu.IsEnabled(entry) ? UiTheme.TextDisabled
                : entry == _menu.Selected ? UiTheme.TextPrimary
                : UiTheme.TextSecondary;
            _font.Draw(batch, label.Text, new Vector2(right - label.Width, top + (index * RowHeight)), color);
        }
    }

    private static string LabelIdOf(CommandEntry entry) => entry switch
    {
        CommandEntry.Attack => MessageIds.BattleAttack,
        CommandEntry.Skills => MessageIds.BattleSkills,
        CommandEntry.Items => MessageIds.BattleItems,
        CommandEntry.Defend => MessageIds.BattleDefend,
        CommandEntry.Run => MessageIds.BattleRun,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, null),
    };

    private void DrawMarkers(SpriteBatch batch, TextureLibrary textures, IReadOnlyList<Combatant> targets)
    {
        for (var index = 0; index < targets.Count; index++)
        {
            var spot = _spots[targets[index]];
            new Diamond(new Vector2(spot.Center.X, spot.Y - MarkerGap), MarkerSide).Draw(batch, textures.Pixel, UiTheme.Accent);
        }
    }
}
