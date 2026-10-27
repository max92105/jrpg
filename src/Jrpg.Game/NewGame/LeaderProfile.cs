using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.NewGame;

/// <summary>
/// What the choice shows of one character: who they are, with the stats they start with (decisions 158, 168). Built
/// once when the screen opens, so moving the cursor allocates nothing.
/// </summary>
internal sealed record LeaderProfile(string Name, string PortraitFile, string ClassName, string ClassDescription, string Summary, IReadOnlyList<LeaderStatLine> Stats)
{
    // After the level: HP, speed and the five primaries (the plan's L1), what tells one class from another at level 1.
    private static readonly StatKind[] _shownKinds =
        [StatKind.MaxHp, StatKind.Speed, StatKind.Power, StatKind.Agility, StatKind.Intelligence, StatKind.Constitution, StatKind.Will];

    /// <param name="choice">A leader choice, naming a member of <paramref name="catalog"/>.</param>
    /// <param name="catalog">The member, their class and the stat rules.</param>
    /// <param name="messages">The stat labels.</param>
    public static LeaderProfile Of(LeaderChoice choice, ContentCatalog catalog, UiMessages messages)
    {
        var member = catalog.PartyMembers[choice.MemberId];
        var characterClass = catalog.ClassOf(member);

        // Everyone starts at level 1 wearing nothing (decision 168), so the numbers are the class's base.
        var sheet = MemberSheet.Of(ClassGrowth.StatsAt(characterClass, 1), StatBonuses.None, catalog.Rules.Stats);
        LeaderStatLine[] stats =
        [
            new(messages.Text(MessageIds.StatusLevel), NumberText.Of(1)),
            .. _shownKinds.Select(kind => new LeaderStatLine(messages.Text(StatNames.IdOf(kind)), StatText.ValueOf(kind, sheet.ValueOf(kind), messages))),
        ];
        return new LeaderProfile(member.Name, member.PortraitFile, characterClass.Name, characterClass.Description, member.Summary, stats);
    }
}
