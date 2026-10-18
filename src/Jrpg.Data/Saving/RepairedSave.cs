using Jrpg.Core.Saving;

namespace Jrpg.Data.Saving;

/// <summary>
/// A save made to fit the current content, and one line per thing that was dropped or adjusted (decision 40). The
/// changes are empty when the save already fit; otherwise they're for a log or a message to the player (part 3).
/// </summary>
public sealed record RepairedSave(SaveGame Save, IReadOnlyList<string> Changes);
