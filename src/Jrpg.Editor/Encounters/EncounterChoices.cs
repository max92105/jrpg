namespace Jrpg.Editor.Encounters;

/// <summary>What the Encounters page's boxes offer, read once when it starts.</summary>
/// <param name="Enemies">Every enemy of the catalog, each slot's name read from it.</param>
internal sealed record EncounterChoices(IReadOnlyList<EnemyChoice> Enemies);
