namespace Jrpg.Game.Ui;

/// <summary>The ids of <c>content/text/ui.json</c> the game uses; a test checks each exists there (ADR 0010).</summary>
internal static class MessageIds
{
    /// <summary>Placeholder <c>{reason}</c>: why content couldn't load.</summary>
    public const string StartupFailed = "startup_failed";

    /// <summary>Placeholder <c>{item}</c>: the item's name. One copy was found.</summary>
    public const string ItemFound = "item_found";

    /// <summary>Placeholders <c>{item}</c> and <c>{count}</c>: several copies were found.</summary>
    public const string ItemFoundMany = "item_found_many";

    /// <summary>Placeholder <c>{lost}</c>: how many copies didn't fit in their full stack and were left behind.</summary>
    public const string ItemFoundLost = "item_found_lost";

    /// <summary>The title's entry that starts a new game.</summary>
    public const string TitleNewGame = "title_new_game";

    /// <summary>The title's entry that closes the game.</summary>
    public const string TitleQuit = "title_quit";
}
