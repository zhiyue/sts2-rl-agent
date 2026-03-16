using System;
using System.IO;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.Saves;

public static class MigrationUtil
{
	public static bool MigrateDirectory(string directoryName, string legacyBasePath, string newBasePath)
	{
		string text = Path.Combine(legacyBasePath, directoryName);
		string text2 = Path.Combine(newBasePath, directoryName);
		if (!Directory.Exists(text))
		{
			Log.Info("No legacy " + directoryName + " directory found, skipping");
			return false;
		}
		if (Directory.Exists(text2))
		{
			Log.Info("Skipping migration because " + text2 + " already exists");
			return false;
		}
		Log.Info($"Migrating {directoryName} directory from {text} to {text2}");
		try
		{
			Directory.CreateDirectory(text2);
			CopyDirectoryRecursively(text, text2);
			Log.Info("Successfully migrated " + directoryName + " directory");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to migrate " + directoryName + " directory: " + ex.Message);
			return false;
		}
	}

	public static bool MigrateFile(string fileName, string legacyBasePath, string newBasePath)
	{
		string text = Path.Combine(legacyBasePath, fileName);
		string text2 = Path.Combine(newBasePath, fileName);
		if (!File.Exists(text))
		{
			Log.Info("No legacy " + fileName + " file found, skipping");
			return false;
		}
		Log.Info($"Migrating {fileName} from {text} to {text2}");
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text2));
			File.Copy(text, text2, overwrite: true);
			Log.Info("Successfully migrated " + fileName);
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to migrate " + fileName + ": " + ex.Message);
			return false;
		}
	}

	public static void CopyDirectoryRecursively(string sourceDir, string targetDir)
	{
		Directory.CreateDirectory(targetDir);
		string[] files = Directory.GetFiles(sourceDir);
		foreach (string text in files)
		{
			string fileName = Path.GetFileName(text);
			string destFileName = Path.Combine(targetDir, fileName);
			File.Copy(text, destFileName, overwrite: true);
		}
		string[] directories = Directory.GetDirectories(sourceDir);
		foreach (string text2 in directories)
		{
			string fileName2 = Path.GetFileName(text2);
			string targetDir2 = Path.Combine(targetDir, fileName2);
			CopyDirectoryRecursively(text2, targetDir2);
		}
	}

	public static void ArchiveLegacyDirectory(string directoryPath, string archivePath)
	{
		if (!Directory.Exists(directoryPath))
		{
			return;
		}
		try
		{
			string fileName = Path.GetFileName(directoryPath);
			string text = Path.Combine(archivePath, fileName);
			Directory.Move(directoryPath, text);
			Log.Info("Archived legacy directory: " + directoryPath + " -> " + text);
		}
		catch (Exception ex)
		{
			Log.Warn("Could not archive legacy directory " + directoryPath + ": " + ex.Message);
		}
	}

	public static void ArchiveLegacyFile(string filePath, string archivePath)
	{
		if (!File.Exists(filePath))
		{
			return;
		}
		try
		{
			string fileName = Path.GetFileName(filePath);
			string text = Path.Combine(archivePath, fileName);
			File.Move(filePath, text);
			Log.Info("Archived legacy file: " + filePath + " -> " + text);
		}
		catch (Exception ex)
		{
			Log.Warn("Could not archive legacy file " + filePath + ": " + ex.Message);
		}
	}
}
