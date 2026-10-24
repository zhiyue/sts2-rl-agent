using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class NightmarePower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// This will be null for the moment after this power is applied but before this is set by Nightmare.OnPlay.
		/// For all current use cases, this means we should never see this being null.
		/// However, if we needed to override AfterApplied in here in the future, this would be null in it, so let's
		/// leave this nullable for future-proofing.
		/// </summary>
		public CardModel? selectedCard;
	}

	private const string _cardKey = "Card";

	public override PowerType Type => PowerType.Buff;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("Card"));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner.Player)
		{
			CardModel card = GetInternalData<Data>().selectedCard;
			for (int i = 0; i < base.Amount; i++)
			{
				CardModel card2 = card.CreateClone();
				await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, base.Owner.Player);
			}
			await PowerCmd.Remove(this);
		}
	}

	public void SetSelectedCard(CardModel card)
	{
		CardModel cardModel = card.CreateClone();
		CardCmd.ClearAffliction(cardModel);
		GetInternalData<Data>().selectedCard = cardModel;
		((StringVar)base.DynamicVars["Card"]).StringValue = cardModel.Title;
	}
}
