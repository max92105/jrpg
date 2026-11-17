using CommunityToolkit.Mvvm.ComponentModel;

namespace Jrpg.Editor.Party;

/// <summary>
/// One optional rule of a resource as the Resources tab types it (decision 951): a box that turns it on, and the rule's
/// own boxes, read only while it is on, so an off rule writes nothing whatever they hold. Each box keeps its text as
/// typed; Data says whether a number is within its range.
/// </summary>
/// <typeparam name="TRule">The rule's record, null on a resource without it.</typeparam>
internal abstract partial class ResourceRuleBoxes<TRule> : ObservableObject
    where TRule : class
{
    /// <summary>
    /// A typed field, the on box included: what the resource listens to, never the problem's notice, so the class's
    /// list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial bool IsOn { get; set; }

    /// <summary>The first of the rule's boxes that isn't a whole number, while it is on; null otherwise.</summary>
    public string? Problem => IsOn ? BoxesProblem : null;

    /// <summary>The first of the rule's boxes that isn't a whole number, on or off.</summary>
    protected abstract string? BoxesProblem { get; }

    /// <returns>The rule as typed, or null while it is off.</returns>
    /// <exception cref="InvalidOperationException">The rule is on and a box isn't a whole number.</exception>
    public TRule? ToRule() =>
        !IsOn ? null
        : BoxesProblem is { } problem ? throw new InvalidOperationException(problem)
        : RuleAsTyped();

    /// <summary>The rule from the boxes; called only while every box reads.</summary>
    protected abstract TRule RuleAsTyped();

    /// <summary>
    /// Raises <see cref="Changed"/>: called by each typed field's change hook, the subclasses' included.
    /// </summary>
    protected void OnTyped() => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIsOnChanged(bool value) => OnTyped();
}
