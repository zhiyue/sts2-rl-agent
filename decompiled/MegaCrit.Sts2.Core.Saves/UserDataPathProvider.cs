using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Saves;

public static class UserDataPathProvider
{
	public static string SavesDir => "saves";

	public static bool IsRunningModded { get; set; }

	public static string GetProfileScopedPath(int profileId, string dataType, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		return $"user://{platformDirectoryName}/{value}/{GetProfileDir(profileId)}/{dataType}";
	}

	public static string GetProfileScopedBasePath(int profileId, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		return $"user://{platformDirectoryName}/{value}/{GetProfileDir(profileId)}";
	}

	public static string GetAccountScopedBasePath(string? dataType, PlatformType? platformOverride = null, ulong? userIdOverride = null)
	{
		PlatformType platformType = platformOverride ?? PlatformUtil.PrimaryPlatform;
		ulong value = userIdOverride ?? PlatformUtil.GetLocalPlayerId(platformType);
		string platformDirectoryName = GetPlatformDirectoryName(platformType);
		if (dataType == null)
		{
			return $"user://{platformDirectoryName}/{value}";
		}
		return $"user://{platformDirectoryName}/{value}/{dataType}";
	}

	public static string GetProfileDir(int profileId)
	{
		return Path.Combine(IsRunningModded ? "modded/" : "", $"profile{profileId}");
	}

	public static string GetLegacyPreAccountPath(string dataType)
	{
		return "user://" + dataType;
	}

	public static string GetPlatformDirectoryName(PlatformType platform)
	{
		if (platform == PlatformType.Steam)
		{
			return "steam";
		}
		return OS.HasFeature("editor") ? "editor" : "default";
	}

	public static bool IsLegacyPath(string path)
	{
		if (!path.Contains("/steam/") && !path.Contains("/default/"))
		{
			return !path.Contains("/editor/");
		}
		return false;
	}
}
