namespace Jrpg.Core.Stats;

/// <summary>Current and maximum HP, never outside 0..max; classes have their own battle resources, not MP.</summary>
public sealed class Vitals
{
    public Vitals(int hp, int maxHp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHp, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(hp);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hp, maxHp);

        Hp = hp;
        MaxHp = maxHp;
    }

    public int Hp { get; private set; }

    public int MaxHp { get; private set; }

    public bool IsAlive => Hp > 0;

    /// <returns>The HP actually lost.</returns>
    public int TakeDamage(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var lost = Math.Min(amount, Hp);
        Hp -= lost;
        return lost;
    }

    /// <returns>The HP actually restored. Always 0 for a fallen member: healing doesn't revive.</returns>
    public int RestoreHp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (!IsAlive)
        {
            return 0;
        }

        return AddHp(amount);
    }

    /// <returns>The HP the fallen member comes back with. Always 0 for a living member.</returns>
    public int Revive(int hp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(hp, 1);
        if (IsAlive)
        {
            return 0;
        }

        return AddHp(hp);
    }

    /// <summary>
    /// A level-up: the gain goes to the maximum and to current HP alike, so the member keeps the HP they had lost
    /// (ADR 0017). A fallen member stays at 0 HP: levelling up doesn't revive.
    /// </summary>
    internal void RaiseMaxima(int maxHpGain)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxHpGain);

        MaxHp += maxHpGain;
        if (IsAlive)
        {
            Hp += maxHpGain;
        }
    }

    /// <summary>
    /// A new maximum, as when a restored level or worn gear changes it: HP stays as it was, within the new maximum.
    /// The maximum is at least 1, so a living holder keeps 1 HP or more: a lost bonus never fells (decision 328).
    /// </summary>
    internal void ResizeMax(int maxHp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHp, 1);

        MaxHp = maxHp;
        Hp = Math.Min(Hp, maxHp);
    }

    private int AddHp(int amount)
    {
        var added = Math.Min(amount, MaxHp - Hp);
        Hp += added;
        return added;
    }
}
