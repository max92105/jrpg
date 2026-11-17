using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Data.Party;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in the classes (decision 944), each checked with the skills it names.
/// </summary>
public static partial class CatalogProblems
{
    /// <summary>
    /// The first problem in the classes (decision 944): a class's own rule, or a skill it names that the content lacks,
    /// that uses a resource the class doesn't have or that needs more of one than its max; null when the loader would
    /// take every class as edited.
    /// </summary>
    /// <param name="classes">The classes as edited.</param>
    /// <param name="content">The skills the classes name.</param>
    public static string? OfClasses(IReadOnlyList<ClassDefinition> classes, ContentCatalog content) =>
        FirstProblem(() => CheckClasses(classes, content.Skills));

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
}
