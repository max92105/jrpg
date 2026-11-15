using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// The battle art the startup check reads: party members' and enemies' battle sprites, an animated enemy's strip split
/// into its frames (ADR 0047), enemies' portraits, attack effects, skills' and items' own strips (decision 709), and
/// the strips and swings <c>effect-strips.json</c> lists, each hit frame within its strip (ADR 0048).
/// </summary>
public static partial class ArtReferenceCheck
{
    private static void AddBattleArtProblems(
        List<string> problems, ContentCatalog catalog, string artRoot, IEnumerable<PartyMemberDefinition> members)
    {
        var memberBattlers = members.Select(member => (member.BattleSpriteFile, $"party member '{member.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, memberBattlers);
        var enemies = catalog.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal);
        var enemyBattlers = enemies.Select(enemy => (enemy.BattleSpriteFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, enemyBattlers);
        AddPartialBattlerStrips(problems, artRoot, enemies);
        var enemyPortraits = enemies.Select(enemy => (enemy.PortraitFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Portraits, enemyPortraits);
        var impactStrips = members.Select(member => (File: member.AttackEffectFile, NamedBy: $"party member '{member.Id}'"))
            .Concat(enemies.Select(enemy => (File: enemy.AttackEffectFile, NamedBy: $"enemy '{enemy.Id}'")))
            .Concat(OwnStripsOf(catalog))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, impactStrips);
        AddPartialFrameStrips(problems, artRoot, impactStrips.Select(effect => effect.File));
        AddEffectStripRecordProblems(problems, artRoot, catalog.EffectStrips);
    }

    // Decision 709: the strips skills and items name for themselves, by id, checked as the attackers' strips are.
    private static IEnumerable<(string File, string NamedBy)> OwnStripsOf(ContentCatalog catalog) =>
        catalog.Skills.Values
            .Where(skill => skill.EffectFile is not null)
            .OrderBy(skill => skill.Id, StringComparer.Ordinal)
            .Select(skill => (File: skill.EffectFile!, NamedBy: $"skill '{skill.Id}'"))
            .Concat(catalog.Items.Values
                .Where(item => item.EffectFile is not null)
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .Select(item => (File: item.EffectFile!, NamedBy: $"item '{item.Id}'")));

    // ADR 0048: a listed strip and its swing are in effects/ by exact name, the swing is whole square frames, as every
    // strip is, and the hit lands on one of the strip's frames. A missing strip is named once, by the exact-name line.
    private static void AddEffectStripRecordProblems(
        List<string> problems, string artRoot, IReadOnlyDictionary<string, EffectStripDefinition> listed)
    {
        var records = listed.Values.OrderBy(strip => strip.File, StringComparer.Ordinal).ToList();
        var strips = records.Select(strip => (strip.File, $"effect-strips.json '{strip.File}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, strips);
        var swings = records
            .Where(strip => strip.SwingFile is not null)
            .Select(strip => (File: strip.SwingFile!, NamedBy: $"effect-strips.json '{strip.File}' swing"))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, swings);
        AddPartialFrameStrips(problems, artRoot, swings.Select(swing => swing.File));
        AddHitFramesPastTheirStrip(problems, artRoot, records);
    }

    private static void AddHitFramesPastTheirStrip(List<string> problems, string artRoot, List<EffectStripDefinition> records)
    {
        foreach (var strip in records)
        {
            if (ReadSize(artRoot, ArtFolders.Effects, strip.File).Size is not { Height: > 0 } size)
            {
                continue;
            }

            var frames = size.Width / size.Height;
            if (strip.HitFrame >= frames)
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Effects, strip.File)}: hit frame {strip.HitFrame} lies past its "
                    + $"{frames} frames (from 0). Named by effect-strips.json.");
            }
        }
    }

    // An animated enemy's strip is its loop's frames side by side (ADR 0047): its width must split into them evenly.
    private static void AddPartialBattlerStrips(List<string> problems, string artRoot, IEnumerable<EnemyDefinition> enemies)
    {
        foreach (var enemy in enemies)
        {
            var file = enemy.BattleSpriteFile;
            if (enemy.IdleAnimation is not { } idle || ReadSize(artRoot, ArtFolders.Battlers, file).Size is not { } size)
            {
                continue;
            }

            if (!SplitsIntoFrames(size.Width, idle.FrameCount))
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Battlers, file)}: a {size.Width}x{size.Height} strip "
                    + $"doesn't split into {idle.FrameCount} frames. Named by enemy '{enemy.Id}'.");
            }
        }
    }

    private static bool SplitsIntoFrames(int width, int frameCount) => width % frameCount == 0;

    // An effect strip is square frames side by side (decision 57), so its width must be a whole number of its heights.
    private static void AddPartialFrameStrips(List<string> problems, string artRoot, IEnumerable<string> files)
    {
        foreach (var file in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (ReadSize(artRoot, ArtFolders.Effects, file).Size is not { } size)
            {
                continue;
            }

            if (!IsRowOfSquareFrames(size))
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Effects, file)}: a {size.Width}x{size.Height} strip "
                    + "isn't a row of whole square frames.");
            }
        }
    }

    /// <summary>Whether an effect strip this size is whole square frames side by side (decision 57).</summary>
    internal static bool IsRowOfSquareFrames((int Width, int Height) size) => size.Height > 0 && size.Width % size.Height == 0;
}
