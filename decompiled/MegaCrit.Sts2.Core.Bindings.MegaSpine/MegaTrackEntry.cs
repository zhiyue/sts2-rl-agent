using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// C# bindings for SpineTrackEntry.
/// </summary>
public class MegaTrackEntry : MegaSpineBinding
{
	protected override string SpineClassName => "SpineTrackEntry";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[9] { "get_animation", "get_animation_end", "get_track_complete", "get_track_time", "is_complete", "set_loop", "set_time_scale", "set_track_time", "set_mix_duration" });

	public MegaTrackEntry(Variant native)
		: base(native)
	{
	}

	private MegaAnimation GetAnimation()
	{
		return new MegaAnimation(Call("get_animation"));
	}

	/// <summary>
	/// Name of this entry's animation. Returns the value rather than the wrapper so no transient
	/// <see cref="T:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimation" /> escapes; the native read is kept GC-safe by the GC.KeepAlive in
	/// MegaSpineBinding.Call (PRG-6985).
	/// </summary>
	public string GetAnimationName()
	{
		return GetAnimation().GetName();
	}

	/// <summary>
	/// Duration of this entry's animation. See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaTrackEntry.GetAnimationName" />.
	/// </summary>
	public float GetAnimationDuration()
	{
		return GetAnimation().GetDuration();
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
