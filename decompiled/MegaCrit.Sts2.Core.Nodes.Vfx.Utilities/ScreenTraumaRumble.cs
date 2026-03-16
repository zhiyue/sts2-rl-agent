using System;
using Godot;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

public class ScreenTraumaRumble
{
	private const float _rumbleAmount = 200f;

	private const float _speed = 1000f;

	private const float _decayRate = 2f;

	private const float _maxShake = 50f;

	private readonly FastNoiseLite _noise = new FastNoiseLite();

	private float _trauma;

	private double _duration;

	private float _multiplier;

	public ScreenTraumaRumble()
	{
		_noise.Frequency = 0.01f;
		_noise.Seed = Rng.Chaotic.NextInt(999999);
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	}

	public void AddTrauma(ShakeStrength amount)
	{
		float trauma = _trauma;
		_trauma = trauma + amount switch
		{
			ShakeStrength.VeryWeak => 0.07f, 
			ShakeStrength.Weak => 0.2f, 
			ShakeStrength.Medium => 0.4f, 
			ShakeStrength.Strong => 0.6f, 
			ShakeStrength.TooMuch => 1f, 
			_ => throw new ArgumentOutOfRangeException("amount", amount, null), 
		};
		_trauma = Mathf.Min(_trauma, 1f);
	}

	public Vector2 Update(double delta)
	{
		if (_trauma <= 0f)
		{
			return Vector2.Zero;
		}
		_duration += delta * 1000.0;
		float num = Mathf.Pow(_trauma, 1.5f) * 200f * _multiplier;
		float noise1D = _noise.GetNoise1D((float)_duration + 100f);
		float noise1D2 = _noise.GetNoise1D((float)_duration + 300f);
		Vector2 result = (new Vector2(noise1D, noise1D2) * num).LimitLength(50f);
		_trauma = Mathf.Max(_trauma - (float)(2.0 * delta), 0f);
		return result;
	}

	public void SetMultiplier(float multiplier)
	{
		_multiplier = multiplier;
	}
}
