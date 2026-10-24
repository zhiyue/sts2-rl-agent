using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Steamworks;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public static class SteamInitializer
{
	public const ulong steamAppId = 2868840uL;

	private static CancellationTokenSource _disconnectCts = new CancellationTokenSource();

	public static bool Initialized { get; private set; }

	/// <summary>
	/// Initialization result. Failure code if Initialization was called and Initialized is false.
	/// </summary>
	public static ESteamAPIInitResult? InitResult { get; private set; }

	public static string? InitErrorMessage { get; private set; }

	/// <summary>
	/// Cancellation token that is cancelled when Steam disconnects. Pass this to SteamCallResult
	/// to prevent async Steam API calls from hanging indefinitely when the callback pump stops.
	/// </summary>
	public static CancellationToken DisconnectToken => _disconnectCts.Token;

	public static event Action? SteamNoLongerRunning;

	private static nint SteamDebugResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName != "steam_api" && libraryName != "steam_api64")
		{
			return IntPtr.Zero;
		}
		if (OS.HasFeature("editor"))
		{
			string text;
			switch (OS.GetName())
			{
			case "Windows":
				text = "steam/SteamApi/steam_api64.dll";
				break;
			case "macOS":
				text = "steam/SteamApi/libsteam_api.dylib";
				break;
			case "FreeBSD":
			case "OpenBSD":
			case "Linux":
			case "NetBSD":
			case "BSD":
				text = "steam/SteamApi/libsteam_api.so";
				break;
			default:
				text = null;
				break;
			}
			string text2 = text;
			if (text2 != null)
			{
				return NativeLibrary.Load(text2);
			}
		}
		return IntPtr.Zero;
	}

	public static bool Initialize(Node node)
	{
		NativeLibrary.SetDllImportResolver(Assembly.GetAssembly(typeof(SteamAPI)), SteamDebugResolver);
		Log.Info("Steamworks: attempting initialization...");
		for (int i = 0; i < 3; i++)
		{
			if (i > 0)
			{
				Thread.Sleep(100);
				Log.Info($"Steam initialization retry attempt {i}");
			}
			Initialized = InitializeInternal();
			if (Initialized)
			{
				break;
			}
		}
		if (Initialized)
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
			TaskHelper.RunSafely(RunCallbacksAsync(node));
		}
		return Initialized;
	}

	private static bool InitializeInternal()
	{
		try
		{
			Log.Info($"Steam is running: {SteamAPI.IsSteamRunning()}");
			InitResult = SteamAPI.InitEx(out var OutSteamErrMsg);
			InitErrorMessage = OutSteamErrMsg;
			if (InitResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
			{
				Log.Info("Steamworks initialization succeeded!");
				return true;
			}
			Log.Error($"Steamworks initialization failed! Result: {InitResult}, message: {InitErrorMessage}");
		}
		catch (Exception ex)
		{
			Log.Error($"Steamworks initialization threw an exception: {ex}");
			InitResult = null;
			InitErrorMessage = ex.Message;
		}
		return false;
	}

	private static async Task RunCallbacksAsync(Node node)
	{
		while (Initialized)
		{
			await node.AwaitProcessFrame();
			if (!SteamAPI.IsSteamRunning())
			{
				Log.Warn("Steam is no longer running. Stopping callbacks.");
				Initialized = false;
				_disconnectCts.Cancel();
				SteamInitializer.SteamNoLongerRunning?.Invoke();
				break;
			}
			SteamAPI.RunCallbacks();
		}
	}

	public static void Uninitialize()
	{
		if (!Initialized)
		{
			return;
		}
		Log.Info("Steamworks: shutting down...");
		try
		{
			SteamAPI.Shutdown();
			Log.Info("Steamworks shutdown succeeded!");
		}
		catch (Exception value)
		{
			Log.Error($"Steamworks shutdown threw an exception: {value}");
		}
		finally
		{
			Initialized = false;
		}
	}
}
