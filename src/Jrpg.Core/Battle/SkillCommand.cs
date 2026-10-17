using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>Use <c>Skill</c> on <c>Targets</c>: one target for a single-target skill, every valid target for an "all" skill.</summary>
public sealed record SkillCommand(SkillDefinition Skill, IReadOnlyList<Combatant> Targets) : BattleCommand;
