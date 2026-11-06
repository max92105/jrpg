namespace Jrpg.Editor.Events;

/// <summary>
/// The ids the event dialog offers: items to give, objects to swap to or wait for, maps to warp to, the members an
/// event can wait to be led by, the shops an openShop step opens, and the quests and stages the quest steps name.
/// </summary>
internal sealed record StepChoices(IReadOnlyList<string> ItemIds, IReadOnlyList<string> ObjectIds, IReadOnlyList<string> MapIds, IReadOnlyList<string> MemberIds)
{
    /// <summary>
    /// The shops, sorted (ADR 0031); an init member, so the dialogs built without shops stay as they are.
    /// </summary>
    public IReadOnlyList<string> ShopIds { get; init; } = [];

    /// <summary>
    /// The quests, sorted, and each quest's stages in its order (ADR 0032); init members as <see cref="ShopIds"/>.
    /// </summary>
    public IReadOnlyList<string> QuestIds { get; init; } = [];

    public IReadOnlyList<QuestStageChoice> QuestStages { get; init; } = [];

    /// <summary>
    /// The maps a warp can lead to, drawn by the destination picker; null where no map can be picked.
    /// </summary>
    public WarpMaps? WarpMaps { get; init; }
}
