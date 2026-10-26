namespace Jrpg.Game.Menus;

/// <summary>A line of the Status page's lower sections: a resource and its amount, or a skill and its effect.</summary>
internal sealed record StatusLine(string Name, string Detail);
