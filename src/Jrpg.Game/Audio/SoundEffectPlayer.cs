using Jrpg.Data.Audio;

namespace Jrpg.Game.Audio;

/// <summary>
/// Plays on the audio device each file of the <see cref="EffectChannel"/> once, fire and forget (ADR 0035).
/// </summary>
internal sealed class SoundEffectPlayer(SoundLibrary sounds)
{
    /// <summary>A silent gain plays nothing; an empty file name, a test catalog's, is skipped.</summary>
    public void Play(EffectChannel effects, float gain)
    {
        if (gain <= 0)
        {
            return;
        }

        foreach (var file in effects.Played)
        {
            if (file.Length > 0)
            {
                // False when too many sounds play at once; losing one effect then is the lesser evil.
                sounds.Get(AudioFolders.Effects, file)?.Play(gain, pitch: 0, pan: 0);
            }
        }
    }
}
