using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Assets;

/// <summary>
/// This class manages the caching and preloading of assets for the game.
/// </summary>
public static class PreloadManager
{
	public static AssetCache Cache { get; } = new AssetCache();

	/// <summary>
	/// Set this to false to disable preloading of assets. This can speed up iteration times.
	/// Assets will still be unloaded when we think they are out of use.
	/// </summary>
	public static bool Enabled { get; set; } = true;

	/// <summary>
	/// This method loads only the logo animation. The logo animation will be unloaded as soon as LoadMainMenuAssets
	/// is called, which is what we want.
	/// The assets are loaded synchronously.
	/// </summary>
	public static async Task LoadLogoAnimation()
	{
		await (await LoadAssetSets("IntroLogo", AssetSets.IntroLogoAssets)).WaitForCompletion();
	}

	/// <summary>
	/// Loads only the essential assets needed to display the main menu.
	/// Call this while the logo animation is playing for faster main menu display.
	/// </summary>
	public static async Task LoadMainMenuEssentials()
	{
		if (!TestMode.IsOn)
		{
			await (await LoadAssetSets("MainMenuEssentials", AssetSets.MainMenuEssentials)).WaitForCompletion();
		}
	}

	/// <summary>
	/// Loads all common and main menu assets including compendium screens, character select, etc.
	/// Call this in background after main menu is displayed.
	/// </summary>
	public static async Task LoadCommonAndMainMenuAssets()
	{
		Cache.UnloadMissedCacheAssets();
		await LoadAssetSets("Common", AssetSets.CommonAssets, AssetSets.MainMenuSet);
	}

	/// <summary>
	/// This method is only used when starting the game, it loads the main menu assets. Otherwise, we use the
	/// LoadCommonAndMainMenuAssets method.
	/// </summary>
	public static async Task LoadMainMenuAssets()
	{
		if (!TestMode.IsOn)
		{
			await (await LoadAssetSets("MainMenu", AssetSets.MainMenuSet)).WaitForCompletion();
		}
	}

	public static async Task LoadRunAssets(IEnumerable<CharacterModel> characters)
	{
		if (!TestMode.IsOn)
		{
			List<CharacterModel> list = characters.ToList();
			bool isMultiplayer = RunManager.Instance.NetService.Type.IsMultiplayer();
			AssetSets.RunSet = new HashSet<string>(GetRunAssetPaths(list, isMultiplayer));
			await (await LoadAssetSets("characters=" + string.Join(',', list.Select((CharacterModel c) => c.Id.Entry)), AssetSets.CommonAssets, AssetSets.RunSet)).WaitForCompletion();
			GC.Collect();
		}
	}

	public static async Task LoadActAssets(ActModel act)
	{
		if (!TestMode.IsOn)
		{
			AssetSets.Act = new HashSet<string>(act.AssetPaths);
			await (await LoadAssetSets("Act=" + act.Id.Entry, AssetSets.CommonAssets, AssetSets.RunSet, AssetSets.Act)).WaitForCompletion();
			GC.Collect();
		}
	}

	public static async Task LoadRoomEventAssets(EventModel eventModel, IRunState runState)
	{
		await LoadRoomAssets("Event Room", eventModel.GetAssetPaths(runState));
	}

	public static async Task LoadRoomCombatAssets(EncounterModel encounter, IRunState runState)
	{
		await LoadRoomAssets("Combat Room", GetCombatAssetPaths(encounter, runState));
	}

	public static async Task LoadRoomTreasureAssets(ActModel actModel)
	{
		List<string> list = new List<string>();
		list.Add(actModel.ChestSpineResourcePath);
		list.AddRange(NTreasureRoom.AssetPaths);
		await LoadRoomAssets("Treasure Room", new _003C_003Ez__ReadOnlyList<string>(list));
	}

	public static async Task LoadRoomMerchantAssets()
	{
		await LoadRoomAssets("Merchant Room", NMerchantRoom.AssetPaths);
	}

