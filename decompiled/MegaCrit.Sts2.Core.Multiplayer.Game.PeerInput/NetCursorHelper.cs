using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public static class NetCursorHelper
{
	public static readonly QuantizeParams quantizeParams = new QuantizeParams(-3f, 3f, 16);

	public static Vector2 GetNormalizedPosition(Vector2 mouseScreenPos, Control rootNode)
	{
		Vector2 vector = rootNode.GetGlobalTransformWithCanvas() * mouseScreenPos;
		Vector2 vector2 = new Vector2(960f, 540f);
		return (vector - rootNode.Size / 2f) / vector2;
	}

	public static Vector2 GetControlSpacePosition(Vector2 normalizedCursorPosition, Control rootNode)
	{
		Vector2 vector = new Vector2(960f, 540f);
		return normalizedCursorPosition * vector + rootNode.Size / 2f;
	}
}
