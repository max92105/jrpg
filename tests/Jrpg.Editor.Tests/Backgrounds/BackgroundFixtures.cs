using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tests.Backgrounds;

/// <summary>
/// The Battle backgrounds page tests' own animated background, the placeholder's shape (decision 910), so no repository
/// record is pinned.
/// </summary>
internal static class BackgroundFixtures
{
    /// <summary>Four 960 x 540 frames at 200 ms, laid 2 x 2 in a <see cref="PulseSheet"/>.</summary>
    public static AnimatedBackgroundDefinition Pulse { get; } = new()
    {
        File = "Pulse.png",
        FrameWidth = 960,
        FrameHeight = 540,
        Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
    };

    /// <summary>The size of a sheet holding <see cref="Pulse"/>'s frames in two columns and two rows.</summary>
    public static (int Width, int Height) PulseSheet { get; } = (1920, 1080);
}
