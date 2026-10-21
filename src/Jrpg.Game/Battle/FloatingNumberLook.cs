namespace Jrpg.Game.Battle;

/// <summary>How a floating number looks at one moment.</summary>
/// <param name="Scale">Its size, 1 at full size.</param>
/// <param name="Rise">How far it has risen, in pixels.</param>
/// <param name="Opacity">0 gone to 1 solid.</param>
internal readonly record struct FloatingNumberLook(float Scale, float Rise, float Opacity);
