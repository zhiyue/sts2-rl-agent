using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class TheBomb : CardModel
{
	private const string _turnsKey = "Turns";

	private const string _bombDamageKey = "BombDamage";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Turns", 3m),
		new DynamicVar("BombDamage", 40m)
	});

	public TheBomb()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		(await PowerCmd.Apply<TheBombPower>(base.Owner.Creature, base.DynamicVars["Turns"].BaseValue, base.Owner.Creature, this)).SetDamage(base.DynamicVars["BombDamage"].BaseValue);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["BombDamage"].UpgradeValueBy(10m);
	}
}
