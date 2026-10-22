namespace Jrpg.Game.Menus;

/// <summary>What the game menu draws around its pages: the header bar and the party sidebar.</summary>
internal sealed record MenuFrame(MenuHeader Header, PartySidebar Sidebar);
