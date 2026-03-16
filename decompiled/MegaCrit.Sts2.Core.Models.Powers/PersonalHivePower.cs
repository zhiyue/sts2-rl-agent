using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PersonalHivePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Dazed>());

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult _, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && dealer != null && props.IsPoweredAttack())
		{
			if (dealer.Monster is Osty)
			{
				dealer = dealer.PetOwner.Creature;
			}
			CardPileAddResult[] statusCards = new CardPileAddResult[base.Amount];
			for (int i = 0; i < base.Amount; i++)
			{
				CardModel card = base.CombatState.CreateCard<Dazed>(dealer.Player);
				CardPileAddResult[] array = statusCards;
				int num = i;
				array[num] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: false, CardPilePosition.Random);
			}
			CardCmd.PreviewCardPileAdd(statusCards);
			await Cmd.Wait(0.5f);
		}
	}
}
