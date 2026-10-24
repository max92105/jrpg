using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.NewGame;

/// <summary>
/// What the choice shows of one character: who they are, with the stats they start with (decisions 158, 168). Built
/// once when the screen opens, so moving the cursor allocates nothing.
/// </summary>
internal sealed record LeaderProfile(string Name, string PortraitFile, string ClassName, string ClassDescription, string Summary, IReadOnlyList<LeaderStatLine> Stats)
{
    /// <param name="choice">A leader choice, naming a member of <paramref name="catalog"/>.</param>
    /// <param name="catalog">The member and their class.</param>
    /// <param name="messages">The stat labels.</param>
    public static LeaderProfile Of(LeaderChoice choice, ContentCatalog catalog, UiMessages messages)
    {
        var member = catalog.PartyMembers[choice.MemberId];
        var characterClass = catalog.ClassOf(member);

        // Everyone starts at level 1 (decision 168), so the numbers are the class's base.
        var atLevelOne = ClassGrowth.StatsAt(characterClass, 1);
        LeaderStatLine[] stats =
        [
            new(messages.Text(MessageIds.StatusLevel), NumberText.Of(1)),
            new(messages.Text(MessageIds.StatHp), NumberText.Of(atLevelOne.MaxHp)),
            new(messages.Text(MessageIds.StatMp), NumberText.Of(atLevelOne.MaxMp)),
            new(messages.Text(MessageIds.StatAttack), NumberText.Of(atLevelOne.Attack)),
            new(messages.Text(MessageIds.StatDefense), NumberText.Of(atLevelOne.Defense)),
        ];
        return new LeaderProfile(member.Name, member.PortraitFile, characterClass.Name, characterClass.Description, member.Summary, stats);
    }
}
