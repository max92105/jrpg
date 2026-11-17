namespace Jrpg.Editor.Encounters;

/// <summary>What the Encounters page's boxes offer, read once when it starts.</summary>
/// <param name="Enemies">Every enemy of the catalog by name, then id: the add box's entries, each slot's name read
/// from it.</param>
/// <param name="Backgrounds">The images in the backgrounds folder whose size can be read, for the Background box
/// (decision 1006).</param>
/// <param name="Tracks">The tracks in the music folder, each with its folder, for the Music box.</param>
internal sealed record EncounterChoices(
    IReadOnlyList<EnemyChoice> Enemies,
    IReadOnlyList<string> Backgrounds,
    IReadOnlyList<string> Tracks);
