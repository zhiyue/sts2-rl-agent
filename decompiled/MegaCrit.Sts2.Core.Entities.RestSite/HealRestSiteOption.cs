using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public sealed class HealRestSiteOption : RestSiteOption
{
	public override string OptionId => "HEAL";

	public override IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.AddRange(base.AssetPaths);
			list.AddRange(NRestSmokeVfx.AssetPaths);
			list.AddRange(NDesaturateTransitionVfx.AssetPaths);
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			HealVar dynamicVar = new HealVar(GetBaseHealAmount(base.Owner.Creature))
			{
				PreviewValue = GetHealAmount(base.Owner)
			};
			description.Add("Character", base.Owner.Character.Id.Entry);
			description.Add(dynamicVar);
			IReadOnlyList<LocString> source = Hook.ModifyExtraRestSiteHealText(base.Owner.RunState, base.Owner, Array.Empty<LocString>());
			if (source.Any())
			{
				description.Add("ExtraText", "\n" + string.Join("\n", source.Select((LocString s) => s.GetFormattedText())));
			}
			else
			{
				description.Add("ExtraText", string.Empty);
			}
			return description;
		}
	}

	public static decimal GetHealAmount(Player player)
	{
		return Hook.ModifyRestSiteHealAmount(player.RunState, player.Creature, GetBaseHealAmount(player.Creature));
	}

	public HealRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		await ExecuteRestSiteHeal(base.Owner, isMimicked: false);
		return true;
	}

	public override async Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		PlayRestSiteHealSfx();
		NRestSiteRoom.Instance?.AddChildSafely(NRestSmokeVfx.Create());
		NRestSiteRoom.Instance?.AddChildSafely(NDesaturateTransitionVfx.Create());
		await Cmd.CustomScaledWait(1.5f, 2.5f, ignoreCombatEnd: false, ct);
	}

	public override Task DoRemotePostSelectVfx()
	{
		NDebugAudioManager.Instance?.Play("SOTE_SFX_SleepBlanket_v1.mp3", 0.5f, PitchVariance.Small);
		return Task.CompletedTask;
	}

	public static decimal GetBaseHealAmount(Creature creature)
	{
		return (decimal)creature.MaxHp * 0.3m;
	}

	public static void PlayRestSiteHealSfx()
	{
		NDebugAudioManager.Instance?.Play("sleep.tres");
		NDebugAudioManager.Instance?.Play("SOTE_SFX_SleepBlanket_v1.mp3", 1f, PitchVariance.Small);
	}

	public static async Task ExecuteRestSiteHeal(Player player, bool isMimicked)
	{
		await CreatureCmd.Heal(player.Creature, GetHealAmount(player));
		await Hook.AfterRestSiteHeal(player.RunState, player, isMimicked);
		List<Reward> rewards = new List<Reward>();
		Hook.ModifyRestSiteHealRewards(player.RunState, player, rewards, isMimicked);
		await RewardsCmd.OfferCustom(player, rewards);
	}
}
