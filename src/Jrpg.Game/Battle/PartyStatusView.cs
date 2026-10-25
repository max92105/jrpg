using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The active party's panels at the top right: the name, then large HP over its maximum and a thin bar, beside a
/// diamond portrait. The acting member's panel slides left; a fallen member's is dimmed, its HP in red.
/// </summary>
internal sealed class PartyStatusView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    private const int RightMargin = 40;
    private const int TopMargin = 24;
    private const int PanelGap = 14;
    private const int ActiveSlide = 36;
    private const float FallenOpacity = 0.55f;
    private const int PortraitSize = 64;
    private const int PortraitGap = 10;
    private const int NamePixels = 22;
    private const int LabelPixels = 14;
    private const int HpPixels = 30;
    private const int MaxHpPixels = 17;

    private readonly UiTextures _ui;
    private readonly OutlinedFont _nameFont;
    private readonly PartyStatusPanel[] _panels;
    private readonly int _panelHeight;

    /// <param name="party">The party's combatants, in active order.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the bars' gradients and the diamond portraits are made, once.</param>
    public PartyStatusView(IReadOnlyList<Combatant> party, ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _nameFont = new OutlinedFont(text.Fonts.FontAt(NamePixels));
        var label = new OutlinedFont(text.Fonts.FontAt(LabelPixels));
        var hpStyle = new VitalRowStyle(label, new OutlinedFont(text.Fonts.FontAt(HpPixels)), new OutlinedFont(text.Fonts.FontAt(MaxHpPixels)), Width: 250, SmallTextLift: 5);
        var hpLabel = MeasuredText.Of(label.Font, text.Messages.Text(MessageIds.StatHp));
        _panels =
        [
            .. party.Select(member => new PartyStatusPanel(
                member,
                MeasuredText.Of(_nameFont.Font, member.Name),
                new BattleVitalRow(hpStyle, hpLabel, ui, UiTheme.BattleHpBar),
                text.Messages)),
        ];
        Refresh();
        // Never shorter than the portrait, so the diamonds of neighboring panels can't overlap.
        _panelHeight = _panels.Length == 0 ? 0 : Math.Max(PortraitSize, (int)_nameFont.Font.LineHeight + _panels[0].HpRow.Height);
    }

    /// <summary>Formats the numbers that changed since the last refresh; allocates nothing when none did.</summary>
    public void Refresh()
    {
        foreach (var panel in _panels)
        {
            panel.Refresh();
        }
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the portraits and the plain pixel come from.</param>
    /// <param name="actor">The combatant acting on screen, whose panel slides out; null when no member acts.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Combatant? actor)
    {
        for (var index = 0; index < _panels.Length; index++)
        {
            var panel = _panels[index];
            var slide = panel.Combatant == actor ? ActiveSlide : 0;
            var topRight = new Point(VirtualScreen.Width - RightMargin - slide, TopMargin + (index * (_panelHeight + PanelGap)));
            DrawPanel(batch, textures, panel, topRight);
        }
    }

    private void DrawPanel(SpriteBatch batch, TextureLibrary textures, PartyStatusPanel panel, Point topRight)
    {
        var isAlive = panel.Combatant.IsAlive;
        var opacity = isAlive ? 1 : FallenOpacity;
        var portrait = new Rectangle(topRight.X - PortraitSize, topRight.Y, PortraitSize, PortraitSize);
        batch.Draw(_ui.DiamondPortrait(textures, panel.PortraitFile, PortraitSize), portrait, Color.White * opacity);

        var textRight = portrait.X - PortraitGap;
        _nameFont.Draw(batch, panel.Name.Text, new Vector2(textRight - panel.Name.Width, topRight.Y), UiTheme.TextPrimary * opacity);
        var hpTop = topRight.Y + (int)_nameFont.Font.LineHeight;
        panel.HpRow.Draw(batch, textures, new Point(textRight, hpTop), (isAlive ? UiTheme.TextPrimary : UiTheme.Danger) * opacity);
    }
}
