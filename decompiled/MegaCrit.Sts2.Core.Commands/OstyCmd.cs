using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class OstyCmd
{
	public static async Task<SummonResult> Summon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, AbstractModel? source)
	{
		CombatState combatState = summoner.Creature.CombatState;
		amount = Hook.ModifySummonAmount(combatState, summoner, amount, source);
		if (amount == 0m)
		{
			return new SummonResult(summoner.Osty, 0m);
		}
		if (CombatManager.Instance.IsInProgress)
		{
			SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
		}
		Creature osty = combatState.Allies.FirstOrDefault((Creature c) => c.Monster is Osty && c.PetOwner == summoner);
		if (summoner.IsOstyAlive)
		{
			await CreatureCmd.GainMaxHp(summoner.Osty, amount);
		}
		else
		{
			bool isReviving = osty != null;
			if (isReviving)
			{
				if (osty.IsAlive)
				{
					throw new InvalidOperationException("We shouldn't make it here if Osty is still alive!");
				}
				summoner.PlayerCombatState.AddPetInternal(osty);
			}
			else
			{
				osty = await PlayerCmd.AddPet<Osty>(summoner);
				NCreature ostyNode = NCombatRoom.Instance?.GetCreatureNode(osty);
				if (ostyNode != null)
				{
					ostyNode.Modulate = Colors.Transparent;
					Tween tween = ostyNode.CreateTween();
					tween.TweenProperty(ostyNode, "modulate:a", 1, 0.3499999940395355).From(0);
					ostyNode.StartReviveAnim();
				}
				await PowerCmd.Apply<DieForYouPower>(osty, 1m, null, null);
				ostyNode?.TrackBlockStatus(summoner.Creature);
			}
			await CreatureCmd.SetMaxHp(osty, amount);
			await CreatureCmd.Heal(osty, amount, isReviving);
			if (isReviving)
			{
				await Hook.AfterOstyRevived(combatState, osty);
			}
		}
		if (TestMode.IsOff)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(osty);
			nCreature.OstyScaleToSize(osty.MaxHp, 0.75f);
		}
		CombatManager.Instance.History.Summoned(combatState, (int)amount, summoner);
		await Hook.AfterSummon(combatState, choiceContext, summoner, amount);
		return new SummonResult(summoner.Osty, amount);
	}
}
