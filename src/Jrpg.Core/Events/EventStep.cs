namespace Jrpg.Core.Events;

/// <summary>One thing a map event does. Run by <c>EventRun</c> in order (ADR 0006).</summary>
public abstract record EventStep
{
    // Only Core defines steps, so EventRun handles every one there is.
    private protected EventStep()
    {
    }

    /// <summary>
    /// Whether the step only shows something, such as a cutscene still, so skipping a cutscene runs past it; steps that
    /// change the game or need the player to read them are never skipped (ADR 0016).
    /// </summary>
    public virtual bool IsPresentation => false;
}
