using System;
using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class Ease
{
	/// <summary>
	/// Easing function enums
	/// </summary>
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

	/// <summary>
	/// Used for safe floating point comparisons with pixel values on a 4K screen.
	/// </summary>
	private const float _tolerance = 0.001f;

	/// <summary>
	/// Constant Pi.
	/// </summary>
	private const float _pi = (float)Math.PI;

	/// <summary>
	/// Constant Pi / 2.
	/// </summary>
	private const float _halfPi = (float)Math.PI / 2f;

	/// <summary>
	/// Interpolate using the specified function.
	/// </summary>
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

	/// <summary>
	/// Modeled after the line y = x
	/// </summary>
	public static float Linear(float p)
	{
		return p;
	}

	/// <summary>
	/// Modeled after the parabola y = x^2
	/// </summary>
	public static float QuadIn(float p)
	{
		return p * p;
	}

	/// <summary>
	/// Modeled after the parabola y = -x^2 + 2x
	/// </summary>
	public static float QuadOut(float p)
	{
		return 0f - p * (p - 2f);
	}

	/// <summary>
	/// Modeled after the piecewise quadratic
	/// y = (1/2)((2x)^2)             ; [0, 0.5)
	/// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
	/// </summary>
	public static float QuadInOut(float p)
	{
		if (p < 0.5f)
		{
			return 2f * p * p;
		}
		return -2f * p * p + 4f * p - 1f;
	}

	/// <summary>
	/// Modeled after the cubic y = x^3
	/// </summary>
	public static float CubicIn(float p)
	{
		return p * p * p;
	}

	/// <summary>
	/// Modeled after the cubic y = (x - 1)^3 + 1
	/// </summary>
	public static float CubicOut(float p)
	{
		float num = p - 1f;
		return num * num * num + 1f;
	}

	/// <summary>
	/// Modeled after the piecewise cubic
	/// y = (1/2)((2x)^3)       ; [0, 0.5)
	/// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
	/// </summary>
	public static float CubicInOut(float p)
	{
		if (p < 0.5f)
		{
			return 4f * p * p * p;
		}
		float num = 2f * p - 2f;
		return 0.5f * num * num * num + 1f;
	}

	/// <summary>
	/// Modeled after the quartic x^4
	/// </summary>
	public static float QuartIn(float p)
	{
		return p * p * p * p;
	}

	/// <summary>
	/// Modeled after the quartic y = 1 - (x - 1)^4
	/// </summary>
	public static float QuartOut(float p)
	{
		float num = p - 1f;
		return num * num * num * (1f - p) + 1f;
	}

	/// <summary>
	/// Modeled after the piecewise quartic
	/// y = (1/2)((2x)^4)        ; [0, 0.5)
	/// y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
	/// </summary>
	public static float QuartInOut(float p)
	{
		if (p < 0.5f)
		{
			return 8f * p * p * p * p;
		}
		float num = p - 1f;
		return -8f * num * num * num * num + 1f;
	}

	/// <summary>
	/// Modeled after the quintic y = x^5
	/// </summary>
	public static float QuintIn(float p)
	{
		return p * p * p * p * p;
	}

	/// <summary>
	/// Modeled after the quintic y = (x - 1)^5 + 1
	/// </summary>
	public static float QuintOut(float p)
	{
		float num = p - 1f;
		return num * num * num * num * num + 1f;
	}

	/// <summary>
	/// Modeled after the piecewise quintic
	/// y = (1/2)((2x)^5)       ; [0, 0.5)
	/// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
	/// </summary>
	public static float QuintInOut(float p)
	{
		if (p < 0.5f)
		{
			return 16f * p * p * p * p * p;
		}
		float num = 2f * p - 2f;
		return 0.5f * num * num * num * num * num + 1f;
	}

	/// <summary>
	/// Modeled after quarter-cycle of sine wave
	/// </summary>
	public static float SineIn(float p)
	{
		return Mathf.Sin((p - 1f) * ((float)Math.PI / 2f)) + 1f;
	}

	/// <summary>
	/// Modeled after quarter-cycle of sine wave (different phase)
	/// </summary>
	public static float SineOut(float p)
	{
		return Mathf.Sin(p * ((float)Math.PI / 2f));
	}

	/// <summary>
	/// Modeled after half sine wave
	/// </summary>
	public static float SineInOut(float p)
	{
		return 0.5f * (1f - Mathf.Cos(p * (float)Math.PI));
	}

	/// <summary>
	/// Modeled after shifted quadrant IV of unit circle
	/// </summary>
	public static float CircIn(float p)
	{
		return 1f - Mathf.Sqrt(1f - p * p);
	}

	/// <summary>
	/// Modeled after shifted quadrant II of unit circle
	/// </summary>
	public static float CircOut(float p)
	{
		return Mathf.Sqrt((2f - p) * p);
	}

	/// <summary>
	/// Modeled after the piecewise circular function
	/// y = (1/2)(1 - Mathf.Sqrt(1 - 4x^2))           ; [0, 0.5)
	/// y = (1/2)(Mathf.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
	/// </summary>
	public static float CircInOut(float p)
	{
		if (p < 0.5f)
		{
			return 0.5f * (1f - Mathf.Sqrt(1f - 4f * (p * p)));
		}
		return 0.5f * (Mathf.Sqrt((0f - (2f * p - 3f)) * (2f * p - 1f)) + 1f);
	}

	/// <summary>
	/// Modeled after the exponential function y = 2^(10(x - 1))
	/// </summary>
	public static float ExpoIn(float p)
	{
		if (p != 0f)
		{
			return Mathf.Pow(2f, 10f * (p - 1f));
		}
		return p;
	}

	/// <summary>
	/// Modeled after the exponential function y = -2^(-10x) + 1
	/// </summary>
	public static float ExpoOut(float p)
	{
		if (!(Math.Abs(p - 1f) < 0.001f))
		{
			return 1f - Mathf.Pow(2f, -10f * p);
		}
		return p;
	}

	/// <summary>
	/// Modeled after the piecewise exponential
	/// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
	/// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
	/// </summary>
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

	/// <summary>
	/// Modeled after the damped sine wave y = sin(13pi/2*x)*Mathf.Pow(2, 10 * (x - 1))
	/// </summary>
	public static float ElasticIn(float p)
	{
		return Mathf.Sin(20.420353f * p) * Mathf.Pow(2f, 10f * (p - 1f));
	}

	/// <summary>
	/// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Mathf.Pow(2, -10x) + 1
	/// </summary>
	public static float ElasticOut(float p)
	{
		return Mathf.Sin(-20.420353f * (p + 1f)) * Mathf.Pow(2f, -10f * p) + 1f;
	}

	/// <summary>
	/// Modeled after the piecewise exponentially-damped sine wave:
	/// y = (1/2)*sin(13pi/2*(2*x))*Mathf.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
	/// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Mathf.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
	/// </summary>
	public static float ElasticInOut(float p)
	{
		if (p < 0.5f)
		{
			return 0.5f * Mathf.Sin(20.420353f * (2f * p)) * Mathf.Pow(2f, 10f * (2f * p - 1f));
		}
		return 0.5f * (Mathf.Sin(-20.420353f * (2f * p - 1f + 1f)) * Mathf.Pow(2f, -10f * (2f * p - 1f)) + 2f);
	}

	/// <summary>
	/// Lerps an object beyond the target destination and brings it back
	/// </summary>
	public static float BackIn(float p, float strength = 1f)
	{
		float num = strength * 1.70158f;
		return p * p * ((num + 1f) * p - num);
	}

	/// <summary>
	/// Lerps an object beyond the target destination and brings it back
	/// </summary>
	public static float BackOut(float p, float strength = 1f)
	{
		float num = strength * 1.70158f;
		return 1f + num * (p - 1f) * (p - 1f) * ((num + 1f) * (p - 1f) + num);
	}

	/// <summary>
	/// Lerps an object beyond the target destination and brings it back
	/// </summary>
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
