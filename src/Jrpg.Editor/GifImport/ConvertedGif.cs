using Jrpg.Core.Maps;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// A GIF converted into a strip file, waiting in a temporary folder of its own for the import to copy it.
/// </summary>
/// <param name="StripPath">The PNG, named as the GIF; its folder is the caller's to delete.</param>
/// <param name="Loop">
/// Its frames and one frame time, for the enemy's idle loop (ADR 0047) or an effect strip's row at its typed pace
/// (decision 988); null for an attack effect without one, which plays at the effects' own pace.
/// </param>
/// <param name="Note">
/// What the import line says of it: a battle strip's "8 frames of 100 x 128 px, 100 ms", and when the delays differed,
/// or "the typed 110 ms"; an effect's "3 frames, 0.24 s; effects play at 80 ms a frame unless effect-strips.json lists
/// them, the GIF's 100 ms dropped", its length at the unlisted pace, or "3 frames, 0.33 s, paced at the typed 110 ms;
/// the Effect strips page's OK writes its record".
/// </param>
internal sealed record ConvertedGif(string StripPath, TileAnimation? Loop, string Note);
