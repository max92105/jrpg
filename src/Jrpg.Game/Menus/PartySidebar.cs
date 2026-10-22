using System.Globalization;
using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The texts of the menu's party sidebar: one card per active member, in party order. Refreshed every update a page is
/// open, it formats again only the cards whose shown values changed, so a page's own changes (a potion, a new order)
/// show at once without formatting every frame.
/// </summary>
internal sealed class PartySidebar
{
    private readonly PartyState _party;
    private readonly UiMessages _messages;
    private readonly string _hpLabel;
    private readonly string _mpLabel;
    private readonly List<SidebarCard> _cards = [];
    private readonly List<ShownValues> _shown = [];

    public PartySidebar(PartyState party, UiMessages messages)
    {
        _party = party;
        _messages = messages;
        _hpLabel = messages.Text(MessageIds.StatHp);
        _mpLabel = messages.Text(MessageIds.StatMp);
        RebuildAll();
    }

    public IReadOnlyList<SidebarCard> Cards => _cards;

    /// <summary>Rebuilds every card if the active members or their order changed, else only the cards whose values did.</summary>
    public void Refresh()
    {
        var active = _party.ActiveMembers;
        if (!IsSameOrder(active))
        {
            RebuildAll();
            return;
        }

        for (var index = 0; index < active.Count; index++)
        {
            var values = ShownValues.Of(active[index]);
            if (values != _shown[index])
            {
                _cards[index] = CardOf(active[index]);
                _shown[index] = values;
            }
        }
    }

    private bool IsSameOrder(IReadOnlyList<PartyMember> active)
    {
        if (active.Count != _cards.Count)
        {
            return false;
        }

        for (var index = 0; index < active.Count; index++)
        {
            if (!ReferenceEquals(active[index], _cards[index].Member))
            {
                return false;
            }
        }

        return true;
    }

    private void RebuildAll()
    {
        _cards.Clear();
        _shown.Clear();
        foreach (var member in _party.ActiveMembers)
        {
            _cards.Add(CardOf(member));
            _shown.Add(ShownValues.Of(member));
        }
    }

    private SidebarCard CardOf(PartyMember member)
    {
        var vitals = member.Vitals;
        return new SidebarCard
        {
            Member = member,
            Name = member.Definition.Name,
            Level = _messages.Format(MessageIds.PartyLevel, new MessageArgument("level", Number(member.Level))),
            Hp = $"{_hpLabel} {ValueOfMax(vitals.Hp, vitals.MaxHp)}",
            Mp = $"{_mpLabel} {ValueOfMax(vitals.Mp, vitals.MaxMp)}",
        };
    }

    private string ValueOfMax(int value, int max) =>
        _messages.Format(MessageIds.StatValueOfMax, new MessageArgument("value", Number(value)), new MessageArgument("max", Number(max)));

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // What a card shows that can change, to tell whether it needs formatting again. A member's max HP and MP are fixed
    // (`Vitals` is built once from the definition), so they need no watching.
    private readonly record struct ShownValues(int Level, int Hp, int Mp)
    {
        public static ShownValues Of(PartyMember member) => new(member.Level, member.Vitals.Hp, member.Vitals.Mp);
    }
}
