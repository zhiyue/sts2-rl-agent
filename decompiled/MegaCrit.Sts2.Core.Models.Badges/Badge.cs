using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Badges;

/// <summary>
/// Data class for Badges.
/// This is the actual backing class with the data for the badge. BadgeModel only exists to listen to combat events.
/// </summary>
public abstract class Badge
{
	protected readonly SerializableRun _run;

	protected readonly bool _won;

	protected readonly SerializablePlayer _localPlayer;

	public string Id { get; }

	public abstract BadgeRarity Rarity { get; }

	/// <summary>
	/// By default, a badge requires you to win to obtain it.
	/// </summary>
	public bool RequiresWin { get; }

	public bool MultiplayerOnly { get; }

	/// <summary>
	/// Given a Badge's Rarity, returns the appropriate asset that's used to hold the badge icon.
	/// </summary>
	public Texture2D BadgeBase => Rarity switch
	{
		BadgeRarity.Bronze => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_bronze.png")), 
		BadgeRarity.Silver => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_silver.png")), 
		BadgeRarity.Gold => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("ui/game_over_screen/badge_gold.png")), 
		_ => PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("atlases/power_atlas.sprites/missing_power.tres")), 
	};

	public Texture2D BadgeIcon
	{
		get
		{
			if (ResourceLoader.Exists(IconPath))
			{
				return PreloadManager.Cache.GetTexture2D(IconPath);
			}
			Log.Error("Badge Icon: " + IconPath + " doesn't exist :(");
			return PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("debug/placeholder_64.png"));
		}
	}

	private string IconPath => ImageHelper.GetImagePath("ui/game_over_screen/badge_" + Id.ToLowerInvariant() + ".png");

	protected Badge(SerializableRun run, bool won, ulong playerId, string id, bool requiresWin, bool multiplayerOnly)
	{
		_run = run;
		_won = won;
		_localPlayer = _run.Players.First((SerializablePlayer p) => p.NetId == playerId);
		Id = id;
		RequiresWin = requiresWin;
		MultiplayerOnly = multiplayerOnly;
	}

	public abstract bool IsObtained();

	public SerializableBadge ToSerializable()
	{
		return new SerializableBadge
		{
			Id = Id,
			Rarity = Rarity
		};
	}
}
