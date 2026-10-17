namespace Jrpg.ContentConverter;

/// <summary>A legacy file can't be converted. The message names the file and, when known, the entry.</summary>
internal sealed class ConversionException : Exception
{
    public ConversionException(string message)
        : base(message)
    {
    }

    public ConversionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
