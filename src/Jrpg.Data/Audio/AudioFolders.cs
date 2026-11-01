namespace Jrpg.Data.Audio;

/// <summary>Where sound lives: <c>{content}/audio/</c>, one folder per kind (ADR 0018).</summary>
public static class AudioFolders
{
    /// <summary>Voice-overs, started by voice steps.</summary>
    public const string Voices = "voices";

    /// <summary>Music: map tracks, and the themes the code names.</summary>
    public const string Music = "music";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "audio");
}
