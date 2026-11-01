using Jrpg.Data.Audio;

namespace Jrpg.Game.Ui;

/// <summary>
/// The sound files the code plays that no content file names, so the game and a test name them in one place (decision
/// 425). Grouped by the audio folder they're in.
/// </summary>
internal static class UiSoundFiles
{
    /// <summary>In <see cref="AudioFolders.Music"/>.</summary>
    public static class Music
    {
        public const string Title = "PlaceholderTitle.wav";

        public const string Battle = "PlaceholderBattle.wav";
    }
}
