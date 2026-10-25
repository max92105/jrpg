using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One party member's panel: the name, the HP row, one row per class resource under it, and the portrait the view
/// draws them with.
/// </summary>
internal sealed class PartyStatusPanel
{
    private readonly VitalsText _vitals;
    private readonly ResourceRowsText _resources;

    /// <param name="combatant">The member it shows.</param>
    /// <param name="name">The member's name, measured in the name font.</param>
    /// <param name="rows">The member's HP and resource rows.</param>
    /// <param name="messages">How "/max" is written.</param>
    public PartyStatusPanel(Combatant combatant, MeasuredText name, PartyStatusRows rows, UiMessages messages)
    {
        Combatant = combatant;
        Name = name;
        (HpRow, ResourceRows) = (rows.Hp, rows.Resources);
        PortraitFile = BattlerArt.Of(combatant).PortraitFile;
        _vitals = new VitalsText(messages);
        _resources = new ResourceRowsText(combatant.Resources, messages);
    }

    public Combatant Combatant { get; }

    public MeasuredText Name { get; }

    public BattleVitalRow HpRow { get; }

    public IReadOnlyList<BattleVitalRow> ResourceRows { get; }

    public string PortraitFile { get; }

    public void Refresh()
    {
        var vitals = Combatant.Vitals;
        _vitals.Refresh(vitals);
        HpRow.Show(_vitals.Hp, _vitals.MaxHp, vitals.Hp, vitals.MaxHp);
        _resources.Refresh();
        var meters = Combatant.Resources;
        for (var index = 0; index < _resources.Count; index++)
        {
            var definition = meters.Definitions[index];
            ResourceRows[index].Show(_resources.AmountAt(index), _resources.MaxAt(index), meters.AmountOf(definition.Id), definition.Max);
        }
    }
}
