using System;
using System.IO;
using Godot;

namespace MegaCrit.Sts2.Core.Saves;

public class FileAccessStream : Stream
{
	private readonly Godot.FileAccess _file;

	private readonly Godot.FileAccess.ModeFlags _flags;

	public override bool CanRead
	{
		get
		{
			if (_file.IsOpen())
			{
				return _flags.HasFlag(Godot.FileAccess.ModeFlags.Read);
			}
			return false;
		}
	}

	public override bool CanSeek => _file.IsOpen();

	public override bool CanWrite
	{
		get
		{
			if (_file.IsOpen())
			{
				return _flags.HasFlag(Godot.FileAccess.ModeFlags.Write);
			}
			return false;
		}
	}

	public override long Length => (long)_file.GetLength();

	public override long Position
	{
		get
		{
			return (long)_file.GetPosition();
		}
		set
		{
			_file.Seek((ulong)value);
		}
	}

	public FileAccessStream(string filePath, Godot.FileAccess.ModeFlags flags)
	{
		_flags = flags;
		Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, flags);
		_file = fileAccess ?? throw new IOException($"Opening file {filePath} with flags {flags} failed: {Godot.FileAccess.GetOpenError()}");
	}

	public override void Flush()
	{
		_file.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		byte[] buffer2 = _file.GetBuffer(count);
		int num = Math.Min(count, buffer2.Length);
		Array.Copy(buffer2, 0, buffer, offset, num);
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		int num = Math.Min(buffer.Length - offset, count);
		if (offset == 0 && buffer.Length <= count)
		{
			_file.StoreBuffer(buffer);
			return;
		}
		byte[] array = new byte[num];
		Array.Copy(buffer, offset, array, 0, num);
		_file.StoreBuffer(array);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			_file.Seek((ulong)offset);
			break;
		case SeekOrigin.Current:
			_file.Seek(_file.GetPosition() + (ulong)offset);
			break;
		case SeekOrigin.End:
			_file.Seek(_file.GetLength() - (ulong)offset);
			break;
		}
		return (long)_file.GetPosition();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	protected override void Dispose(bool disposing)
	{
		_file.Close();
	}
}
