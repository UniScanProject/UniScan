using DotNetty.Common.Utilities;
using Array = System.Array;

namespace UniScan.Client.App.Platform.Browser.Filesystem.Stream;

public class OriginPrivateFileStream(IOPFSWorkerService worker, string id, long initialLength) : System.IO.Stream
{
    private bool _disposed;
    
    private long _length = initialLength;
    private long _pos;
    
    public override long Length => _length;
    public override long Position { get => _pos; set => Seek(value, SeekOrigin.Begin); }
    
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        byte[] read = await worker.ReadAsync(id, _pos, count);
        _pos += read.Length;
        
        Array.Copy(read, 0, buffer, offset, read.Length);
        return read.Length;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        byte[] read = await worker.ReadAsync(id, _pos, buffer.Length);
        _pos += read.Length;
        
        read.AsSpan().CopyTo(buffer.Span);
        return read.Length;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        byte[] b = buffer.Slice(offset, offset + count);
        long written = await worker.WriteAsync(id, b, (int)_pos);
        
        _pos += written;
        if (_pos > _length) _length = _pos;
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException("Cannot call synch Read on OPFS stream");

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Cannot call synch Write on OPFS stream");

    public override void SetLength(long value) => throw new NotSupportedException("Cannot call synch SetLength on OPFS stream");

    public async Task SetLengthAsync(long value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        await worker.TruncateAsync(id, value);
        
        _length = value;
        if (_pos > value) _pos = value;
    }

    public override void Flush() => throw new NotSupportedException("Cannot call synch Flush on OPFS stream");

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        await worker.FlushAsync(id);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        long np = origin switch
        {
            SeekOrigin.Begin   => offset,
            SeekOrigin.Current => _pos + offset,
            SeekOrigin.End     => Length + offset,
            _                  => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
        
        if (np < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, null);
        
        _pos = np;
        return _pos;
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await worker.CloseAsync(id);
            _disposed = true;
        }
        
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _ = worker.CloseAsync(id);
            _disposed = true;
        }
        
        base.Dispose(disposing);
    }


}