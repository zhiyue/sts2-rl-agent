using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
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

	public static void StopLoop(string sfx)
	{
		if (!NonInteractiveMode.IsActive)
		{
			NAudioManager.Instance.StopLoop(sfx);
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
}
