namespace Jrpg.Game.Ui;

/// <summary>A value for a placeholder: <c>Name</c> "item" fills <c>{item}</c>.</summary>
internal readonly record struct MessageArgument(string Name, string Value)
{
    /// <summary>A placeholder filled with a whole number, written as <see cref="NumberText"/> writes it.</summary>
    public static MessageArgument Number(string name, int value) => new(name, NumberText.Of(value));
}
