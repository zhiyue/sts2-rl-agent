using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ReattachPower : PowerModel
{
	private class Data
	{
		public bool isReviving;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldScaleInMultiplayer => true;

	private bool IsReviving => GetInternalData<Data>().isReviving;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public async Task DoReattach()
	{
		if (!AreAllOtherSegmentsDead())
		{
			(NCombatRoom.Instance?.GetCreatureNode(base.Owner))?.GetSpecialNode<NDecimillipedeSegmentVfx>("%NDecimillipedeSegmentVfx")?.Regenerate();
			GetInternalData<Data>().isReviving = false;
			NCombatRoom.Instance?.SetCreatureIsInteractable(base.Owner, on: true);
			await CreatureCmd.Heal(base.Owner, base.Amount);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (wasRemovalPrevented || base.Owner != creature)
		{
			return;
		}
		if (!AreAllOtherSegmentsDead() || !base.Owner.IsDead)
		{
			GetInternalData<Data>().isReviving = true;
			if (creature.Monster is DecimillipedeSegment decimillipedeSegment)
			{
				base.Owner.Monster.SetMoveImmediate(decimillipedeSegment.DeadState);
			}
			NCombatRoom.Instance?.SetCreatureIsInteractable(base.Owner, on: false);
		}
		else
		{
			await Cmd.Wait(0.25f, ignoreCombatEnd: true);
			DoFadeOutOnAllSegments();
		}
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		if (IsReviving)
		{
			return false;
		}
		return true;
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override bool ShouldOwnerDeathTriggerFatal()
	{
		return AreAllOtherSegmentsDead();
	}

	private void DoFadeOutOnAllSegments()
	{
		float val = 0f;
		List<NCreature> list = new List<NCreature>();
		foreach (Creature enemy in base.CombatState.Enemies)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(enemy);
			if (nCreature != null)
			{
				nCreature.AnimHideIntent();
				val = Math.Max(val, nCreature.GetCurrentAnimationLength());
				list.Add(nCreature);
			}
		}
		NMonsterDeathVfx nMonsterDeathVfx = NMonsterDeathVfx.Create(list);
		if (nMonsterDeathVfx == null || list.Count <= 0)
		{
			return;
		}
		Node parent = list[0].GetParent();
		parent.AddChildSafely(nMonsterDeathVfx);
		parent.MoveChild(nMonsterDeathVfx, list[0].GetIndex());
		Task deathAnimationTask = TaskHelper.RunSafely(PlayVfxAndThenRemoveNodes(nMonsterDeathVfx, list));
		foreach (NCreature item in list)
		{
			item.DeathAnimationTask = deathAnimationTask;
			NCombatRoom.Instance?.RemoveCreatureNode(item);
		}
	}

	private async Task PlayVfxAndThenRemoveNodes(NMonsterDeathVfx vfx, List<NCreature> creatures)
	{
		await Cmd.Wait(0.25f, ignoreCombatEnd: true);
		await vfx.PlayVfx();
		foreach (NCreature creature in creatures)
		{
			creature.QueueFreeSafely();
		}
	}

	private IEnumerable<Creature> GetOtherSegments()
	{
		return from c in base.Owner.CombatState.GetTeammatesOf(base.Owner).Except(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(base.Owner))
			where c.HasPower<ReattachPower>()
			select c;
	}

	private bool AreAllOtherSegmentsDead()
	{
		return GetOtherSegments().All((Creature s) => s.IsDead);
	}
}
