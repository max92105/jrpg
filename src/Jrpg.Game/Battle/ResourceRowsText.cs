using Jrpg.Core.Resources;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant's resource amounts as the battle panel writes them, in the order their class lists them: each amount
/// large, its maximum small as "/max". An amount is formatted again only when it changed, so refreshing every update
/// allocates nothing; maxima are fixed for the battle and formatted once.
/// </summary>
internal sealed class ResourceRowsText
{
    private readonly ResourceMeters _meters;
    private readonly int?[] _shownAmounts;
    private readonly string[] _amounts;
    private readonly string[] _maxima;

    /// <param name="meters">The combatant's live amounts, read on every refresh (ADR 0013).</param>
    /// <param name="messages">How "/max" is written.</param>
    public ResourceRowsText(ResourceMeters meters, UiMessages messages)
    {
        _meters = meters;
        var count = meters.Definitions.Count;
        _shownAmounts = new int?[count];
        _amounts = new string[count];
        Array.Fill(_amounts, string.Empty);
        _maxima = [.. meters.Definitions.Select(definition => messages.Format(MessageIds.BattleVitalMax, MessageArgument.Number("max", definition.Max)))];
    }

    public int Count => _amounts.Length;

    public string AmountAt(int index) => _amounts[index];

    public string MaxAt(int index) => _maxima[index];

    public void Refresh()
    {
        for (var index = 0; index < _amounts.Length; index++)
        {
            var amount = _meters.AmountOf(_meters.Definitions[index].Id);
            if (_shownAmounts[index] != amount)
            {
                _shownAmounts[index] = amount;
                _amounts[index] = NumberText.Of(amount);
            }
        }
    }
}
