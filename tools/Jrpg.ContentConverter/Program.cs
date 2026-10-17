using Jrpg.ContentConverter;
using Jrpg.Data.Content;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: Jrpg.ContentConverter <assets folder> <content folder>");
    return 1;
}

try
{
    var report = LegacyContentConversion.Convert(assetsRoot: args[0], contentRoot: args[1]);
    foreach (var line in report.Lines)
    {
        Console.WriteLine(line);
    }

    return 0;
}
catch (Exception exception) when (exception is ConversionException or ContentValidationException)
{
    // These are thrown before the generated folder is replaced, so the last good content is still there.
    // A disk error while writing isn't caught: it can leave the folder half-written, and re-running fixes it.
    Console.Error.WriteLine($"Conversion failed: {exception.Message}");
    return 1;
}
