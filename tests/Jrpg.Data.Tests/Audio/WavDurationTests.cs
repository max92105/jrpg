using System.Buffers.Binary;
using System.Text;
using Jrpg.Data.Audio;

namespace Jrpg.Data.Tests.Audio;

// A WAV's length from its header (decision 620), on headers the tests write: no sample is read, so the data chunk only
// declares its size; the LIST chunk's odd size is padded to an even one, as RIFF pads every chunk.
public sealed class WavDurationTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"jrpg-wav-{Guid.NewGuid():N}.wav");

    public void Dispose() => File.Delete(_path);

    [Fact]
    public void Read_OneSecondAt44100Mono16Bit_IsOneSecond()
    {
        File.WriteAllBytes(_path, [.. Riff(), .. Format(byteRate: 88200), .. Chunk("data", 88200)]);

        Assert.Equal(TimeSpan.FromSeconds(1), WavDuration.Read(_path));
    }

    [Fact]
    public void Read_ListChunkBeforeData_SkipsIt()
    {
        byte[] list = [.. Chunk("LIST", 5), 1, 2, 3, 4, 5, 0];
        File.WriteAllBytes(_path, [.. Riff(), .. Format(byteRate: 88200), .. list, .. Chunk("data", 44100)]);

        Assert.Equal(TimeSpan.FromSeconds(0.5), WavDuration.Read(_path));
    }

    [Fact]
    public void Read_NotRiff_IsNull()
    {
        File.WriteAllText(_path, "not a wave file at all");

        Assert.Null(WavDuration.Read(_path));
    }

    [Fact]
    public void Read_NoDataChunk_IsNull()
    {
        File.WriteAllBytes(_path, [.. Riff(), .. Format(byteRate: 88200)]);

        Assert.Null(WavDuration.Read(_path));
    }

    [Fact]
    public void Read_MissingFile_IsNull()
    {
        Assert.Null(WavDuration.Read(_path));
    }

    [Fact]
    public void Read_RepositoryPlaceholderVoice_IsAboveZero()
    {
        var audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));
        var voice = Path.Combine(audioRoot, AudioFolders.Voices, "Placeholder.wav");

        Assert.True(WavDuration.Read(voice) > TimeSpan.Zero);
    }

    private static byte[] Riff() => [.. Encoding.ASCII.GetBytes("RIFF"), 0, 0, 0, 0, .. Encoding.ASCII.GetBytes("WAVE")];

    // PCM, mono, 44100 Hz, the given byte rate, block align 2, 16 bits.
    private static byte[] Format(uint byteRate)
    {
        var body = new byte[16];
        BinaryPrimitives.WriteUInt16LittleEndian(body, 1);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(2), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(4), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(body.AsSpan(8), byteRate);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(12), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(14), 16);
        return [.. Chunk("fmt ", 16), .. body];
    }

    private static byte[] Chunk(string id, uint size)
    {
        var header = new byte[8];
        Encoding.ASCII.GetBytes(id).CopyTo(header, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), size);
        return header;
    }
}
