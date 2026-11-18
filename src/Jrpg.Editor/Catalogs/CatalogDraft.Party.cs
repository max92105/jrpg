using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The classes, the party members, the new game and the experience table, edited by the Classes, Party members, New
/// game and Experience pages (decisions 945, 954, 968, 955), each session recorded as one undo step.
/// </summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Replaces the classes as one undo step (decision 945); nothing when they are the same by content, skills and
    /// resources in order included. No undo refusal: only party members name a class, and catalog steps undo in order,
    /// so a member naming a class the undo removes came in a later step, already undone.
    /// </summary>
    public void SetClasses(IReadOnlyList<ClassDefinition> classes)
    {
        var before = Current.Classes;
        var after = classes.ToDictionary(classDefinition => classDefinition.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, IsSameClass))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Classes = after }, catalog => catalog with { Classes = before }));
    }

    // As an enemy's: the with-copy compares every field but the two lists, the lists by content and order (the skills'
    // order is the menu's, ADR 0020, and a skill's level alone is a change, ADR 0062).
    private static bool IsSameClass(ClassDefinition before, ClassDefinition after) =>
        before with { Skills = after.Skills, Resources = after.Resources } == after
        && before.Skills.SequenceEqual(after.Skills)
        && before.Resources.SequenceEqual(after.Resources);

    /// <summary>
    /// Replaces the party members as one undo step (decision 954); nothing when they are the same by content, which a
    /// member's record compares whole, holding no list. An undo removing one a map's event recruits or names as its
    /// leader gate is refused; the new game's step naming an added member came later, so it is undone first.
    /// </summary>
    public void SetPartyMembers(IReadOnlyList<PartyMemberDefinition> members)
    {
        var before = Current.PartyMembers;
        var after = members.ToDictionary(member => member.Id, StringComparer.Ordinal);
        if (HasSameRecords(before, after, EqualityComparer<PartyMemberDefinition>.Default.Equals))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { PartyMembers = after }, catalog => catalog with { PartyMembers = before }));
    }

    /// <summary>
    /// The party members the next undo would remove, so it can be refused while a map's event recruits one or is gated
    /// on one leading.
    /// </summary>
    public IReadOnlyList<string> PartyMembersRemovedByUndo() => KeysRemovedByUndo(catalog => catalog.PartyMembers);

    /// <summary>
    /// Replaces the new game as one undo step (decision 968); nothing when it is the same by content, its leader
    /// choices and starting items in order included. No undo refusal (decision 954): the members, items and maps it
    /// names were in the draft before its step, and catalog steps undo in order.
    /// </summary>
    public void SetNewGame(NewGameDefinition newGame)
    {
        var before = Current.NewGame;
        if (IsSameNewGame(before, newGame))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { NewGame = newGame }, catalog => catalog with { NewGame = before }));
    }

    /// <summary>
    /// Replaces the rules' experience table as one undo step (decision 955: the only part of <c>rules.json</c> edited);
    /// nothing when its amounts are the same in order. No undo refusal (decision 943): a save above a lowered max level
    /// is repaired as it loads, and a class skill learned at a level the undo removes came in a later step, already
    /// undone, since catalog steps undo in order.
    /// </summary>
    public void SetExperienceTable(ExperienceTable table)
    {
        var before = Current.Rules.Experience;
        if (before.ExperienceToNextLevel.SequenceEqual(table.ExperienceToNextLevel))
        {
            return;
        }

        Do(new CatalogEdit(
            catalog => catalog with { Rules = catalog.Rules with { Experience = table } },
            catalog => catalog with { Rules = catalog.Rules with { Experience = before } }));
    }

    // As a class's: the with-copy compares every field but the two lists, the lists by content and order: a leader
    // moved changes the leader screen's order.
    private static bool IsSameNewGame(NewGameDefinition before, NewGameDefinition after) =>
        before with { LeaderChoices = after.LeaderChoices, StartingItems = after.StartingItems } == after
        && before.LeaderChoices.SequenceEqual(after.LeaderChoices)
        && before.StartingItems.SequenceEqual(after.StartingItems);
}
