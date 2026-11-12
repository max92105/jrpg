using System.Security.Cryptography;

namespace Jrpg.Data.Content;

/// <summary>
/// What the editor knows of each catalog file since it read or wrote it (decision 559): the hash of its bytes on disk
/// and of its content as the writer serializes it. A save writes a catalog only when its content differs from the
/// stamp, so a file the editor didn't change keeps a hand edit and a hand-laid file isn't relaid; and it refuses a
/// changed file whose bytes on disk moved since the stamp. Empty stamps know nothing: every file counts as changed and
/// none as edited.
/// </summary>
internal sealed class CatalogFileStamps
{
    private readonly Dictionary<string, string> _onDisk = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _content = new(StringComparer.Ordinal);

    /// <summary>Stamps every catalog as just read: its file on disk, if any, and its content as serialized.</summary>
    /// <param name="contentRoot">The folder the files are in.</param>
    /// <param name="catalogs">Each catalog file's name and its loaded content's bytes as the writer lays them.</param>
    public void Take(string contentRoot, IEnumerable<(string File, byte[] Bytes)> catalogs)
    {
        foreach (var (file, bytes) in catalogs)
        {
            var path = Path.Combine(contentRoot, file);
            if (File.Exists(path))
            {
                _onDisk[file] = HashOf(File.ReadAllBytes(path));
            }

            _content[file] = HashOf(bytes);
        }
    }

    /// <summary>Whether the content differs from the stamp; a file never stamped counts as changed.</summary>
    public bool IsChanged(string file, byte[] bytes) => !_content.TryGetValue(file, out var stamp) || stamp != HashOf(bytes);

    /// <summary>
    /// Whether the file on disk no longer matches its stamp: edited by hand, or gone, since the editor read or wrote
    /// it. A file never stamped isn't watched, and one already holding the bytes about to be written isn't edited: a
    /// save whose moves failed partway left it new on disk with its stamp old, and the next save finishes the job.
    /// </summary>
    /// <param name="contentRoot">The folder the file is in.</param>
    /// <param name="file">The catalog file's name.</param>
    /// <param name="bytes">What the save is about to write there.</param>
    public bool IsEditedOnDisk(string contentRoot, string file, byte[] bytes)
    {
        if (!_onDisk.TryGetValue(file, out var stamp))
        {
            return false;
        }

        var path = Path.Combine(contentRoot, file);
        if (!File.Exists(path))
        {
            return true;
        }

        var onDisk = HashOf(File.ReadAllBytes(path));
        return onDisk != stamp && onDisk != HashOf(bytes);
    }

    /// <summary>Stamps a file as just written: its bytes on disk are its content.</summary>
    public void Renew(string file, byte[] bytes)
    {
        var hash = HashOf(bytes);
        _onDisk[file] = hash;
        _content[file] = hash;
    }

    private static string HashOf(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
