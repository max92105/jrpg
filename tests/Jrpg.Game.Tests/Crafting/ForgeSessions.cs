using Jrpg.Core.Session;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Crafting;

internal static class ForgeSessions
{
    /// <summary>
    /// A new game with its loose starting pieces removed: those are plain, equal by value to a plain piece a test adds,
    /// so a test that tells its pieces apart starts without them.
    /// </summary>
    public static GameSession WithoutLoosePieces()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        foreach (var piece in session.Inventory.Gear.ToList())
        {
            Assert.True(session.Inventory.TryRemoveGear(piece));
        }

        return session;
    }
}
