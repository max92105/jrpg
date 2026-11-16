using Jrpg.Core.Battle;

namespace Jrpg.Editor.Skills;

/// <summary>
/// What the Skills page's boxes offer, read once when it opens: no other session can change the folders or the strip
/// records while it is open.
/// </summary>
/// <param name="Icons">The images in the icons folder.</param>
/// <param name="Sounds">The files in the audio effects folder.</param>
/// <param name="EffectImages">The images in the effects folder, which a skill's own strip names.</param>
/// <param name="StripPaces">The draft's effect strip records by file, for the strip's pace line (decision 982).</param>
internal sealed record SkillChoices(
    IReadOnlyList<string> Icons,
    IReadOnlyList<string> Sounds,
    IReadOnlyList<string> EffectImages,
    IReadOnlyDictionary<string, EffectStripDefinition> StripPaces);
