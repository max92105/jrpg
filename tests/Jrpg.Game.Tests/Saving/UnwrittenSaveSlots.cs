using Jrpg.Data.Saving;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Saving;

/// <summary>
/// Save files for tests that build a menu but never save: a folder that doesn't exist reads as twenty empty files, and
/// nothing creates it.
/// </summary>
internal static class UnwrittenSaveSlots
{
    public static SaveSlots Create()
    {
        var neverCreated = Path.Combine(Path.GetTempPath(), $"jrpg-unwritten-{Guid.NewGuid():N}");
        var catalog = StartMapScenarios.BelowClosedDoor;
        return new SaveSlots(new FileSaveStore(neverCreated, catalog), catalog, () => DateTimeOffset.UnixEpoch);
    }
}
