namespace Jrpg.Core.Events;

/// <summary>One thing a map event does. Run by <c>EventRun</c> in order (ADR 0006).</summary>
public abstract record EventStep
{
    // Only Core defines steps, so EventRun handles every one there is.
    private protected EventStep()
    {
    }
}
