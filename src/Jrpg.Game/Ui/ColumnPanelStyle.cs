using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>A column panel's look (decision 269): a flat translucent fill and the hairline at its side.</summary>
/// <param name="Fill">Over the whole column, premultiplied like every color the game draws.</param>
/// <param name="Edge">The 1 px line down the column's side, premultiplied likewise.</param>
internal sealed record ColumnPanelStyle(Color Fill, Color Edge);
