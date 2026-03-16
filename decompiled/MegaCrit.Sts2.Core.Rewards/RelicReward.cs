using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Rewards;

public class RelicReward : Reward
{
	private readonly RelicRarity _rarity;

	private RelicModel? _relic;

	private bool _wasTaken;

	protected override RewardType RewardType => RewardType.Relic;

	public override int RewardsSetIndex => 3;

	public RelicRarity Rarity => _rarity;

	public RelicModel? ClaimedRelic { get; private set; }

	public override LocString Description => _relic.Title;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => _relic.HoverTips;

	public override bool IsPopulated => _relic != null;

	public RelicReward(Player player)
		: base(player)
	{
	}

	public RelicReward(RelicModel relic, Player player)
		: base(player)
	{
		relic.AssertMutable();
		_relic = relic;
	}

	public RelicReward(RelicRarity rarity, Player player)
		: base(player)
	{
		_rarity = rarity;
	}

	public override Task Populate()
	{
		if (_relic != null)
		{
			return Task.CompletedTask;
		}
		if (_rarity == RelicRarity.None)
		{
			if (_rngOverride != null)
			{
				_relic = RelicFactory.PullNextRelicFromFront(base.Player, _rngOverride).ToMutable();
			}
			else
			{
				_relic = RelicFactory.PullNextRelicFromFront(base.Player).ToMutable();
			}
		}
		else
		{
			_relic = RelicFactory.PullNextRelicFromFront(base.Player, _rarity).ToMutable();
		}
		return Task.CompletedTask;
	}

	public override TextureRect CreateIcon()
	{
		TextureRect textureRect = new TextureRect();
		textureRect.Texture = _relic.BigIcon;
		textureRect.Material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/ui/relic_mat.tres").Duplicate(deep: true);
		_relic.UpdateTexture(textureRect);
		textureRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		return textureRect;
	}

	protected override async Task<bool> OnSelect()
	{
		Log.Info($"Obtained {_relic.Id} from relic reward");
		ClaimedRelic = await RelicCmd.Obtain(_relic, base.Player);
		RunManager.Instance.RewardSynchronizer.SyncLocalObtainedRelic(_relic);
		_wasTaken = true;
		return true;
	}

	public override void OnSkipped()
	{
		if (!_wasTaken)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).RelicChoices.Add(new ModelChoiceHistoryEntry(_relic.Id, wasPicked: false));
			RunManager.Instance.RewardSynchronizer.SyncLocalSkippedRelic(_relic);
		}
	}

	public override void MarkContentAsSeen()
	{
		SaveManager.Instance.MarkRelicAsSeen(_relic);
	}
}
