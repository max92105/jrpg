using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jrpg.Core.Content;
using Jrpg.Core.Saving;

namespace Jrpg.Data.Saving;

/// <summary>
/// Saves in a folder, one <c>{slot}.json</c> per slot. Which slots exist (manual slots, the autosave) is the Game's
/// (developer decision 34); here a slot is any name of lowercase letters, digits and underscores.
/// </summary>
public sealed partial class FileSaveStore(string directory, ContentCatalog catalog)
{
    private const string FormatVersionProperty = "formatVersion";

    // The pre-parse must reject duplicate keys too, or one would slip past the strict serializer as an ArgumentException.
    private static readonly JsonDocumentOptions _strictDocument = new() { AllowDuplicateProperties = false };

    /// <summary>Replaces the slot's file atomically: written to a temporary file first, then moved over the old one.</summary>
    /// <exception cref="ArgumentException">The slot name has characters other than a-z, 0-9 and underscore.</exception>
    public void Write(string slotName, SaveGame save)
    {
        var path = PathOf(slotName);
        Directory.CreateDirectory(directory);
        var temporaryPath = path + ".tmp";
        using (var stream = File.Create(temporaryPath))
        {
            JsonSerializer.Serialize(stream, new SaveFile(SaveFile.CurrentFormatVersion, save), SaveJsonOptions.Strict);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }

    /// <returns>
    /// The save repaired against the current content, with what the repair changed (decision 40), or null when the slot is empty.
    /// </returns>
    /// <exception cref="ArgumentException">The slot name has characters other than a-z, 0-9 and underscore.</exception>
    /// <exception cref="UnreadableSaveException">
    /// The file is damaged, has no format version (older than versioned saves, ADR 0007), an older or a newer
    /// one (ADR 0022), or none of its active members exists anymore.
    /// </exception>
    public RepairedSave? Read(string slotName)
    {
        var path = PathOf(slotName);
        if (!File.Exists(path))
        {
            return null;
        }

        var root = ParseObject(slotName, File.ReadAllText(path));
        CheckFormatVersion(slotName, root);
        var game = Deserialize(slotName, root).Game;
        CheckNoNullEntries(slotName, game);
        return SaveGameRepair.Repair(slotName, game, catalog);
    }

    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex SlotNamePattern();

    private static JsonObject ParseObject(string slotName, string json)
    {
        try
        {
            return JsonNode.Parse(json, documentOptions: _strictDocument) as JsonObject
                ?? throw new UnreadableSaveException(slotName, "the file doesn't hold a save.");
        }
        catch (JsonException exception)
        {
            throw new UnreadableSaveException(slotName, $"the file is damaged ({exception.Message}).", exception);
        }
    }

    // Saves older than versioned saves (ADR 0007) have no version; a newer version may hold things this build can't know
    // about; an older one is refused, not migrated, until the first release (ADR 0022).
    private static void CheckFormatVersion(string slotName, JsonObject root)
    {
        if (!root.TryGetPropertyValue(FormatVersionProperty, out var versionNode) || versionNode is null)
        {
            throw new UnreadableSaveException(slotName, "it's not a save from this version of the game.");
        }

        if (versionNode.GetValueKind() != JsonValueKind.Number || !versionNode.AsValue().TryGetValue<int>(out var version) || version < 1)
        {
            throw new UnreadableSaveException(slotName, $"its format version '{versionNode.ToJsonString()}' isn't valid.");
        }

        if (version > SaveFile.CurrentFormatVersion)
        {
            throw new UnreadableSaveException(slotName, $"it was made by a newer version of the game (format {version}).");
        }

        if (version < SaveFile.CurrentFormatVersion)
        {
            throw new UnreadableSaveException(slotName, $"it was made by an older version of the game (format {version}).");
        }
    }

    private static SaveFile Deserialize(string slotName, JsonObject root)
    {
        try
        {
            // A non-null JSON object never deserializes to null.
            return root.Deserialize<SaveFile>(SaveJsonOptions.Strict)!;
        }
        catch (JsonException exception)
        {
            var location = exception.Path is null ? string.Empty : $" at {exception.Path}";
            throw new UnreadableSaveException(slotName, $"the file is damaged{location} ({exception.Message}).", exception);
        }
    }

    // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
    private static void CheckNoNullEntries(string slotName, SaveGame game)
    {
        var hasNullEntry = game.Members.Contains(null) || game.ActiveMemberIds.Contains(null) || game.Inventory.Contains(null)
            || game.CompletedEvents.Contains(null) || game.ObjectSwaps.Contains(null);
        if (hasNullEntry)
        {
            throw new UnreadableSaveException(slotName, "the file is damaged (a list holds a null entry).");
        }
    }

    private string PathOf(string slotName)
    {
        if (!SlotNamePattern().IsMatch(slotName))
        {
            throw new ArgumentException($"Slot name '{slotName}' must use only a-z, 0-9 and underscore.", nameof(slotName));
        }

        return Path.Combine(directory, $"{slotName}.json");
    }
}
