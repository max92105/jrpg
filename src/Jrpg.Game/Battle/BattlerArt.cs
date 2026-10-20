using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>The files that show one combatant in battle; the one place a combatant is mapped to its art.</summary>
/// <param name="SpriteFile">In the battlers folder.</param>
/// <param name="IsFlipped">Whether the sprite is drawn mirrored, for art that faces the wrong side.</param>
/// <param name="PortraitFile">In the portraits folder.</param>
/// <param name="AttackEffectFile">The effect an attack by this combatant plays.</param>
internal sealed record BattlerArt(string SpriteFile, bool IsFlipped, string PortraitFile, string AttackEffectFile)
{
    /// <exception cref="ArgumentException">The combatant is neither a party member nor an enemy.</exception>
    public static BattlerArt Of(Combatant combatant) => combatant switch
    {
        { Member: { } member } => new(member.Definition.BattleSpriteFile, false, member.Definition.PortraitFile, member.Definition.AttackEffectFile),
        { Enemy: { } enemy } => new(enemy.BattleSpriteFile, enemy.IsSpriteFlipped, enemy.PortraitFile, enemy.AttackEffectFile),
        _ => throw new ArgumentException($"{combatant.Name} is neither a party member nor an enemy.", nameof(combatant)),
    };
}
