using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

/// <summary>
/// A set of static math functions which are missing from Godot's Mathf.
/// </summary>
public static class MathHelper
{
	public const float degToRad = 0.0174533f;

	/// <summary>
	/// Linearly scales the input value from the source range [from1, to1] to the target range [from2, to2].
	/// </summary>
	/// <param name="value"></param>
	/// <param name="from1"></param>
	/// <param name="to1"></param>
	/// <param name="from2"></param>
	/// <param name="to2"></param>
	/// <returns></returns>
	public static float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	/// <summary>
	/// Bezier Curve curve function.
	/// </summary>
	/// <param name="v0"></param>
	/// <param name="v1"></param>
	/// <param name="c0"></param>
	/// <param name="t"></param>
	/// <returns></returns>
	public static Vector2 BezierCurve(Vector2 v0, Vector2 v1, Vector2 c0, float t)
	{
		return Mathf.Pow(1f - t, 2f) * v0 + 2f * (1f - t) * t * c0 + Mathf.Pow(t, 2f) * v1;
	}

	public static float GetAngle(Vector2 vector)
	{
		return Mathf.Atan2(vector.Y, vector.X);
	}

	/// <summary>
	/// Clamps a Vector2, only if both X and Y are equal values.
	/// </summary>
	/// <param name="input"></param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static Vector2 Clamp(Vector2 input, float min, float max)
	{
		float num = Mathf.Clamp(input.X, min, max);
		return new Vector2(num, num);
	}

	/// <summary>
	/// Gradually changes a value towards a desired goal over time.
	/// </summary>
	/// <param name="current">The current value</param>
	/// <param name="target">The target value</param>
	/// <param name="currentVelocity">The current velocity. This should start at zero, be stored somewhere, and passed to
	/// this method every frame.</param>
	/// <param name="smoothTime">The approximate amount of time it will take to reach the target. Smaller values mean that
	/// we'll reach the target value faster.</param>
	/// <param name="deltaTime">The last process time.</param>
	/// <param name="maxSpeed">Maximum speed we'll move towards the target.</param>
	/// <returns>New value after interpolation.</returns>
	public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
	{
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float num = 2f / smoothTime;
		float num2 = num * deltaTime;
		float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
		float value = current - target;
		float num4 = target;
		float num5 = maxSpeed * smoothTime;
		value = Mathf.Clamp(value, 0f - num5, num5);
		target = current - value;
		float num6 = (currentVelocity + num * value) * deltaTime;
		currentVelocity = (currentVelocity - num * num6) * num3;
		float num7 = target + (value + num6) * num3;
		if (num4 - current > 0f == num7 > num4)
		{
			num7 = num4;
			currentVelocity = (num7 - num4) / deltaTime;
		}
		return num7;
	}

	/// <summary>
	/// Gradually changes a value towards a desired goal over time.
	/// </summary>
	/// <param name="current">The current value</param>
	/// <param name="target">The target value</param>
	/// <param name="currentVelocity">The current velocity. This should start at zero, be stored somewhere, and passed to
	/// this method every frame.</param>
	/// <param name="smoothTime">The approximate amount of time it will take to reach the target. Smaller values mean that
	/// we'll reach the target value faster.</param>
	/// <param name="deltaTime">The last process time.</param>
	/// <param name="maxSpeed">Maximum speed we'll move towards the target.</param>
	/// <returns>New value after interpolation.</returns>
	public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
	{
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float num = 2f / smoothTime;
		float num2 = num * deltaTime;
		float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
		float num4 = current.X - target.X;
		float num5 = current.Y - target.Y;
		Vector2 vector = target;
		float num6 = maxSpeed * smoothTime;
		float num7 = num6 * num6;
		float num8 = num4 * num4 + num5 * num5;
		if (num8 > num7)
		{
			float num9 = Mathf.Sqrt(num8);
			num4 = num4 / num9 * num6;
			num5 = num5 / num9 * num6;
		}
		target.X = current.X - num4;
		target.Y = current.Y - num5;
		float num10 = (currentVelocity.X + num * num4) * deltaTime;
		float num11 = (currentVelocity.Y + num * num5) * deltaTime;
		currentVelocity.X = (currentVelocity.X - num * num10) * num3;
		currentVelocity.Y = (currentVelocity.Y - num * num11) * num3;
		float num12 = target.X + (num4 + num10) * num3;
		float num13 = target.Y + (num5 + num11) * num3;
		float num14 = vector.X - current.X;
		float num15 = vector.Y - current.Y;
		float num16 = num12 - vector.X;
		float num17 = num13 - vector.Y;
		if (num14 * num16 + num15 * num17 > 0f)
		{
			num12 = vector.X;
			num13 = vector.Y;
			currentVelocity.X = (num12 - vector.X) / deltaTime;
			currentVelocity.Y = (num13 - vector.Y) / deltaTime;
		}
		return new Vector2(num12, num13);
	}
}
