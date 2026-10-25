using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant's HP as the battle screen writes it: each value large, its maximum small after it as "/max".
/// A string is formatted again only when its number changed, so refreshing every update allocates nothing.
/// </summary>
internal sealed class VitalsText(UiMessages messages)
{
    private int? _hp;
    private int? _maxHp;

    public string Hp { get; private set; } = string.Empty;

    public string MaxHp { get; private set; } = string.Empty;

    public void Refresh(Vitals vitals)
    {
        Hp = NumberIfChanged(ref _hp, vitals.Hp, Hp);
        MaxHp = MaxIfChanged(ref _maxHp, vitals.MaxHp, MaxHp);
    }

    private static string NumberIfChanged(ref int? shown, int value, string current)
    {
        if (shown == value)
        {
            return current;
        }

        shown = value;
        return NumberText.Of(value);
    }

    private string MaxIfChanged(ref int? shown, int value, string current)
    {
        if (shown == value)
        {
            return current;
        }

        shown = value;
        return messages.Format(MessageIds.BattleVitalMax, MessageArgument.Number("max", value));
    }
}
