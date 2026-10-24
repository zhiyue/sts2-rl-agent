using System.IO;
using System.IO.Compression;
using System.Text;
using MegaCrit.Sts2.Core.Logging;
using Sentry;

namespace MegaCrit.Sts2.Core.Debug;

public static class SentryExtension
{
	public static void AddCompressedAttachment(this Scope scope, string text, string fileName)
	{
		byte[] array = GzipCompress(text);
		if (array.Length <= 102400)
		{
			scope.AddAttachment(array, fileName);
			return;
		}
		Log.Warn($"Skipping Sentry attachment {fileName}: {array.Length / 1024} KB exceeds {100} KB limit");
	}

	private static byte[] GzipCompress(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return memoryStream.ToArray();
	}
}
