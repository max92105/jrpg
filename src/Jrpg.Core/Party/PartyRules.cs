namespace Jrpg.Core.Party;

/// <summary>Design limits of the party.</summary>
public sealed record PartyRules(int MaxActiveMembers)
{
    /// <summary>
    /// The official party size (decision 149): the battle screens are laid out for this many active members, so content
    /// may set <see cref="MaxActiveMembers"/> up to it, never past.
    /// </summary>
    public const int MostActiveMembers = 5;
}
