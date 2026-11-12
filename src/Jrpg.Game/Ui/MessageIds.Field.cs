namespace Jrpg.Game.Ui;

// What the map's events say: found items, recruits, rests, the inn, quest notices.
internal static partial class MessageIds
{
    /// <summary>Placeholder <c>{item}</c>: the item's name. One copy was found.</summary>
    public const string ItemFound = "item_found";

    /// <summary>Placeholders <c>{item}</c> and <c>{count}</c>: several copies were found.</summary>
    public const string ItemFoundMany = "item_found_many";

    /// <summary>
    /// Placeholder <c>{lost}</c>: how many copies didn't fit in their full stack and were left behind.
    /// </summary>
    public const string ItemFoundLost = "item_found_lost";

    /// <summary>Placeholder <c>{name}</c>: a recruit step added that member to the party.</summary>
    public const string MemberJoined = "member_joined";

    /// <summary>A rest step rested the whole party (decision 230).</summary>
    public const string PartyRests = "event_party_rests";

    /// <summary>Placeholder <c>{name}</c>: a recruit step named a member already in the party.</summary>
    public const string MemberAlreadyInParty = "member_already_in_party";

    /// <summary>The inn's question before a priced rest (decision 382); its price is drawn under it.</summary>
    public const string InnQuestion = "inn_question";

    /// <summary>The inn's line when Yes is answered short of gold: the party stays unrested.</summary>
    public const string InnShortOfGold = "inn_short_of_gold";

    /// <summary>
    /// The line a quest step shows when it started a quest (decision 387); placeholder <c>{title}</c>.
    /// </summary>
    public const string QuestStarted = "quest_started";

    /// <summary>The line when it moved a quest to another stage; placeholder <c>{title}</c>.</summary>
    public const string QuestUpdated = "quest_updated";

    /// <summary>The line when it completed a quest; placeholder <c>{title}</c>.</summary>
    public const string QuestCompleted = "quest_completed";
}
