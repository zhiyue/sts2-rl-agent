using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DustyTome : RelicModel
{
	private const string _ancientCardKey = "AncientCard";

	private ModelId? _ancientCard;

	private IEnumerable<IHoverTip> _extraHoverTips = Array.Empty<IHoverTip>();

	public override RelicRarity Rarity => RelicRarity.Ancient;

	[SavedProperty]
	public ModelId? AncientCard
	{
		get
		{
			return _ancientCard;
		}
		set
		{
			AssertMutable();
			_ancientCard = value;
			if (_ancientCard != null)
			{
				CardModel cardModel = SaveUtil.CardOrDeprecated(_ancientCard);
				_extraHoverTips = cardModel.HoverTips.Concat(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard(cardModel, upgrade: true)));
				((StringVar)base.DynamicVars["AncientCard"]).StringValue = cardModel.Title;
			}
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("AncientCard"));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => _extraHoverTips;

	public void SetupForPlayer(Player player)
	{
		IEnumerable<CardModel> items = from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
			where c.Rarity == CardRarity.Ancient && !ArchaicTooth.TranscendenceCards.Contains(c)
			select c;
		AncientCard = player.PlayerRng.Rewards.NextItem(items).Id;
	}

	public override async Task AfterObtained()
	{
		CardModel card = base.Owner.RunState.CreateCard(ModelDb.GetById<CardModel>(AncientCard), base.Owner);
		CardCmd.Upgrade(card);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
	}
}
