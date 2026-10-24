using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace MegaCrit.Sts2.Core.Commands;

public static class SfxCmd
{
	public static void Play(string sfx, float volume = 1f)
	{
		if (!NonInteractiveMode.IsActive && !CombatManager.Instance.IsEnding)
		{
			NAudioManager.Instance.PlayOneShot(sfx, volume);
		}
	}

	public static void Play(string sfx, string param, float val, float volume = 1f)
	{
		if (!NonInteractiveMode.IsActive && !CombatManager.Instance.IsEnding)
		{
			NAudioManager.Instance.PlayOneShot(sfx, new Dictionary<string, float> { { param, val } }, volume);
		}
	}

	public static void PlayLoop(string sfx, bool usesLoopParam = true)
	{
		if (!NonInteractiveMode.IsActive)
		{
			NAudioManager.Instance.PlayLoop(sfx, usesLoopParam);
		}
	}

	public static void PlayLoop(Creature creature, string sfx)
	{
		if (!NonInteractiveMode.IsActive)
		{
			creature.GetCreatureNode()?.StartSfxLoop(sfx);
		}
	}

	public static void PlayLoop(Creature creature, string sfx, string loopParam, float loopStopValue)
	{
		if (!NonInteractiveMode.IsActive)
		{
			creature.GetCreatureNode()?.StartSfxLoop(sfx, loopParam, loopStopValue);
		}
	}

	public static void StopLoop(string sfx)
	{
		if (!NonInteractiveMode.IsActive)
		{
			NAudioManager.Instance.StopLoop(sfx);
		}
	}

	public static void StopLoop(Creature creature, string sfx)
	{
		if (!NonInteractiveMode.IsActive)
		{
			creature.GetCreatureNode()?.StopSfxLoop(sfx);
		}
	}

	public static void SetParam(string sfx, string param, float value)
	{
		if (!NonInteractiveMode.IsActive)
		{
			NAudioManager.Instance.SetParam(sfx, param, value);
		}
	}

	public static void PlayDamage(MonsterModel? monster, int damageAmount)
	{
		if (!NonInteractiveMode.IsActive && !CombatManager.Instance.IsEnding && monster != null)
		{
			NAudioManager.Instance.PlayOneShot(monster.TakeDamageSfx, new Dictionary<string, float> { { "EnemyImpact_Intensity", 2f } });
		}
	}

	public static void PlayDeath(MonsterModel? monster)
	{
		if (!NonInteractiveMode.IsActive && monster != null)
		{
			NAudioManager.Instance.PlayOneShot(monster.DeathSfx);
		}
	}

	public static void PlayDeath(Player player)
	{
		if (!NonInteractiveMode.IsActive)
		{
			NAudioManager.Instance.PlayOneShot(player.Character.DeathSfx);
		}
	}

	/// <summary>
	/// Plays the correct swoosh sfx depending on the pile the carf came from and is going to
	/// </summary>
	public static void PlayCardSwooshSfx(CardPile currentPile, CardPile? prevPile = null)
	{
		if (currentPile.Type == PileType.Draw)
		{
			Play("event:/sfx/ui/cards/card_movement_B_into_draw");
		}
		else if (currentPile.Type == PileType.Discard)
		{
			if (prevPile != null && prevPile.Type == PileType.Play)
			{
				Play("event:/sfx/ui/cards/card_movement_B_play_into_discard");
			}
			else
			{
				Play("event:/sfx/ui/cards/card_movement_B_into_discard");
			}
		}
		else
		{
			Play("event:/sfx/ui/cards/card_movement_B_into_deck");
		}
	}
}
