using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SweepingBeam : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(6m, ValueProp.Move),
		new CardsVar(1)
	});

	public SweepingBeam()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithAttackerAnim("Cast", 0.5f)
			.BeforeDamage(async delegate
			{
				List<Creature> targets = base.CombatState.HittableEnemies.ToList();
				NSweepingBeamVfx nSweepingBeamVfx = NSweepingBeamVfx.Create(base.Owner.Creature, targets);
				if (nSweepingBeamVfx != null)
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nSweepingBeamVfx);
					await Cmd.Wait(0.5f);
				}
			})
			.Execute(choiceContext);
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
