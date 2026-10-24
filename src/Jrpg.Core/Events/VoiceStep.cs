namespace Jrpg.Core.Events;

/// <summary>
/// Starts a voice-over and runs on at once, so the stills that follow show while it speaks (ADR 0016, decision 172).
/// Skipping the cutscene runs past it and silences it.
/// </summary>
/// <param name="File">A file in the voices audio folder (ADR 0018).</param>
public sealed record VoiceStep(string File) : EventStep
{
    public override bool IsPresentation => true;
}
