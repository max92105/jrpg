using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Menus;

/// <summary>A stat preview's new attack and defense as text, each colored by whether it goes up or down.</summary>
internal sealed record StatPreviewText(StatPreviewText.PreviewValue Attack, StatPreviewText.PreviewValue Defense)
{
    public static StatPreviewText Of(StatPreview preview) => new(
        new PreviewValue(NumberText.Of(preview.WithItem.Attack), UiTheme.PreviewColorOf(preview.AttackChange)),
        new PreviewValue(NumberText.Of(preview.WithItem.Defense), UiTheme.PreviewColorOf(preview.DefenseChange)));

    /// <summary>One previewed value and its color.</summary>
    public sealed record PreviewValue(string Text, Color Color);
}
