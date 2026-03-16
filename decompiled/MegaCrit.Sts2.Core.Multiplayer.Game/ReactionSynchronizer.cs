using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Nodes.Reaction;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class ReactionSynchronizer : IDisposable
{
	private readonly NReactionContainer _container;

	public INetGameService NetService { get; }

	public ReactionSynchronizer(INetGameService netService, NReactionContainer container)
	{
		NetService = netService;
		_container = container;
		NetService.RegisterMessageHandler<ReactionMessage>(HandleReactionMessage);
	}

	public void Dispose()
	{
		NetService.UnregisterMessageHandler<ReactionMessage>(HandleReactionMessage);
	}

	public void SendLocalReaction(ReactionType type, Vector2 mouseScreenPos)
	{
		ReactionMessage message = new ReactionMessage
		{
			type = type,
			normalizedPosition = NetCursorHelper.GetNormalizedPosition(mouseScreenPos, _container)
		};
		NetService.SendMessage(message);
	}

	private void HandleReactionMessage(ReactionMessage message, ulong senderId)
	{
		_container.DoRemoteReaction(message.type, NetCursorHelper.GetControlSpacePosition(message.normalizedPosition, _container));
	}
}
