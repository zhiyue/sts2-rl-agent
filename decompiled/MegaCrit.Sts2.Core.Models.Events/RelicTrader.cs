using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class RelicTrader : EventModel
{
	private const string _topRelicOwnedKey = "TopRelicOwned";

	private const string _topRelicNewKey = "TopRelicNew";

	private const string _middleRelicOwnedKey = "MiddleRelicOwned";

	private const string _middleRelicNewKey = "MiddleRelicNew";

	private const string _bottomRelicOwnedKey = "BottomRelicOwned";

	private const string _bottomRelicNewKey = "BottomRelicNew";

	private IReadOnlyList<RelicModel>? _ownedRelics;

	private IReadOnlyList<RelicModel>? _newRelics;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[6]
	{
		new StringVar("TopRelicOwned"),
		new StringVar("TopRelicNew"),
		new StringVar("MiddleRelicOwned"),
		new StringVar("MiddleRelicNew"),
		new StringVar("BottomRelicOwned"),
		new StringVar("BottomRelicNew")
	});

	private IReadOnlyList<RelicModel> OwnedRelics
	{
		get
		{
			AssertMutable();
			if (_ownedRelics == null)
			{
				_ownedRelics = GetValidRelics(base.Owner).ToList().StableShuffle(base.Rng).Take(3)
					.ToList();
			}
			return _ownedRelics;
		}
	}

	private IReadOnlyList<RelicModel> NewRelics
	{
		get
		{
			AssertMutable();
			if (_newRelics == null)
			{
				RelicModel[] array = new RelicModel[3];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = RelicFactory.PullNextRelicFromFront(base.Owner);
				}
				_newRelics = array;
			}
			return _newRelics;
		}
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = new List<EventOption>();
		if (OwnedRelics.Count >= 1)
		{
			list.Add(new EventOption(this, Top, "RELIC_TRADER.pages.INITIAL.options.TOP", GetRelicHoverTips(0)));
		}
		if (OwnedRelics.Count >= 2)
		{
			list.Add(new EventOption(this, Middle, "RELIC_TRADER.pages.INITIAL.options.MIDDLE", GetRelicHoverTips(1)));
		}
		if (OwnedRelics.Count >= 3)
		{
			list.Add(new EventOption(this, Bottom, "RELIC_TRADER.pages.INITIAL.options.BOTTOM", GetRelicHoverTips(2)));
		}
		if (list.Count == 0)
		{
			list.Add(new EventOption(this, Done, "PROCEED"));
		}
		return list;
	}

	public override bool IsAllowed(RunState runState)
	{
		if (runState.CurrentActIndex == 0)
		{
			return false;
		}
		return runState.Players.All((Player p) => GetValidRelics(p).Count() >= 5);
	}

	private IEnumerable<RelicModel> GetValidRelics(Player player)
	{
		return player.Relics.Where((RelicModel r) => r.IsTradable);
	}

	public override void CalculateVars()
	{
		if (OwnedRelics.Count > 0 && NewRelics.Count > 0)
		{
			((StringVar)base.DynamicVars["TopRelicOwned"]).StringValue = OwnedRelics[0].Title.GetFormattedText();
			((StringVar)base.DynamicVars["TopRelicNew"]).StringValue = NewRelics[0].Title.GetFormattedText();
		}
		if (OwnedRelics.Count > 1 && NewRelics.Count > 1)
		{
			((StringVar)base.DynamicVars["MiddleRelicOwned"]).StringValue = OwnedRelics[1].Title.GetFormattedText();
			((StringVar)base.DynamicVars["MiddleRelicNew"]).StringValue = NewRelics[1].Title.GetFormattedText();
		}
		if (OwnedRelics.Count > 2 && NewRelics.Count > 2)
		{
			((StringVar)base.DynamicVars["BottomRelicOwned"]).StringValue = OwnedRelics[2].Title.GetFormattedText();
			((StringVar)base.DynamicVars["BottomRelicNew"]).StringValue = NewRelics[2].Title.GetFormattedText();
		}
	}

	private async Task Top()
	{
		await Trade(0);
	}

	private async Task Middle()
	{
		await Trade(1);
	}

	private async Task Bottom()
	{
		await Trade(2);
	}

	private async Task Trade(int index)
	{
		await RelicCmd.Remove(OwnedRelics[index]);
		await RelicCmd.Obtain(NewRelics[index].ToMutable(), base.Owner);
		await Done();
	}

	private Task Done()
	{
		SetEventFinished(L10NLookup("RELIC_TRADER.pages.DONE.description"));
		return Task.CompletedTask;
	}

	private IEnumerable<IHoverTip> GetRelicHoverTips(int index)
	{
		if (OwnedRelics.Count <= index || NewRelics.Count <= index)
		{
			return Array.Empty<IHoverTip>();
		}
		List<IHoverTip> list = new List<IHoverTip>();
		list.AddRange(OwnedRelics.ElementAt(index).HoverTips);
		list.AddRange(NewRelics.ElementAt(index).HoverTips);
		return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
	}
}
