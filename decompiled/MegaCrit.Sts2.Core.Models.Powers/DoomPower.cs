using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DoomPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;

	public static async Task DoomKill(IReadOnlyList<Creature> creatures)
	{
		if (creatures.Count == 0)
		{
			return;
		}
		CombatState combatState = creatures.First().CombatState;
		foreach (Creature creature in creatures)
		{
			await PlayVfx(creature);
			await CreatureCmd.Kill(creature);
		}
		await Hook.AfterDiedToDoom(combatState, creatures);
	}

	public static IReadOnlyList<Creature> GetDoomedCreatures(IReadOnlyList<Creature> creatures)
	{
		return creatures.Where((Creature c) => c.GetPower<DoomPower>()?.IsOwnerDoomed() ?? false).ToList();
	}

	public bool IsOwnerDoomed()
	{
		return base.Owner.CurrentHp <= base.Amount;
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side && !base.Owner.IsDead && IsOwnerDoomed())
		{
			IReadOnlyList<Creature> doomedCreatures = GetDoomedCreatures(base.Owner.CombatState.GetCreaturesOnSide(side));
			if (doomedCreatures.First() == base.Owner)
			{
				await DoomKill(doomedCreatures);
			}
		}
	}

	private static async Task PlayVfx(Creature creature)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature == null)
		{
			return;
		}
		bool flag = false;
		if (creature.IsMonster)
		{
			flag = Hook.ShouldDie(creature.Player?.RunState ?? creature.CombatState.RunState, creature.CombatState, creature, out AbstractModel _) && creature.Monster.ShouldDisappearFromDoom;
		}
		StartDoomAnim(nCreature, flag);
		NDoomOverlayVfx orCreate = NDoomOverlayVfx.GetOrCreate();
		if (orCreate != null && !orCreate.IsInsideTree())
		{
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(orCreate);
		}
		List<Creature> source = (from c in creature.CombatState.GetTeammatesOf(creature)
			where c.IsAlive
			select c).ToList();
		if (flag)
		{
			if (source.Count() != 1 || source.First() != creature)
			{
				await Cmd.Wait(0.25f);
			}
			else
			{
				await Cmd.Wait(1.5f);
			}
		}
	}

	private static void StartDoomAnim(NCreature creature, bool shouldDie)
	{
		Task task = null;
		if (shouldDie)
		{
			creature.Entity.Monster?.OnDieToDoom();
			Tween tween = creature.AnimDisableUi();
			tween.TweenCallback(Callable.From(creature.QueueFreeSafely));
			task = WaitForTween(tween);
			if (creature.HasSpineAnimation)
			{
				creature.SetAnimationTrigger("Hit");
				if (creature.SpineController.GetAnimationState().GetCurrent(0).GetAnimation()
					.GetName() == "hurt")
				{
					MegaTrackEntry current = creature.SpineController.GetAnimationState().GetCurrent(0);
					current.SetTrackTime(0.1f);
					current.SetTimeScale(0f);
				}
			}
			NCombatRoom.Instance?.RemoveCreatureNode(creature);
		}
		NDoomVfx nDoomVfx = NDoomVfx.Create(creature.Visuals, creature.Hitbox.GlobalPosition, creature.Hitbox.Size, shouldDie);
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nDoomVfx);
		if (shouldDie)
		{
			global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = task;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = nDoomVfx.VfxTask;
			creature.DeathAnimationTask = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
		}
	}

	private static async Task WaitForTween(Tween t)
	{
		if (t.IsValid() && t.IsRunning())
		{
			await t.ToSignal(t, Tween.SignalName.Finished);
		}
	}
}
