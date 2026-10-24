namespace Jrpg.Game.Audio;

/// <summary>
/// The voice-over the game wants heard now (ADR 0018): scenes write it while they update, and the host's
/// <see cref="VoicePlayer"/> makes the sound match it afterwards, so no scene touches the audio device.
/// </summary>
internal sealed class VoiceChannel
{
    /// <summary>The file in the voices folder to hear, or null for silence.</summary>
    public string? VoiceFile { get; private set; }

    public void Play(string voiceFile) => VoiceFile = voiceFile;

    public void Stop() => VoiceFile = null;
}
