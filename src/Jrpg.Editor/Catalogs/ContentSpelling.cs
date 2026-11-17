using System.Text.Json;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// A Core enum value as the content files spell it ("fireResistance"), so a page's lines and its own messages name a
/// value as Data's messages and the files do.
/// </summary>
internal static class ContentSpelling
{
    public static string Of<T>(T value) where T : struct, Enum => JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
}
