using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class NoxiousFumesPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Side)
		{
			return;
		}
		Flash();
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(hittableEnemy);
			if (nCreature != null)
			{
				NGaseousImpactVfx child = NGaseousImpactVfx.Create(nCreature.VfxSpawnPosition, new Color("83eb85"));
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
			}
		}
		await PowerCmd.Apply<PoisonPower>(base.CombatState.HittableEnemies, base.Amount, base.Owner, null);
	}
}
