using System;
using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

/// <summary>
/// Utilities to help with transforming mouse position from screen coordinates to a normalized position suitable for
/// syncing mouse positions across various aspect ratios.
/// </summary>
public static class NetCursorHelper
{
	public static readonly QuantizeParams quantizeParams = new QuantizeParams(-3f, 3f, 16);

	public static Vector2 GetNormalizedPosition(Vector2 mouseScreenPos, Control? rootNode)
	{
		if (rootNode == null)
		{
			if (TestMode.IsOn)
			{
				return mouseScreenPos;
			}
			throw new InvalidOperationException("Root node should only be null in tests!");
		}
		Vector2 vector = rootNode.GetGlobalTransformWithCanvas() * mouseScreenPos;
		Vector2 vector2 = new Vector2(960f, 540f);
		return (vector - rootNode.Size / 2f) / vector2;
	}

	public static Vector2 GetControlSpacePosition(Vector2 normalizedCursorPosition, Control? rootNode)
	{
		if (rootNode == null)
		{
			if (TestMode.IsOn)
			{
				return normalizedCursorPosition;
			}
			throw new InvalidOperationException("Root node should only be null in tests!");
		}
		Vector2 vector = new Vector2(960f, 540f);
		return normalizedCursorPosition * vector + rootNode.Size / 2f;
	}
}
