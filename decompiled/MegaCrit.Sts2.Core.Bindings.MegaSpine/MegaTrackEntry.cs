using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaTrackEntry : MegaSpineBinding
{
	protected override string SpineClassName => "SpineTrackEntry";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[9] { "get_animation", "get_animation_end", "get_track_complete", "get_track_time", "is_complete", "set_loop", "set_time_scale", "set_track_time", "set_mix_duration" });

	public MegaTrackEntry(Variant native)
		: base(native)
	{
	}

	public MegaAnimation GetAnimation()
	{
		return new MegaAnimation(Call("get_animation"));
	}

	public float GetAnimationEnd()
	{
		return Call("get_animation_end").AsSingle();
	}

	public float GetTrackComplete()
	{
		return Call("get_track_complete").AsSingle();
	}

	public float GetTrackTime()
	{
		return Call("get_track_time").AsSingle();
	}

	public bool IsComplete()
	{
		return Call("is_complete").AsBool();
	}

	public void SetLoop(bool loop)
	{
		Call("set_loop", loop);
	}

	public void SetTimeScale(float scale)
	{
		Call("set_time_scale", scale);
	}

	public void SetTrackTime(float time)
	{
		Call("set_track_time", time);
	}

	public void SetMixDuration(float time)
	{
		Call("set_mix_duration", time);
	}
}
