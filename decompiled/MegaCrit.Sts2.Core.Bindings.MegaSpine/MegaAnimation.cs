using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaAnimation : MegaSpineBinding
{
	protected override string SpineClassName => "SpineAnimation";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "get_name", "get_duration" });

	public MegaAnimation(Variant native)
		: base(native)
	{
	}

	public string GetName()
	{
		return Call("get_name").AsString();
	}

	public float GetDuration()
	{
		return Call("get_duration").AsSingle();
	}
}
