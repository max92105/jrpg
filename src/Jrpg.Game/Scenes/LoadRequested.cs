using Jrpg.Core.Saving;

namespace Jrpg.Game.Scenes;

/// <summary>
/// End whatever is on and continue from a save. It carries the save already read, so a file that turned unreadable is
/// caught while its list is still open rather than once the screen is black.
/// </summary>
/// <param name="Save">Fitted to the current content.</param>
internal sealed record LoadRequested(SaveGame Save) : SceneRequest;
