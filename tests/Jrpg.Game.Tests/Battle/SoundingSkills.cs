using Jrpg.Core.Effects;
using Jrpg.Core.Skills;

namespace Jrpg.Game.Tests.Battle;

// A damage skill with a chosen sound file, so a test of an action's sounds doesn't hang on what content's skills name.
internal static class SoundingSkills
{
    public static SkillDefinition WithSound(string soundFile) => new()
    {
        Id = "own",
        Name = "Own",
        Description = "",
        IconFile = "",
        SoundFile = soundFile,
        Effect = new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy),
    };
}
