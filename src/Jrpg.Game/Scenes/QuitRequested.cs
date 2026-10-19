namespace Jrpg.Game.Scenes;

/// <summary>Close the game; the flow passes it up to the host, which ends the run.</summary>
internal sealed record QuitRequested : SceneRequest;
