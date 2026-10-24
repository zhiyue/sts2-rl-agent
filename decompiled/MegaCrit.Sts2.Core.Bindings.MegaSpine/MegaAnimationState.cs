using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// C# bindings for SpineAnimationState.
/// </summary>
public class MegaAnimationState : MegaSpineBinding
{
	protected override string SpineClassName => "SpineAnimationState";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[7] { "add_animation", "add_empty_animation", "apply", "get_current", "set_animation", "set_time_scale", "update" });

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

	public MegaTrackEntry? GetCurrent(int trackIndex)
	{
		Variant native = Call("get_current", trackIndex);
		if (native.VariantType != Variant.Type.Object)
		{
			return null;
		}
		return new MegaTrackEntry(native);
	}

	/// <summary>
	/// Returns the name of the animation currently playing on the given track, or null if no track is
	/// active. Value-only so no transient wrapper escapes; the native reads are kept GC-safe by the
	/// GC.KeepAlive in MegaSpineBinding.Call (PRG-6985).
	/// </summary>
	public string? GetCurrentAnimationName(int trackIndex = 0)
	{
		return GetCurrent(trackIndex)?.GetAnimationName();
	}

	/// <summary>
	/// Returns the duration of the animation currently playing on the given track, or null if no track
	/// is active. See <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaAnimationState.GetCurrentAnimationName(System.Int32)" />.
	/// </summary>
	public float? GetCurrentAnimationDuration(int trackIndex = 0)
	{
		return GetCurrent(trackIndex)?.GetAnimationDuration();
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

	public MegaTrackEntry? AddEmptyAnimation(int trackId = 0)
	{
		Variant native = Call("add_empty_animation", trackId, 0, 0);
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
