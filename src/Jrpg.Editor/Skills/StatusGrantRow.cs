using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Skills;

/// <summary>
/// One status a skill's effect grants, as the Skills page types it (decision 981): the status, its turns and its chance
/// in percent. <see cref="Problem"/> is only what keeps the grant from being built; turns below 1, a chance outside
/// 1..100 or a status granted twice is Data's rule, shown by the list.
/// </summary>
internal sealed partial class StatusGrantRow : ObservableObject
{
    // A new row's starting boxes (decision 981): one turn, always landing.
    private const int NewTurns = 1;
    private const int NewChance = 100;

    /// <param name="statusId">
    /// The status's id; a row is added only while the draft has a status, so one is always picked.
    /// </param>
    /// <param name="turnsText">The turns as typed.</param>
    /// <param name="chanceText">The chance in percent as typed.</param>
    public StatusGrantRow(string statusId, string turnsText, string chanceText)
    {
        StatusId = statusId;
        TurnsText = turnsText;
        ChanceText = chanceText;
    }

    /// <summary>A row as Add starts it: the status for one turn, always landing.</summary>
    public static StatusGrantRow New(string firstStatusId) =>
        new(firstStatusId, TypedNumber.Text(NewTurns), TypedNumber.Text(NewChance));

    public static StatusGrantRow From(StatusGrant grant) =>
        new(grant.Status.Id, TypedNumber.Text(grant.Turns), TypedNumber.Text(grant.Chance));

    [ObservableProperty]
    public partial string StatusId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string TurnsText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ChanceText { get; set; }

    /// <summary>Turns or a chance that isn't a whole number; null when the grant can be built.</summary>
    public string? Problem => TypedNumber.ProblemOf("Turns", TurnsText) ?? TypedNumber.ProblemOf("Chance", ChanceText);

    /// <param name="statuses">The draft's statuses by id, holding the picked one.</param>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public StatusGrant ToGrant(IReadOnlyDictionary<string, StatusDefinition> statuses) =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new StatusGrant(statuses[StatusId], TypedNumber.Parse(TurnsText)!.Value)
            {
                Chance = TypedNumber.Parse(ChanceText)!.Value,
            };
}
