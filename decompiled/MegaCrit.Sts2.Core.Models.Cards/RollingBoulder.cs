using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class RollingBoulder : CardModel
{
	public const int incrementAmount = 5;

	private const string _incrementAmountKey = "IncrementAmount";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<RollingBoulderPower>(5m),
		new DynamicVar("IncrementAmount", 5m)
	});

	public RollingBoulder()
		: base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<RollingBoulderPower>(base.Owner.Creature, base.DynamicVars["RollingBoulderPower"].IntValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["RollingBoulderPower"].UpgradeValueBy(5m);
	}
}
