using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

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

	public bool HasAnimation(string animId)
	{
		return GetSkeleton().GetData().FindAnimation(animId) != null;
	}

	public MegaAnimationState GetAnimationState()
	{
		return new MegaAnimationState(Call("get_animation_state"));
	}

	public MegaSkeleton GetSkeleton()
	{
		return new MegaSkeleton(Call("get_skeleton"));
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
