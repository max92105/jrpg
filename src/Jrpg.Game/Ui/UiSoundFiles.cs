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

    /// <summary>In <see cref="AudioFolders.Effects"/>: the interface's sounds (decision 421).</summary>
    public static class Effects
    {
        public const string MenuMove = "PlaceholderMenuMove.wav";

        public const string MenuConfirm = "PlaceholderMenuConfirm.wav";

        public const string MenuCancel = "PlaceholderMenuCancel.wav";

        public const string TextTyping = "PlaceholderTextTyping.wav";
    }
}
