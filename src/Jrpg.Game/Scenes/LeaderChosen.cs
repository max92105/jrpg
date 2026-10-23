using Jrpg.Core.Content;

namespace Jrpg.Game.Scenes;

/// <summary>Start a new playthrough led by this choice, in place of whatever is showing.</summary>
internal sealed record LeaderChosen(LeaderChoice Choice) : SceneRequest;
