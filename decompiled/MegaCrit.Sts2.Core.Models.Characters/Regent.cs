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

public sealed class Regent : CharacterModel
{
	public const string energyColorName = "regent";

	public override Color NameColor => StsColors.orange;

	public override CharacterGender Gender => CharacterGender.Masculine;

	protected override CharacterModel UnlocksAfterRunAs => ModelDb.Character<Silent>();

	public override int StartingHp => 75;

	public override int StartingGold => 99;

	public override bool ShouldAlwaysShowStarCounter => true;

	public override CardPoolModel CardPool => ModelDb.CardPool<RegentCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<RegentRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<RegentPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<StrikeRegent>(),
		ModelDb.Card<StrikeRegent>(),
		ModelDb.Card<StrikeRegent>(),
		ModelDb.Card<StrikeRegent>(),
		ModelDb.Card<DefendRegent>(),
		ModelDb.Card<DefendRegent>(),
		ModelDb.Card<DefendRegent>(),
		ModelDb.Card<DefendRegent>(),
		ModelDb.Card<FallingStar>(),
		ModelDb.Card<Venerate>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<DivineRight>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new Color("784000FF");

	public override Color DialogueColor => new Color("52371D");

	public override Color MapDrawingColor => new Color("935206");

	public override Color RemoteTargetingLineColor => new Color("BFA270FF");

	public override Color RemoteTargetingLineOutline => new Color("784000FF");

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 5;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_starry_impact";
		num2++;
		span[num2] = "vfx/vfx_attack_blunt";
		num2++;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_heavy_blunt";
		num2++;
		span[num2] = "vfx/vfx_attack_lightning";
		return list;
	}
}
