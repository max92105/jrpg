namespace Jrpg.Data.Content;

/// <summary>A content file can't be loaded. The message names the file and, when known, the id and field.</summary>
public sealed class ContentValidationException : Exception
{
    public ContentValidationException(string message)
        : base(message)
    {
    }

    public ContentValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
