namespace Jrpg.Data.Content;

/// <summary>Range checks for one numeric content property: each returns a message naming the property, or null if the value is in range.</summary>
internal static class RangeProblems
{
    public static string? AtLeast(string field, int value, int minimum) =>
        value < minimum ? $"{field} must be at least {minimum} (was {value})." : null;

    public static string? Within(string field, int value, int minimum, int maximum) =>
        value < minimum || value > maximum ? $"{field} must be within {minimum}..{maximum} (was {value})." : null;
}
