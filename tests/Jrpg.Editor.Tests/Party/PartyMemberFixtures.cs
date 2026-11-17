using Jrpg.Core.Content;
using Jrpg.Core.Party;

namespace Jrpg.Editor.Tests.Party;

/// <summary>
/// The Party members page tests' own member, so no repository member is pinned: a scholar whose three art names differ,
/// so a box that drops or swaps one shows in a record compared whole; and the draft holding it.
/// </summary>
internal static class PartyMemberFixtures
{
    /// <summary>A member of <see cref="ClassFixtures.Scholar"/>'s class.</summary>
    public static PartyMemberDefinition Scholar { get; } = new()
    {
        Id = "ilsa",
        Name = "Ilsa",
        ClassId = ClassFixtures.Scholar.Id,
        Summary = "A reader who left the library to see what the books got wrong.",
        PortraitFile = "IlsaPortrait.png",
        BattleSpriteFile = "IlsaBattler.png",
        AttackEffectFile = "InkSplash.png",
    };

    /// <summary>The class fixtures' draft with the scholar as its only member.</summary>
    public static ContentCatalog World { get; } = ClassFixtures.World with
    {
        PartyMembers = new Dictionary<string, PartyMemberDefinition> { [Scholar.Id] = Scholar },
    };
}
