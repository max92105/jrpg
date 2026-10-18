namespace Jrpg.Game.Ui;

/// <summary>A value for a placeholder: <c>Name</c> "item" fills <c>{item}</c>.</summary>
internal readonly record struct MessageArgument(string Name, string Value);
