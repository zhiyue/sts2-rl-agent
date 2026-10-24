using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

/// <summary>
/// Base config class for rest site button options.
/// </summary>
public abstract class RestSiteOption
{
	public abstract string OptionId { get; }

	protected Player Owner { get; }

	public LocString Title => new LocString("rest_site_ui", "OPTION_" + OptionId + ".name");

	public virtual LocString Description => new LocString("rest_site_ui", "OPTION_" + OptionId + ".description");

	private string IconPath => ImageHelper.GetImagePath("ui/rest_site/option_" + OptionId.ToLowerInvariant() + ".png");

	public Texture2D Icon => PreloadManager.Cache.GetTexture2D(IconPath);

	public virtual IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(IconPath);

	/// <summary>
	/// Whether this option is usable or not. You can still hover it for info.
	/// </summary>
	public virtual bool IsEnabled => true;

	protected RestSiteOption(Player owner)
	{
		Owner = owner;
	}

	/// <summary>
	/// Generates a list of rest site options to use for the next rest site.
	/// This list will include extra options added by models like the Shovel relic if the player has them.
	/// Calling this may increment RNG counters and make other run state changes.
	/// </summary>
	/// <returns>List of rest site options.</returns>
	public static List<RestSiteOption> Generate(Player player)
	{
		int num = 2;
		List<RestSiteOption> list = new List<RestSiteOption>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<RestSiteOption> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new HealRestSiteOption(player);
		num2++;
		span[num2] = new SmithRestSiteOption(player);
		List<RestSiteOption> list2 = list;
		if (player.RunState.Players.Count > 1)
		{
			list2.Add(new MendRestSiteOption(player));
		}
		Hook.ModifyRestSiteOptions(player.RunState, player, list2);
		return list2;
	}

	/// <summary>
	/// Logic to run when this option is selected.
	/// </summary>
	/// <returns>
	/// Whether or not the option was "successful". Usually true, but false in certain cases (like if Smith is chosen
	/// and no upgradable cards are available).
	/// </returns>
	public abstract Task<bool> OnSelect();

	/// <summary>
	/// Runs only for the owning player after the option is selected.
	/// </summary>
	public virtual Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Runs only for non-owning players after the option is selected.
	/// </summary>
	public virtual Task DoRemotePostSelectVfx()
	{
		return Task.CompletedTask;
	}

	public override bool Equals(object? obj)
	{
		if (obj is RestSiteOption restSiteOption && OptionId == restSiteOption.OptionId)
		{
			return Owner == restSiteOption.Owner;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (OptionId, Owner).GetHashCode();
	}

	public static bool operator ==(RestSiteOption? left, RestSiteOption? right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null)
		{
			if ((object)left == null)
			{
				return (object)right == null;
			}
			return false;
		}
		return left.Equals(right);
	}

	public static bool operator !=(RestSiteOption? left, RestSiteOption? right)
	{
		return !(left == right);
	}
}
