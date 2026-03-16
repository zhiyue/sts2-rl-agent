using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public class HoveredModelTracker
{
	private readonly PeerInputSynchronizer _inputSynchronizer;

	private CardModel? _localSelectedCard;

	private PotionModel? _localSelectedPotion;

	private CardModel? _localHoveredCard;

	private RelicModel? _localHoveredRelic;

	private PotionModel? _localHoveredPotion;

	private readonly List<AbstractModel?> _hoveredModels = new List<AbstractModel>();

	private readonly IPlayerCollection _playerCollection;

	public event Action<ulong>? HoverChanged;

	public HoveredModelTracker(PeerInputSynchronizer inputSynchronizer, IPlayerCollection playerCollection)
	{
		_inputSynchronizer = inputSynchronizer;
		_playerCollection = playerCollection;
		foreach (Player player in playerCollection.Players)
		{
			_hoveredModels.Add(null);
		}
		_inputSynchronizer.StateChanged += OnPlayerStateChanged;
		_inputSynchronizer.StateRemoved += OnPlayerStateRemoved;
	}

	private void OnPlayerStateChanged(ulong playerId)
	{
		Player player = _playerCollection.GetPlayer(playerId);
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		if (playerSlotIndex < _hoveredModels.Count)
		{
			HoveredModelData hoveredModelData = _inputSynchronizer.GetHoveredModelData(playerId);
			AbstractModel abstractModel = hoveredModelData.type switch
			{
				HoveredModelType.None => null, 
				HoveredModelType.Card => hoveredModelData.hoveredCombatCard?.ToCardModelOrNull(), 
				HoveredModelType.Relic => (hoveredModelData.hoveredRelicIndex < player.Relics.Count) ? player.Relics[hoveredModelData.hoveredRelicIndex.Value] : null, 
				HoveredModelType.Potion => (hoveredModelData.hoveredPotionIndex < player.PotionSlots.Count && hoveredModelData.hoveredPotionIndex >= 0) ? player.GetPotionAtSlotIndex(hoveredModelData.hoveredPotionIndex.Value) : null, 
				_ => throw new InvalidOperationException($"Unsupported hover type {hoveredModelData.type}"), 
			};
			if (abstractModel == null && hoveredModelData.type != HoveredModelType.None)
			{
				abstractModel = ModelDb.GetById<AbstractModel>(hoveredModelData.hoveredModelId);
			}
			AbstractModel abstractModel2 = _hoveredModels[playerSlotIndex];
			if (abstractModel2 != abstractModel)
			{
				_hoveredModels[playerSlotIndex] = abstractModel;
				this.HoverChanged?.Invoke(playerId);
			}
		}
	}

	private void OnPlayerStateRemoved(ulong playerId)
	{
		Player player = _playerCollection.GetPlayer(playerId);
		int playerSlotIndex = _playerCollection.GetPlayerSlotIndex(player);
		if (_hoveredModels[playerSlotIndex] != null)
		{
			_hoveredModels[playerSlotIndex] = null;
			this.HoverChanged?.Invoke(playerId);
		}
	}

	public AbstractModel? GetHoveredModel(ulong playerId)
	{
		return _hoveredModels[_playerCollection.GetPlayerSlotIndex(_playerCollection.GetPlayer(playerId))];
	}

	public void OnLocalCardSelected(CardModel cardModel)
	{
		_localSelectedCard = cardModel;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalCardDeselected()
	{
		_localSelectedCard = null;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalPotionSelected(PotionModel potionModel)
	{
		_localSelectedPotion = potionModel;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalPotionDeselected()
	{
		_localSelectedPotion = null;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalCardHovered(CardModel cardModel)
	{
		_localHoveredCard = cardModel;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalCardUnhovered()
	{
		_localHoveredCard = null;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalRelicHovered(RelicModel relicModel)
	{
		_localHoveredRelic = relicModel;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalRelicUnhovered()
	{
		_localHoveredRelic = null;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalPotionHovered(PotionModel potionModel)
	{
		_localHoveredPotion = potionModel;
		SynchronizeLocalHoveredModel();
	}

	public void OnLocalPotionUnhovered()
	{
		_localHoveredPotion = null;
		SynchronizeLocalHoveredModel();
	}

	private void SynchronizeLocalHoveredModel()
	{
		AbstractModel model = (AbstractModel)(_localSelectedCard ?? _localSelectedPotion ?? _localHoveredCard ?? ((object)_localHoveredPotion) ?? ((object)_localHoveredRelic));
		_inputSynchronizer.SyncLocalHoveredModel(model);
	}
}
