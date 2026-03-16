using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Characters;

public sealed class Defect : CharacterModel
{
	public const string energyColorName = "defect";

	public override Color NameColor => StsColors.blue;

	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel UnlocksAfterRunAs => ModelDb.Character<Necrobinder>();

	public override int StartingHp => 75;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<DefectCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<DefectRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<DefectPotionPool>();

	public Vector2 EyelineOffset => new Vector2(34f, -30f);

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<StrikeDefect>(),
		ModelDb.Card<StrikeDefect>(),
		ModelDb.Card<StrikeDefect>(),
		ModelDb.Card<StrikeDefect>(),
		ModelDb.Card<DefendDefect>(),
		ModelDb.Card<DefendDefect>(),
		ModelDb.Card<DefendDefect>(),
		ModelDb.Card<DefendDefect>(),
		ModelDb.Card<Zap>(),
		ModelDb.Card<Dualcast>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<CrackedCore>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new Color("163E64FF");

	public override int BaseOrbSlotCount => 3;

	public override Color DialogueColor => new Color("13446B");

	public override Color MapDrawingColor => new Color("0D638C");

	public override Color RemoteTargetingLineColor => new Color("70B6EDFF");

	public override Color RemoteTargetingLineOutline => new Color("163E64FF");

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 5;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_attack_lightning";
		num2++;
		span[num2] = "vfx/vfx_attack_blunt";
		num2++;
		span[num2] = "vfx/vfx_scratch";
		num2++;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_heavy_blunt";
		return list;
	}
}
