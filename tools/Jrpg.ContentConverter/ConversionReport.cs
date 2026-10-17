namespace Jrpg.ContentConverter;

/// <summary>What a conversion run did, one line per source file or entry, to read after every run.</summary>
internal sealed class ConversionReport
{
    private readonly List<string> _lines = [];

    public IReadOnlyList<string> Lines => _lines;

    public void AddConverted(string sourceFile, string summary) => _lines.Add($"{sourceFile}: converted {summary}.");
}
