using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>How one size of turn order diamond looks.</summary>
/// <param name="Size">The diamond's square side in pixels.</param>
/// <param name="Frame">The hairline frame's color.</param>
/// <param name="OuterFrame">The second, brighter frame around the first; null for none.</param>
internal sealed record TurnDiamondStyle(int Size, Color Frame, Color? OuterFrame);
