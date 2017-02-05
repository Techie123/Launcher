using System.IO;

public class ProgressionStream : Stream
{
    public delegate void ProgressionHandler(int progression);

    private Stream _sourceStream;
    private ProgressionHandler _progressionHandler;

    public ProgressionStream(Stream sourceStream, ProgressionHandler progressionHandler)
    {
        _sourceStream = sourceStream;
        _progressionHandler = progressionHandler;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _progressionHandler((int)((Position * 100) / Length));
        return _sourceStream.Read(buffer, offset, count);
    }

    public override bool CanRead
    {
        get { return _sourceStream.CanRead; }
    }

    public override bool CanSeek
    {
        get { return _sourceStream.CanSeek; }
    }

    public override bool CanWrite
    {
        get { return _sourceStream.CanWrite; }
    }

    public override long Length
    {
        get { return _sourceStream.Length; }
    }

    public override long Position
    {
        get { return _sourceStream.Position; }
        set { _sourceStream.Position = value; }
    }

    public override void Flush()
    {
        _sourceStream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _sourceStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _sourceStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _sourceStream.Write(buffer, offset, count);
    }
}
