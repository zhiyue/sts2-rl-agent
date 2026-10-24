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

	/// <summary>
	/// The color of the card back when viewing a card from this pool in the Run History screen.
	/// </summary>
	public abstract Color DeckEntryCardColor { get; }

	/// <summary>
	/// The color of the outline of the text in this card's Energy UI.
	/// It must blend with the energy icon of this card!
	/// </summary>
	public virtual Color EnergyOutlineColor => new Color("5C5440");

	public string EnergyIconPath => EnergyIconHelper.GetPath(EnergyColorName);

	/// <summary>
	/// Get every card in this pool (ignores Unlocks/Epoch state).
	/// </summary>
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

	/// <summary>
	/// Get the IDs of every card in this pool (ignores Unlocks/Epoch state).
	/// </summary>
	public IEnumerable<ModelId> AllCardIds => _allCardIds ?? (_allCardIds = AllCards.Select((CardModel c) => c.Id).ToHashSet());

	/// <summary>
	/// Is this a colorless card pool?
	/// False for card pools that are associated with a specific character (like <see cref="T:MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool" />), true
	/// for other card pools (like <see cref="T:MegaCrit.Sts2.Core.Models.CardPools.ColorlessCardPool" /> or <see cref="T:MegaCrit.Sts2.Core.Models.CardPools.EventCardPool" />).
	/// </summary>
	public abstract bool IsColorless { get; }

	public override bool ShouldReceiveCombatHooks => false;

	/// <summary>
	/// Generates every card in this pool (ignores Unlocks/Epoch state).
	/// Overridden in subclasses, but should only be called once by <see cref="P:MegaCrit.Sts2.Core.Models.CardPoolModel.AllCards" /> so it can be cached.
	/// </summary>
	protected abstract CardModel[] GenerateAllCards();

	/// <summary>
	/// Returns every card in this pool that the player has unlocked.
	/// This excludes cards that haven't been unlocked via certain epochs. We also can filter cards based on whether
	/// they are allowed to appear during a singleplayer or multiplayer run.
	/// <param name="unlockState">Used to filter cards through unlocked epochs.</param>
	/// <param name="multiplayerConstraint">Used to filter cards based on if this is a singleplayer or multiplayer run.</param>
	/// </summary>
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

	/// <summary>
	/// Returns a list of cards but prunes out cards if the associated Epochs aren't revealed.
	/// </summary>
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
