using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using Steamworks;

namespace MegaCrit.Sts2.Core.Modding;

public static class ModManager
{
	public delegate void MetricsUploadHook(SerializableRun run, bool isVictory, ulong localPlayerId);

	private static bool _allowInitForTests;

	private static List<Mod> _mods = new List<Mod>();

	private static Callback<ItemInstalled_t>? _steamItemInstalledCallback;

	private static ModSettings? _settings;

	private static IModManagerFileIo? _fileIo;

	private static SemanticVersion? _gameVersion;

	private static readonly Dictionary<string, string> _circularDependencies = new Dictionary<string, string>();

	private static bool? _hasHarmonyPatches;

	public static ModManagerState State { get; private set; }

	public static IReadOnlyList<Mod> Mods => _mods;

	public static bool PlayerAgreedToModLoading => _settings?.PlayerAgreedToModLoading ?? false;

	/// <summary>
	/// Called when a new mod is detected. Use this to display a warning at runtime if the user adds new mods.
	/// </summary>
	public static event Action<Mod>? OnModDetected;

	/// <summary>
	/// Used by mods to hook into the metrics upload process.
	/// This is unused by STS2 code. It is only used by mods, and is called when we would ordinarily upload metrics.
	/// </summary>
	public static event MetricsUploadHook? OnMetricsUpload;

	/// <summary>
	/// Loads all mods from the appropriate directories. This includes the "mods" directory next to the executable, as
	/// well as Steam workshop files.
	/// This should be called as early as possible in the game's initialization process so that downstream classes
	/// (ModelDb, LocManager) pick up the changes that the mods apply.
	/// </summary>
	public static async Task Initialize(IModManagerFileIo fileIo, ModSettings? settings, SemanticVersion? gameVersion)
	{
		_settings = settings;
		_fileIo = fileIo;
		_gameVersion = gameVersion;
		if (_gameVersion == null)
		{
			Log.Warn("Game doesn't have ReleaseInfo. We can't check version compatibility, so assuming all mods are supported.");
		}
		if (CommandLineHelper.HasArg("nomods"))
		{
			Log.Info("'nomods' passed as executable argument, skipping mod initialization");
			State = ModManagerState.Skipped;
			return;
		}
		if (TestMode.IsOn && !_allowInitForTests)
		{
			State = ModManagerState.Skipped;
			return;
		}
		_allowInitForTests = false;
		AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolveFailure;
		string executablePath = OS.GetExecutablePath();
		string directoryName = Path.GetDirectoryName(executablePath);
		string path = Path.Combine(directoryName, "mods");
		string path2 = Path.Combine(directoryName, "mods_STEAMTEST");
		if (fileIo.DirectoryExists(path))
		{
			ReadModsInDirRecursive(path, ModSource.ModsDirectory, null);
		}
		if (fileIo.DirectoryExists(path2))
		{
			ReadModsInDirRecursive(path2, ModSource.SteamWorkshop, null);
		}
		if (SteamInitializer.Initialized)
		{
			await ReadSteamMods();
		}
		if (OS.IsDebugBuild())
		{
			await Task.Yield();
		}
		if (_mods.Count == 0)
		{
			State = ModManagerState.Initialized;
			return;
		}
		RemoveDisabledMods();
		SortModList(_settings?.ModList ?? new List<SettingsSaveMod>());
		foreach (Mod mod2 in _mods)
		{
			TryLoadMod(mod2);
		}
		if (IsRunningModded())
		{
			int value = _mods.Count((Mod m) => m.state == ModLoadState.Loaded);
			Log.Info($" --- RUNNING MODDED! --- Loaded {value} mods ({_mods.Count} total)");
		}
		State = ModManagerState.Initialized;
		if (_settings == null)
		{
			return;
		}
		List<SettingsSaveMod> list = new List<SettingsSaveMod>();
		foreach (Mod mod in _mods)
		{
			SettingsSaveMod settingsSaveMod = new SettingsSaveMod(mod);
			bool isEnabled = _settings.ModList.FirstOrDefault((SettingsSaveMod m) => m.Id == mod.manifest?.id)?.IsEnabled ?? true;
			settingsSaveMod.IsEnabled = isEnabled;
			list.Add(settingsSaveMod);
		}
		_settings.ModList = list;
	}

