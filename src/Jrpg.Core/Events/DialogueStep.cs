using Jrpg.Core.Dialogue;

namespace Jrpg.Core.Events;

/// <summary>Shows a speech and waits until the player has read it.</summary>
public sealed record DialogueStep(Speech Speech) : EventStep;
