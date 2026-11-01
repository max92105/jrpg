namespace Jrpg.Game.Audio;

/// <summary>
/// The track the game wants heard now (ADR 0035): the flow writes it, the host plays it after the update.
/// </summary>
internal sealed class MusicChannel
{
    /// <summary>The file in the music folder to hear, or null for silence.</summary>
    public string? Track { get; private set; }

    public void Want(string? file) => Track = file;
}
