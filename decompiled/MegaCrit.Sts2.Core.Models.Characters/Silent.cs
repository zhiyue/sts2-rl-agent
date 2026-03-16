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

public sealed class Silent : CharacterModel
{
	public const string shivTrigger = "Shiv";

	public const string energyColorName = "silent";

	public override Color NameColor => StsColors.green;

	public override CharacterGender Gender => CharacterGender.Feminine;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override int StartingHp => 70;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<SilentCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<SilentRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<SilentPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[12]
	{
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<Neutralize>(),
		ModelDb.Card<Survivor>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<RingOfTheSnake>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new Color("004f04FF");

	public override Color DialogueColor => new Color("284719");

	public override Color MapDrawingColor => new Color("2F6729");

	public override Color RemoteTargetingLineColor => new Color("2EBD5EFF");

	public override Color RemoteTargetingLineOutline => new Color("004f04FF");

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 4;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_dagger_spray";
		num2++;
		span[num2] = "vfx/vfx_flying_slash";
		num2++;
		span[num2] = "vfx/vfx_dramatic_stab";
		num2++;
		span[num2] = "vfx/vfx_dagger_throw";
		return list;
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("shiv");
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
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Shiv", animState5);
		creatureAnimator.AddAnyState("Relaxed", animState6);
		return creatureAnimator;
	}
}
