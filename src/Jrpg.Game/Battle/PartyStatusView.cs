using Jrpg.Core.Battle;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The active party's panels at the top right: the name, large HP over its maximum and a thin bar, then the class
/// resources as pips (<see cref="ResourceGaugeView"/>), beside a diamond portrait; each panel is as tall as its rows.
/// The acting member's panel slides left; a fallen member's is dimmed, its HP in red.
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
    private const int ResourceTopGap = 4;
    private const int StatusTagGap = 12;

    private readonly UiTextures _ui;
    private readonly OutlinedFont _nameFont;
    private readonly PartyStatusPanel[] _panels;
    private readonly OutlinedFont _tagFont;
    private readonly MeasuredText[] _statusTags;
    private readonly Formation _formation;
    private readonly int[] _drawOrder;

    /// <param name="formation">The party's line; the panels follow it from top to bottom (ADR 0023).</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">Where the bars' gradients and the diamond portraits are made, once.</param>
    public PartyStatusView(Formation formation, ScreenText text, UiTextures ui)
    {
        _formation = formation;
        _ui = ui;
        _nameFont = new OutlinedFont(text.Fonts.FontAt(NamePixels));
        var label = new OutlinedFont(text.Fonts.FontAt(LabelPixels));
        var hpStyle = new VitalRowStyle(label, new OutlinedFont(text.Fonts.FontAt(HpPixels)), new OutlinedFont(text.Fonts.FontAt(MaxHpPixels)), Width: 250, SmallTextLift: 5);
        var hpLabel = MeasuredText.Of(label.Font, text.Messages.Text(MessageIds.StatHp));
        _tagFont = label;
        _panels =
        [
            .. formation.Line.Select(member => new PartyStatusPanel(
                member,
                MeasuredText.Of(_nameFont.Font, member.Name),
                new PartyStatusRows(new BattleVitalRow(hpStyle, hpLabel, new StatBar(ui, UiTheme.BattleHpBar, UiTheme.BattleBarTrack)), new ResourceGaugeView(member.Resources, label, ui)),
                text.Messages)),
        ];
        _statusTags = [.. _panels.Select(_ => new MeasuredText(string.Empty, 0))];
        _drawOrder = new int[_panels.Length];
        Refresh();
    }

    /// <summary>
    /// Formats the numbers that changed since the last refresh, allocating nothing when none did, and orders the panels
    /// by the formation now.
    /// </summary>
    public void Refresh()
    {
        for (var index = 0; index < _panels.Length; index++)
        {
            _drawOrder[_formation.PositionOf(_panels[index].Combatant)] = index;
        }

        for (var index = 0; index < _panels.Length; index++)
        {
            _panels[index].Refresh();
            var line = _panels[index].StatusTags.Line;
            if (!ReferenceEquals(line, _statusTags[index].Text))
            {
                _statusTags[index] = MeasuredText.Of(_tagFont.Font, line);
            }
        }
    }

    /// <summary>Advances the resource pips'"'"' flashes.</summary>
    public void Update(TimeSpan delta)
    {
        foreach (var panel in _panels)
        {
            panel.Resources.Update(delta);
        }
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the portraits and the plain pixel come from.</param>
    /// <param name="actor">The combatant acting on screen, whose panel slides out; null when no member acts.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Combatant? actor)
    {
        var top = TopMargin;
        foreach (var index in _drawOrder)
        {
            var panel = _panels[index];
            var slide = panel.Combatant == actor ? ActiveSlide : 0;
            var topRight = new Point(VirtualScreen.Width - RightMargin - slide, top);
            DrawPanel(batch, textures, panel, topRight);
            DrawStatusTags(batch, _statusTags[index], panel, topRight);
            top += HeightOf(panel) + PanelGap;
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
        panel.Resources.Draw(batch, textures, new Point(textRight, hpTop + panel.HpRow.Height + ResourceTopGap), opacity);
    }

    // Left of the name, on its line, so the rows under it keep their place.
    private void DrawStatusTags(SpriteBatch batch, MeasuredText tags, PartyStatusPanel panel, Point topRight)
    {
        if (tags.Width == 0)
        {
            return;
        }

        var opacity = panel.Combatant.IsAlive ? 1 : FallenOpacity;
        var nameLeft = topRight.X - PortraitSize - PortraitGap - panel.Name.Width;
        var nameMiddle = topRight.Y + (_nameFont.Font.LineHeight / 2);
        var position = new Vector2(nameLeft - StatusTagGap - tags.Width, nameMiddle - (_tagFont.Font.LineHeight / 2));
        _tagFont.Draw(batch, tags.Text, position, UiTheme.TextSecondary * opacity);
    }

    // Never shorter than the portrait, so the diamonds of neighboring panels can't overlap.
    private int HeightOf(PartyStatusPanel panel)
    {
        var resources = panel.Resources.Height == 0 ? 0 : ResourceTopGap + panel.Resources.Height;
        var rows = (int)_nameFont.Font.LineHeight + panel.HpRow.Height + resources;

        return Math.Max(PortraitSize, rows);
    }
}
