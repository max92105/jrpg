namespace Jrpg.Core.Events;

/// <summary>Turns the event's object into another object for good, such as a chest into an opened chest. Doesn't wait.</summary>
public sealed record SwapObjectStep(string ObjectId) : EventStep;
