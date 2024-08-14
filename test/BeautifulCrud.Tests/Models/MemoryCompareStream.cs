using System.Diagnostics;

namespace BeautifulCrud.Tests.Models;

public sealed class MemoryCompareStream(byte[] comparand) : Stream
{
    public override void Write(byte[] buffer, int offset, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (buffer[offset + i] == comparand[Position + i])
                continue;

            Debug.Assert(false);
            throw new Exception("Data mismatch");
        }

        Position += count;
    }

    public override void WriteByte(byte value)
    {
        if (comparand[Position] != value)
        {
            Debug.Assert(false);
            throw new Exception("Data mismatch");
        }

        Position++;
    }

    public override bool CanRead => false;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override void Flush() { }
    public override long Length => comparand.Length;
    public override long Position { get; set; }

    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = comparand.Length - offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }
        return Position;
    }

    public override void SetLength(long value) => throw new InvalidOperationException();
}