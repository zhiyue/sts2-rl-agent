using System.Collections.Generic;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public class MegaEventData : MegaSpineBinding
{
	protected override string SpineClassName => "SpineEventData";

	protected override IEnumerable<string> SpineMethods => new global::_003C_003Ez__ReadOnlySingleElementList<string>("get_event_name");

	public MegaEventData(Variant native)
		: base(native)
	{
	}

	public string GetEventName()
	{
		return Call("get_event_name").AsString();
	}
}
