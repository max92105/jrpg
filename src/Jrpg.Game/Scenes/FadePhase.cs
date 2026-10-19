namespace Jrpg.Game.Scenes;

/// <summary>Where a <see cref="ScreenFade"/> is, in the order it goes through them.</summary>
internal enum FadePhase
{
    Idle,
    FadingOut,
    Holding,
    FadingIn,
}
