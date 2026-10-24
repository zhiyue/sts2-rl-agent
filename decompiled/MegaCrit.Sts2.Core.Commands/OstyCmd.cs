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
	/// <summary>
	/// Summon Osty with the specified number of HP. If the specified creature already owns an instance of Osty, raise
	/// Osty's max HP by the specified number instead.
	/// </summary>
	/// <param name="choiceContext">The context with which to handle player choices.</param>
	/// <param name="summoner">The player who is summoning.</param>
	/// <param name="amount">
	/// The number of HP that Osty should be summoned with (or that should be added to the existing Osty instance).
	/// </param>
	/// <param name="source">
	/// The model that this Summon came from. For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Bodyguard" /> and <see cref="T:MegaCrit.Sts2.Core.Models.Relics.BoundPhylactery" />
	/// pass themselves here.
	/// Null if the Summon did not come from any model (generally only relevant in tests).
	/// </param>
	/// <returns>The result of the summon.</returns>
	public static async Task<SummonResult> Summon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, AbstractModel? source)
	{
		ICombatState combatState = summoner.Creature.CombatState;
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
				if (ostyNode != null && source is CardModel)
				{
					ostyNode.Modulate = Colors.Transparent;
					Tween tween = ostyNode.CreateTween();
					tween.TweenProperty(ostyNode, "modulate", Colors.White, 0.3499999940395355).SetDelay(0.10000000149011612);
					ostyNode.StartReviveAnim();
				}
				await PowerCmd.Apply<DieForYouPower>(choiceContext, osty, 1m, null, null);
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
			nCreature.OstyScaleToSize(osty.MaxHp, 0.75);
		}
		CombatManager.Instance.History.Summoned(combatState, (int)amount, summoner);
		await Hook.AfterSummon(combatState, choiceContext, summoner, amount);
		return new SummonResult(summoner.Osty, amount);
	}
}
