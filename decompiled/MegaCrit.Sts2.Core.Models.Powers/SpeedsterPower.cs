using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SpeedsterPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (!fromHandDraw && card.Owner.Creature == base.Owner && card.Owner.Creature.CombatState.CurrentSide == card.Owner.Creature.Side)
		{
			VfxCmd.PlayOnCreatureCenters(base.CombatState.HittableEnemies, "vfx/vfx_attack_slash");
			SfxCmd.Play("slash_attack.mp3");
			await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}
}
