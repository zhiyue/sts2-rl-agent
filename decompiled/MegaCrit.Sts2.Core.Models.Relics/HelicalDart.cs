using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class HelicalDart : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<DexterityPower>(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Shiv>().Concat(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DexterityPower>()));

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && cardPlay.Card.Tags.Contains(CardTag.Shiv))
		{
			Flash();
			await PowerCmd.Apply<HelicalDartPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Dexterity.IntValue, base.Owner.Creature, null);
		}
	}
}
