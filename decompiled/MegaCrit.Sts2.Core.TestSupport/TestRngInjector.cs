using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.TestSupport;

public static class TestRngInjector
{
	private static RelicModel? _relicOverride;

	private static RelicRarity? _relicRarityOverride;

	private static Action<List<CardModel>>? _initialShuffleOverride;

	private static List<CardModel>? _combatCardGenerationOverride;

	public static void SetRelicOverride<T>() where T : RelicModel
	{
		_relicOverride = ModelDb.Relic<T>();
	}

	public static RelicModel? ConsumeRelicOverride()
	{
		RelicModel relicOverride = _relicOverride;
		_relicOverride = null;
		return relicOverride;
	}

	public static void SetRelicRarityOverride(RelicRarity relicRarity)
	{
		_relicRarityOverride = relicRarity;
	}

	public static RelicRarity? GetRelicRarityOverride()
	{
		return _relicRarityOverride;
	}

	/// <summary>
	/// Force specific cards to show up for CardFactory.GetDistinctForCombat.
	/// </summary>
	public static void SetCombatCardGenerationOverride(List<CardModel> cards)
	{
		_combatCardGenerationOverride = cards;
	}

	/// <summary>
	/// Consume the combat cards generation override and clear it out.
	/// </summary>
	public static List<CardModel>? ConsumeCombatCardGenerationOverride()
	{
		List<CardModel> combatCardGenerationOverride = _combatCardGenerationOverride;
		_combatCardGenerationOverride = null;
		return combatCardGenerationOverride;
	}

	/// <summary>
	/// Force a specific order for the initial shuffle.
	/// </summary>
	public static void SetInitialShuffleOverride(Action<List<CardModel>> reorder)
	{
		_initialShuffleOverride = reorder;
	}

	/// <summary>
	/// Consume the initial shuffle override and clear it out.
	/// </summary>
	public static Action<List<CardModel>>? ConsumeInitialShuffleOverride()
	{
		Action<List<CardModel>> initialShuffleOverride = _initialShuffleOverride;
		_initialShuffleOverride = null;
		return initialShuffleOverride;
	}

	public static void Cleanup()
	{
		_relicOverride = null;
		_relicRarityOverride = null;
		_initialShuffleOverride = null;
		_combatCardGenerationOverride = null;
	}
}
