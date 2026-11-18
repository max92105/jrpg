using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Data.Party;
using Jrpg.Data.Sprites;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in the classes (decision 944), each checked with the skills it names and the experience
/// table its skills' levels must fit (decision 965), in the party members (decision 952), each checked with the class
/// it names, and in the new game (decision 969), checked with the members, items, maps and field sprites it names.
/// </summary>
public static partial class CatalogProblems
{
    /// <summary>
    /// The first problem in the classes (decision 944): a class's own rule, or a skill it names that the content lacks,
    /// that uses a resource the class doesn't have, that needs more of one than its max or that is learned past the max
    /// level; null when the loader would take every class as edited.
    /// </summary>
    /// <param name="classes">The classes as edited.</param>
    /// <param name="content">The skills the classes name, and the experience table.</param>
    public static string? OfClasses(IReadOnlyList<ClassDefinition> classes, ContentCatalog content) =>
        FirstProblem(() =>
        {
            CheckClasses(classes, content.Skills);
            CheckClassSkillLevels(classes, content.Rules.Experience);
        });

    /// <summary>
    /// The checks the loader makes on <c>classes.json</c> against these skills, for the problem lines and for
    /// <see cref="CheckSkills"/>, through which the writer checks the classes once per save.
    /// </summary>
    /// <param name="classes">The classes as they will be written.</param>
    /// <param name="skills">The skills by id, as they will be written.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckClasses(
        IReadOnlyList<ClassDefinition> classes, IReadOnlyDictionary<string, SkillDefinition> skills) =>
        ClassCatalogValidator.Validate(ContentCatalogLoader.ClassesFile, classes, new ClassReferences(skills));

    /// <summary>
    /// The loader's check that every class skill is learned within the experience table (decision 965), made once
    /// <c>rules.json</c> is read: for the classes' problem line and <see cref="CheckRules"/>, through which the writer
    /// runs it once the classes' own rules are checked.
    /// </summary>
    /// <param name="classes">The classes as they will be written.</param>
    /// <param name="table">The experience table as it will be written.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckClassSkillLevels(IReadOnlyList<ClassDefinition> classes, ExperienceTable table) =>
        ClassSkillLevelCheck.Check(ContentCatalogLoader.ClassesFile, classes, table);

    /// <summary>
    /// The first problem in the party members (decision 952): a member's own rule, or a class it names that the content
    /// lacks; null when the loader would take every member as edited.
    /// </summary>
    /// <param name="members">The members as edited.</param>
    /// <param name="content">The classes the members name.</param>
    public static string? OfPartyMembers(IReadOnlyList<PartyMemberDefinition> members, ContentCatalog content) =>
        FirstProblem(() => CheckPartyMembers(members, content));

    /// <summary>
    /// The checks the loader makes on <c>party-members.json</c> against the content's classes, for the problem lines
    /// and the writer.
    /// </summary>
    /// <param name="members">The members as they will be written.</param>
    /// <param name="content">The classes the members name, as they will be written.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckPartyMembers(IReadOnlyList<PartyMemberDefinition> members, ContentCatalog content) =>
        PartyMemberCatalogValidator.Validate(
            ContentCatalogLoader.PartyMembersFile, members, new PartyMemberReferences(content.Classes.Keys.ToHashSet()));

    /// <summary>
    /// The first problem in the new game (decision 969): its own rules, a member, item or map it names that the content
    /// lacks, a start outside its map, or a field sprite without a record; null when the loader would take it.
    /// </summary>
    /// <param name="newGame">The new game as edited.</param>
    /// <param name="content">The members, items, maps and field sprites the new game names.</param>
    public static string? OfNewGame(NewGameDefinition newGame, ContentCatalog content) =>
        FirstProblem(() => CheckNewGame(newGame, content));

    /// <summary>
    /// The checks the loader makes on <c>new-game.json</c>, then its field sprite's record as the loader checks it once
    /// the new game is read (the objects' sprites included): for the problem line and the writer's map saves.
    /// </summary>
    /// <param name="newGame">The new game as it will be written.</param>
    /// <param name="content">The members, items, maps, field sprites and objects, as they will be written.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckNewGame(NewGameDefinition newGame, ContentCatalog content)
    {
        var references = new NewGameReferences(content.PartyMembers.Keys.ToHashSet(), content.Items, content.Maps);
        NewGameValidator.Validate(ContentCatalogLoader.NewGameFile, newGame, references);
        FieldSpriteReferenceCheck.Check(
            ContentCatalogLoader.FieldSpritesFile, content.FieldSprites, content.Objects.Values, newGame);
    }

    // By id, as the writer writes them, so the first problem named is the one the loader would name.
    private static List<ClassDefinition> ClassesById(ContentCatalog content) =>
        [.. content.Classes.Values.OrderBy(classDefinition => classDefinition.Id, StringComparer.Ordinal)];
}
