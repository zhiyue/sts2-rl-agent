using System;
using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// C# bindings for SpineSprite.
/// </summary>
public class MegaSprite : MegaSpineBinding
{
	public const string spineClassName = "SpineSprite";

	protected override string SpineClassName => "SpineSprite";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[7] { "get_animation_state", "get_additive_material", "get_normal_material", "get_skeleton", "new_skin", "set_normal_material", "set_skeleton_data_res" });

	protected override IEnumerable<string> SpineSignals => new global::_003C_003Ez__ReadOnlyArray<string>(new string[10] { "animation_started", "animation_interrupted", "animation_ended", "animation_completed", "animation_disposed", "animation_event", "before_animation_state_update", "before_animation_state_apply", "before_world_transforms_change", "world_transforms_changed" });

	public MegaSprite(Variant native)
		: base(native)
	{
	}

	public Error ConnectAnimationStarted(Callable callable)
	{
		return Connect("animation_started", callable);
	}

	public Error ConnectAnimationInterrupted(Callable callable)
	{
		return Connect("animation_interrupted", callable);
	}

	public Error ConnectAnimationEnded(Callable callable)
	{
		return Connect("animation_ended", callable);
	}

	public Error ConnectAnimationCompleted(Callable callable)
	{
		return Connect("animation_completed", callable);
	}

	public Error ConnectAnimationDisposed(Callable callable)
	{
		return Connect("animation_disposed", callable);
	}

	public Error ConnectAnimationEvent(Callable callable)
	{
		return Connect("animation_event", callable);
	}

	public Error ConnectBeforeAnimationStateUpdate(Callable callable)
	{
		return Connect("before_animation_state_update", callable);
	}

	public Error ConnectBeforeAnimationStateApply(Callable callable)
	{
		return Connect("before_animation_state_apply", callable);
	}

	public Error ConnectBeforeWorldTransformsChange(Callable callable)
	{
		return Connect("before_world_transforms_change", callable);
	}

	public Error ConnectWorldTransformsChanged(Callable callable)
	{
		return Connect("world_transforms_changed", callable);
	}

	public void DisconnectAnimationStarted(Callable callable)
	{
		Disconnect("animation_started", callable);
	}

	public void DisconnectAnimationInterrupted(Callable callable)
	{
		Disconnect("animation_interrupted", callable);
	}

	public void DisconnectAnimationEnded(Callable callable)
	{
		Disconnect("animation_ended", callable);
	}

	public void DisconnectAnimationCompleted(Callable callable)
	{
		Disconnect("animation_completed", callable);
	}

	public void DisconnectAnimationDisposed(Callable callable)
	{
		Disconnect("animation_disposed", callable);
	}

	public void DisconnectAnimationEvent(Callable callable)
	{
		Disconnect("animation_event", callable);
	}

	public void DisconnectBeforeAnimationStateUpdate(Callable callable)
	{
		Disconnect("before_animation_state_update", callable);
	}

	public void DisconnectBeforeAnimationStateApply(Callable callable)
	{
		Disconnect("before_animation_state_apply", callable);
	}

	public void DisconnectBeforeWorldTransformsChange(Callable callable)
	{
		Disconnect("before_world_transforms_change", callable);
	}

	public void DisconnectWorldTransformsChanged(Callable callable)
	{
		Disconnect("world_transforms_changed", callable);
	}

	/// <summary>
	/// Helper method for the lazy
	/// </summary>
	public bool HasAnimation(string animId)
	{
		return GetSkeleton()?.GetData().HasAnimation(animId) ?? false;
	}

	/// <summary>
	/// Returns the animation state, throwing a descriptive error if it does not exist yet. Prefer
	/// <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite.TryGetAnimationState" /> / <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite.IsAnimationStateReady" /> in code that can run
	/// before the SpineSprite is ready (e.g. a child node's _Ready, which Godot runs before the parent
	/// SpineSprite has built its skeleton). Failing fast here turns the old opaque NullReferenceException
	/// (from driving a null-wrapped state) into a clear, actionable message.
	/// </summary>
	public MegaAnimationState GetAnimationState()
	{
		return TryGetAnimationState() ?? throw new InvalidOperationException("GetAnimationState() was called before the SpineSprite's skeleton finished initializing. Godot runs _Ready() bottom-up and the skeleton loads asynchronously; drive animations from a spine-ready callback (Node.RunWhenSpineReady) or gate on IsAnimationStateReady()/TryGetAnimationState().");
	}

	/// <summary>
	/// Returns the animation state, or null if it has not been created yet. The skeleton (and with it
	/// the animation state) may not exist when this is called before the SpineSprite has finished
	/// initializing, which happens when a child node's _Ready() runs first: Godot invokes _Ready()
	/// bottom-up (children before parents). Prefer this over <see cref="M:MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite.GetAnimationState" /> in code
	/// that can run during that window, since GetAnimationState() would wrap a null object and throw
	/// on first use.
	/// </summary>
	public MegaAnimationState? TryGetAnimationState()
	{
		Variant native = Call("get_animation_state");
		if (native.VariantType != Variant.Type.Object || native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaAnimationState(native);
	}

	/// <summary>
	/// True once the SpineSprite has finished initializing and it is safe to drive its animations.
	/// This gates on the skeleton existing (get_skeleton returns null until the skeleton data has
	/// loaded, the same null-skeleton condition NCreatureVisuals checks to detect a failed load) and
	/// additionally confirms the animation state object we drive exists. It returns false during the
	/// window where a child node's _Ready() runs before the parent SpineSprite is ready, because Godot
	/// invokes _Ready() bottom-up and the skeleton loads asynchronously.
	/// </summary>
	public bool IsAnimationStateReady()
	{
		if (GetSkeleton() != null)
		{
			return TryGetAnimationState() != null;
		}
		return false;
	}

	public MegaSkeleton? GetSkeleton()
	{
		Variant? variant = CallNullable("get_skeleton");
		if (!variant.HasValue)
		{
			return null;
		}
		return new MegaSkeleton(variant.Value);
	}

	public Material? GetAdditiveMaterial()
	{
		return CallNullable("get_additive_material")?.As<Material>();
	}

	public Material? GetNormalMaterial()
	{
		return CallNullable("get_normal_material")?.As<Material>();
	}

	public MegaSkin NewSkin(string name)
	{
		return new MegaSkin(Call("new_skin", name));
	}

	public void SetNormalMaterial(Material material)
	{
		Call("set_normal_material", material);
	}

	public void SetSkeletonDataRes(MegaSkeletonDataResource skeletonData)
	{
		Call("set_skeleton_data_res", skeletonData.BoundObject);
	}
}
