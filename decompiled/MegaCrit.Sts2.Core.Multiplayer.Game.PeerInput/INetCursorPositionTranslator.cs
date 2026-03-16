using Godot;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

public interface INetCursorPositionTranslator
{
	Vector2 GetNetPositionFromScreenPosition(Vector2 screenPosition);

	Vector2 GetScreenPositionFromNetPosition(Vector2 netPosition);
}
