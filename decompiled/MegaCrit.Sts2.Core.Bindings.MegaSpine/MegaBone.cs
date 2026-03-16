using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaBone : MegaSpineBinding
{
	protected override string SpineClassName => "SpineBone";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "get_data", "set_rotation", "set_scale_x", "set_scale_y" });

	public MegaBone(Variant native)
		: base(native)
	{
	}

	public MegaBoneData GetData()
	{
		return new MegaBoneData(Call("get_data"));
	}

	public void SetRotation(float rotation)
	{
		Call("set_rotation", rotation);
	}

	public void SetScaleX(float scaleX)
	{
		Call("set_scale_x", scaleX);
	}

	public void SetScaleY(float scaleY)
	{
		Call("set_scale_y", scaleY);
	}

	public void Hide()
	{
		SetScaleX(0f);
		SetScaleY(0f);
	}
}
