using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>One party member's panel: the name, the HP and MP rows and the portrait the view draws them with.</summary>
internal sealed class PartyStatusPanel
{
    private readonly VitalsText _vitals;

    /// <param name="combatant">The member it shows.</param>
    /// <param name="name">The member's name, measured in the name font.</param>
    /// <param name="rows">The member's HP and MP rows.</param>
    /// <param name="messages">How "/max" is written.</param>
    public PartyStatusPanel(Combatant combatant, MeasuredText name, (BattleVitalRow Hp, BattleVitalRow Mp) rows, UiMessages messages)
    {
        Combatant = combatant;
        Name = name;
        (HpRow, MpRow) = rows;
        PortraitFile = BattlerArt.Of(combatant).PortraitFile;
        _vitals = new VitalsText(messages);
    }

    public Combatant Combatant { get; }

    public MeasuredText Name { get; }

    public BattleVitalRow HpRow { get; }

    public BattleVitalRow MpRow { get; }

    public string PortraitFile { get; }

    public void Refresh()
    {
        var vitals = Combatant.Vitals;
        _vitals.Refresh(vitals);
        HpRow.Show(_vitals.Hp, _vitals.MaxHp, vitals.Hp, vitals.MaxHp);
        MpRow.Show(_vitals.Mp, _vitals.MaxMp, vitals.Mp, vitals.MaxMp);
    }
}
