using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaSkeletonDataResource : MegaSpineBinding
{
	protected override string SpineClassName => "SpineSkeletonDataResource";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "find_animation", "find_skin", "get_animations", "get_skins" });

	public MegaSkeletonDataResource(Variant native)
		: base(native)
	{
	}

	public MegaSkin? FindSkin(string skinName)
	{
		Variant native = Call("find_skin", skinName);
		if (native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaSkin(native);
	}

	public MegaAnimation? FindAnimation(string animName)
	{
		Variant native = Call("find_animation", animName);
		if (native.AsGodotObject() == null)
		{
			return null;
		}
		return new MegaAnimation(native);
	}

	public Array<GodotObject> GetAnimations()
	{
		return (Array<GodotObject>)Call("get_animations");
	}

	public Array<GodotObject> GetSkins()
	{
		return (Array<GodotObject>)Call("get_skins");
	}
}
