using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Commands;

public static class CreatureCmd
{
	public static async Task<Creature> Add<T>(CombatState combatState, string? slotName = null) where T : MonsterModel
	{
		Creature creature = combatState.CreateCreature(ModelDb.Monster<T>().ToMutable(), CombatSide.Enemy, slotName);
		await Add(creature);
		return creature;
	}

	public static async Task<Creature> Add(MonsterModel monster, CombatState combatState, CombatSide side = CombatSide.Enemy, string? slotName = null)
	{
		monster.AssertMutable();
		Creature creature = combatState.CreateCreature(monster, side, slotName);
		await Add(creature);
		return creature;
	}

	public static async Task Add(Creature creature)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			throw new InvalidOperationException("Attempted to add a creature outside of combat.");
		}
		CombatState combatState = creature.CombatState;
		if (combatState == null)
		{
			throw new InvalidOperationException("Attempted to add a creature with no combat state.");
		}
		combatState.AddCreature(creature);
		CombatManager.Instance.AddCreature(creature);
		NCombatRoom.Instance?.AddCreature(creature);
		await CombatManager.Instance.AfterCreatureAdded(creature);
		if (combatState.CurrentSide != CombatSide.Enemy && creature.IsMonster)
		{
			creature.PrepareForNextTurn(combatState.Players.Select((Player p) => p.Creature), rollNewMove: false);
		}
		await Hook.AfterCreatureAddedToCombat(creature.CombatState, creature);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, DamageVar damageVar, CardModel cardSource)
	{
		return await Damage(choiceContext, target, damageVar.BaseValue, damageVar.Props, cardSource);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, CardModel cardSource)
	{
		return await Damage(choiceContext, new List<Creature> { target }, amount, props, cardSource.Owner.Creature, cardSource);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, DamageVar damageVar, Creature dealer)
	{
		return await Damage(choiceContext, targets, damageVar.BaseValue, damageVar.Props, dealer);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature dealer)
	{
		return await Damage(choiceContext, targets, amount, props, dealer, null);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, DamageVar damageVar, Creature dealer)
	{
		return await Damage(choiceContext, target, damageVar.BaseValue, damageVar.Props, dealer);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature dealer)
	{
		return await Damage(choiceContext, new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(target), amount, props, dealer, null);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, DamageVar damageVar, Creature? dealer, CardModel? cardSource)
	{
		return await Damage(choiceContext, new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(target), damageVar.BaseValue, damageVar.Props, dealer, cardSource);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return await Damage(choiceContext, new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(target), amount, props, dealer, cardSource);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, DamageVar damageVar, Creature? dealer, CardModel? cardSource)
	{
		return await Damage(choiceContext, targets, damageVar.BaseValue, damageVar.Props, dealer, cardSource);
	}

	public static async Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != null && dealer.IsDead)
		{
			return targets.Select((Creature t) => new DamageResult(t, props)).ToList();
		}
		List<DamageResult> results = new List<DamageResult>();
		List<Creature> targetList = targets.ToList();
		if (targetList.Count == 0)
		{
			return results;
		}
		CombatState combatState = targetList[0].CombatState;
		IRunState runState = IRunState.GetFrom(targetList.Append<Creature>(dealer).OfType<Creature>());
		foreach (Creature originalTarget in targetList)
		{
			if (originalTarget.IsDead)
			{
				continue;
			}
			IEnumerable<AbstractModel> modifiers;
			decimal modifiedAmount = Hook.ModifyDamage(runState, combatState, originalTarget, dealer, amount, props, cardSource, ModifyDamageHookType.All, CardPreviewMode.None, out modifiers);
			await Hook.AfterModifyingDamageAmount(runState, combatState, cardSource, modifiers);
			await Hook.BeforeDamageReceived(choiceContext, runState, combatState, originalTarget, modifiedAmount, props, dealer, cardSource);
			Creature creature = originalTarget.PetOwner?.Creature ?? originalTarget;
			decimal blockedDamage = creature.DamageBlockInternal(modifiedAmount, props);
			decimal unblockedDamage = Hook.ModifyHpLostBeforeOsty(runState, combatState, originalTarget, Math.Max(modifiedAmount - blockedDamage, 0m), props, dealer, cardSource, out modifiers);
			await Hook.AfterModifyingHpLostBeforeOsty(runState, combatState, modifiers);
			Creature unblockedDamageTarget = ((combatState == null) ? originalTarget : Hook.ModifyUnblockedDamageTarget(combatState, originalTarget, unblockedDamage, props, dealer));
			unblockedDamage = Hook.ModifyHpLostAfterOsty(runState, combatState, unblockedDamageTarget, unblockedDamage, props, dealer, cardSource, out modifiers);
			await Hook.AfterModifyingHpLostAfterOsty(runState, combatState, modifiers);
			DamageResult unblockedDamageResult = unblockedDamageTarget.LoseHpInternal(unblockedDamage, props);
			List<DamageResult> damageResults = new List<DamageResult>(1) { unblockedDamageResult };
			bool wasBlockBroken = originalTarget.Block <= 0 && blockedDamage > 0m;
			bool wasFullyBlocked = !props.HasFlag(ValueProp.Unblockable) && (blockedDamage > 0m || originalTarget.Block > 0) && (int)unblockedDamage == 0;
			if (originalTarget == unblockedDamageTarget)
			{
				unblockedDamageResult.BlockedDamage = (int)blockedDamage;
				unblockedDamageResult.WasBlockBroken = wasBlockBroken;
				unblockedDamageResult.WasFullyBlocked = wasFullyBlocked;
			}
			else
			{
				decimal originalTargetDamage = Hook.ModifyHpLostAfterOsty(runState, combatState, originalTarget, unblockedDamageResult.OverkillDamage, props, dealer, cardSource, out modifiers);
				await Hook.AfterModifyingHpLostAfterOsty(runState, combatState, modifiers);
				DamageResult damageResult = ((!(originalTargetDamage > 0m)) ? new DamageResult(originalTarget, props) : originalTarget.LoseHpInternal(originalTargetDamage, props));
				damageResult.BlockedDamage = (int)blockedDamage;
				damageResult.WasBlockBroken = wasBlockBroken;
				damageResult.WasFullyBlocked = wasFullyBlocked;
				damageResults.Add(damageResult);
			}
			Node vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
			List<Task> hitTriggers = new List<Task>();
			foreach (DamageResult item in damageResults)
			{
				int damage = item.UnblockedDamage + item.OverkillDamage;
				Creature receiver = item.Receiver;
				if (CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding)
				{
					CombatManager.Instance.History.DamageReceived(combatState, receiver, dealer, item, cardSource);
				}
				if (item.WasFullyBlocked)
				{
					continue;
				}
				if (damage > 0 || (modifiedAmount == 0m && item.Receiver == unblockedDamageTarget))
				{
					NDamageNumVfx nDamageNumVfx = NDamageNumVfx.Create(receiver, item);
					if (nDamageNumVfx != null)
					{
						if (vfxContainer != null)
						{
							vfxContainer.AddChildSafely(nDamageNumVfx);
						}
						else
						{
							NRun.Instance.GlobalUi.AddChildSafely(nDamageNumVfx);
						}
					}
				}
				if (damage > 0)
				{
					vfxContainer?.AddChildSafely(NHitSparkVfx.Create(receiver));
					if (receiver != dealer && !props.HasFlag(ValueProp.SkipHurtAnim))
					{
						hitTriggers.Add(TriggerAnim(receiver, "Hit", 0f));
						if (receiver.IsMonster && receiver.Monster.HasHurtSfx)
						{
							SfxCmd.Play(receiver.Monster.HurtSfx);
						}
					}
					MapPointHistoryEntry mapPointHistoryEntry = receiver.Player?.RunState.CurrentMapPointHistoryEntry;
					if (mapPointHistoryEntry != null)
					{
						mapPointHistoryEntry.GetEntry(receiver.Player.NetId).DamageTaken += item.UnblockedDamage;
					}
				}
				await Task.WhenAll(hitTriggers);
				if (damage <= 0)
				{
					continue;
				}
				if (damageResults.Any((DamageResult r) => r.WasBlockBroken))
				{
					SfxCmd.Play("event:/sfx/block_break");
				}
				if (LocalContext.IsMe(originalTarget) && (!CombatManager.Instance.IsInProgress || originalTarget.GetHpPercentRemaining() <= 0.25))
				{
					PlayerHurtVignetteHelper.Play();
				}
				if (originalTarget.Side == CombatSide.Enemy)
				{
					SfxCmd.PlayDamage(originalTarget.Monster, unblockedDamageResult.UnblockedDamage);
				}
				if (CombatManager.Instance.IsInProgress || LocalContext.ContainsMe(targetList))
				{
					if (unblockedDamageResult.UnblockedDamage < 6)
					{
						NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
					}
					else if (unblockedDamageResult.UnblockedDamage < 11)
					{
						NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
					}
					else
					{
						NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
					}
				}
			}
			results.AddRange(damageResults);
		}
		List<Creature> killedCreatures = new List<Creature>();
		foreach (DamageResult unblockedDamageResult in results)
		{
			Creature originalTarget = unblockedDamageResult.Receiver;
			if (unblockedDamageResult.WasBlockBroken)
			{
				await Hook.AfterBlockBroken(originalTarget.CombatState, originalTarget);
			}
			if (unblockedDamageResult.UnblockedDamage > 0)
			{
				await Hook.AfterCurrentHpChanged(runState, combatState, originalTarget, -unblockedDamageResult.UnblockedDamage);
			}
			if (combatState != null)
			{
				await Hook.AfterDamageGiven(choiceContext, combatState, dealer, unblockedDamageResult, props, originalTarget, cardSource);
			}
			if (!unblockedDamageResult.WasTargetKilled || !originalTarget.IsDead)
			{
				await Hook.AfterDamageReceived(choiceContext, runState, combatState, originalTarget, unblockedDamageResult, props, dealer, cardSource);
			}
			else
			{
				killedCreatures.Add(originalTarget);
			}
			if (unblockedDamageResult.WasFullyBlocked)
			{
				if (!CombatManager.Instance.IsInProgress)
				{
					throw new InvalidOperationException("Damage was blocked while combat was not active!");
				}
				SfxCmd.Play("event:/sfx/block_hit");
				Node node = NCombatRoom.Instance?.CombatVfxContainer;
				node?.AddChildSafely(NBlockSparkVfx.Create(unblockedDamageResult.Receiver));
				node?.AddChildSafely(NDamageBlockedVfx.Create(unblockedDamageResult.Receiver));
				NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
			}
		}
		await Kill(killedCreatures);
		await Cmd.CustomScaledWait(0.1f, 0.2f);
		return results;
	}

	public static async Task Kill(Creature creature, bool force = false)
	{
		await Kill(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(creature), force);
	}

	public static async Task Kill(IReadOnlyCollection<Creature> creatures, bool force = false)
	{
		if (creatures.Count == 0)
		{
			return;
		}
		IRunState runState = creatures.FirstOrDefault((Creature c) => c.IsPlayer)?.Player?.RunState;
		foreach (Creature item in creatures.ToList())
		{
			await KillWithoutCheckingWinCondition(item, force);
		}
		if (runState != null && runState.Players.All((Player p) => p.Creature.IsDead))
		{
			if (CombatManager.Instance.IsInProgress)
			{
				CombatManager.Instance.LoseCombat();
			}
			if (TestMode.IsOff)
			{
				NRun.Instance.RunMusicController.StopMusic();
				NAudioManager.Instance.PlayMusic("event:/temp/sfx/game_over");
				SerializableRun serializableRun = RunManager.Instance.OnEnded(isVictory: false);
				NRun.Instance.ShowGameOverScreen(serializableRun);
			}
		}
	}

	private static async Task KillWithoutCheckingWinCondition(Creature creature, bool force, int recursion = 0)
	{
		if (creature.CombatState == null && !creature.IsPlayer)
		{
			return;
		}
		CombatState combatState = creature.CombatState;
		IRunState runState = IRunState.GetFrom(new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(creature));
		int currentHp = creature.CurrentHp;
		if (currentHp > 0)
		{
			creature.LoseHpInternal(currentHp, ValueProp.Unblockable | ValueProp.Unpowered);
			await Hook.AfterCurrentHpChanged(runState, creature.CombatState, creature, -currentHp);
		}
		await Hook.BeforeDeath(runState, combatState, creature);
		AbstractModel preventer = null;
		if (force || creature.MaxHp <= 0 || Hook.ShouldDie(runState, combatState, creature, out preventer))
		{
			creature.InvokeDiedEvent();
			bool shouldRemoveFromCombat = combatState != null && Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, creature);
			float deathAnimLength = 0f;
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
			if (nCreature != null)
			{
				deathAnimLength = nCreature.StartDeathAnim(shouldRemoveFromCombat && creature.IsMonster);
				if (shouldRemoveFromCombat && creature.IsMonster)
				{
					NCombatRoom.Instance?.RemoveCreatureNode(nCreature);
				}
			}
			await Hook.AfterDeath(runState, combatState, creature, wasRemovalPrevented: false, deathAnimLength);
			List<Creature> teammates = (from t in combatState?.GetTeammatesOf(creature)
				where t.IsAlive
				select t).ToList() ?? new List<Creature>();
			if (shouldRemoveFromCombat && creature.Side == CombatSide.Enemy && (combatState?.Enemies.Contains(creature) ?? false))
			{
				CombatManager.Instance.RemoveCreature(creature);
				MonsterModel monster = creature.Monster;
				if (monster != null && !monster.IsPerformingMove)
				{
					combatState.RemoveCreature(creature);
				}
			}
			bool isPrimaryEnemy = creature.IsPrimaryEnemy;
			IEnumerable<PowerModel> enumerable = creature.RemoveAllPowersAfterDeath();
			foreach (PowerModel item in enumerable)
			{
				await item.AfterRemoved(creature);
			}
			if (creature.Side == CombatSide.Enemy)
			{
				if (isPrimaryEnemy && teammates.Count != 0 && teammates.All((Creature t) => t.IsSecondaryEnemy))
				{
					await Kill(teammates);
				}
			}
			else if (creature.IsPlayer)
			{
				Player player = creature.Player;
				player.PlayerCombatState?.OrbQueue.Clear();
				if (player.IsOstyAlive)
				{
					await Kill(player.Osty, force);
				}
				player.DeactivateHooks();
				if (combatState != null && !combatState.Players.All((Player p) => p.Creature.IsDead))
				{
					await CombatManager.Instance.HandlePlayerDeath(player);
				}
			}
		}
		else
		{
			if (recursion >= 10)
			{
				throw new InvalidOperationException("Combat is ending, but something is continually preventing the last creature from being killed!");
			}
			await Hook.AfterDeath(runState, combatState, creature, wasRemovalPrevented: true, 0f);
			await Hook.AfterPreventingDeath(runState, combatState, preventer, creature);
			if (creature.IsDead)
			{
				await KillWithoutCheckingWinCondition(creature, force, recursion + 1);
			}
		}
	}

	public static Task Escape(Creature creature, bool removeCreatureNode = true)
	{
		if (creature.IsDead)
		{
			return Task.CompletedTask;
		}
		creature.RemoveAllPowersInternalExcept();
		if (removeCreatureNode)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
			if (nCreature != null)
			{
				NCombatRoom.Instance.RemoveCreatureNode(nCreature);
				nCreature.ToggleIsInteractable(on: false);
				nCreature.Visible = false;
			}
		}
		CombatManager.Instance.RemoveCreature(creature);
		creature.CombatState.CreatureEscaped(creature);
		return Task.CompletedTask;
	}

	public static async Task<decimal> GainBlock(Creature creature, BlockVar blockVar, CardPlay? cardPlay, bool fast = false)
	{
		return await GainBlock(creature, blockVar.BaseValue, blockVar.Props, cardPlay, fast);
	}

	public static async Task<decimal> GainBlock(Creature creature, decimal amount, ValueProp props, CardPlay? cardPlay, bool fast = false)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return default(decimal);
		}
		CombatState combatState = creature.CombatState;
		await Hook.BeforeBlockGained(combatState, creature, amount, props, cardPlay?.Card);
		decimal modifiedAmount = amount;
		modifiedAmount = Hook.ModifyBlock(combatState, creature, modifiedAmount, props, cardPlay?.Card, cardPlay, out IEnumerable<AbstractModel> modifiers);
		modifiedAmount = Math.Max(modifiedAmount, 0m);
		await Hook.AfterModifyingBlockAmount(combatState, modifiedAmount, cardPlay?.Card, cardPlay, modifiers);
		if (modifiedAmount > 0m)
		{
			SfxCmd.Play("event:/sfx/block_gain");
			VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_block");
			creature.GainBlockInternal(modifiedAmount);
			CombatManager.Instance.History.BlockGained(combatState, creature, (int)modifiedAmount, props, cardPlay);
			if (!fast)
			{
				await Cmd.CustomScaledWait(0.1f, 0.25f);
			}
			else
			{
				await Cmd.CustomScaledWait(0f, 0.03f);
			}
		}
		await Hook.AfterBlockGained(combatState, creature, modifiedAmount, props, cardPlay?.Card);
		return modifiedAmount;
	}

	public static async Task LoseBlock(Creature creature, decimal amount)
	{
		if (!CombatManager.Instance.IsOverOrEnding && !creature.IsDead)
		{
			int block = creature.Block;
			creature.LoseBlockInternal(amount);
			if (block > 0 && creature.Block <= 0)
			{
				SfxCmd.Play("event:/sfx/block_break");
				await Hook.AfterBlockBroken(creature.CombatState, creature);
			}
		}
	}

	public static async Task Heal(Creature creature, decimal amount, bool playAnim = true)
	{
		if (CombatManager.Instance.IsEnding && !creature.IsPlayer)
		{
			return;
		}
		bool isDead = creature.IsDead;
		amount = Hook.ModifyHealAmount(creature.Player?.RunState ?? creature.CombatState?.RunState ?? NullRunState.Instance, creature.CombatState, creature, amount);
		decimal num = Math.Min(amount, creature.MaxHp - creature.CurrentHp);
		if (creature == null || !(creature.Monster is Osty))
		{
			SfxCmd.Play("event:/sfx/heal");
		}
		creature.HealInternal(amount);
		if (playAnim)
		{
			if (CombatManager.Instance.IsInProgress || creature.Player?.RunState.CurrentRoom is CombatRoom)
			{
				if (creature != null && creature.Monster is Osty)
				{
					VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_heal_osty");
				}
				else
				{
					VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_cross_heal");
				}
				(NCombatRoom.Instance?.CombatVfxContainer)?.AddChildSafely(NHealNumVfx.Create(creature, amount));
				if (isDead)
				{
					NCombatRoom.Instance?.GetCreatureNode(creature)?.StartReviveAnim();
				}
			}
			else if (LocalContext.IsMe(creature))
			{
				if (creature.Player.RunState.CurrentRoom is EventRoom)
				{
					PlayerFullscreenHealVfx.Play(creature.Player, amount, NEventRoom.Instance?.VfxContainer);
				}
				else if (creature.Player.RunState.CurrentRoom is MerchantRoom)
				{
					PlayerFullscreenHealVfx.Play(creature.Player, amount, NMerchantRoom.Instance);
				}
			}
			else if (creature.Player?.RunState.CurrentRoom is RestSiteRoom && TestMode.IsOff)
			{
				NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First((NRestSiteCharacter c) => c.Player == creature.Player);
				string scenePath = SceneHelper.GetScenePath("vfx/vfx_cross_heal");
				Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
				nRestSiteCharacter?.AddChildSafely(node2D);
				node2D.Position = Vector2.Zero;
			}
		}
		MapPointHistoryEntry mapPointHistoryEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
		if (mapPointHistoryEntry != null && num > 0m)
		{
			mapPointHistoryEntry.GetEntry(creature.Player.NetId).HpHealed += (int)num;
		}
		if (CombatManager.Instance.IsInProgress)
		{
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		if (amount > 0m)
		{
			await Hook.AfterCurrentHpChanged(creature.Player?.RunState ?? creature.CombatState.RunState, creature.CombatState, creature, amount);
		}
	}

	public static async Task SetCurrentHp(Creature creature, decimal amount)
	{
		bool flag = creature.IsDead && amount > 0m;
		decimal num = creature.CurrentHp;
		creature.SetCurrentHpInternal(amount);
		if (amount != num)
		{
			if (CombatManager.Instance.IsInProgress && flag)
			{
				NCombatRoom.Instance?.GetCreatureNode(creature)?.StartReviveAnim();
			}
			await Hook.AfterCurrentHpChanged(creature.Player?.RunState ?? creature.CombatState.RunState, creature.CombatState, creature, amount - num);
		}
		if (creature.IsDead)
		{
			await Kill(creature);
		}
	}

	public static async Task GainMaxHp(Creature creature, decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be non-negative. Use LoseMaxHp for max HP loss.");
		}
		await SetMaxHp(creature, (decimal)creature.MaxHp + amount);
		MapPointHistoryEntry mapPointHistoryEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
		if (mapPointHistoryEntry != null)
		{
			mapPointHistoryEntry.GetEntry(creature.Player.NetId).MaxHpGained += (int)amount;
		}
		await Heal(creature, amount);
	}

	public static async Task LoseMaxHp(PlayerChoiceContext choiceContext, Creature creature, decimal amount, bool isFromCard)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("amount must be non-negative. Use GainMaxHp for max HP gain.");
		}
		decimal num = (decimal)creature.MaxHp - amount;
		MapPointHistoryEntry mapPointHistoryEntry = creature.Player?.RunState.CurrentMapPointHistoryEntry;
		if (mapPointHistoryEntry != null)
		{
			mapPointHistoryEntry.GetEntry(creature.Player.NetId).MaxHpLost += (int)amount;
		}
		if (num < (decimal)creature.CurrentHp)
		{
			await Damage(choiceContext, creature, (decimal)creature.CurrentHp - num, isFromCard ? (ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move) : (ValueProp.Unblockable | ValueProp.Unpowered), null, null);
		}
		await SetMaxHp(creature, (decimal)creature.MaxHp - amount);
	}

	public static async Task SetMaxHp(Creature creature, decimal amount)
	{
		creature.SetMaxHpInternal(Math.Max(0m, amount));
		if (creature.MaxHp <= 0)
		{
			await Kill(creature);
		}
	}

	public static async Task SetMaxAndCurrentHp(Creature creature, decimal amount)
	{
		await SetMaxHp(creature, amount);
		await SetCurrentHp(creature, amount);
	}

	public static async Task Stun(Creature creature, string? nextMoveId = null)
	{
		await Stun(creature, (IReadOnlyList<Creature> _) => Task.CompletedTask, nextMoveId);
	}

	public static Task Stun(Creature creature, Func<IReadOnlyList<Creature>, Task> stunMove, string? nextMoveId = null)
	{
		creature.StunInternal(Wrapper, nextMoveId);
		return Task.CompletedTask;
		async Task Wrapper(IReadOnlyList<Creature> c)
		{
			NStunnedVfx vfx = NStunnedVfx.Create(creature);
			if (vfx != null)
			{
				Callable.From(delegate
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
				}).CallDeferred();
			}
			await stunMove(c);
		}
	}

	public static async Task TriggerAnim(Creature creature, string triggerName, float waitTime)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature == null)
		{
			if (!TestMode.IsOn && CombatManager.Instance.IsInProgress)
			{
				Log.Error($"Attempted to play animation on creature {creature} but its creature node doesn't exist!");
			}
			return;
		}
		if (creature.IsPlayer)
		{
			CharacterModel character = creature.Player.Character;
			if (creature.IsDead)
			{
				return;
			}
			if (!(triggerName == "Attack"))
			{
				if (triggerName == "Cast")
				{
					SfxCmd.Play(character.CastSfx);
				}
			}
			else
			{
				SfxCmd.Play(character.AttackSfx);
			}
		}
		nCreature.SetAnimationTrigger(triggerName);
		await Cmd.CustomScaledWait(Mathf.Min(waitTime * 0.5f, 0.25f), waitTime);
	}
}
