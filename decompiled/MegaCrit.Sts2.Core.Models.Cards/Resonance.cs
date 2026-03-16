using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Resonance : CardModel
{
	public override int CanonicalStarCost => 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public Resonance()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		int intValue = base.DynamicVars["StrengthPower"].IntValue;
		await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, intValue, base.Owner.Creature, this);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<StrengthPower>(hittableEnemy, -1m, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["StrengthPower"].UpgradeValueBy(1m);
	}
}
