namespace Jrpg.Core.Quests;

/// <summary>What an event waits for of a quest (ADR 0033, decision 389); the two stage kinds name a stage.</summary>
public enum QuestConditionKind
{
    /// <summary>The quest was never started.</summary>
    NotStarted,

    /// <summary>Started and not completed, at any stage.</summary>
    Active,

    /// <summary>Active at the named stage.</summary>
    AtStage,

    /// <summary>At the named stage or past it in the quest's stage order, or completed.</summary>
    ReachedStage,

    Completed,
}
