using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models;

public abstract class CardPoolModel : AbstractModel, IPoolModel
{
	private CardModel[]? _allCards;

	private HashSet<ModelId>? _allCardIds;

	public abstract string Title { get; }

	public abstract string EnergyColorName { get; }

	public abstract string CardFrameMaterialPath { get; }

	public string FrameMaterialPath => "res://materials/cards/frames/" + CardFrameMaterialPath + "_mat.tres";

	public Material FrameMaterial => PreloadManager.Cache.GetMaterial(FrameMaterialPath);

	public abstract Color DeckEntryCardColor { get; }

	public virtual Color EnergyOutlineColor => new Color("5C5440");

	public string EnergyIconPath => EnergyIconHelper.GetPath(EnergyColorName);

	public virtual IEnumerable<CardModel> AllCards
	{
		get
		{
			if (_allCards == null)
			{
				_allCards = GenerateAllCards();
				_allCards = ModHelper.ConcatModelsFromMods(this, _allCards).ToArray();
			}
			return _allCards;
		}
	}

	public IEnumerable<ModelId> AllCardIds => _allCardIds ?? (_allCardIds = AllCards.Select((CardModel c) => c.Id).ToHashSet());

	public abstract bool IsColorless { get; }

	public override bool ShouldReceiveCombatHooks => false;

	protected abstract CardModel[] GenerateAllCards();

	public IEnumerable<CardModel> GetUnlockedCards(UnlockState unlockState, CardMultiplayerConstraint multiplayerConstraint)
	{
		List<CardModel> list = FilterThroughEpochs(unlockState, AllCards).ToList();
		switch (multiplayerConstraint)
		{
		case CardMultiplayerConstraint.MultiplayerOnly:
			list.RemoveAll((CardModel c) => c.MultiplayerConstraint == CardMultiplayerConstraint.SingleplayerOnly);
			break;
		case CardMultiplayerConstraint.SingleplayerOnly:
			list.RemoveAll((CardModel c) => c.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly);
			break;
		}
		return list;
	}

	protected virtual IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
	{
		return cards.ToList();
	}

	public CardPoolModel ToMutable()
	{
		AssertCanonical();
		return (CardPoolModel)MutableClone();
	}
}
