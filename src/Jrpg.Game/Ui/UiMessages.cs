using System.Text;

namespace Jrpg.Game.Ui;

/// <summary>
/// The game's own text from the message table (ADR 0010). Build formatted text when the state changes, never every
/// frame: formatting allocates.
/// </summary>
/// <param name="messages">Loaded by <c>UiMessageLoader</c>, which guarantees balanced, non-empty placeholders.</param>
internal sealed class UiMessages(IReadOnlyDictionary<string, string> messages)
{
    /// <exception cref="KeyNotFoundException">The table has no such id.</exception>
    public string Text(string id) =>
        messages.TryGetValue(id, out var text) ? text : throw new KeyNotFoundException($"The message table has no '{id}'.");

    /// <summary>The text with each <c>{name}</c> replaced by the argument of that name.</summary>
    /// <exception cref="KeyNotFoundException">The table has no such id.</exception>
    /// <exception cref="ArgumentException">A placeholder in the text has no argument.</exception>
    public string Format(string id, params ReadOnlySpan<MessageArgument> arguments)
    {
        var text = Text(id);
        var result = new StringBuilder(text.Length);
        var position = 0;
        for (var open = text.IndexOf('{'); open >= 0; open = text.IndexOf('{', position))
        {
            var close = text.IndexOf('}', open);
            result.Append(text, position, open - position);
            result.Append(ValueOf(id, text.AsSpan(open + 1, close - open - 1), arguments));
            position = close + 1;
        }

        return result.Append(text, position, text.Length - position).ToString();
    }

    private static string ValueOf(string id, ReadOnlySpan<char> name, ReadOnlySpan<MessageArgument> arguments)
    {
        foreach (var argument in arguments)
        {
            if (name.SequenceEqual(argument.Name))
            {
                return argument.Value;
            }
        }

        throw new ArgumentException($"Message '{id}' needs a value for {{{name}}}.", nameof(arguments));
    }
}
