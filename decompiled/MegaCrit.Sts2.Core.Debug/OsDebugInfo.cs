using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Logging;
using SharpGen.Runtime;
using Vortice.DXGI;

namespace MegaCrit.Sts2.Core.Debug;

public static class OsDebugInfo
{
	public static async Task LogSystemInfo()
	{
		if (ReleaseInfoManager.Instance.ReleaseInfo != null)
		{
			string systemInfo = "Fetching system info failed!";
			await Task.Run(() => systemInfo = GetSystemInfoString());
			Log.Info(systemInfo);
		}
	}

	public static string GetSystemInfoString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== Godot OS Debug Information ===");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
		handler.AppendLiteral("Timestamp: ");
		handler.AppendFormatted(DateTime.Now);
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
		handler.AppendLiteral("OS: ");
		handler.AppendFormatted(OS.GetName());
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
		handler.AppendLiteral("OS Version: ");
		handler.AppendFormatted(OS.GetVersion());
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
		handler.AppendLiteral("Distribution Name: ");
		handler.AppendFormatted(OS.GetDistributionName());
		stringBuilder6.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder7 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
		handler.AppendLiteral("Device Model: ");
		handler.AppendFormatted(OS.GetModelName());
		stringBuilder7.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder8 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
		handler.AppendLiteral("Is Debug Build: ");
		handler.AppendFormatted(OS.IsDebugBuild());
		stringBuilder8.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder9 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
		handler.AppendLiteral("Is Sandboxed: ");
		handler.AppendFormatted(OS.IsSandboxed());
		stringBuilder9.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder10 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
		handler.AppendLiteral("Executable Path: ");
		handler.AppendFormatted(OS.GetExecutablePath());
		stringBuilder10.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder11 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
		handler.AppendLiteral("Data Directory: ");
		handler.AppendFormatted(OS.GetDataDir());
		stringBuilder11.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder12 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
		handler.AppendLiteral("User Data Directory: ");
		handler.AppendFormatted(OS.GetUserDataDir());
		stringBuilder12.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder13 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
		handler.AppendLiteral("Command Line Args: ");
		handler.AppendFormatted(string.Join(" ", OS.GetCmdlineArgs()));
		stringBuilder13.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder14 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(24, 1, stringBuilder2);
		handler.AppendLiteral("User Command Line Args: ");
		handler.AppendFormatted(string.Join(" ", OS.GetCmdlineUserArgs()));
		stringBuilder14.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder15 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
		handler.AppendLiteral("OS Locale: ");
		handler.AppendFormatted(OS.GetLocale());
		stringBuilder15.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder16 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
		handler.AppendLiteral("OS Language: ");
		handler.AppendFormatted(OS.GetLocaleLanguage());
		stringBuilder16.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder17 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
		handler.AppendLiteral("Game Locale: ");
		handler.AppendFormatted(TranslationServer.GetLocale());
		stringBuilder17.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder18 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
		handler.AppendLiteral("Is UserFS Persistent: ");
		handler.AppendFormatted(OS.IsUserfsPersistent());
		stringBuilder18.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder19 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
		handler.AppendLiteral("Is Stdout Verbose: ");
		handler.AppendFormatted(OS.IsStdOutVerbose());
		stringBuilder19.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder20 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, stringBuilder2);
		handler.AppendLiteral("Is Low Processor Usage Mode: ");
		handler.AppendFormatted(OS.IsInLowProcessorUsageMode());
		stringBuilder20.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder21 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
		handler.AppendLiteral("Architecture: ");
		handler.AppendFormatted(Engine.GetArchitectureName());
		stringBuilder21.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder22 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
		handler.AppendLiteral("Engine Version: ");
		handler.AppendFormatted(Engine.GetVersionInfo()["string"]);
		stringBuilder22.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder23 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
		handler.AppendLiteral("Is Editor: ");
		handler.AppendFormatted(Engine.IsEditorHint());
		stringBuilder23.AppendLine(ref handler);
		ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
		if (releaseInfo != null)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder24 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
			handler.AppendLiteral("Release Version: ");
			handler.AppendFormatted(releaseInfo.Version);
			stringBuilder24.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder25 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("Release Commit: ");
			handler.AppendFormatted(releaseInfo.Commit);
			stringBuilder25.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder26 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
			handler.AppendLiteral("Release Date: ");
			handler.AppendFormatted(releaseInfo.Date);
			stringBuilder26.AppendLine(ref handler);
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder27 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
		handler.AppendLiteral("Processor Count: ");
		handler.AppendFormatted(OS.GetProcessorCount());
		stringBuilder27.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder28 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
		handler.AppendLiteral("Processor Name: ");
		handler.AppendFormatted(OS.GetProcessorName());
		stringBuilder28.AppendLine(ref handler);
		RenderingDevice renderingDevice = RenderingServer.GetRenderingDevice();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder29 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("Rendering device name: ");
		handler.AppendFormatted(renderingDevice?.GetDeviceName() ?? "N/A (headless)");
		stringBuilder29.AppendLine(ref handler);
		if (DXGI.CreateDXGIFactory1<IDXGIFactory1>(out IDXGIFactory1 factory) == Result.Ok)
		{
			IDXGIAdapter1 adapterOut;
			for (uint num = 0u; factory.EnumAdapters1(num, out adapterOut) == Result.Ok; num++)
			{
				adapterOut.CheckInterfaceSupport(typeof(IDXGIDevice), out var userModeDriverVersion);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder30 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(21, 2, stringBuilder2);
				handler.AppendLiteral("  Graphics adapter ");
				handler.AppendFormatted(num);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(adapterOut.Description.Description);
				stringBuilder30.AppendLine(ref handler);
				global::_003C_003Ey__InlineArray4<object> buffer = default(global::_003C_003Ey__InlineArray4<object>);
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray4<object>, object>(ref buffer, 0) = userModeDriverVersion >> 48;
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray4<object>, object>(ref buffer, 1) = (userModeDriverVersion >> 32) & 0xFFFF;
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray4<object>, object>(ref buffer, 2) = (userModeDriverVersion >> 16) & 0xFFFF;
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray4<object>, object>(ref buffer, 3) = (userModeDriverVersion >> 32) & 0xFFFF;
				stringBuilder.AppendLine(string.Format("  version: {0}.{1}.{2}.{3}", global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray4<object>, object>(in buffer, 4)));
			}
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder31 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("Screen info (primary ");
		handler.AppendFormatted(DisplayServer.GetPrimaryScreen());
		handler.AppendLiteral("):");
		stringBuilder31.AppendLine(ref handler);
		for (int i = 0; i < DisplayServer.GetScreenCount(); i++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder32 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(59, 6, stringBuilder2);
			handler.AppendLiteral("  Index ");
			handler.AppendFormatted(i);
			handler.AppendLiteral(": Size: ");
			handler.AppendFormatted(DisplayServer.ScreenGetSize(i));
			handler.AppendLiteral(" Orientation: ");
			handler.AppendFormatted(DisplayServer.ScreenGetOrientation(i));
			handler.AppendLiteral(" ");
			handler.AppendLiteral("Scale: ");
			handler.AppendFormatted(DisplayServer.ScreenGetScale(i));
			handler.AppendLiteral(" DPI: ");
			handler.AppendFormatted(DisplayServer.ScreenGetDpi());
			handler.AppendLiteral(" Refresh Rate: ");
			handler.AppendFormatted(DisplayServer.ScreenGetRefreshRate());
			stringBuilder32.AppendLine(ref handler);
		}
		ulong renderingInfo = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.VideoMemUsed);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder33 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
		handler.AppendLiteral("Video Memory Used: ");
		handler.AppendFormatted(FormatBytes(renderingInfo));
		stringBuilder33.AppendLine(ref handler);
		Dictionary memoryInfo = OS.GetMemoryInfo();
		if (memoryInfo.Count > 0)
		{
			stringBuilder.AppendLine("Memory Info:");
			foreach (KeyValuePair<Variant, Variant> item in memoryInfo)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder34 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
				handler.AppendLiteral("  ");
				handler.AppendFormatted(item.Key);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(FormatBytes((ulong)item.Value));
				stringBuilder34.AppendLine(ref handler);
			}
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder35 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("  Static Memory Usage: ");
		handler.AppendFormatted(FormatBytes(OS.GetStaticMemoryUsage()));
		stringBuilder35.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder36 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder2);
		handler.AppendLiteral("  Static Memory Peak Usage: ");
		handler.AppendFormatted(FormatBytes(OS.GetStaticMemoryPeakUsage()));
		stringBuilder36.AppendLine(ref handler);
		string[] grantedPermissions = OS.GetGrantedPermissions();
		if (grantedPermissions.Length != 0)
		{
			stringBuilder.AppendLine("Granted Permissions:");
			string[] array = grantedPermissions;
			foreach (string value in array)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder37 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
				handler.AppendLiteral("  ");
				handler.AppendFormatted(value);
				stringBuilder37.AppendLine(ref handler);
			}
		}
		string[] array2 = new string[5] { "PATH", "GODOT_ROOT_DIR", "HOME", "DYLD_LIBRARY_PATH", "LD_LIBRARY_PATH" };
		stringBuilder.AppendLine("Important Environment Variables:");
		string[] array3 = array2;
		foreach (string text in array3)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder38 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
			handler.AppendLiteral("  ");
			handler.AppendFormatted(text);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(OS.GetEnvironment(text));
			stringBuilder38.AppendLine(ref handler);
		}
		return stringBuilder.ToString();
	}

	private static string FormatBytes(ulong bytes)
	{
		string[] array = new string[6] { "B", "KB", "MB", "GB", "TB", "PB" };
		int num = 0;
		decimal num2 = bytes;
		while (Math.Round(num2 / 1024m) >= 1m)
		{
			num2 /= 1024m;
			num++;
		}
		return $"{num2:n1}{array[num]}";
	}
}
