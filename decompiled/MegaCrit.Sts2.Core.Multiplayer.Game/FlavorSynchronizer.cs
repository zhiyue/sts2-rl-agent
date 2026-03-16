using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class FlavorSynchronizer : IDisposable
{
	private const ulong _pingDebounceMsec = 1000uL;

	private const ulong _mapPingDebounceMsec = 200uL;

	private readonly INetGameService _gameService;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	private ulong _nextAllowedPingTime;

	private readonly Dictionary<Player, NSpeechBubbleVfx?> _endTurnPingDialogues = new Dictionary<Player, NSpeechBubbleVfx>();

	private Player LocalPlayer => _playerCollection.GetPlayer(_localPlayerId);

	public event Action<ulong>? OnEndTurnPingReceived;

	public FlavorSynchronizer(INetGameService gameService, IPlayerCollection playerCollection, ulong localPlayerId)
	{
		_gameService = gameService;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		_gameService.RegisterMessageHandler<EndTurnPingMessage>(HandleEndTurnPingMessage);
		_gameService.RegisterMessageHandler<MapPingMessage>(HandleMapPingMessage);
	}

	public void Dispose()
	{
		_gameService.UnregisterMessageHandler<EndTurnPingMessage>(HandleEndTurnPingMessage);
		_gameService.UnregisterMessageHandler<MapPingMessage>(HandleMapPingMessage);
	}

	public void SendEndTurnPing()
	{
		if (Time.GetTicksMsec() >= _nextAllowedPingTime)
		{
			_gameService.SendMessage(default(EndTurnPingMessage));
			_nextAllowedPingTime = Time.GetTicksMsec() + 1000;
			CreateEndTurnPingDialogueIfNecessary(LocalPlayer);
		}
	}

	public void SendMapPing(MapCoord coord)
	{
		if (Time.GetTicksMsec() >= _nextAllowedPingTime)
		{
			_gameService.SendMessage(new MapPingMessage
			{
				coord = coord
			});
			_nextAllowedPingTime = Time.GetTicksMsec() + 200;
			CreateMapPing(coord, LocalPlayer);
		}
	}

	private void HandleEndTurnPingMessage(EndTurnPingMessage message, ulong senderId)
	{
		this.OnEndTurnPingReceived?.Invoke(senderId);
		CreateEndTurnPingDialogueIfNecessary(_playerCollection.GetPlayer(senderId));
	}

	private void CreateEndTurnPingDialogueIfNecessary(Player player)
	{
		if (NRun.Instance != null)
		{
			_endTurnPingDialogues.TryGetValue(player, out NSpeechBubbleVfx value);
			if (value != null && GodotObject.IsInstanceValid(value))
			{
				value.QueueFreeSafely();
			}
			string text = (player.Creature.IsDead ? "dead" : "alive");
			LocString locString = new LocString("characters", player.Character.Id.Entry + ".banter." + text + ".endTurnPing");
			value = NSpeechBubbleVfx.Create(locString.GetFormattedText(), player.Creature, 1.5);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(value);
			_endTurnPingDialogues[player] = value;
		}
	}

	private void HandleMapPingMessage(MapPingMessage message, ulong senderId)
	{
		CreateMapPing(message.coord, _playerCollection.GetPlayer(senderId));
	}

	private void CreateMapPing(MapCoord coord, Player player)
	{
		if (NRun.Instance != null)
		{
			NRun.Instance.GlobalUi.MapScreen.PingMapCoord(coord, player);
		}
	}
}
