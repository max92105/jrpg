using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Skills;

/// <summary>
/// One requirement of a skill as the Skills page types it (decision 979): a minimum of a resource its user must hold,
/// never spent. <see cref="Problem"/> is only what keeps the requirement from being built; a minimum below 1 is Data's
/// rule, shown by the list.
/// </summary>
internal sealed partial class ResourceNeedRow : ObservableObject
{
    /// <param name="resourceId">The resource's id; empty while none is picked.</param>
    /// <param name="atLeastText">The minimum as typed.</param>
    public ResourceNeedRow(string resourceId, string atLeastText)
    {
        ResourceId = resourceId;
        AtLeastText = atLeastText;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ResourceId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string AtLeastText { get; set; }

    /// <summary>
    /// A resource not picked, or a minimum that isn't a whole number; null when the requirement can be built.
    /// </summary>
    public string? Problem =>
        ResourceId.Length == 0 ? "Resource must be picked." : TypedNumber.ProblemOf("At least", AtLeastText);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ResourceRequirement ToRequirement() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new ResourceRequirement(ResourceId, TypedNumber.Parse(AtLeastText)!.Value);
}
