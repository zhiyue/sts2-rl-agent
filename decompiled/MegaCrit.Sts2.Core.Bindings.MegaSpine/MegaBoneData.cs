using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaBoneData : MegaSpineBinding
{
	protected override string SpineClassName => "SpineBoneData";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlySingleElementList<string>("set_color");

	public MegaBoneData(Variant native)
		: base(native)
	{
	}

	public void SetColor(Color color)
	{
		Call("set_color", color);
	}
}
