using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The words the plaque shows for an action from its wind-up's start (decision 646): "Attack", the skill's or item's
/// name, "{name} defends.", "Move"; nothing for Run.
/// </summary>
internal static class ActionBannerText
{
    /// <exception cref="InvalidOperationException">
    /// The command is of a kind the battle screen can't show yet.
    /// </exception>
    public static string? For(Combatant actor, BattleCommand command, UiMessages messages) => command switch
    {
        AttackCommand => messages.Text(MessageIds.BattleAttack),
        SkillCommand skill => skill.Skill.Name,
        ItemCommand item => item.Item.Name,
        DefendCommand => messages.Format(MessageIds.BattleDefends, new MessageArgument("name", actor.Name)),
        MoveCommand => messages.Text(MessageIds.BattleMove),
        FleeCommand => null,
        _ => throw new InvalidOperationException($"The battle screen can't show a {command.GetType().Name} yet."),
    };
}
