using System;
using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class Ease
{
	public enum Functions
	{
		QuadIn,
		QuadOut,
		QuadInOut,
		CubicIn,
		CubicOut,
		CubicInOut,
		QuartIn,
		QuartOut,
		QuartInOut,
		QuintIn,
		QuinOut,
		QuinInOut,
		SineIn,
		SineOut,
		SineInOut,
		CircIn,
		CircOut,
		CircInOut,
		ExpoIn,
		ExpoOut,
		ExpoInOut,
		ElasticIn,
		ElasticOut,
		ElasticInOut,
		BackIn,
		BackOut,
		BackInOut,
		BounceIn,
		BounceOut,
		BounceInOut
	}

	private const float _tolerance = 0.001f;

	private const float _pi = (float)Math.PI;

	private const float _halfPi = (float)Math.PI / 2f;

	public static float Interpolate(float p, Functions function)
	{
		return function switch
		{
			Functions.QuadOut => QuadOut(p), 
			Functions.QuadIn => QuadIn(p), 
			Functions.QuadInOut => QuadInOut(p), 
			Functions.CubicIn => CubicIn(p), 
			Functions.CubicOut => CubicOut(p), 
			Functions.CubicInOut => CubicInOut(p), 
			Functions.QuartIn => QuartIn(p), 
			Functions.QuartOut => QuartOut(p), 
			Functions.QuartInOut => QuartInOut(p), 
			Functions.QuintIn => QuintIn(p), 
			Functions.QuinOut => QuintOut(p), 
			Functions.QuinInOut => QuintInOut(p), 
			Functions.SineIn => SineIn(p), 
			Functions.SineOut => SineOut(p), 
			Functions.SineInOut => SineInOut(p), 
			Functions.CircIn => CircIn(p), 
			Functions.CircOut => CircOut(p), 
			Functions.CircInOut => CircInOut(p), 
			Functions.ExpoIn => ExpoIn(p), 
			Functions.ExpoOut => ExpoOut(p), 
			Functions.ExpoInOut => ExpoInOut(p), 
			Functions.ElasticIn => ElasticIn(p), 
			Functions.ElasticOut => ElasticOut(p), 
			Functions.ElasticInOut => ElasticInOut(p), 
			Functions.BackIn => BackIn(p), 
			Functions.BackOut => BackOut(p), 
			Functions.BackInOut => BackInOut(p), 
			Functions.BounceIn => BounceIn(p), 
			Functions.BounceOut => BounceOut(p), 
			Functions.BounceInOut => BounceInOut(p), 
			_ => Linear(p), 
		};
	}

	public static float Linear(float p)
	{
		return p;
	}

	public static float QuadIn(float p)
	{
		return p * p;
	}

	public static float QuadOut(float p)
	{
		return 0f - p * (p - 2f);
	}

	public static float QuadInOut(float p)
	{
		if (p < 0.5f)
		{
			return 2f * p * p;
		}
		return -2f * p * p + 4f * p - 1f;
	}

	public static float CubicIn(float p)
	{
		return p * p * p;
	}

	public static float CubicOut(float p)
	{
		float num = p - 1f;
		return num * num * num + 1f;
	}

	public static float CubicInOut(float p)
	{
		if (p < 0.5f)
		{
			return 4f * p * p * p;
		}
		float num = 2f * p - 2f;
		return 0.5f * num * num * num + 1f;
	}

	public static float QuartIn(float p)
	{
		return p * p * p * p;
	}

	public static float QuartOut(float p)
	{
		float num = p - 1f;
		return num * num * num * (1f - p) + 1f;
	}

	public static float QuartInOut(float p)
	{
		if (p < 0.5f)
		{
			return 8f * p * p * p * p;
		}
		float num = p - 1f;
		return -8f * num * num * num * num + 1f;
	}

	public static float QuintIn(float p)
	{
		return p * p * p * p * p;
	}

	public static float QuintOut(float p)
	{
		float num = p - 1f;
		return num * num * num * num * num + 1f;
	}

	public static float QuintInOut(float p)
	{
		if (p < 0.5f)
		{
			return 16f * p * p * p * p * p;
		}
		float num = 2f * p - 2f;
		return 0.5f * num * num * num * num * num + 1f;
	}

	public static float SineIn(float p)
	{
		return Mathf.Sin((p - 1f) * ((float)Math.PI / 2f)) + 1f;
	}

	public static float SineOut(float p)
	{
		return Mathf.Sin(p * ((float)Math.PI / 2f));
	}

	public static float SineInOut(float p)
	{
		return 0.5f * (1f - Mathf.Cos(p * (float)Math.PI));
	}

	public static float CircIn(float p)
	{
		return 1f - Mathf.Sqrt(1f - p * p);
	}

	public static float CircOut(float p)
	{
		return Mathf.Sqrt((2f - p) * p);
	}

	public static float CircInOut(float p)
	{
		if (p < 0.5f)
		{
			return 0.5f * (1f - Mathf.Sqrt(1f - 4f * (p * p)));
		}
		return 0.5f * (Mathf.Sqrt((0f - (2f * p - 3f)) * (2f * p - 1f)) + 1f);
	}

	public static float ExpoIn(float p)
	{
		if (p != 0f)
		{
			return Mathf.Pow(2f, 10f * (p - 1f));
		}
		return p;
	}

	public static float ExpoOut(float p)
	{
		if (!(Math.Abs(p - 1f) < 0.001f))
		{
			return 1f - Mathf.Pow(2f, -10f * p);
		}
		return p;
	}

	public static float ExpoInOut(float p)
	{
		if (p == 0f || Math.Abs(p - 1f) < 0.001f)
		{
			return p;
		}
		if (p < 0.5f)
		{
			return 0.5f * Mathf.Pow(2f, 20f * p - 10f);
		}
		return -0.5f * Mathf.Pow(2f, -20f * p + 10f) + 1f;
	}

	public static float ElasticIn(float p)
	{
		return Mathf.Sin(20.420353f * p) * Mathf.Pow(2f, 10f * (p - 1f));
	}

	public static float ElasticOut(float p)
	{
		return Mathf.Sin(-20.420353f * (p + 1f)) * Mathf.Pow(2f, -10f * p) + 1f;
	}

	public static float ElasticInOut(float p)
	{
		if (p < 0.5f)
		{
			return 0.5f * Mathf.Sin(20.420353f * (2f * p)) * Mathf.Pow(2f, 10f * (2f * p - 1f));
		}
		return 0.5f * (Mathf.Sin(-20.420353f * (2f * p - 1f + 1f)) * Mathf.Pow(2f, -10f * (2f * p - 1f)) + 2f);
	}

	public static float BackIn(float p, float strength = 1f)
	{
		float num = strength * 1.70158f;
		return p * p * ((num + 1f) * p - num);
	}

	public static float BackOut(float p, float strength = 1f)
	{
		float num = strength * 1.70158f;
		return 1f + num * (p - 1f) * (p - 1f) * ((num + 1f) * (p - 1f) + num);
	}

	public static float BackInOut(float p, float strength = 1f)
	{
		float num = strength * 1.70158f;
		p *= 2f;
		if (p < 1f)
		{
			return 0.5f * (p * p * (((num *= 1.525f) + 1f) * p - num));
		}
		return 0.5f * ((p -= 2f) * p * (((num *= 1.525f) + 1f) * p + num) + 2f);
	}

	public static float BounceIn(float p)
	{
		return 1f - BounceOut(1f - p);
	}

	public static float BounceOut(float p)
	{
		if (p < 0.36363637f)
		{
			return 121f * p * p / 16f;
		}
		if (p < 0.72727275f)
		{
			return 9.075f * p * p - 9.9f * p + 3.4f;
		}
		if (p < 0.9f)
		{
			return 12.066482f * p * p - 19.635458f * p + 8.898061f;
		}
		return 10.8f * p * p - 20.52f * p + 10.72f;
	}

	public static float BounceInOut(float p)
	{
		if (p < 0.5f)
		{
			return 0.5f * BounceIn(p * 2f);
		}
		return 0.5f * BounceOut(p * 2f - 1f) + 0.5f;
	}
}
