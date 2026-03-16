using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class OneTwoPunch : CardModel
{
	private const string _attacksKey = "Attacks";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Attacks", 1m));

	public OneTwoPunch()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<OneTwoPunchPower>(base.Owner.Creature, base.DynamicVars["Attacks"].BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Attacks"].UpgradeValueBy(1m);
	}
}
