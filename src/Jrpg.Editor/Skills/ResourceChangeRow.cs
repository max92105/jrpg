using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Skills;

/// <summary>
/// One resource change of a skill as the Skills page types it (decision 979): a negative amount spends, a positive one
/// gains. <see cref="Problem"/> is only what keeps the change from being built; a 0 amount or a resource listed twice
/// is Data's rule, shown by the list.
/// </summary>
internal sealed partial class ResourceChangeRow : ObservableObject
{
    /// <param name="resourceId">The resource's id; empty while none is picked.</param>
    /// <param name="amountText">The amount as typed.</param>
    public ResourceChangeRow(string resourceId, string amountText)
    {
        ResourceId = resourceId;
        AmountText = amountText;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ResourceId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string AmountText { get; set; }

    /// <summary>
    /// A resource not picked, or an amount that isn't a whole number; null when the change can be built.
    /// </summary>
    public string? Problem =>
        ResourceId.Length == 0 ? "Resource must be picked." : TypedNumber.ProblemOf("Amount", AmountText);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ResourceChange ToChange() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new ResourceChange(ResourceId, TypedNumber.Parse(AmountText)!.Value);
}
