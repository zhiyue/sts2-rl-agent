using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaSkin : MegaSpineBinding
{
	protected override string SpineClassName => "SpineSkin";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlySingleElementList<string>("add_skin");

	public MegaSkin(Variant native)
		: base(native)
	{
	}

	public void AddSkin(MegaSkin? skin)
	{
		if (skin != null)
		{
			Call("add_skin", skin.BoundObject);
		}
	}
}
