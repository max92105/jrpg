using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>One skill a member learned from a battle's experience, with the level it came at (ADR 0062).</summary>
public sealed record LearnedSkill(int Level, SkillDefinition Skill);
