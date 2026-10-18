namespace Jrpg.Game.Input;

/// <summary>Which game buttons are down, as bits, so reading input every update allocates nothing.</summary>
internal readonly record struct ButtonSet
{
    private readonly uint _bits;

    private ButtonSet(uint bits)
    {
        _bits = bits;
    }

    public static ButtonSet Empty => default;

    public bool Contains(GameButton button) => (_bits & BitOf(button)) != 0;

    public ButtonSet With(GameButton button) => new(_bits | BitOf(button));

    /// <summary>The buttons in this set that <paramref name="other"/> doesn't have.</summary>
    public ButtonSet Except(ButtonSet other) => new(_bits & ~other._bits);

    private static uint BitOf(GameButton button) => 1u << (int)button;
}
