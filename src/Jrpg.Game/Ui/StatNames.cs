using System.Collections.Frozen;
using Jrpg.Core.Stats;

namespace Jrpg.Game.Ui;

/// <summary>
/// Each stat's name in ui.json (ADR 0010), shared by the leader choice, the Status page and the Equipment page. A table
/// rather than a switch: an arm per stat would pass the build's complexity limit.
/// </summary>
internal static class StatNames
{
    private static readonly FrozenDictionary<StatKind, string> _ids = new Dictionary<StatKind, string>
    {
        [StatKind.Power] = MessageIds.StatPower,
        [StatKind.Agility] = MessageIds.StatAgility,
        [StatKind.Intelligence] = MessageIds.StatIntelligence,
        [StatKind.Constitution] = MessageIds.StatConstitution,
        [StatKind.Will] = MessageIds.StatWill,
        [StatKind.MaxHp] = MessageIds.StatHp,
        [StatKind.Speed] = MessageIds.StatusSpeed,
        [StatKind.Accuracy] = MessageIds.StatAccuracy,
        [StatKind.CritChance] = MessageIds.StatCritChance,
        [StatKind.CritDamage] = MessageIds.StatCritDamage,
        [StatKind.Dodge] = MessageIds.StatDodge,
        [StatKind.CrowdControlResistance] = MessageIds.StatCrowdControlResistance,
        [StatKind.BluntResistance] = MessageIds.StatBluntResistance,
        [StatKind.PiercingResistance] = MessageIds.StatPiercingResistance,
        [StatKind.SlashingResistance] = MessageIds.StatSlashingResistance,
        [StatKind.FireResistance] = MessageIds.StatFireResistance,
        [StatKind.IceResistance] = MessageIds.StatIceResistance,
        [StatKind.LightningResistance] = MessageIds.StatLightningResistance,
        [StatKind.LightResistance] = MessageIds.StatLightResistance,
        [StatKind.DarkResistance] = MessageIds.StatDarkResistance,
        [StatKind.LifeSteal] = MessageIds.StatLifeSteal,
        [StatKind.ArmorPenetration] = MessageIds.StatArmorPenetration,
        [StatKind.BlockChance] = MessageIds.StatBlockChance,
        [StatKind.DropLuck] = MessageIds.StatDropLuck,
    }.ToFrozenDictionary();

    /// <exception cref="KeyNotFoundException">A stat with no name in the table.</exception>
    public static string IdOf(StatKind kind) => _ids[kind];
}