	public static async Task LoadRoomRestSite(ActModel actModel, IEnumerable<RestSiteOption> restSiteOptions)
	{
		List<string> list = new List<string>();
		list.Add(actModel.RestSiteBackgroundPath);
		list.AddRange(restSiteOptions.SelectMany((RestSiteOption s) => s.AssetPaths));
		await LoadRoomAssets("RestSite Room", new _003C_003Ez__ReadOnlyList<string>(list));
	}

	private static async Task LoadRoomAssets(string roomName, IEnumerable<string> additionalAssets)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		Cache.UnloadMissedCacheAssets();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string additionalAsset in additionalAssets)
		{
			hashSet.Add(additionalAsset);
		}
		HashSet<string> hashSet2 = hashSet;
		await (await LoadAssetSets(roomName, AssetSets.CommonAssets, AssetSets.RunSet, AssetSets.Act, hashSet2)).WaitForCompletion();
		GC.Collect();
	}

	/// <summary>
	/// This method is the magic for preloading assets. It unloads assets that are no longer needed and loads assets
	/// that are needed.
	/// It does this through a basic set subtraction algorithm to determine what assets are not needed and can be
	/// unloaded and what assets are needed and not yet loaded (this avoids loading assets that already exist in memory).
	/// </summary>
	private static async Task<AssetLoadingSession> LoadAssetSets(string name, params IEnumerable<string>[] assetSets)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in assetSets.SelectMany((IEnumerable<string> set) => set))
		{
			hashSet.Add(item);
		}
		HashSet<string> hashSet2 = hashSet;
		IReadOnlySet<string> loadedCacheAssets = Cache.GetLoadedCacheAssets();
		IEnumerable<string> assetsToUnloadSet = loadedCacheAssets.Except(hashSet2);
		IEnumerable<string> needLoaded = hashSet2.Except(loadedCacheAssets);
		Cache.UnloadAssets(assetsToUnloadSet);
		await Task.Yield();
		if (!Enabled)
		{
			return AssetLoadingSession.Empty();
		}
		return LoadAssets(needLoaded, name);
	}

	private static AssetLoadingSession LoadAssets(IEnumerable<string> assetPaths, string name)
	{
		AssetLoadingSession assetLoadingSession = Cache.CreateSession(name, assetPaths);
		NAssetLoader.Instance.LoadInTheBackground(assetLoadingSession);
		return assetLoadingSession;
	}

	private static IEnumerable<string> GetRunAssetPaths(IEnumerable<CharacterModel> characters, bool isMultiplayer)
	{
		IEnumerable<CharacterModel> source = characters.ToList();
		IEnumerable<CardPoolModel> allSharedCardPools = ModelDb.AllSharedCardPools;
		IEnumerable<CardModel> source2 = allSharedCardPools.SelectMany((CardPoolModel pool) => pool.AllCards);
		if (!isMultiplayer)
		{
			source2 = source2.Where((CardModel card) => card.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);
		}
		return new IEnumerable<string>[10]
		{
			source2.SelectMany((CardModel card) => card.RunAssetPaths),
			source.SelectMany((CharacterModel c) => c.CardPool.AllCards.SelectMany((CardModel card) => card.RunAssetPaths)),
			source.SelectMany((CharacterModel c) => c.AssetPaths),
			NCard.AssetPaths,
			NMapRoom.AssetPaths,
			NChooseACardSelectionScreen.AssetPaths,
			NGameOverScreen.AssetPaths,
			NRelicInventoryHolder.AssetPaths,
			from e in ModelDb.DebugEnchantments
				select e.IconPath into p
				where p != EnchantmentModel.MissingIconPath
				select p,
			ModelDb.AllCardPools.Select((CardPoolModel p) => p.EnergyIconPath)
		}.SelectMany((IEnumerable<string> s) => s);
	}

	private static IEnumerable<string> GetCombatAssetPaths(EncounterModel encounter, IRunState runState)
	{
		if (TestMode.IsOn)
		{
			return Array.Empty<string>();
		}
		return new IEnumerable<string>[2]
		{
			NCombatRoom.AssetPaths,
			encounter.GetAssetPaths(runState)
		}.SelectMany((IEnumerable<string> s) => s);
	}
}
