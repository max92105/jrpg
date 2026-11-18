using Jrpg.Core.Stats;

namespace Jrpg.Editor.Balance;

/// <summary>One enemy in the balance table, as the battle builds it.</summary>
/// <param name="EnemyName">The enemy's name.</param>
/// <param name="MaxHp">The HP it starts every battle with.</param>
/// <param name="Power">What its tiered skills multiply.</param>
/// <param name="AttackDamage">Its basic attack's raw damage, which its power doesn't raise.</param>
/// <param name="AttackType">What its basic attack hits as (ADR 0027).</param>
internal sealed record EnemyBalanceRow(string EnemyName, int MaxHp, int Power, int AttackDamage, DamageType AttackType);
