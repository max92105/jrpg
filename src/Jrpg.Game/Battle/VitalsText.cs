using System.Globalization;
using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant's HP and MP as the battle screen writes them (legacy <c>CharacterPanel</c>): each value large, its
/// maximum small after it as "/max". A string is formatted again only when its number changed, so refreshing every
/// update allocates nothing.
/// </summary>
internal sealed class VitalsText(UiMessages messages)
{
    private int? _hp;
    private int? _maxHp;
    private int? _mp;
    private int? _maxMp;

    public string Hp { get; private set; } = string.Empty;

    public string MaxHp { get; private set; } = string.Empty;

    public string Mp { get; private set; } = string.Empty;

    public string MaxMp { get; private set; } = string.Empty;

    public void Refresh(Vitals vitals)
    {
        Hp = NumberIfChanged(ref _hp, vitals.Hp, Hp);
        MaxHp = MaxIfChanged(ref _maxHp, vitals.MaxHp, MaxHp);
        Mp = NumberIfChanged(ref _mp, vitals.Mp, Mp);
        MaxMp = MaxIfChanged(ref _maxMp, vitals.MaxMp, MaxMp);
    }

    private static string NumberIfChanged(ref int? shown, int value, string current)
    {
        if (shown == value)
        {
            return current;
        }

        shown = value;
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private string MaxIfChanged(ref int? shown, int value, string current)
    {
        if (shown == value)
        {
            return current;
        }

        shown = value;
        return messages.Format(MessageIds.BattleVitalMax, new MessageArgument("max", value.ToString(CultureInfo.InvariantCulture)));
    }
}
