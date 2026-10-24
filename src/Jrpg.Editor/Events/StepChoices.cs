namespace Jrpg.Editor.Events;

/// <summary>
/// The ids the event dialog offers: items to give, objects to swap to or wait for, maps to warp to, and the members an
/// event can wait to be led by.
/// </summary>
internal sealed record StepChoices(IReadOnlyList<string> ItemIds, IReadOnlyList<string> ObjectIds, IReadOnlyList<string> MapIds, IReadOnlyList<string> MemberIds);
