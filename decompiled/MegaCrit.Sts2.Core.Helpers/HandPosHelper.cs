using System;
using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class HandPosHelper
{
	private const float _offsetY = -50f;

	private static readonly Vector2[][] _cardPositionData = new Vector2[10][]
	{
		new Vector2[1]
		{
			new Vector2(0f, -50f)
		},
		new Vector2[2]
		{
			new Vector2(-100f, -50f),
			new Vector2(100f, -50f)
		},
		new Vector2[3]
		{
			new Vector2(-180f, -50f),
			new Vector2(0f, -59f),
			new Vector2(180f, -50f)
		},
		new Vector2[4]
		{
			new Vector2(-240f, -25f),
			new Vector2(-80f, -50f),
			new Vector2(80f, -50f),
			new Vector2(240f, -25f)
		},
		new Vector2[5]
		{
			new Vector2(-340f, 10f),
			new Vector2(-170f, -30f),
			new Vector2(0f, -50f),
			new Vector2(170f, -30f),
			new Vector2(340f, 10f)
		},
		new Vector2[6]
		{
			new Vector2(-460f, 13f),
			new Vector2(-273f, -25f),
			new Vector2(-90f, -50f),
			new Vector2(90f, -50f),
			new Vector2(273f, -25f),
			new Vector2(460f, 13f)
		},
		new Vector2[7]
		{
			new Vector2(-534f, 18f),
			new Vector2(-365f, -14f),
			new Vector2(-189f, -39f),
			new Vector2(0f, -50f),
			new Vector2(189f, -39f),
			new Vector2(365f, -14f),
			new Vector2(534f, 18f)
		},
		new Vector2[8]
		{
			new Vector2(-565f, 28f),
			new Vector2(-400f, -14f),
			new Vector2(-231f, -39f),
			new Vector2(-80f, -50f),
			new Vector2(80f, -50f),
			new Vector2(231f, -39f),
			new Vector2(400f, -14f),
			new Vector2(565f, 28f)
		},
		new Vector2[9]
		{
			new Vector2(-600f, 37f),
			new Vector2(-445f, -2f),
			new Vector2(-300f, -29f),
			new Vector2(-150f, -45f),
			new Vector2(0f, -50f),
			new Vector2(150f, -45f),
			new Vector2(300f, -29f),
			new Vector2(445f, -2f),
			new Vector2(600f, 37f)
		},
		new Vector2[10]
		{
			new Vector2(-610f, 38f),
			new Vector2(-472f, 5f),
			new Vector2(-340f, -21f),
			new Vector2(-200f, -41f),
			new Vector2(-64f, -50f),
			new Vector2(64f, -50f),
			new Vector2(200f, -41f),
			new Vector2(340f, -21f),
			new Vector2(472f, 5f),
			new Vector2(610f, 38f)
		}
	};

	private static readonly float[][] _cardAngleData = new float[10][]
	{
		new float[1],
		new float[2] { -2f, 2f },
		new float[3] { -3f, 0f, 3f },
		new float[4] { -8f, -4f, 4f, 8f },
		new float[5] { -8f, -4f, 0f, 4f, 8f },
		new float[6] { -9f, -6f, -3f, 3f, 6f, 9f },
		new float[7] { -9f, -6f, -3f, 0f, 3f, 6f, 9f },
		new float[8] { -12f, -9f, -6f, -3f, 3f, 6f, 9f, 12f },
		new float[9] { -12f, -9f, -6f, -3f, 0f, 3f, 6f, 9f, 12f },
		new float[10] { -15f, -12f, -9f, -6f, -3f, 3f, 6f, 9f, 12f, 15f }
	};

	private static readonly Vector2 _baseScale = Vector2.One * 0.8f;

	public static Vector2 GetPosition(int handSize, int cardIndex)
	{
		if (handSize - 1 >= _cardPositionData.Length)
		{
			throw new ArgumentOutOfRangeException($"Hand size {handSize} is greater than {_cardPositionData.Length + 1}!");
		}
		if (cardIndex >= _cardPositionData[handSize - 1].Length)
		{
			throw new ArgumentOutOfRangeException($"Card index {cardIndex} is greater than {_cardPositionData[handSize - 1].Length}!");
		}
		return _cardPositionData[handSize - 1][cardIndex];
	}

	public static float GetAngle(int handSize, int cardIndex)
	{
		if (handSize - 1 >= _cardPositionData.Length)
		{
			throw new ArgumentOutOfRangeException($"Hand size {handSize} is greater than {_cardPositionData.Length + 1}!");
		}
		if (cardIndex >= _cardPositionData[handSize - 1].Length)
		{
			throw new ArgumentOutOfRangeException($"Card index {cardIndex} is greater than {_cardPositionData[handSize - 1].Length}!");
		}
		return _cardAngleData[handSize - 1][cardIndex];
	}

	public static Vector2 GetScale(int handSize)
	{
		return _baseScale * handSize switch
		{
			8 => 0.95f, 
			9 => 0.9f, 
			10 => 0.85f, 
			11 => 0.8f, 
			12 => 0.75f, 
			_ => 1f, 
		};
	}
}
