using Jrpg.Core.Battle;

namespace Jrpg.Editor.Skills;

/// <summary>
/// What the Skills page's boxes offer, read once when it opens: no other session can change the folders or the strip
/// records while it is open.
/// </summary>
/// <param name="Icons">
/// The images in the icons folder after an empty entry for no icon, then the icons the skills name that it lacks, as
/// <see cref="SkillFileChoices"/> lists them.
/// </param>
/// <param name="Sounds">
/// The files in the audio effects folder after an empty entry, a new skill's before one is picked, then the sounds the
/// skills name that it lacks.
/// </param>
/// <param name="EffectImages">
/// The images in the effects folder, which a skill's own strip names, after an empty entry for the actor's own attack
/// strip, then the strips the skills name that it lacks.
/// </param>
/// <param name="StripPaces">The draft's effect strip records by file, for the strip's pace line (decision 982).</param>
/// <param name="Resources">
/// What the cost boxes offer (decision 979), as <see cref="ResourceChoice.AllIn"/> lists the draft's resources.
/// </param>
internal sealed record SkillChoices(
    IReadOnlyList<string> Icons,
    IReadOnlyList<string> Sounds,
    IReadOnlyList<string> EffectImages,
    IReadOnlyDictionary<string, EffectStripDefinition> StripPaces,
    IReadOnlyList<ResourceChoice> Resources);
