using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

public abstract class MegaSpineBinding
{
	public GodotObject BoundObject { get; private set; }

	protected abstract string SpineClassName { get; }

	protected virtual IEnumerable<string> SpineMethods => Array.Empty<string>();

	protected virtual IEnumerable<string> SpineSignals => Array.Empty<string>();

	protected MegaSpineBinding(Variant native)
	{
		if (native.VariantType != Variant.Type.Object)
		{
			throw new InvalidOperationException($"Expected a GodotObject but was {native.VariantType}!");
		}
		BoundObject = native.AsGodotObject();
		ValidateBoundObject();
	}

	protected Error Connect(string signalName, Callable callable)
	{
		return BoundObject.Connect(signalName, callable);
	}

	protected void Disconnect(string signalName, Callable callable)
	{
		BoundObject.Disconnect(signalName, callable);
	}

	protected Variant Call(string methodName, params Variant[] args)
	{
		if (!SpineMethods.Contains(methodName))
		{
			throw new InvalidOperationException($"You must add {methodName} to {GetType().Name}.SpineMethods before calling it!");
		}
		return BoundObject.Call(methodName, args);
	}

	protected Variant? CallNullable(string methodName, params Variant[] args)
	{
		Variant variant = Call(methodName, args);
		return (variant.VariantType == Variant.Type.Nil) ? default(Variant) : variant;
	}

	private void ValidateBoundObject()
	{
		if (BoundObject == null)
		{
			return;
		}
		if (BoundObject.GetClass() != SpineClassName)
		{
			throw new InvalidOperationException($"Expected {"BoundObject"} to be a {SpineClassName}, but it is a {BoundObject.GetClass()}!");
		}
		foreach (string spineMethod in SpineMethods)
		{
			if (!BoundObject.HasMethod(spineMethod))
			{
				throw new InvalidOperationException(SpineClassName + " does not have method " + spineMethod + "!");
			}
		}
		foreach (string spineSignal in SpineSignals)
		{
			if (!BoundObject.HasSignal(spineSignal))
			{
				throw new InvalidOperationException(SpineClassName + " does not have signal " + spineSignal + "!");
			}
		}
	}
}
