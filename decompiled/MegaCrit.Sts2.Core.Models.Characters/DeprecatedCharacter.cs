using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace MegaCrit.Sts2.Core.Models.Characters;

/// <summary>
/// Placeholder character for if we can't find the character we are looking for (like if it's been deprecated).
/// </summary>
public sealed class DeprecatedCharacter : CharacterModel
{
	private MockCardPool? _mockCardPool;

	public override bool IsPlayable => false;

	public override Color NameColor => StsColors.gold;

	public override CharacterGender Gender => CharacterGender.Neutral;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	protected override string IconPath => SceneHelper.GetScenePath("ui/character_icons/ironclad_icon");

	public override int StartingHp => 1000;

	public override int StartingGold => 99;

	public override int MaxEnergy => 100;

	public override CardPoolModel CardPool => _mockCardPool ?? ModelDb.CardPool<MockCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => Array.Empty<CardModel>();

	public override IReadOnlyList<RelicModel> StartingRelics => Array.Empty<RelicModel>();

	public override float AttackAnimDelay => 0f;

	public override float CastAnimDelay => 0f;

	public override Color MapDrawingColor => new Color("462996");

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = "vfx/vfx_attack_blunt";
		return list;
	}

	public void ResetMockCardPool()
	{
		_mockCardPool = null;
	}

	public void AddToPool(CardModel card)
	{
		card.AssertCanonical();
		if (_mockCardPool == null)
		{
			_mockCardPool = (MockCardPool)ModelDb.CardPool<MockCardPool>().ToMutable();
		}
		_mockCardPool.Add(card);
	}
}
