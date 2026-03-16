using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Armaments : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(5m, ValueProp.Move));

	public Armaments()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		if (base.IsUpgraded)
		{
			foreach (CardModel item in PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable))
			{
				CardCmd.Upgrade(item);
			}
			return;
		}
		CardModel cardModel = await CardSelectCmd.FromHandForUpgrade(choiceContext, base.Owner, this);
		if (cardModel != null)
		{
			CardCmd.Upgrade(cardModel);
		}
	}
}
