using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>What a <see cref="PipRow"/> shows this frame.</summary>
/// <param name="Count">How many pips: the resource's max.</param>
/// <param name="Lit">How many of them are lit, from the left.</param>
/// <param name="Color">The lit pips' color.</param>
/// <param name="Opacity">0 to 1, while the panel is dimmed or fading.</param>
/// <param name="Flash">The pips that just changed and how bright their flash still is.</param>
internal readonly record struct PipRowLook(int Count, int Lit, Color Color, float Opacity, PipFlash Flash);
