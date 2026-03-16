using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

public class ScreenRumbleInstance : ShakeInstance
{
	private readonly FastNoiseLite _noise = new FastNoiseLite();

	private readonly float _randomOffset = Rng.Chaotic.NextFloat(99999f);

	private readonly float _speed = 10f;

	private Vector2 _targetOffset = Vector2.Zero;

	private readonly RumbleStyle _style;

	public ScreenRumbleInstance(float strength, double duration, float speedMultiplier, RumbleStyle style)
	{
		_noise.Frequency = ((style == RumbleStyle.Rumble) ? 6f : 0.1f);
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_strength = strength * ((style == RumbleStyle.Rumble) ? 0.5f : 5f);
		_speed *= speedMultiplier;
		_startDuration = duration;
		_duration = duration;
		_style = style;
	}

	public override Vector2 Update(double delta)
	{
		if (base.IsDone)
		{
			return Vector2.Zero;
		}
		_duration -= delta;
		float num = (float)_duration;
		float noise1D = _noise.GetNoise1D(num + _randomOffset);
		float noise1D2 = _noise.GetNoise1D(num + _randomOffset + _randomOffset);
		_ease = Ease.CubicOut((float)(_duration / _startDuration));
		Vector2 result;
		if (_style == RumbleStyle.Drunk)
		{
			_targetOffset = _targetOffset.Lerp(new Vector2(noise1D, noise1D2), Mathf.Clamp((float)delta * _speed, 0f, 1f));
			result = _targetOffset * _strength * _ease;
		}
		else
		{
			result = new Vector2(noise1D, noise1D2) * _strength * _ease;
		}
		if (_duration < 0.0)
		{
			base.IsDone = true;
		}
		return result;
	}
}
