namespace Jrpg.Data.Audio;

/// <summary>Where sound lives: <c>{content}/audio/</c>, one folder per kind (ADR 0018).</summary>
public static class AudioFolders
{
    /// <summary>Voice-overs, started by voice steps.</summary>
    public const string Voices = "voices";

    public static string RootIn(string contentRoot) => Path.Combine(contentRoot, "audio");
}
