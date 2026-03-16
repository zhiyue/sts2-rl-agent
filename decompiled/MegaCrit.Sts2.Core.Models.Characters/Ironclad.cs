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

public sealed class Ironclad : CharacterModel
{
	public const string energyColorName = "ironclad";

	public override CharacterGender Gender => CharacterGender.Masculine;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override Color NameColor => StsColors.red;

	public override int StartingHp => 80;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<IroncladCardPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<Bash>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<BurningBlood>());

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	public override Color EnergyLabelOutlineColor => new Color("801212FF");

	public override Color DialogueColor => new Color("590700");

	public override Color MapDrawingColor => new Color("CB282B");

	public override Color RemoteTargetingLineColor => new Color("E15847FF");

	public override Color RemoteTargetingLineOutline => new Color("801212FF");

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 5;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "vfx/vfx_attack_blunt";
		num2++;
		span[num2] = "vfx/vfx_heavy_blunt";
		num2++;
		span[num2] = "vfx/vfx_attack_slash";
		num2++;
		span[num2] = "vfx/vfx_bloody_impact";
		num2++;
		span[num2] = "vfx/vfx_rock_shatter";
		return list;
	}
}
