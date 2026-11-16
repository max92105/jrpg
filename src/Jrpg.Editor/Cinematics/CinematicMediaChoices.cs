namespace Jrpg.Editor.Cinematics;

/// <summary>
/// The pictures, voice lines and tracks the Cinematics page's boxes offer (ADR 0046) and the voice lines' lengths, read
/// through the shell's lookups when the page opens. A name the folders lack is kept and saved (decision 442):
/// <see cref="MissingOf"/> says which, for the page's note; the game's startup refuses it.
/// </summary>
internal sealed class CinematicMediaChoices
{
    /// <param name="pictures">
    /// The PNGs under the cinematics art folder, each with its cinematic's folder, sorted.
    /// </param>
    /// <param name="voices">The voice lines in the voices folder, sorted.</param>
    /// <param name="tracks">The tracks in the music folder, each with its folder, sorted.</param>
    /// <param name="voiceLengthOf">A voice line's playing length by its name, null when it can't be read.</param>
    public CinematicMediaChoices(
        Func<IReadOnlyList<string>> pictures,
        Func<IReadOnlyList<string>> voices,
        Func<IReadOnlyList<string>> tracks,
        Func<string, TimeSpan?> voiceLengthOf)
    {
        Pictures = pictures();
        Voices = voices();
        Tracks = ["", .. tracks()];
        // Only a name the folder holds exactly, as the game's start checks it: Windows would open another case too.
        VoiceLengthOf = voice => Voices.Contains(voice, StringComparer.Ordinal) ? voiceLengthOf(voice) : null;
    }

    public IReadOnlyList<string> Pictures { get; }

    /// <summary>
    /// For each still's timing line beside its Seconds box (decision 620): null for a name the voices folder lacks.
    /// </summary>
    public Func<string, TimeSpan?> VoiceLengthOf { get; }

    public IReadOnlyList<string> Voices { get; }

    /// <summary>"" first, for no track: the cinematic keeps the map's (decision 614).</summary>
    public IReadOnlyList<string> Tracks { get; }

    /// <summary>The pictures, voices and track a cinematic names that the folders lack, in its stills' order.</summary>
    public IReadOnlyList<string> MissingOf(CinematicViewModel cinematic)
    {
        var named = cinematic.Stills
            .SelectMany(row => new[]
            {
                (Name: row.ImageFile.Trim(), Offered: Pictures),
                (Name: row.VoiceFile.Trim(), Offered: Voices),
            })
            .Append((Name: cinematic.MusicFile, Offered: Tracks));
        return [.. named
            .Where(file => file.Name.Length > 0 && !file.Offered.Contains(file.Name, StringComparer.Ordinal))
            .Select(file => file.Name)
            .Distinct(StringComparer.Ordinal)];
    }
}
