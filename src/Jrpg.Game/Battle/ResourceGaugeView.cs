using System.Globalization;
using Jrpg.Core.Resources;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// One member's class resources in their battle panel, a row each, right-aligned: the name in small spaced caps, a
/// hollow diamond for a resource that lasts until rest, then pips in the resource's color (a thin bar for a large one).
/// The dominant force's name is lit. Pips that a refresh changed flash once.
/// </summary>
internal sealed class ResourceGaugeView
{
    /// <summary>A resource with at most this many points shows as pips, like the reference BP; else as a bar.</summary>
    public const int PipsUpTo = 10;

    // Placeholder layout on the 1920 x 1080 virtual screen.
    private const float LabelSpacing = 2;
    private const int LabelGap = 8;
    private const int MarkerSize = 11;
    private const float MarkerInnerReach = 0.5f;
    private const int BarWidth = 150;
    private const int BarHeight = 3;
    private const int RowGap = 2;

    private static readonly TimeSpan _flashTime = TimeSpan.FromSeconds(0.25);
    private static readonly DiamondShape _lastingMarker = new(MarkerSize, MarkerInnerReach, 1, Color.White, Color.White);

    private readonly ResourceMeters _meters;
    private readonly OutlinedFont _labelFont;
    private readonly UiTextures _ui;
    private readonly PipRow _pips;
    private readonly GaugeRow[] _rows;

    /// <param name="meters">The member's live amounts, read on every refresh (ADR 0013).</param>
    /// <param name="labelFont">The small font of the resources' names.</param>
    /// <param name="ui">Where the pips, the marker and the bar's gradient are baked, once.</param>
    public ResourceGaugeView(ResourceMeters meters, OutlinedFont labelFont, UiTextures ui)
    {
        _meters = meters;
        _labelFont = labelFont;
        _ui = ui;
        _pips = new PipRow(ui);
        _rows = [.. meters.Definitions.Select(NewRow)];
    }

    public int Height => _rows.Length == 0 ? 0 : (_rows.Length * (RowHeight + RowGap)) - RowGap;

    private int RowHeight => Math.Max((int)_labelFont.Font.LineHeight, PipRow.PipSize);

    public static bool ShowsPips(int max) => max <= PipsUpTo;

    /// <returns>How bright a change's flash still is: 1 as it happens, down to 0 once the flash time is over.</returns>
    public static float FlashShare(TimeSpan sinceChange) => 1 - Math.Clamp((float)(sinceChange / _flashTime), 0, 1);

    /// <summary>A force's name is lit while it dominates (decision 204); other names stay secondary.</summary>
    public static Color LabelColor(bool isDominant) => isDominant ? UiTheme.TextPrimary : UiTheme.TextSecondary;

    /// <summary>Takes the amounts now; a changed one flashes the pips between its old and new amount.</summary>
    public void Refresh()
    {
        foreach (var row in _rows)
        {
            var amount = _meters.AmountOf(row.Resource.Id);
            row.Bar?.Show(amount, row.Max);
            if (amount != row.Shown)
            {
                row.Flash = PipFlash.Between(row.Shown, amount);
                (row.Shown, row.SinceChange) = (amount, TimeSpan.Zero);
            }
        }
    }

    public void Update(TimeSpan delta)
    {
        foreach (var row in _rows)
        {
            row.SinceChange += delta;
        }
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the baked pips and the plain pixel come from.</param>
    /// <param name="topRight">The first row's top right corner.</param>
    /// <param name="opacity">0 to 1, while the panel is dimmed.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Point topRight, float opacity)
    {
        var top = topRight.Y;
        foreach (var row in _rows)
        {
            var middle = top + (RowHeight / 2);
            var gaugeLeft = DrawGauge(batch, textures, row, new Point(topRight.X, middle), opacity);
            var labelRight = gaugeLeft - LabelGap;
            if (row.Resource.IsLasting)
            {
                var marker = new Rectangle(labelRight - MarkerSize, middle - (MarkerSize / 2), MarkerSize, MarkerSize);
                batch.Draw(_ui.Diamond(textures, _lastingMarker), marker, UiTheme.TextSecondary * opacity);
                labelRight = marker.X - LabelGap;
            }

            var labelColor = LabelColor(_meters.IsDominant(row.Resource.Id)) * opacity;
            var labelTop = middle - (_labelFont.Font.LineHeight / 2);
            _labelFont.Draw(batch, row.Label.Text, new Vector2(labelRight - row.Label.Width, labelTop), labelColor, LabelSpacing);
            top += RowHeight + RowGap;
        }
    }

    // Right-aligned on the row's middle; returns where it starts.
    private int DrawGauge(SpriteBatch batch, TextureLibrary textures, GaugeRow row, Point rightMiddle, float opacity)
    {
        if (row.Bar is { } bar)
        {
            var area = new Rectangle(rightMiddle.X - BarWidth, rightMiddle.Y - (BarHeight / 2), BarWidth, BarHeight);
            bar.Draw(batch, textures, area, opacity);
            return area.X;
        }

        var left = rightMiddle.X - PipRow.WidthOf(row.Max);
        var flash = row.Flash with { Share = FlashShare(row.SinceChange) };
        _pips.Draw(batch, textures, new Point(left, rightMiddle.Y - (PipRow.PipSize / 2)), new PipRowLook(row.Max, row.Shown, row.Color, opacity, flash));
        return left;
    }

    private GaugeRow NewRow(ResourceDefinition resource)
    {
        var max = _meters.MaxOf(resource.Id);
        var color = UiTheme.ColorFromHex(resource.Color);
        var caps = resource.Name.ToUpper(CultureInfo.InvariantCulture);
        var label = new MeasuredText(caps, _labelFont.Font.MeasureString(caps, characterSpacing: LabelSpacing).X);

        // One grey gradient for every resource, tinted with its color, so a battle bakes no texture per resource.
        var bar = ShowsPips(max) ? null : new StatBar(_ui, UiTheme.BattleResourceShade, UiTheme.BattleBarTrack, color);
        bar?.Show(_meters.AmountOf(resource.Id), max);
        return new GaugeRow(resource, label, color, max, bar) { Shown = _meters.AmountOf(resource.Id), SinceChange = _flashTime };
    }

    // One resource's row: what's fixed for the battle, then the amount shown and its flash.
    private sealed class GaugeRow(ResourceDefinition resource, MeasuredText label, Color color, int max, StatBar? bar)
    {
        public ResourceDefinition Resource => resource;

        public MeasuredText Label => label;

        public Color Color => color;

        public int Max => max;

        public StatBar? Bar => bar;

        public int Shown { get; set; }

        public PipFlash Flash { get; set; }

        public TimeSpan SinceChange { get; set; }
    }
}
