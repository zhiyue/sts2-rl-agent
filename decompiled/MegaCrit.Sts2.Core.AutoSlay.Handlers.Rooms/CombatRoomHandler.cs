using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.AutoSlay.Helpers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms;

public class CombatRoomHandler : IRoomHandler, IHandler
{
	public RoomType[] HandledTypes => new RoomType[3]
	{
		RoomType.Monster,
		RoomType.Elite,
		RoomType.Boss
	};

	public TimeSpan Timeout => TimeSpan.FromMinutes(5L);

	public async Task HandleAsync(Rng random, CancellationToken ct)
	{
		AutoSlayLog.Action("Waiting for combat to start");
		await WaitHelper.Until(() => CombatManager.Instance.IsInProgress, ct, AutoSlayConfig.nodeWaitTimeout, "Combat not started");
		AutoSlayLog.Action("Combat started, applying defensive buffs");
		Player player = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());
		Creature playerCreature = player.Creature;
		await PowerCmd.Apply<PlatingPower>(playerCreature, 999m, playerCreature, null);
		await PowerCmd.Apply<RegenPower>(playerCreature, 999m, playerCreature, null);
		int turnCount = 0;
		while (CombatManager.Instance.IsInProgress && turnCount < 100)
		{
			ct.ThrowIfCancellationRequested();
			turnCount++;
			await WaitHelper.Until(() => CombatManager.Instance.IsPlayPhase || !CombatManager.Instance.IsInProgress, ct, TimeSpan.FromSeconds(30L), "Play phase not started");
			if (!CombatManager.Instance.IsInProgress)
			{
				break;
			}
			AutoSlayer.CurrentWatchdog?.Reset($"Combat turn {turnCount}");
			AutoSlayLog.Action($"Turn {turnCount}: playing cards");
			await UseAllPotions(player, random, ct);
			int cardsPlayed = 0;
			HashSet<CardModel> attemptedCards = new HashSet<CardModel>();
			while (cardsPlayed < 50 && CombatManager.Instance.IsPlayPhase)
			{
				ct.ThrowIfCancellationRequested();
				if (cardsPlayed > 0 && cardsPlayed % 10 == 0)
				{
					AutoSlayer.CurrentWatchdog?.Reset($"Combat turn {turnCount}, played {cardsPlayed} cards");
				}
				CardPile pile = PileType.Hand.GetPile(player);
				UnplayableReason reason;
				AbstractModel preventer;
				List<CardModel> list = pile.Cards.Where((CardModel c) => c.CanPlay(out reason, out preventer) && !attemptedCards.Contains(c)).ToList();
				if (list.Count == 0)
				{
					AutoSlayLog.Action("No more playable cards, ending turn");
					break;
				}
				CardModel cardModel = random.NextItem(list);
				Creature randomTarget = GetRandomTarget(cardModel, random);
				attemptedCards.Add(cardModel);
				AutoSlayLog.Info("Playing " + cardModel.Id.Entry);
				await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), cardModel, randomTarget);
				cardsPlayed++;
				await Task.Delay(100, ct);
			}
			if (CombatManager.Instance.IsPlayPhase && CombatManager.Instance.IsInProgress)
			{
				PlayerCmd.EndTurn(player, canBackOut: false);
			}
		}
		await WaitHelper.Until(() => !CombatManager.Instance.IsInProgress, ct, TimeSpan.FromSeconds(30L), "Combat did not end");
		AutoSlayLog.Action("Combat finished");
	}

	private static Creature? GetRandomTarget(CardModel card, Rng random)
	{
		if (card.TargetType != TargetType.AnyEnemy)
		{
			return null;
		}
		CombatState combatState = card.CombatState;
		if (combatState == null)
		{
			return null;
		}
		List<Creature> list = combatState.HittableEnemies.ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return random.NextItem(list);
	}

	private static async Task UseAllPotions(Player player, Rng random, CancellationToken ct)
	{
		List<PotionModel> list = player.Potions.ToList();
		if (list.Count == 0)
		{
			return;
		}
		AutoSlayLog.Action($"Using {list.Count} potion(s)");
		CombatState combatState = player.Creature.CombatState;
		foreach (PotionModel item in list)
		{
			ct.ThrowIfCancellationRequested();
			if (CombatManager.Instance.IsPlayPhase && CombatManager.Instance.IsInProgress)
			{
				Creature potionTarget = GetPotionTarget(item, combatState, random);
				if (potionTarget == null && item.TargetType.IsSingleTarget())
				{
					AutoSlayLog.Warn("Skipping potion " + item.Id.Entry + ": no valid target");
					continue;
				}
				AutoSlayLog.Info("Using potion: " + item.Id.Entry);
				item.EnqueueManualUse(potionTarget);
				await Task.Delay(300, ct);
				continue;
			}
			break;
		}
	}

	private static Creature? GetPotionTarget(PotionModel potion, CombatState? combatState, Rng random)
	{
		if (combatState == null)
		{
			return null;
		}
		return potion.TargetType switch
		{
			TargetType.AnyEnemy => random.NextItem(combatState.HittableEnemies.ToList()), 
			TargetType.AnyAlly => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			TargetType.AnyPlayer => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			TargetType.Self => combatState.PlayerCreatures.FirstOrDefault((Creature c) => c.IsAlive), 
			_ => null, 
		};
	}
}
