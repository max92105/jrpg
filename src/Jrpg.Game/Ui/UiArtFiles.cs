using Jrpg.Data.Art;

namespace Jrpg.Game.Ui;

/// <summary>
/// The images the screens draw that no content file names, so the game and a test name them in one place
/// (decision 84). Grouped by the art folder they're in.
/// </summary>
internal static class UiArtFiles
{
    /// <summary>In <see cref="ArtFolders.Backgrounds"/>.</summary>
    public static class Backgrounds
    {
        public const string Title = "StartMenu.png";

        public const string Menu = "GameMenu.png";
    }

    /// <summary>In <see cref="ArtFolders.Icons"/>: the inventory's category rail (decision 51) and the battle commands.</summary>
    public static class Icons
    {
        public const string AllItems = "All.png";

        public const string Consumables = "Consumable.png";

        public const string Weapons = "Weapon.png";

        public const string Armor = "Armor.png";

        public const string Attack = "Attack.png";

        public const string Skills = "Skill.png";

        public const string Items = "Item.png";

        public const string Defend = "Defend.png";

        public const string Run = "Run.png";

        // Placeholder: no Move icon exists yet, so Move shows the Run icon.
        public const string Move = "Run.png";
    }
}
