using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaSkeleton : MegaSpineBinding
{
	protected override string SpineClassName => "SpineSkeleton";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "find_bone", "get_bounds", "get_data", "set_skin", "set_skin_by_name", "set_slots_to_setup_pose" });

	public MegaSkeleton(Variant native)
		: base(native)
	{
	}

	public MegaBone? FindBone(string boneName)
	{
		Variant native = Call("find_bone", boneName);
		if (native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaBone(native);
	}

	public Rect2 GetBounds()
	{
		return Call("get_bounds").As<Rect2>();
	}

	public MegaSkeletonDataResource GetData()
	{
		return new MegaSkeletonDataResource(Call("get_data"));
	}

	public void SetSkin(MegaSkin? skin)
	{
		if (skin != null)
		{
			Call("set_skin", skin.BoundObject);
		}
	}

	public void SetSkinByName(string skinName)
	{
		Call("set_skin_by_name", skinName);
	}

	public void SetSlotsToSetupPose()
	{
		Call("set_slots_to_setup_pose");
	}
}
