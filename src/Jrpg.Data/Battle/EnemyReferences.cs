using Jrpg.Core.Skills;

namespace Jrpg.Data.Battle;

/// <summary>What an enemy in <c>enemies.json</c> may name.</summary>
/// <param name="DropTableIds">The drop tables; several enemies may share one (decision 343).</param>
/// <param name="Skills">The skills its actions may use, by id.</param>
internal sealed record EnemyReferences(IReadOnlySet<string> DropTableIds, IReadOnlyDictionary<string, SkillDefinition> Skills);
