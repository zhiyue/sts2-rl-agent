using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Characters;

public sealed class Necrobinder : CharacterModel
{
	private const string _summonTrigger = "summonTrigger";

	public const string energyColorName = "necrobinder";

	public const string healOstyPath = "vfx/vfx_heal_osty";

	private const string _ostyVisualsPath = "creature_visuals/osty";

	public override Color NameColor => StsColors.purple;

	public override CharacterGender Gender => CharacterGender.Feminine;

	protected override CharacterModel UnlocksAfterRunAs => ModelDb.Character<Regent>();

	public override int StartingHp => 66;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<NecrobinderCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<NecrobinderRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<NecrobinderPotionPool>();

	protected override IEnumerable<string> ExtraAssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2]
	{
		SceneHelper.GetScenePath("vfx/vfx_heal_osty"),
		SceneHelper.GetScenePath("creature_visuals/osty")
	});

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<StrikeNecrobinder>(),
		ModelDb.Card<StrikeNecrobinder>(),
		ModelDb.Card<StrikeNecrobinder>(),
		ModelDb.Card<StrikeNecrobinder>(),
		ModelDb.Card<DefendNecrobinder>(),
		ModelDb.Card<DefendNecrobinder>(),
		ModelDb.Card<DefendNecrobinder>(),
		ModelDb.Card<DefendNecrobinder>(),
		ModelDb.Card<Bodyguard>(),
		ModelDb.Card<Unleash>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<BoundPhylactery>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.4f;

	public override Color EnergyLabelOutlineColor => new Color("702D6FFF");

	public override Color DialogueColor => new Color("6B4658");

	public override Color MapDrawingColor => new Color("AC0486");

	public override Color RemoteTargetingLineColor => new Color("FD98C9FF");

	public override Color RemoteTargetingLineOutline => new Color("702D6FFF");

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 4;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_thrash";
		num2++;
		span[num2] = "vfx/vfx_heavy_blunt";
		num2++;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_bloody_impact";
		return list;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("cast_mighty");
		AnimState animState6 = new AnimState("relaxed_loop", isLooping: true);
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.AddBranch("Idle", animState);
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("summonTrigger", animState2);
		creatureAnimator.AddAnyState("Cast", animState5);
		creatureAnimator.AddAnyState("Relaxed", animState6);
		creatureAnimator.AddAnyState("PowerUp", animState5);
		return creatureAnimator;
	}

	public static string GetSummonAnimIfApplicable(CharacterModel character)
	{
		if (!(character is Necrobinder))
		{
			return "Cast";
		}
		return "summonTrigger";
	}

	public static float GetSummonDelayIfApplicable(CharacterModel character)
	{
		if (!(character is Necrobinder))
		{
			return character.CastAnimDelay;
		}
		return 0.25f;
	}
}
