using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The names enemies are shown with (developer decision 99): enemies sharing a name get letters in formation order,
/// "Goblin A", "Goblin B"; an enemy alone with its name has none. Display only; Core knows them by their place.
/// </summary>
internal static class EnemyLabels
{
    /// <param name="names">The enemies' names, in formation order.</param>
    /// <param name="messages">How a name and its letter are written.</param>
    /// <returns>One label per enemy, in the same order. Built once per battle.</returns>
    public static IReadOnlyList<string> Of(IReadOnlyList<string> names, UiMessages messages)
    {
        var labels = new string[names.Count];
        var repeatedNames = names.CountBy(name => name).Where(entry => entry.Value > 1).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);
        var lettersGiven = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < names.Count; index++)
        {
            var name = names[index];
            if (!repeatedNames.Contains(name))
            {
                labels[index] = name;
                continue;
            }

            var letterIndex = lettersGiven.GetValueOrDefault(name);
            lettersGiven[name] = letterIndex + 1;
            var letter = ((char)('A' + letterIndex)).ToString();
            labels[index] = messages.Format(MessageIds.BattleEnemyLetter, new MessageArgument("name", name), new MessageArgument("letter", letter));
        }

        return labels;
    }
}
