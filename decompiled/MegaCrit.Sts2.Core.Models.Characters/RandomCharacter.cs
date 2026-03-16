using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Characters;

public sealed class RandomCharacter : CharacterModel
{
	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel? UnlocksAfterRunAs { get; }

	public override Color NameColor => StsColors.gold;

	public override int StartingHp => 1;

	public override int StartingGold => 1;

	public override CardPoolModel CardPool => ModelDb.CardPool<IroncladCardPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();

	public override IEnumerable<CardModel> StartingDeck => new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[10]
	{
		ModelDb.Card<StrikeIronclad>(),
		ModelDb.Card<StrikeSilent>(),
		ModelDb.Card<StrikeRegent>(),
		ModelDb.Card<StrikeNecrobinder>(),
		ModelDb.Card<StrikeDefect>(),
		ModelDb.Card<DefendIronclad>(),
		ModelDb.Card<DefendSilent>(),
		ModelDb.Card<DefendRegent>(),
		ModelDb.Card<DefendNecrobinder>(),
		ModelDb.Card<DefendDefect>()
	});

	public override IReadOnlyList<RelicModel> StartingRelics => new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<Circlet>());

	protected override string CharacterSelectIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_random.png");

	protected override string CharacterSelectLockedIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_random_locked.png");

	public override float AttackAnimDelay => 0f;

	public override float CastAnimDelay => 0f;

	public override Color EnergyLabelOutlineColor => Colors.Magenta;

	public override Color DialogueColor => Colors.Magenta;

	public override Color MapDrawingColor => Colors.Magenta;

	public override Color RemoteTargetingLineColor => Colors.Magenta;

	public override Color RemoteTargetingLineOutline => Colors.Magenta;

	public override List<string> GetArchitectAttackVfx()
	{
		return new List<string>();
	}
}
