using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Data.Party;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in the classes (decision 944), each checked with the skills it names and the experience
/// table its skills' levels must fit (decision 965).
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
    /// <c>rules.json</c> is read: for the problem lines and the writer, which checks the classes' own rules first.
    /// </summary>
    /// <param name="classes">The classes as they will be written.</param>
    /// <param name="table">The experience table as it will be written.</param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckClassSkillLevels(IReadOnlyList<ClassDefinition> classes, ExperienceTable table) =>
        ClassSkillLevelCheck.Check(ContentCatalogLoader.ClassesFile, classes, table);
}
