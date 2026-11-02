namespace Jrpg.Game.Audio;

/// <summary>
/// What the flow and the scenes write during an update and the host plays after it (ADR 0018, ADR 0035).
/// </summary>
internal sealed class AudioChannels
{
    public VoiceChannel Voice { get; } = new();

    public MusicChannel Music { get; } = new();

    public EffectChannel Effects { get; } = new();
}
