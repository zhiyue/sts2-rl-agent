using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class PrimalForce : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<GiantRock>(base.IsUpgraded));

	public PrimalForce()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c != null && c.IsTransformable && c.Type == CardType.Attack).ToList();
		foreach (CardModel item in list)
		{
			CardModel cardModel = base.CombatState.CreateCard<GiantRock>(base.Owner);
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(cardModel);
			}
			await CardCmd.Transform(item, cardModel);
		}
	}
}
