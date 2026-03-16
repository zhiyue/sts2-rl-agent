using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rewards;

public class SpecialCardReward : Reward
{
	private bool _wasTaken;

	private readonly CardModel? _card;

	private ModelId _customDescriptionEncounterSourceId = ModelId.none;

	private static string RewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_special_card.png");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(RewardIcon);

	protected override RewardType RewardType => RewardType.SpecialCard;

	public override int RewardsSetIndex => 4;

	protected override string IconPath => RewardIcon;

	public override LocString Description
	{
		get
		{
			LocString locString = null;
			if (_customDescriptionEncounterSourceId != ModelId.none)
			{
				locString = ModelDb.GetById<EncounterModel>(_customDescriptionEncounterSourceId).CustomRewardDescription;
			}
			if (locString == null)
			{
				locString = new LocString("gameplay_ui", "COMBAT_REWARD_ADD_SPECIAL_CARD");
			}
			locString.Add("Card", _card.Title);
			return locString;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard(_card));

	public override bool IsPopulated => _card != null;

	public SpecialCardReward(CardModel card, Player player)
		: base(player)
	{
		card.AssertMutable();
		_card = card;
	}

	public void SetCustomDescriptionEncounterSource(ModelId encounterId)
	{
		if (ModelDb.GetByIdOrNull<EncounterModel>(encounterId) == null)
		{
			throw new ArgumentException($"Encounter {encounterId} does not exist!");
		}
		_customDescriptionEncounterSourceId = encounterId;
	}

	public override Task Populate()
	{
		return Task.CompletedTask;
	}

	protected override async Task<bool> OnSelect()
	{
		Log.Info($"Obtained {_card.Id} from special card reward");
		CardPileAddResult result = await CardPileCmd.Add(_card, PileType.Deck);
		if (result.success)
		{
			RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(_card);
			CardCmd.PreviewCardPileAdd(result, 2f);
		}
		_wasTaken = true;
		return true;
	}

	public override void OnSkipped()
	{
		if (!_wasTaken)
		{
			base.Player.RunState.CurrentMapPointHistoryEntry.GetEntry(LocalContext.NetId.Value).CardChoices.Add(new CardChoiceHistoryEntry(_card, wasPicked: false));
			RunManager.Instance.RewardSynchronizer.SyncLocalSkippedCard(_card);
		}
	}

	public override SerializableReward ToSerializable()
	{
		return new SerializableReward
		{
			RewardType = RewardType,
			SpecialCard = _card.ToSerializable(),
			CustomDescriptionEncounterSourceId = _customDescriptionEncounterSourceId
		};
	}

	public override void MarkContentAsSeen()
	{
	}
}
