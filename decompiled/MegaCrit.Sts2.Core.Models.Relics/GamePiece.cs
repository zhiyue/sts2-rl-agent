using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GamePiece : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(1));

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && CombatManager.Instance.IsInProgress && cardPlay.Card.Type == CardType.Power)
		{
			Flash();
			await CardPileCmd.Draw(context, base.DynamicVars.Cards.BaseValue, base.Owner);
		}
	}
}
