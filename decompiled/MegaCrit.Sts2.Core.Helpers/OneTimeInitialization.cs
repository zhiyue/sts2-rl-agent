using System;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using SmartFormat.Extensions;

namespace MegaCrit.Sts2.Core.Helpers;

public static class OneTimeInitialization
{
	private static bool _initialized;

	private static bool _deferredExecuted;

	private static AtlasResourceLoader? _atlasResourceLoader;

	public static ReadSaveResult<SettingsSave> SettingsReadResult { get; private set; }

	public static void Execute()
	{
		ExecuteEssential();
		ExecuteDeferred();
	}

	public static void ExecuteEssential()
	{
		if (!_initialized)
		{
			_initialized = true;
			_atlasResourceLoader = new AtlasResourceLoader();
			ResourceLoader.AddResourceFormatLoader(_atlasResourceLoader, atFront: true);
			AtlasManager.LoadEssentialAtlases();
			if (TestMode.IsOn)
			{
				SettingsReadResult = SaveManager.Instance.InitSettingsDataForTest();
			}
			else
			{
				SettingsReadResult = SaveManager.Instance.InitSettingsData();
			}
			ModManager.Initialize();
			UserDataPathProvider.IsRunningModded = ModManager.LoadedMods.Count > 0;
			LocManager.Initialize();
			ModelDb.Init();
			ModelIdSerializationCache.Init();
			ModelDb.InitIds();
		}
	}

	public static void ExecuteDeferred()
	{
		if (!_deferredExecuted)
		{
			_deferredExecuted = true;
			AtlasManager.LoadAllAtlases();
			if (!OS.HasFeature("editor"))
			{
				ModelDb.Preload();
				PrewarmJit();
				ConditionalFormatter conditionalFormatter = new ConditionalFormatter();
			}
		}
	}

	private static void PrewarmJit()
	{
		Type typeFromHandle = typeof(PacketWriter);
		Type typeFromHandle2 = typeof(PacketReader);
		foreach (Type subtype in ReflectionHelper.GetSubtypes<IPacketSerializable>())
		{
			RuntimeHelpers.PrepareMethod(subtype.GetMethod("Serialize").MethodHandle);
			RuntimeHelpers.PrepareMethod(subtype.GetMethod("Deserialize").MethodHandle);
			RuntimeHelpers.PrepareMethod(typeFromHandle.GetMethod("WriteList").MethodHandle, new RuntimeTypeHandle[1] { subtype.TypeHandle });
			RuntimeHelpers.PrepareMethod(typeFromHandle.GetMethod("Write").MethodHandle, new RuntimeTypeHandle[1] { subtype.TypeHandle });
			RuntimeHelpers.PrepareMethod(typeFromHandle2.GetMethod("ReadList").MethodHandle, new RuntimeTypeHandle[1] { subtype.TypeHandle });
			RuntimeHelpers.PrepareMethod(typeFromHandle2.GetMethod("Read").MethodHandle, new RuntimeTypeHandle[1] { subtype.TypeHandle });
		}
	}
}
