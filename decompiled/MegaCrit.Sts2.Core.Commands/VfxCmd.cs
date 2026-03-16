using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Ui;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class VfxCmd
{
	public const string adrenalinePath = "vfx/vfx_adrenaline";

	public const string bitePath = "vfx/vfx_bite";

	public const string blockPath = "vfx/vfx_block";

	public const string bloodyImpactPath = "vfx/vfx_bloody_impact";

	public const string bluntPath = "vfx/vfx_attack_blunt";

	public const string chainPath = "vfx/vfx_chain";

	public const string coinExplosionSmallPath = "vfx/vfx_coin_explosion_small";

	public const string coinExplosionRegularPath = "vfx/vfx_coin_explosion_regular";

	public const string coinExplosionJumboPath = "vfx/vfx_coin_explosion_jumbo";

	public const string daggerThrowPath = "vfx/vfx_dagger_throw";

	public const string daggerSprayPath = "vfx/vfx_dagger_spray";

	public const string dramaticStabPath = "vfx/vfx_dramatic_stab";

	public const string flyingSlashPath = "vfx/vfx_flying_slash";

	public const string gazePath = "vfx/vfx_gaze";

	public const string giantHorizontalSlashPath = "vfx/vfx_giant_horizontal_slash";

	public const string healPath = "vfx/vfx_cross_heal";

	public const string lightningPath = "vfx/vfx_attack_lightning";

	public const string thrashPath = "vfx/vfx_thrash";

	public const string rockShatterPath = "vfx/vfx_rock_shatter";

	public const string sandyImpactPath = "vfx/vfx_sandy_impact";

	public const string scratchPath = "vfx/vfx_scratch";

	public const string slashPath = "vfx/vfx_attack_slash";

	public const string slimeImpactVfxPath = "vfx/vfx_slime_impact";

	public const string hellraiserSwordVfxPath = "vfx/hellraiser_attack_vfx";

	public const string heavyBluntPath = "vfx/vfx_heavy_blunt";

	public const string starryImpactVfx = "vfx/vfx_starry_impact";

	public const string screamVfx = "vfx/vfx_scream";

	public const string spookyScreamVfx = "vfx/vfx_spooky_scream";

	public static IEnumerable<string> AssetPaths => new string[27]
	{
		"vfx/vfx_block", "vfx/vfx_attack_slash", "vfx/vfx_attack_blunt", "vfx/vfx_gaze", "vfx/vfx_bloody_impact", "vfx/vfx_bite", "vfx/vfx_chain", "vfx/vfx_flying_slash", "vfx/vfx_coin_explosion_small", "vfx/vfx_coin_explosion_regular",
		"vfx/vfx_coin_explosion_jumbo", "vfx/vfx_adrenaline", "vfx/vfx_rock_shatter", "vfx/vfx_scratch", "vfx/vfx_sandy_impact", "vfx/vfx_attack_lightning", "vfx/vfx_giant_horizontal_slash", "vfx/vfx_dagger_throw", "vfx/vfx_dagger_spray", "vfx/vfx_dramatic_stab",
		"vfx/vfx_slime_impact", "vfx/vfx_thrash", "vfx/vfx_cross_heal", "vfx/vfx_heavy_blunt", "vfx/vfx_starry_impact", "vfx/vfx_scream", "vfx/vfx_spooky_scream"
	}.Select(SceneHelper.GetScenePath).Concat(new global::_003C_003Ez__ReadOnlyArray<string>(new string[25]
	{
		NItemThrowVfx.scenePath,
		NSplashVfx.scenePath,
		NLiquidOverlayVfx.scenePath,
		NLargeMagicMissileVfx.scenePath,
		NBigSlashVfx.scenePath,
		NBigSlashImpactVfx.scenePath,
		NSmallMagicMissileVfx.scenePath,
		NGaseousImpactVfx.scenePath,
		NDaggerSprayFlurryVfx.scenePath,
		NDaggerSprayImpactVfx.scenePath,
		NFireBurstVfx.scenePath,
		NFireBurningVfx.scenePath,
		NPoisonImpactVfx.scenePath,
		NScratchVfx.scenePath,
		NSporeImpactVfx.scenePath,
		NShivThrowVfx.scenePath,
		NHyperbeamVfx.scenePath,
		NHyperbeamImpactVfx.scenePath,
		NMinionDiveBombVfx.scenePath,
		NSweepingBeamImpactVfx.scenePath,
		NSweepingBeamVfx.scenePath,
		NGoopyImpactVfx.scenePath,
		NGaseousImpactVfx.scenePath,
		NWormyImpactVfx.scenePath,
		NLowHpBorderVfx.scenePath
	})).Concat(NHealNumVfx.AssetPaths);

	public static void PlayFullScreenInCombat(string path)
	{
		if (!TestMode.IsOn && NCombatRoom.Instance != null)
		{
			string scenePath = SceneHelper.GetScenePath(path);
			Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(node2D);
			node2D.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f;
		}
	}

	public static Vector2? GetSideCenter(CombatSide side, CombatState combatState)
	{
		if (NCombatRoom.Instance == null)
		{
			return null;
		}
		Vector2 zero = Vector2.Zero;
		IReadOnlyList<Creature> readOnlyList = (from c in combatState.GetCreaturesOnSide(side)
			where c.IsHittable
			select c).ToList();
		foreach (Creature item in readOnlyList)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(item);
			if (creatureNode != null)
			{
				zero += creatureNode.VfxSpawnPosition;
			}
		}
		return zero / readOnlyList.Count;
	}

	public static Vector2? GetSideCenterFloor(CombatSide side, CombatState combatState)
	{
		if (NCombatRoom.Instance == null)
		{
			return null;
		}
		Vector2 zero = Vector2.Zero;
		IReadOnlyList<Creature> readOnlyList = (from c in combatState.GetCreaturesOnSide(side)
			where c.IsHittable
			select c).ToList();
		foreach (Creature item in readOnlyList)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(item);
			if (creatureNode != null)
			{
				if (creatureNode.GetBottomOfHitbox().Y > zero.Y)
				{
					zero.Y = creatureNode.GetBottomOfHitbox().Y;
				}
				zero.X += creatureNode.GetBottomOfHitbox().X;
			}
		}
		zero.X /= readOnlyList.Count;
		return zero;
	}

	public static void PlayOnSide(CombatSide side, string path, CombatState combatState)
	{
		if (!TestMode.IsOn)
		{
			Vector2? sideCenter = GetSideCenter(side, combatState);
			if (sideCenter.HasValue)
			{
				PlayVfx(sideCenter.Value, path);
			}
		}
	}

	public static void PlayOnCreatureCenters(IEnumerable<Creature> targets, string path)
	{
		foreach (Creature target in targets)
		{
			PlayOnCreatureCenter(target, path);
		}
	}

	public static void PlayOnCreatureCenter(Creature target, string path)
	{
		if (!TestMode.IsOn && !target.IsDead)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
			if (nCreature != null)
			{
				PlayVfx(nCreature.VfxSpawnPosition, path);
			}
		}
	}

	public static void PlayOnCreatures(IEnumerable<Creature> targets, string path)
	{
		foreach (Creature target in targets)
		{
			PlayOnCreature(target, path);
		}
	}

	public static void PlayOnCreature(Creature target, string path)
	{
		if (!TestMode.IsOn && NCombatRoom.Instance != null && !target.IsDead)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
			if (creatureNode != null)
			{
				PlayVfx(creatureNode.GlobalPosition, path);
			}
		}
	}

	public static void PlayVfx(Vector2 position, string path)
	{
		if (!TestMode.IsOn && NCombatRoom.Instance != null)
		{
			string scenePath = SceneHelper.GetScenePath(path);
			Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(node2D);
			node2D.GlobalPosition = position;
		}
	}

	public static Node2D? PlayNonCombatVfx(Node container, Vector2 position, string path)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		string scenePath = SceneHelper.GetScenePath(path);
		Node2D node2D = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		container.AddChildSafely(node2D);
		node2D.GlobalPosition = position;
		return node2D;
	}
}
