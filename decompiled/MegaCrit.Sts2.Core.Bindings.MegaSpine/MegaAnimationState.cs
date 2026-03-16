using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaAnimationState : MegaSpineBinding
{
	protected override string SpineClassName => "SpineAnimationState";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "add_animation", "apply", "get_current", "set_animation", "set_time_scale", "update" });

	public MegaAnimationState(Variant native)
		: base(native)
	{
	}

	public MegaTrackEntry AddAnimation(string animationName, float delay = 0f, bool loop = true, int trackId = 0)
	{
		return new MegaTrackEntry(Call("add_animation", animationName, delay, loop, trackId));
	}

	public void Apply(MegaSkeleton skeleton)
	{
		Call("apply", skeleton.BoundObject);
	}

	public MegaTrackEntry GetCurrent(int trackIndex)
	{
		return new MegaTrackEntry(Call("get_current", trackIndex));
	}

	public MegaTrackEntry? SetAnimation(string animationName, bool loop = true, int trackId = 0)
	{
		Variant native = Call("set_animation", animationName, loop, trackId);
		if (native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaTrackEntry(native);
	}

	public void SetTimeScale(float scale)
	{
		Call("set_time_scale", scale);
	}

	public void Update(float delta)
	{
		Call("update", delta);
	}
}
