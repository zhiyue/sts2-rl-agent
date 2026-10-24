using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class HuddleUp : CardModel
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public HuddleUp()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer
			select c;
		foreach (Creature item in enumerable)
		{
			await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, item.Player);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
