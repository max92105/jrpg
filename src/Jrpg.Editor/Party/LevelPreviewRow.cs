using Jrpg.Core.Stats;

namespace Jrpg.Editor.Party;

/// <summary>
/// One level's numbers in the Classes page's Levels tab, as a member of the class wearing nothing has them: the
/// experience from it to the next, null at the table's max level, and the accuracy in whole percents, as the game's
/// screens show it.
/// </summary>
internal sealed record LevelPreviewRow(
    int Level,
    int? ExperienceToNext,
    int MaxHp,
    int Speed,
    int AccuracyPercent,
    PrimaryStats Primaries);