	public static void ResetForTests()
	{
		if (TestMode.IsOff)
		{
			throw new NotImplementedException("Tried to reset ModManager outside of tests! This is not allowed, as we cannot unload DLLs or PCKs");
		}
		_mods.Clear();
		State = ModManagerState.None;
		_settings = null;
		_fileIo = null;
		_allowInitForTests = true;
		_circularDependencies.Clear();
	}

	/// <summary>
	/// Sorts mods so that dependencies are loaded before the mods that depend on them (topological sort).
	/// Uses Kahn's algorithm: https://en.wikipedia.org/wiki/Topological_sorting#Kahn's_algorithm
	///
	/// How it works:
	///   1. Build a graph: count each mod's dependencies (in-degree) and track reverse edges (dependentsMap).
	///   2. Seed a frontier queue with all mods that have zero dependencies (they're ready to load).
	///   3. Dequeue a mod, add it to the sorted output, and decrement the in-degree of each mod that depends
	///      on it. When a mod's in-degree reaches zero, all its dependencies are satisfied, so enqueue it.
	///   4. When the queue is empty, any mod NOT in the sorted output is part of a dependency cycle (its
	///      in-degree never reached zero because it's waiting on something that's waiting on it).
	///
	/// For mods with no dependency relationship between them, we use a PriorityQueue keyed by the user's
	/// manual ordering (from settings) to keep the sort stable.
	/// </summary>
	private static void SortModList(List<SettingsSaveMod> manualOrdering)
	{
		List<Mod> list = new List<Mod>();
		List<Mod> list2 = new List<Mod>();
		foreach (Mod mod4 in _mods)
		{
			if (mod4.state != ModLoadState.None)
			{
				list2.Add(mod4);
			}
			else
			{
				list.Add(mod4);
			}
		}
		List<int> list3 = new List<int>();
		Dictionary<Mod, List<Mod>> dictionary = new Dictionary<Mod, List<Mod>>();
		for (int i = 0; i < list.Count; i++)
		{
			Mod mod = list[i];
			int num = 0;
			if (mod.manifest?.dependencies != null)
			{
				foreach (ModDependency declaredDependency in mod.manifest.dependencies)
				{
					Mod mod2 = list.FirstOrDefault((Mod m) => m.manifest?.id == declaredDependency.id);
					if (mod2 != null)
					{
						num++;
						if (!dictionary.TryGetValue(mod2, out var value))
						{
							value = (dictionary[mod2] = new List<Mod>());
						}
						value.Add(mod);
					}
				}
			}
			list3.Add(num);
		}
		PriorityQueue<Mod, int> priorityQueue = new PriorityQueue<Mod, int>();
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		for (int num2 = 0; num2 < manualOrdering.Count; num2++)
		{
			dictionary2[manualOrdering[num2].Id] = num2;
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			if (list3[num3] == 0)
			{
				int value2;
				int priority = (dictionary2.TryGetValue(list[num3].manifest.id, out value2) ? value2 : 999999999);
				priorityQueue.Enqueue(list[num3], priority);
			}
		}
		List<Mod> list5 = new List<Mod>();
		while (priorityQueue.Count > 0)
		{
			Mod mod3 = priorityQueue.Dequeue();
			list5.Add(mod3);
			if (!dictionary.TryGetValue(mod3, out var value3))
			{
				continue;
			}
			foreach (Mod item in value3)
			{
				int num4 = list.IndexOf(item);
				if (num4 < 0)
				{
					throw new InvalidOperationException("Bug in mod sorting logic!");
				}
				list3[num4]--;
				if (list3[num4] == 0)
				{
					int value4;
					int priority2 = (dictionary2.TryGetValue(item.manifest.id, out value4) ? value4 : 999999999);
					priorityQueue.Enqueue(item, priority2);
				}
			}
		}
		HashSet<Mod> hashSet = new HashSet<Mod>();
		foreach (Mod item2 in list5)
		{
			hashSet.Add(item2);
		}
		HashSet<Mod> sortedSet = hashSet;
		string value5 = string.Join(", ", from m in list
			where !sortedSet.Contains(m)
			select m.manifest?.id);
		foreach (Mod item3 in list)
		{
			if (!sortedSet.Contains(item3) && item3.manifest?.id != null)
			{
				_circularDependencies[item3.manifest.id] = value5;
			}
		}
		foreach (Mod item4 in list)
		{
			if (!sortedSet.Contains(item4))
			{
				list5.Add(item4);
			}
		}
		bool flag = manualOrdering.Count != list5.Count;
		if (!flag)
		{
			for (int num5 = 0; num5 < manualOrdering.Count; num5++)
			{
				if (manualOrdering[num5].Id != list5[num5].manifest?.id)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Log.Info("Mods have been re-sorted because we detected a change or dependency order was broken. New sorting order:");
			for (int num6 = 0; num6 < list5.Count; num6++)
			{
				Log.Info($"  {num6}: {list5[num6].manifest?.name} ({list5[num6].manifest?.id})");
			}
		}
		list5.AddRange(list2);
		_mods = list5;
	}

	/// <summary>
	/// If the user has subscribed to a mod via Steam workshop and also has it in their local directory, we never want
	/// to load the Steam mod. This is to help mod developers in development.
	/// </summary>
	private static void RemoveDisabledMods()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Mod mod in _mods)
		{
			if (mod.manifest?.id != null)
			{
				ModSettings? settings = _settings;
				if (settings != null && settings.IsModDisabled(mod.manifest.id, mod.modSource))
				{
					Log.Info("Skipping loading mod " + mod.manifest.id + ", it is set to disabled in settings");
					mod.state = ModLoadState.Disabled;
				}
				else if (mod.modSource == ModSource.ModsDirectory)
				{
					hashSet.Add(mod.manifest.id);
				}
			}
		}
		foreach (Mod mod2 in _mods)
		{
			if (mod2.manifest?.id != null && mod2.modSource == ModSource.SteamWorkshop && mod2.state == ModLoadState.None && hashSet.Contains(mod2.manifest.id))
			{
				Log.Warn("Mod with ID " + mod2.manifest.id + " is loaded both via Steam and local mods directory. Disabling the Steam workshop version.");
				mod2.state = ModLoadState.DisabledDuplicate;
				mod2.errors?.Clear();
			}
		}
	}

	/// <summary>
	/// Recurses through the directory and reads all the mod PCKs and DLLs in it, putting it in the _mods list.
	/// We allow mods in directories to give modders a little slack in how they organize their files.
	/// Pass the newMods list if you wish to get the mods that have been newly read.
	/// </summary>
	private static void ReadModsInDirRecursive(string path, ModSource source, List<Mod>? newMods)
	{
		string[] array = _fileIo?.GetFilesAt(path) ?? Array.Empty<string>();
		foreach (string text in array)
		{
			if (text.EndsWith(".json"))
			{
				string text2 = Path.Combine(path, text);
				Log.Info("Found mod manifest file " + text2);
				Mod mod = ReadModManifest(text2, source);
				if (mod != null)
				{
					_mods.Add(mod);
					newMods?.Add(mod);
				}
			}
		}
		string[] array2 = _fileIo?.GetDirectoriesAt(path) ?? Array.Empty<string>();
		foreach (string path2 in array2)
		{
			string path3 = Path.Combine(path, path2);
			if (_fileIo.DirectoryExists(path3))
			{
				ReadModsInDirRecursive(path3, source, newMods);
			}
		}
	}

	private static Mod? ReadModManifest(string filename, ModSource source)
	{
		if (_fileIo == null)
		{
			return null;
		}
		try
		{
			using Stream stream = _fileIo.OpenStream(filename, Godot.FileAccess.ModeFlags.Read);
			List<LocString> errors;
			ModManifest modManifest = ModManifest.ReadFromStream(stream, out errors);
			if (modManifest == null)
			{
				throw new InvalidOperationException("JSON deserialization returned null when trying to deserialize mod manifest!");
			}
			if (modManifest.id == null)
			{
				Log.Error("Mod manifest " + filename + " is missing the 'id' field! This is not allowed. The mod will not be loaded.");
				return null;
			}
			return new Mod
			{
				path = filename.GetBaseDir(),
				modSource = source,
				manifest = modManifest,
				errors = errors
			};
		}
		catch (Exception ex)
		{
			Log.Error($"Caught {ex.GetType()} trying to deserialize mod manifest json at path {filename}:\n{ex}");
			return null;
		}
	}

	public static (bool IsSupported, PlatformBranch? MaxSupportedBranch) EvaluateBranchSupport(PlatformBranch currentBranch, IReadOnlyList<(string MinBranch, string MaxBranch)> supportedVersions)
	{
		if (supportedVersions.Count == 0)
		{
			return (IsSupported: true, MaxSupportedBranch: null);
		}
		PlatformBranch? platformBranch = null;
		foreach (var supportedVersion in supportedVersions)
		{
			string item = supportedVersion.MinBranch;
			string item2 = supportedVersion.MaxBranch;
			bool flag = string.IsNullOrEmpty(item);
			bool flag2 = string.IsNullOrEmpty(item2);
			PlatformBranch? platformBranch2 = (flag ? ((PlatformBranch?)null) : PlatformBranchExtensions.FromName(item));
			PlatformBranch? platformBranch3 = (flag2 ? ((PlatformBranch?)null) : PlatformBranchExtensions.FromName(item2));
			PlatformBranch? platformBranch4 = (flag2 ? new PlatformBranch?(PlatformBranch.DevTest) : platformBranch3);
			if (platformBranch4.HasValue && (!platformBranch.HasValue || platformBranch4 > platformBranch))
			{
				platformBranch = platformBranch4;
			}
			bool flag3 = flag || (platformBranch2.HasValue && currentBranch >= platformBranch2);
			bool flag4 = flag2 || (platformBranch3.HasValue && currentBranch <= platformBranch3);
			if (flag3 && flag4)
			{
				return (IsSupported: true, MaxSupportedBranch: platformBranch);
			}
		}
		return (IsSupported: false, MaxSupportedBranch: platformBranch);
	}

	private static async Task ReadSteamMods()
	{
		uint subscribedItemCount = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] workshopItems = new PublishedFileId_t[subscribedItemCount];
		subscribedItemCount = SteamUGC.GetSubscribedItems(workshopItems, subscribedItemCount);
		for (int i = 0; i < subscribedItemCount; i++)
		{
			PublishedFileId_t workshopItemId = workshopItems[i];
			await TryReadModFromSteam(workshopItemId, null);
		}
		_steamItemInstalledCallback = Callback<ItemInstalled_t>.Create(OnSteamWorkshopItemInstalled);
	}

	private static async Task TryReadModFromSteam(PublishedFileId_t workshopItemId, List<Mod>? newMods)
	{
		if (!SteamUGC.GetItemInstallInfo(workshopItemId, out var punSizeOnDisk, out var pchFolder, 256u, out var punTimeStamp))
		{
			Log.Warn($"Could not get Steam Workshop item install info for item {workshopItemId.m_PublishedFileId}");
			return;
		}
		Log.Info($"Looking for mods to load from Steam Workshop mod {workshopItemId.m_PublishedFileId} in {pchFolder} (size {punSizeOnDisk}, last modified {punTimeStamp})");
		if (_fileIo != null && !_fileIo.DirectoryExists(pchFolder))
		{
			Log.Warn("Could not open Steam Workshop folder: " + pchFolder);
			return;
		}
		List<Mod> loadedMods = new List<Mod>();
		ReadModsInDirRecursive(pchFolder, ModSource.SteamWorkshop, loadedMods);
		await CheckSteamBranchSupport(workshopItemId, loadedMods);
		newMods?.AddRange(loadedMods);
	}

	private static async Task CheckSteamBranchSupport(PublishedFileId_t workshopItemId, List<Mod> mods)
	{
		UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { workshopItemId }, 1u);
		try
		{
			using SteamCallResult<SteamUGCQueryCompleted_t> callResult = new SteamCallResult<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(queryHandle), SteamInitializer.DisconnectToken);
			SteamUGCQueryCompleted_t steamUGCQueryCompleted_t = await callResult.Task;
			uint numSupportedGameVersions = SteamUGC.GetNumSupportedGameVersions(steamUGCQueryCompleted_t.m_handle, 0u);
			List<(string, string)> list = new List<(string, string)>();
			for (int i = 0; i < numSupportedGameVersions; i++)
			{
				if (SteamUGC.GetSupportedGameVersionData(steamUGCQueryCompleted_t.m_handle, 0u, (uint)i, out var pchGameBranchMin, out var pchGameBranchMax, 999u))
				{
					list.Add((pchGameBranchMin, pchGameBranchMax));
				}
			}
			if (numSupportedGameVersions != 0 && list.Count == 0)
			{
				Log.Warn($"Steam reported {numSupportedGameVersions} supported game version range(s) for mod {workshopItemId.m_PublishedFileId} but none could be read; loading it without the branch check.");
			}
			PlatformBranch platformBranch = PlatformUtil.GetPlatformBranch();
			var (flag, platformBranch2) = EvaluateBranchSupport(platformBranch, list);
			if (flag)
			{
				return;
			}
			foreach (Mod mod2 in mods)
			{
				LocString locString = new LocString("main_menu_ui", "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED");
				locString.Add("id", mod2.manifest?.id ?? "<null>");
				locString.Add("currentBranch", platformBranch.ToName());
				locString.Add("supportedBranch", platformBranch2?.ToName() ?? "<null>");
				Mod mod = mod2;
				if (mod.errors == null)
				{
					mod.errors = new List<LocString>();
				}
				mod2.errors.Add(locString);
				Log.Error($"Tried to load mod with id {mod2.manifest?.id}, but the current Steam branch {platformBranch.ToName()} does not lie in the min/max steam branches the mod supports! Max supported: {platformBranch2?.ToName()}");
			}
		}
		catch (Exception value)
		{
			Log.Warn($"Could not verify Steam branch support for mod {workshopItemId.m_PublishedFileId}; loading it without the check. {value}");
		}
		finally
		{
			SteamUGC.ReleaseQueryUGCRequest(queryHandle);
		}
	}

	private static void OnSteamWorkshopItemInstalled(ItemInstalled_t ev)
	{
		TaskHelper.RunSafely(OnSteamWorkshopItemInstalledAsync(ev));
	}

	private static async Task OnSteamWorkshopItemInstalledAsync(ItemInstalled_t ev)
	{
		if ((ulong)ev.m_unAppID.m_AppId != 2868840)
		{
			return;
		}
		Log.Info($"Detected new Steam Workshop item installation, id: {ev.m_nPublishedFileId.m_PublishedFileId}");
		List<Mod> loadedMods = new List<Mod>();
		await TryReadModFromSteam(ev.m_nPublishedFileId, loadedMods);
		foreach (Mod item in loadedMods)
		{
			item.state = ModLoadState.AddedAtRuntime;
			ModManager.OnModDetected?.Invoke(item);
		}
	}

	private static void TryLoadMod(Mod mod)
	{
		if (mod.state != ModLoadState.None)
		{
			ModManager.OnModDetected?.Invoke(mod);
			return;
		}
		Assembly assembly = null;
		List<LocString> list = mod.errors ?? new List<LocString>();
		if (mod.manifest == null)
		{
			throw new InvalidOperationException("Tried to load mod before its manifest was loaded!");
		}
		SemanticVersion version;
		if (mod.manifest.version == null)
		{
			Log.Warn("Mod " + mod.manifest.id + " does not declare a version");
		}
		else if (!SemanticVersion.TryFromString(mod.manifest.version, out version))
		{
			Log.Warn($"Mod {mod.manifest.id} declares version {mod.manifest.version} which is not a valid Semantic Version");
		}
		else
		{
			mod.version = version;
		}
		string modId = mod.manifest.id;
		bool flag = _settings?.IsModDisabled(modId, mod.modSource) ?? false;
		bool flag2 = _mods.Any((Mod m) => m.manifest?.id == modId && m.state == ModLoadState.Loaded);
		bool flag3 = false;
		bool flag4 = true;
		if (_gameVersion != null)
		{
			SemanticVersion version2;
			if (mod.manifest.minGameVersion == null)
			{
				Log.Warn("Mod " + mod.manifest.id + " does not declare min game version. Assuming that it is supported.");
			}
			else if (!SemanticVersion.TryFromString(mod.manifest.minGameVersion, out version2))
			{
				flag3 = true;
			}
			else
			{
				flag4 = _gameVersion.CompareTo(version2) >= 0;
			}
		}
		string value;
		if (State != ModManagerState.None)
		{
			Log.Info("Skipping loading mod " + modId + ", can't load mods at runtime");
			mod.state = ModLoadState.AddedAtRuntime;
		}
		else if (!PlayerAgreedToModLoading)
		{
			Log.Info("Skipping loading mod " + modId + ", user has not yet seen the mods warning");
			mod.state = ModLoadState.Disabled;
		}
		else if (flag2)
		{
			LocString locString = new LocString("main_menu_ui", "MOD_ERROR.DUPLICATE_ID");
			locString.Add("id", modId);
			list.Add(locString);
			Log.Error("Tried to load mod with id " + modId + ", but a mod is already loaded with that name!");
			mod.state = ModLoadState.Failed;
		}
		else if (_circularDependencies.TryGetValue(modId, out value))
		{
			LocString locString2 = new LocString("main_menu_ui", "MOD_ERROR.CIRCULAR_DEPENDENCY");
			locString2.Add("id", modId);
			locString2.Add("dependencyChain", value);
			list.Add(locString2);
			Log.Error($"Tried to load mod with id {modId}, but it is part of a circular dependency chain: {value}!");
			mod.state = ModLoadState.Failed;
		}
		else if (flag3)
		{
			LocString locString3 = new LocString("main_menu_ui", "MOD_ERROR.GAME_VERSION_INVALID");
			locString3.Add("id", modId);
			locString3.Add("minGameVersion", mod.manifest.minGameVersion ?? "<null>");
			list.Add(locString3);
			Log.Error($"Mod {mod.manifest.id} declares min game version {mod.manifest.minGameVersion} that can't be parsed! Assuming it is supported");
			mod.state = ModLoadState.Failed;
		}
		else if (!flag4)
		{
			LocString locString4 = new LocString("main_menu_ui", "MOD_ERROR.GAME_VERSION_UNSUPPORTED");
			locString4.Add("id", modId);
			locString4.Add("minGameVersion", mod.manifest.minGameVersion ?? "<null>");
			locString4.Add("gameVersion", _gameVersion?.ToString() ?? "<null>");
			list.Add(locString4);
			Log.Error($"Tried to load mod with id {modId}, but its declared min game version {mod.manifest.minGameVersion} is higher than the current game version {_gameVersion}");
			mod.state = ModLoadState.Failed;
		}
		else
		{
			List<string> list2 = new List<string>();
			if (mod.manifest.dependencies != null)
			{
				foreach (ModDependency declaredDependency in mod.manifest.dependencies)
				{
					Mod mod2 = _mods.FirstOrDefault((Mod m) => m.manifest?.id == declaredDependency.id);
					if (mod2 == null || mod2.state != ModLoadState.Loaded)
					{
						list2.Add(declaredDependency.id);
					}
					else if (declaredDependency.minVersion != null)
					{
						if (!SemanticVersion.TryFromString(declaredDependency.minVersion, out SemanticVersion version3))
						{
							LocString locString5 = new LocString("main_menu_ui", "MOD_ERROR.DEPENDENCY_MIN_VERSION_INVALID");
							locString5.Add("id", mod.manifest.id);
							locString5.Add("dependency", declaredDependency.id);
							locString5.Add("minVersion", declaredDependency.minVersion);
							list.Add(locString5);
							Log.Error($"Mod {modId} which depends on {declaredDependency.id} with min version {declaredDependency.minVersion} which cannot be parsed");
							mod.state = ModLoadState.Failed;
						}
						else if (mod2.manifest?.version == null)
						{
							LocString locString6 = new LocString("main_menu_ui", "MOD_ERROR.DEPENDENCY_VERSION_MISSING");
							locString6.Add("id", mod.manifest.id);
							locString6.Add("dependency", declaredDependency.id);
							locString6.Add("minVersion", declaredDependency.minVersion);
							list.Add(locString6);
							Log.Error($"Tried to load mod {modId} which depends on {declaredDependency.id} with min version {declaredDependency.minVersion}, but the mod declares no version!");
							mod.state = ModLoadState.Failed;
						}
						else if (mod2.version == null)
						{
							LocString locString7 = new LocString("main_menu_ui", "MOD_ERROR.DEPENDENCY_VERSION_INVALID");
							locString7.Add("id", mod.manifest.id);
							locString7.Add("dependency", declaredDependency.id);
							locString7.Add("minVersion", declaredDependency.minVersion);
							locString7.Add("version", mod2.manifest.version);
							list.Add(locString7);
							Log.Error($"Tried to load mod {modId} which depends on {declaredDependency.id} with min version {declaredDependency.minVersion}, but the mod declares version {mod2.manifest.version} which cannot be parsed!");
							mod.state = ModLoadState.Failed;
						}
						else if (mod2.version.CompareTo(version3) < 0)
						{
							LocString locString8 = new LocString("main_menu_ui", "MOD_ERROR.DEPENDENCY_VERSION_UNSUPPORTED");
							locString8.Add("id", mod.manifest.id);
							locString8.Add("dependency", declaredDependency.id);
							locString8.Add("minVersion", declaredDependency.minVersion);
							locString8.Add("version", mod2.manifest.version);
							list.Add(locString8);
							Log.Error($"Tried to load mod {modId} which depends on {declaredDependency.id} with min version {declaredDependency.minVersion} but you have {mod2.version}!");
							mod.state = ModLoadState.Failed;
						}
					}
				}
				if (list2.Count > 0)
				{
					string text = string.Join(",", list2);
					LocString locString9 = new LocString("main_menu_ui", "MOD_ERROR.MISSING_DEPENDENCY");
					locString9.Add("id", mod.manifest.id);
					locString9.Add("missingCount", list2.Count);
					locString9.Add("missingDependencies", text);
					list.Add(locString9);
					Log.Error($"Tried to load mod {modId}, but it depends on mods which have not been loaded: {text}!");
					mod.state = ModLoadState.Failed;
				}
			}
		}
		if (mod.state != ModLoadState.None)
		{
			mod.errors = ((list.Count == 0) ? null : list);
			ModManager.OnModDetected?.Invoke(mod);
			return;
		}
		try
		{
			bool flag5 = false;
			string text2 = Path.Combine(mod.path, modId + ".dll");
			if (mod.manifest.hasDll)
			{
				if (_fileIo != null && _fileIo.FileExists(text2))
				{
					Log.Info("Loading assembly DLL " + text2);
					AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
					if (loadContext != null)
					{
						assembly = loadContext.LoadFromAssemblyPath(text2);
						flag5 = true;
					}
				}
				else
				{
					Log.Error($"Mod manifest for mod {mod.manifest.id} declares that it should load an assembly, but no assembly at path {text2} was found!");
				}
			}
			string text3 = Path.Combine(mod.path, modId + ".pck");
			if (mod.manifest.hasPck)
			{
				if (_fileIo != null && _fileIo.FileExists(text3))
				{
					Log.Info("Loading Godot PCK " + text3);
					if (!ProjectSettings.LoadResourcePack(text3))
					{
						throw new InvalidOperationException("Godot errored while loading PCK file " + modId + "!");
					}
					flag5 = true;
				}
				else
				{
					Log.Error($"Mod manifest for mod {mod.manifest.id} declares that it should load a PCK, but no PCK at path {text3} was found!");
				}
			}
			if (!flag5)
			{
				Log.Warn("Neither a DLL nor a PCK was loaded for mod " + mod.manifest.id + ", something seems wrong!");
			}
			bool? flag6 = null;
			if (assembly != null)
			{
				flag6 = true;
				List<Type> list3 = (from t in assembly.GetTypes()
					where t.GetCustomAttribute<ModInitializerAttribute>() != null
					select t).ToList();
				if (list3.Count > 0)
				{
					foreach (Type item in list3)
					{
						Log.Info($"Calling initializer method of type {item} for {assembly}");
						bool flag7 = CallModInitializer(item);
						flag6 = flag6.Value && flag7;
					}
				}
				else
				{
					try
					{
						Log.Info($"No ModInitializerAttribute detected. Calling Harmony.PatchAll for {assembly}");
						Harmony harmony = new Harmony((mod.manifest.author ?? "unknown") + "." + modId);
						harmony.PatchAll(assembly);
					}
					catch (Exception value2)
					{
						Log.Error($"Exception caught while trying to run PatchAll on assembly {assembly}:\n{value2}");
						flag6 = false;
					}
				}
			}
			if (flag6 == false)
			{
				LocString locString10 = new LocString("main_menu_ui", "MOD_ERROR.ASSEMBLY_LOAD");
				locString10.Add("id", mod.manifest.id);
				list.Add(locString10);
			}
			Log.Info($"Finished mod initialization for '{mod.manifest.name}' ({modId}).");
			mod.state = ModLoadState.Loaded;
			mod.assembly = assembly;
			mod.errors = ((list.Count == 0) ? null : list);
			ModManager.OnModDetected?.Invoke(mod);
		}
		catch (Exception ex)
		{
			Log.Error($"Exception thrown while loading mod {modId}: {ex}");
			LocString locString11 = new LocString("main_menu_ui", "MOD_ERROR.EXCEPTION");
			locString11.Add("exceptionType", ex.GetType().ToString());
			locString11.Add("id", mod.manifest.id);
			list.Add(locString11);
			mod.state = ModLoadState.Failed;
			mod.assembly = assembly;
			mod.errors = ((list.Count == 0) ? null : list);
			ModManager.OnModDetected?.Invoke(mod);
		}
	}

	/// <summary>
	/// Calls the mod initializer on a type that has the ModInitializerAttribute on it.
	/// </summary>
	private static bool CallModInitializer(Type initializerType)
	{
		ModInitializerAttribute customAttribute = initializerType.GetCustomAttribute<ModInitializerAttribute>();
		MethodInfo method = initializerType.GetMethod(customAttribute.initializerMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			method = initializerType.GetMethod(customAttribute.initializerMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				Log.Error($"Tried to call mod initializer {initializerType.Name}.{customAttribute.initializerMethod} but it's not static! Declare it to be static");
			}
			else
			{
				Log.Error($"Found mod initializer class of type {initializerType}, but it does not contain the method {customAttribute.initializerMethod} declared in the ModInitializerAttribute!");
			}
			return false;
		}
		try
		{
			method.Invoke(null, null);
		}
		catch (Exception value)
		{
			Log.Error($"Exception thrown when calling mod initializer of type {initializerType}: {value}");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns the filenames of all the loc tables available in loaded mods for the given language and filename.
	/// For example, if "eng" and "cards.json" are provided, this returns all mods that supply a cards.json in english.
	/// </summary>
	public static IEnumerable<string> GetModdedLocTables(string language, string file)
	{
		foreach (Mod mod in _mods)
		{
			if (mod.state == ModLoadState.Loaded)
			{
				string text = $"res://{mod.manifest.id}/localization/{language}/{file}";
				if (ResourceLoader.Exists(text))
				{
					yield return text;
				}
			}
		}
	}

	/// <summary>
	/// Returns a list of the names of all loaded mods that affect gameplay.
	/// For multiplayer - used to compare mods between different players.
	/// </summary>
	public static List<string>? GetGameplayRelevantModNameList()
	{
		if (!IsRunningModded())
		{
			return null;
		}
		return (from m in GetLoadedMods()
			where m.manifest?.affectsGameplay ?? true
			select m.manifest?.id + "-" + m.manifest?.version).ToList();
	}

	/// <summary>
	/// Returns a list of the names of all loaded mods that do not affect gameplay.
	/// </summary>
	public static List<string>? GetNonGameplayRelevantModNameList()
	{
		if (!IsRunningModded())
		{
			return null;
		}
		return (from m in GetLoadedMods()
			where !(m.manifest?.affectsGameplay ?? true)
			select m.manifest?.id + "-" + m.manifest?.version).ToList();
	}

	/// <summary>
	/// Resolves assemblies loaded by mods that may have a different version.
	/// When writing a mod DLL, the implementer may target a STS2 DLL that does not match the current version. When the
	/// dotnet runtime attempts to load the DLL, it tries to strictly match the version. Usually, we don't really need
	/// it to - a lot of our APIs don't change that often. Attaching this method to the AssemblyResolve event allows us
	/// to force dotnet to resolve the assembly to the correct one.
	/// </summary>
	private static Assembly HandleAssemblyResolveFailure(object? source, ResolveEventArgs ev)
	{
		if (ev.Name.StartsWith("sts2,"))
		{
			Log.Info($"Failed to resolve assembly '{ev.Name}' but it looks like the STS2 assembly. Resolving using {Assembly.GetExecutingAssembly()}");
			return Assembly.GetExecutingAssembly();
		}
		if (ev.Name.StartsWith("0Harmony,"))
		{
			Log.Info($"Failed to resolve assembly '{ev.Name}' but it looks like the Harmony assembly. Resolving using {typeof(Harmony).Assembly}");
			return typeof(Harmony).Assembly;
		}
		return null;
	}

	/// <summary>
	/// Called by the metrics uploader when metrics would be uploaded, but are not because there are mods present.
	/// </summary>
	public static void CallMetricsHooks(SerializableRun run, bool isVictory, ulong localPlayerId)
	{
		ModManager.OnMetricsUpload?.Invoke(run, isVictory, localPlayerId);
	}

	public static bool IsRunningModded()
	{
		return _mods.Any(delegate(Mod m)
		{
			ModLoadState state = m.state;
			return (uint)(state - 1) <= 1u;
		});
	}

	public static bool HasHarmonyPatches()
	{
		try
		{
			bool valueOrDefault = _hasHarmonyPatches == true;
			if (!_hasHarmonyPatches.HasValue)
			{
				valueOrDefault = Harmony.GetAllPatchedMethods().Any();
				_hasHarmonyPatches = valueOrDefault;
			}
		}
		catch
		{
			_hasHarmonyPatches = true;
		}
		return _hasHarmonyPatches.Value;
	}

	public static IEnumerable<Mod> GetLoadedMods()
	{
		return _mods.Where((Mod m) => m.state == ModLoadState.Loaded);
	}

	public static void Dispose()
	{
		_steamItemInstalledCallback?.Dispose();
	}
}
