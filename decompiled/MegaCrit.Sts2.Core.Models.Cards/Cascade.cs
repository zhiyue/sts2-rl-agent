using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Cascade : CardModel
{
	protected override bool HasEnergyCostX => true;

	public Cascade()
		: base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ResolveEnergyXValue();
		if (base.IsUpgraded)
		{
			num++;
		}
		await CardPileCmd.AutoPlayFromDrawPile(choiceContext, base.Owner, num, CardPilePosition.Top, forceExhaust: false);
	}
}
