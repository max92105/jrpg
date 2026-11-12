namespace Jrpg.Game.Ui;

/// <summary>
/// The ids of <c>content/text/ui.json</c> the game uses; a test checks each exists there (ADR 0010). Partial by the
/// screen that shows each id (decision 552): a new id goes in the file of its screen (<c>.Field</c> for the map's
/// events, <c>.StatusPage</c>, <c>.Stats</c> for names several screens share, <c>.Menus</c>, <c>.Battle</c>,
/// <c>.Crafting</c>, <c>.Trade</c>), or here for startup, the title, the leader choice, the cutscene skip, the menu's
/// title and its System page, and the dialogs' shared buttons.
/// </summary>
internal static partial class MessageIds
{
    /// <summary>Placeholder <c>{reason}</c>: why content couldn't load.</summary>
    public const string StartupFailed = "startup_failed";

    /// <summary>The game's name, written large on the title.</summary>
    public const string GameTitle = "game_title";

    /// <summary>The title's entry that starts a new game.</summary>
    public const string TitleNewGame = "title_new_game";

    /// <summary>The title's entry that continues from the most recent save.</summary>
    public const string TitleContinue = "title_continue";

    /// <summary>The title's entry that opens the save files to load one; also their heading there.</summary>
    public const string TitleLoad = "title_load";

    /// <summary>The title's entry that closes the game.</summary>
    public const string TitleQuit = "title_quit";

    /// <summary>The line at the bottom of the title screen.</summary>
    public const string TitleFooter = "title_footer";

    /// <summary>The heading of the screen where a new game's leader is chosen.</summary>
    public const string LeaderChoiceTitle = "leader_choice_title";

    /// <summary>Placeholder <c>{name}</c>: asked before a new game starts led by that character.</summary>
    public const string LeaderChoiceConfirm = "leader_choice_confirm";

    /// <summary>The leader choice's footer hint.</summary>
    public const string LeaderChoiceHint = "leader_choice_hint";

    /// <summary>Placeholder <c>{button}</c>: the Confirm button's name; shown while cutscene stills play.</summary>
    public const string CinematicSkip = "cinematic_skip";

    /// <summary>The menu header's title while no page is open.</summary>
    public const string MenuTitle = "menu_title";

    /// <summary>The game menu's System tab.</summary>
    public const string MenuSystem = "menu_system";

    /// <summary>The System page's entry that ends the playthrough and goes back to the title.</summary>
    public const string SystemTitleScreen = "system_title_screen";

    /// <summary>The System page's entry that closes the game.</summary>
    public const string SystemQuit = "system_quit";

    /// <summary>Asked before going back to the title.</summary>
    public const string ConfirmTitleScreen = "confirm_title_screen";

    /// <summary>Asked before closing the game.</summary>
    public const string ConfirmQuit = "confirm_quit";

    /// <summary>A confirmation's button that goes ahead.</summary>
    public const string ConfirmYes = "confirm_yes";

    /// <summary>A confirmation's button that changes nothing.</summary>
    public const string ConfirmNo = "confirm_no";

    /// <summary>A notice dialog's one button, which closes it.</summary>
    public const string NoticeOk = "notice_ok";
}
