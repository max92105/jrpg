namespace Jrpg.Game.Ui;

/// <summary>The pips from <c>First</c> up to <c>End</c>, excluded, flash white; <c>Share</c> fades 1 to 0.</summary>
internal readonly record struct PipFlash(int First, int End, float Share)
{
    /// <summary>A fresh flash over the pips an amount changed, from the lower amount up to the higher.</summary>
    public static PipFlash Between(int before, int after) => new(Math.Min(before, after), Math.Max(before, after), Share: 1);

    public bool Covers(int index) => Share > 0 && index >= First && index < End;
}
