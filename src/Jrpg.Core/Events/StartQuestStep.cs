namespace Jrpg.Core.Events;

/// <summary>Starts a quest at its first stage (ADR 0032); nothing on a quest already started or completed.</summary>
public sealed record StartQuestStep(string QuestId) : EventStep;
