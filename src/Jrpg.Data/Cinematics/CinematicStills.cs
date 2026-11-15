using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;

namespace Jrpg.Data.Cinematics;

/// <summary>The stills of the cinematics, for the checks that a picture or a voice line a still names exists.</summary>
internal static class CinematicStills
{
    /// <summary>
    /// Every still of every cinematic, in id order then the stills' order, with what names it: <c>cinematic
    /// 'ragnir_intro' still 3</c>, counted from 1 as the editor's window shows them.
    /// </summary>
    public static IEnumerable<(CinematicStill Still, string NamedBy)> Of(ContentCatalog catalog) =>
        from cinematic in catalog.Cinematics.Values.OrderBy(cinematic => cinematic.Id, StringComparer.Ordinal)
        from numbered in cinematic.Stills.Select((still, index) => (Still: still, Number: index + 1))
        select (numbered.Still, $"cinematic '{cinematic.Id}' still {numbered.Number}");
}
