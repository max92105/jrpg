namespace Jrpg.Editor.Events;

/// <summary>
/// The ids the event dialog offers: items to give, objects to swap to or wait for, maps to warp to, the members an
/// event can wait to be led by, and the shops an openShop step opens.
/// </summary>
internal sealed record StepChoices(IReadOnlyList<string> ItemIds, IReadOnlyList<string> ObjectIds, IReadOnlyList<string> MapIds, IReadOnlyList<string> MemberIds)
{
    /// <summary>
    /// The shops, sorted (ADR 0031); an init member, so the dialogs built without shops stay as they are.
    /// </summary>
    public IReadOnlyList<string> ShopIds { get; init; } = [];
}
