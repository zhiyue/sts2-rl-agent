using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Charge : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<MinionDiveBomb>(base.IsUpgraded));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public Charge()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> selection = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, base.DynamicVars.Cards.IntValue))).ToList();
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		foreach (CardModel item in selection)
		{
			CardPileAddResult? cardPileAddResult = await CardCmd.TransformTo<MinionDiveBomb>(item);
			if (base.IsUpgraded && cardPileAddResult.HasValue)
			{
				CardCmd.Upgrade(cardPileAddResult.Value.cardAdded);
			}
		}
	}
}
