namespace Jrpg.Core.Stats;

/// <summary>Current and maximum HP and MP. HP and MP never leave 0..max.</summary>
public sealed class Vitals
{
    public Vitals(int hp, int maxHp, int mp, int maxMp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxHp, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(hp);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hp, maxHp);
        ArgumentOutOfRangeException.ThrowIfNegative(maxMp);
        ArgumentOutOfRangeException.ThrowIfNegative(mp);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mp, maxMp);

        Hp = hp;
        MaxHp = maxHp;
        Mp = mp;
        MaxMp = maxMp;
    }

    public int Hp { get; private set; }

    public int MaxHp { get; private set; }

    public int Mp { get; private set; }

    public int MaxMp { get; private set; }

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

    public bool TrySpendMp(int cost)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cost);
        if (cost > Mp)
        {
            return false;
        }

        Mp -= cost;
        return true;
    }

    /// <returns>The MP actually restored.</returns>
    public int RestoreMp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var restored = Math.Min(amount, MaxMp - Mp);
        Mp += restored;
        return restored;
    }

    /// <summary>
    /// A level-up: the gains go to the maxima and to current HP and MP alike, so the member keeps the HP and MP they
    /// had spent (ADR 0017). A fallen member stays at 0 HP: levelling up doesn't revive.
    /// </summary>
    internal void RaiseMaxima(int maxHpGain, int maxMpGain)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxHpGain);
        ArgumentOutOfRangeException.ThrowIfNegative(maxMpGain);

        MaxHp += maxHpGain;
        MaxMp += maxMpGain;
        Mp += maxMpGain;
        if (IsAlive)
        {
            Hp += maxHpGain;
        }
    }

    private int AddHp(int amount)
    {
        var added = Math.Min(amount, MaxHp - Hp);
        Hp += added;
        return added;
    }
}
