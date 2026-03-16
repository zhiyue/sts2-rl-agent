using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class NegativePulse : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(5m, ValueProp.Move),
		new PowerVar<DoomPower>(7m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public NegativePulse()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<DoomPower>(hittableEnemy, base.DynamicVars.Doom.BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
		base.DynamicVars.Doom.UpgradeValueBy(4m);
	}
}
