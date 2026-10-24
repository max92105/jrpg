using System.Diagnostics;

namespace Jrpg.Game.Hosting;

/// <summary>Problems only a developer can act on, written where they look: the console and the debug output.</summary>
internal static class DeveloperLog
{
    public static void Report(string message)
    {
        Console.Error.WriteLine(message);
        Debug.WriteLine(message);
    }
}
