namespace Jrpg.Game.Battle;

/// <summary>The impact's light at one moment (decision 660), each part from 0, unlit, to its height.</summary>
/// <param name="SceneFlash">The warm flash's opacity over the battlefield.</param>
/// <param name="Wash">
/// The white over the hit target; nearer battlers get a share (<see cref="ImpactLight.WashAt"/>).
/// </param>
/// <param name="FloorGlow">The warm ellipse's opacity on the floor at each hit target's feet.</param>
/// <param name="Halo">The warm disc's opacity on each hit target, after the flash.</param>
internal readonly record struct ImpactLightLook(float SceneFlash, float Wash, float FloorGlow, float Halo);
