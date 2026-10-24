using System;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Debug;

/// <summary>
/// Hashes assemblies to aid in mod detection.
/// </summary>
public static class AssemblyHasher
{
	private static int? _mainAssemblyHash;

	public static int GetMainAssemblyHash()
	{
		if (_mainAssemblyHash.HasValue)
		{
			return _mainAssemblyHash.Value;
		}
		if (OS.HasFeature("editor"))
		{
			Log.Info("Assembly hashing disabled in editor");
			_mainAssemblyHash = 0;
			return 0;
		}
		Assembly assembly = typeof(AssemblyHasher).Assembly;
		try
		{
			using FileStream source = new FileStream(assembly.Location, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
			byte[] array = SHA1.HashData(source);
			_mainAssemblyHash = BinaryPrimitives.ReadInt32LittleEndian(array);
		}
		catch (Exception value)
		{
			Log.Warn($"Could not read main assembly {assembly} from {assembly.Location}. Exception: {value}");
			_mainAssemblyHash = 0;
		}
		return _mainAssemblyHash.Value;
	}
}
