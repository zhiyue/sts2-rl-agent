using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class CharacterCards : ModifierModel
{
	private ModelId? _characterModel;

	public override LocString Title => ModelDb.GetById<CharacterModel>(CharacterModel).CardsModifierTitle;

	public override LocString Description => ModelDb.GetById<CharacterModel>(CharacterModel).CardsModifierDescription;

	[SavedProperty]
	public ModelId CharacterModel
	{
		get
		{
			return _characterModel ?? throw new InvalidOperationException("CharacterCards modifier used without CharacterModel set!");
		}
		set
		{
			AssertMutable();
			_characterModel = value;
		}
	}

	public override IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
	{
		CardPoolModel cardPool = player.Character.CardPool;
		CardModel[] array = options.ToArray();
		if (array.Any((CardModel c) => c.Pool != cardPool))
		{
			return array;
		}
		return array.Concat(ModelDb.GetById<CharacterModel>(CharacterModel).CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint));
	}

	public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
		{
			return options;
		}
		return options.WithCustomPool(options.GetPossibleCards(player).Concat(ModelDb.GetById<CharacterModel>(CharacterModel).CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)));
	}

	public override bool IsEquivalent(ModifierModel other)
	{
		if (base.IsEquivalent(other))
		{
			return ((CharacterCards)other)._characterModel == _characterModel;
		}
		return false;
	}
}
