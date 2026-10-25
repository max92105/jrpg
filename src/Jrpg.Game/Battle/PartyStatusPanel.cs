using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One party member's panel: the name, the HP row, the class resources under it, the status tags, and the
/// portrait the view draws them with.
/// </summary>
internal sealed class PartyStatusPanel
{
    private readonly VitalsText _vitals;

    /// <param name="combatant">The member it shows.</param>
    /// <param name="name">The member's name, measured in the name font.</param>
    /// <param name="rows">The member's HP and resource rows.</param>
    /// <param name="messages">How "/max" is written.</param>
    public PartyStatusPanel(Combatant combatant, MeasuredText name, PartyStatusRows rows, UiMessages messages)
    {
        Combatant = combatant;
        Name = name;
        (HpRow, Resources) = (rows.Hp, rows.Resources);
        PortraitFile = BattlerArt.Of(combatant).PortraitFile;
        _vitals = new VitalsText(messages);
        StatusTags = new StatusTagsText(combatant.Statuses, messages);
    }

    public Combatant Combatant { get; }

    public MeasuredText Name { get; }

    public BattleVitalRow HpRow { get; }

    public ResourceGaugeView Resources { get; }

    public string PortraitFile { get; }

    /// <summary>The member's statuses as a line of tags, refreshed with the rest.</summary>
    public StatusTagsText StatusTags { get; }

    public void Refresh()
    {
        var vitals = Combatant.Vitals;
        _vitals.Refresh(vitals);
        HpRow.Show(_vitals.Hp, _vitals.MaxHp, vitals.Hp, vitals.MaxHp);
        StatusTags.Refresh();
        Resources.Refresh();
    }
}
