namespace Jrpg.Core.Effects;

public enum Targeting
{
    SingleEnemy,
    AllEnemies,
    SingleAlly,
    Self,
    AllAllies,
    SingleFallenAlly,
    AllFallenAllies,

    /// <summary>
    /// The actor and every living ally within the effect's range along the battle formation (ADR 0023): the paladin's
    /// aura. Battle only, so no item uses it.
    /// </summary>
    AlliesInRange,

    /// <summary>Any one living combatant on either side but the actor: whom a Ready places its turn after.</summary>
    OtherCombatant,
}
