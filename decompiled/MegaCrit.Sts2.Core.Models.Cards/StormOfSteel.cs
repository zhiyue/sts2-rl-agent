using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class StormOfSteel : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Shiv>(base.IsUpgraded));

	public StormOfSteel()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<CardModel> enumerable = PileType.Hand.GetPile(base.Owner).Cards.ToList();
		int handSize = enumerable.Count();
		await CardCmd.Discard(choiceContext, enumerable);
		await Cmd.CustomScaledWait(0f, 0.25f);
		IEnumerable<CardModel> enumerable2 = await Shiv.CreateInHand(base.Owner, handSize, base.CombatState);
		if (!base.IsUpgraded)
		{
			return;
		}
		foreach (CardModel item in enumerable2)
		{
			CardCmd.Upgrade(item);
		}
	}
}
