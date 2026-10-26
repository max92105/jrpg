using System.Globalization;

namespace Jrpg.Game.Hosting;

/// <summary>Names the debug captures of the virtual screen, so a new one never overwrites an older one.</summary>
/// <param name="directory">The folder the captures go in.</param>
internal sealed class CaptureFiles(string directory)
{
    /// <summary>The folder the captures go in.</summary>
    public string Folder { get; } = directory;

    /// <summary>
    /// <c>capture-yyyyMMdd-HHmmss-n.png</c>, with <c>n</c> the first number from 1 no file in the folder has taken: two
    /// captures within a second keep both.
    /// </summary>
    /// <param name="stamp">The host's wall clock, used only for the file name.</param>
    public string NextPath(DateTime stamp)
    {
        var prefix = $"capture-{stamp.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)}-";
        var number = 1;
        while (File.Exists(PathOf(prefix, number)))
        {
            number++;
        }

        return PathOf(prefix, number);
    }

    private string PathOf(string prefix, int number) =>
        Path.Combine(Folder, $"{prefix}{number.ToString(CultureInfo.InvariantCulture)}.png");
}
