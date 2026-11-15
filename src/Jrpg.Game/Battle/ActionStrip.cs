using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// Which effects strip an action plays (decision 678): the one its skill or item names for itself, else its actor's
/// attack strip; and which swing goes with it (decision 708). The wind-up and the playback both read it, so the lead
/// before the impact and the frames after it are one strip.
/// </summary>
internal static class ActionStrip
{
    /// <summary>The strip its skill or item names for itself, else its actor's attack strip.</summary>
    /// <param name="actor">Who acts.</param>
    /// <param name="command">What they do.</param>
    /// <returns>The strip's file in the effects folder.</returns>
    /// <exception cref="ArgumentException">The actor is neither a party member nor an enemy.</exception>
    public static string FileOf(Combatant actor, BattleCommand command) =>
        OwnFileOf(command) ?? BattlerArt.Of(actor).AttackEffectFile;

    /// <summary>
    /// The swing swept in front of the actor first: the strip's record names it, for an Attack or an action on its own
    /// strip. A skill or item on its actor's strip plays none, as the swing there is the Attack's (decision 657).
    /// </summary>
    /// <param name="actor">Who acts.</param>
    /// <param name="command">What they do.</param>
    /// <param name="strips">The effect strips' records by file (ADR 0048).</param>
    public static string? SwingFileOf(
        Combatant actor, BattleCommand command, IReadOnlyDictionary<string, EffectStripDefinition> strips) =>
        command is AttackCommand || OwnFileOf(command) is not null
            ? EffectStripDefinition.Of(FileOf(actor, command), strips).SwingFile
            : null;

    private static string? OwnFileOf(BattleCommand command) => command switch
    {
        SkillCommand skill => skill.Skill.EffectFile,
        ItemCommand item => item.Item.EffectFile,
        _ => null,
    };
}
