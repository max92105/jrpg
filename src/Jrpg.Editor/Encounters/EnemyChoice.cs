namespace Jrpg.Editor.Encounters;

/// <summary>
/// One enemy the Encounters page's add box offers, shown by name with the id that tells two enemies named alike apart.
/// </summary>
internal sealed record EnemyChoice(string Id, string Name)
{
    public string Label => $"{Name} ({Id})";
}
