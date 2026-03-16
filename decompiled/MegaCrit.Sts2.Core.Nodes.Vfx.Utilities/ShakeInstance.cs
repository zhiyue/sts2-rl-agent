using Godot;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

public abstract class ShakeInstance
{
	protected float _strength;

	protected double _startDuration;

	protected double _duration;

	protected float _ease;

	protected float _angle;

	protected static float WiggleSpeed => 60f;

	public bool IsDone { get; protected set; }

	public abstract Vector2 Update(double delta);
}
