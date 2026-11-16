namespace Jrpg.Editor.Loot;

/// <summary>
/// One item the Drop tables page's entry box offers: every item, priced or not, shown by name with the id that tells
/// two items named alike apart.
/// </summary>
internal sealed record DropItemChoice(string Id, string Name)
{
    public string Label => $"{Name} ({Id})";
}
