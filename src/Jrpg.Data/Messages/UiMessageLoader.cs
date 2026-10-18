using System.Collections.Frozen;
using Jrpg.Data.Content;

namespace Jrpg.Data.Messages;

/// <summary>Reads the game's own text: one id -> text table with named placeholders such as <c>{item}</c> (ADR 0010).</summary>
public static class UiMessageLoader
{
    internal const string MessagesFile = "text/ui.json";

    /// <exception cref="ContentValidationException">The file is malformed, or a text is empty or has an unbalanced brace.</exception>
    public static IReadOnlyDictionary<string, string> Load(string contentRoot)
    {
        // Nullable values: STJ doesn't apply nullable annotations to a dictionary's values, so null is checked here.
        var messages = ContentFileReader.Read<Dictionary<string, string?>>(Path.Combine(contentRoot, MessagesFile));
        foreach (var (id, text) in messages)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ContentValidationException($"{MessagesFile}: message '{id}' has no text.");
            }

            if (!HasBalancedPlaceholders(text))
            {
                throw new ContentValidationException($"{MessagesFile}: message '{id}' has an unbalanced brace or an empty placeholder.");
            }
        }

        return messages.ToFrozenDictionary(message => message.Key, message => message.Value!);
    }

    /// <summary>Every <c>{</c> closes with a <c>}</c> around a non-empty name, with no nesting.</summary>
    private static bool HasBalancedPlaceholders(string text)
    {
        var nameStart = -1;
        for (var index = 0; index < text.Length; index++)
        {
            switch (text[index])
            {
                case '{' when nameStart >= 0:
                    return false;
                case '{':
                    nameStart = index + 1;
                    break;
                case '}' when nameStart < 0 || nameStart == index:
                    return false;
                case '}':
                    nameStart = -1;
                    break;
            }
        }

        return nameStart < 0;
    }
}
