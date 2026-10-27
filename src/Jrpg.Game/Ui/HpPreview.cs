using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// A card's HP now and after an item, measured once in the card's value font when aiming starts or after a use, so
/// drawing measures nothing (<see cref="MemberCardView.HpPreviewOf"/>).
/// </summary>
/// <param name="Now">The current HP alone, without its maximum.</param>
/// <param name="After">The HP the item would leave.</param>
/// <param name="Color">The after value's color, by whether it rises (<see cref="UiTheme.PreviewColorOf"/>).</param>
internal sealed record HpPreview(MeasuredText Now, MeasuredText After, Color Color);
