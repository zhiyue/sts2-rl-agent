using System;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

public class ScreenPunchInstance : ShakeInstance
{
	public ScreenPunchInstance(float strength, double duration, float degAngle)
	{
		_strength = strength;
		_startDuration = duration;
		_duration = duration;
		_angle = Mathf.DegToRad(degAngle);
	}

	public override Vector2 Update(double delta)
	{
		if (base.IsDone)
		{
			return Vector2.Zero;
		}
		_duration -= delta;
		float x = (float)Math.Cos(_duration * (double)ShakeInstance.WiggleSpeed);
		_ease = Ease.CubicOut((float)(_duration / _startDuration));
		Vector2 result = new Vector2(x, 0f).Rotated(_angle) * _strength * _ease;
		if (_duration < 0.0)
		{
			base.IsDone = true;
		}
		return result;
	}

	public void Cancel()
	{
		_duration = 0.0;
		base.IsDone = true;
	}
}
