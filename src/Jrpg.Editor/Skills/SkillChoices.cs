using Jrpg.Core.Battle;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Effects;

namespace Jrpg.Editor.Skills;

/// <summary>
/// What the Skills page's boxes offer, read once when it opens: no other session can change the folders, the strip
/// records, the tiers or the statuses while it is open.
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
/// <param name="Tiers">What the Tier box offers, as <see cref="TierChoice.AllIn"/> lists the draft's records.</param>
/// <param name="Statuses">
/// What the grant boxes offer (decision 981), as <see cref="StatusChoice.AllIn"/> lists the draft's statuses.
/// </param>
/// <param name="StatusRecords">The draft's statuses by id, which a grant row's pick resolves to.</param>
internal sealed record SkillChoices(
    IReadOnlyList<string> Icons,
    IReadOnlyList<string> Sounds,
    IReadOnlyList<string> EffectImages,
    IReadOnlyDictionary<string, EffectStripDefinition> StripPaces,
    IReadOnlyList<ResourceChoice> Resources,
    IReadOnlyList<TierChoice> Tiers,
    IReadOnlyList<StatusChoice> Statuses,
    IReadOnlyDictionary<string, StatusDefinition> StatusRecords);
