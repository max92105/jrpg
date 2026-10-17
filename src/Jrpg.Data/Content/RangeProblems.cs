namespace Jrpg.Data.Content;

/// <summary>Range checks for one numeric content property: each returns a message naming the property, or null if the value is in range.</summary>
internal static class RangeProblems
{
    public static string? AtLeast(string field, int value, int minimum) =>
        value < minimum ? $"{field} must be at least {minimum} (was {value})." : null;

    public static string? AtMost(string field, int value, string limitName, int limit) =>
        value > limit ? $"{field} must be at most {limitName} ({limit}) (was {value})." : null;
}
