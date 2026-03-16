using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

public class MapDrawingMessage : INetMessage, IPacketSerializable
{
	public static readonly int maxEventCount = (int)Math.Pow(2.0, 4.0) - 1;

	private const int _listBits = 4;

	private List<NetMapDrawingEvent> _events = new List<NetMapDrawingEvent>();

	public DrawingMode? drawingMode;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => NetTransferMode.Unreliable;

	public LogLevel LogLevel => LogLevel.VeryDebug;

	public IReadOnlyList<NetMapDrawingEvent> Events => _events;

	public bool TryAddEvent(NetMapDrawingEvent ev)
	{
		if (_events.Count >= maxEventCount)
		{
			return false;
		}
		_events.Add(ev);
		return true;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteList(_events, 4);
		writer.WriteBool(drawingMode.HasValue);
		if (drawingMode.HasValue)
		{
			writer.WriteEnum(drawingMode.Value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		_events = reader.ReadList<NetMapDrawingEvent>(4);
		if (reader.ReadBool())
		{
			drawingMode = reader.ReadEnum<DrawingMode>();
		}
	}
}
