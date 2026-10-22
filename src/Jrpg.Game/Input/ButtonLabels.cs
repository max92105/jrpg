using System.Collections.Frozen;

namespace Jrpg.Game.Input;

/// <summary>
/// The name of the key or controller button that presses a game button, such as "E" or "A": the first binding for that
/// device in <see cref="InputBindings"/> (question 147 placeholder). Built once, so asking every frame allocates nothing.
/// </summary>
internal static class ButtonLabels
{
    private static readonly FrozenDictionary<(GameButton Button, InputDevice Device), string> _labels = BuildLabels();

    public static string Of(GameButton button, InputDevice device) =>
        _labels.TryGetValue((button, device), out var label)
            ? label
            : throw new InvalidOperationException($"{button} has no binding on the {device}.");

    private static FrozenDictionary<(GameButton Button, InputDevice Device), string> BuildLabels()
    {
        var labels = new Dictionary<(GameButton Button, InputDevice Device), string>();
        foreach (var (key, button) in InputBindings.KeyBindings)
        {
            labels.TryAdd((button, InputDevice.Keyboard), key.ToString());
        }

        foreach (var (padButton, button) in InputBindings.PadBindings)
        {
            labels.TryAdd((button, InputDevice.Controller), padButton.ToString());
        }

        return labels.ToFrozenDictionary();
    }
}
