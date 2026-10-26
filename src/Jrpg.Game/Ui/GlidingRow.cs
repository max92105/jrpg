using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>A page list's selected row where its <see cref="SelectionMotion"/> has it now.</summary>
/// <param name="Area">The band's area: a row's, or one between two while the highlight glides.</param>
/// <param name="PointerOffset">How far left of the band the pointer has bobbed, in pixels.</param>
internal readonly record struct GlidingRow(Rectangle Area, float PointerOffset);
