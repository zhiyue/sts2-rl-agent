using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rewards;

public class RelicReward : Reward
{
	private readonly RelicRarity _rarity;

	/// <summary>
	/// The relic that this reward gives, if it's predetermined.
	/// For randomly-rolled relic rewards (like from Elite combats), this is null.
	/// For predetermined relic rewards (like from <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" />), this is set to the predetermined relic.
	/// </summary>
	private RelicModel? _predeterminedRelic;

	/// <summary>
	/// The relic that this reward gives.
	/// For randomly-rolled relic rewards (like from Elite combats), this starts out null, and is populated by
	/// <see cref="M:MegaCrit.Sts2.Core.Rewards.RelicReward.Populate" />.
	/// For predetermined relic rewards (like from <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" />), this is set to the predetermined relic.
	/// </summary>
	private RelicModel? _relic;

	private bool _wasTaken;

	protected override RewardType RewardType => RewardType.Relic;

	public override int RewardsSetIndex => 3;

	public RelicRarity Rarity => _rarity;

	public RelicModel? ClaimedRelic { get; private set; }

	public RelicModel? Relic => _relic;

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
		_predeterminedRelic = relic;
		_relic = relic;
	}

	public RelicReward(RelicRarity rarity, Player player)
		: base(player)
	{
		_rarity = rarity;
	}

	public override void Populate()
	{
		if (_relic != null)
		{
			return;
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
		Log.Info($"Player {base.Player.NetId} obtained {_relic.Id} from relic reward");
		ClaimedRelic = await RelicCmd.Obtain(_relic, base.Player);
		_wasTaken = true;
		return true;
	}

	public override void OnSkipped()
	{
		if (!_wasTaken)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(base.Player.NetId).RelicChoices.Add(new ModelChoiceHistoryEntry(_relic.Id, wasPicked: false));
		}
	}

	public override void MarkContentAsSeen()
	{
		SaveManager.Instance.MarkRelicAsSeen(_relic);
	}

	public override SerializableReward ToSerializable()
	{
		SerializableReward serializableReward = base.ToSerializable();
		if (_predeterminedRelic != null)
		{
			serializableReward.PredeterminedModelId = _predeterminedRelic.Id;
		}
		return serializableReward;
	}
}
