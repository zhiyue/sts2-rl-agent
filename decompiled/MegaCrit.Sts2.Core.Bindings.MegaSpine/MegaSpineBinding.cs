using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MegaCrit.Sts2.Core.Bindings.MegaSpine;

/// <summary>
/// Base class for C# Spine bindings.
/// </summary>
/// <remarks>
/// Lifetime gotcha for any wrapper around a native-created RefCounted, not just Spine: while the managed
/// wrapper is the sole owner, Godot gives it a weak GCHandle, so the GC can finalize it (freeing the native
/// object) the moment no managed code will read it again, even mid-call into native. The classic trigger is
/// an unrooted temporary in a chain like wrapper.GetX().GetName(): GetX() returns a fresh sole-owned
/// wrapper, collectible while the native GetName() is still using its pointer. Use-after-free on the
/// finalizer thread.
///
/// The defense is the choke point in Call below, which keeps the receiver rooted via GC.KeepAlive across
/// every native call routed through it, plus value-returning accessors so transient wrappers never escape.
/// Keep that pattern in any new binding of this kind. See GC.KeepAlive
/// (https://learn.microsoft.com/dotnet/api/system.gc.keepalive) and PRG-6985.
/// </remarks>
public abstract class MegaSpineBinding
{
	/// <summary>
	/// The Spine GodotObject that this binding wraps.
	/// ONLY USE THIS TO PASS TO OTHER SPINE API METHODS!
	/// If you want to call methods directly on this object, you probably want to add a method binding for the relevant
	/// MegaSpineBinding subclass instead.
	/// </summary>
	public GodotObject BoundObject { get; private set; }

	/// <summary>
	/// The name of this class in the Spine API (https://en.esotericsoftware.com/spine-api-reference).
	/// </summary>
	protected abstract string SpineClassName { get; }

	/// <summary>
	/// The methods from the Spine API (https://en.esotericsoftware.com/spine-api-reference) that this binding exposes.
	/// </summary>
	protected virtual IEnumerable<string> SpineMethods => Array.Empty<string>();

	/// <summary>
	/// The signals from the Spine API (https://en.esotericsoftware.com/spine-api-reference) that this binding exposes.
	/// </summary>
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

	/// <summary>
	/// Connect to the specified signal on the bound Spine object.
	/// </summary>
	/// <param name="signalName">Name of the signal to connect to.</param>
	/// <param name="callable">Callable to connect to the signal.</param>
	protected Error Connect(string signalName, Callable callable)
	{
		return BoundObject.Connect(signalName, callable);
	}

	/// <summary>
	/// Disconnect from the specified signal on the bound Spine object.
	/// </summary>
	/// <param name="signalName">Name of the signal to disconnect from.</param>
	/// <param name="callable">Callable to disconnect from the signal.</param>
	protected void Disconnect(string signalName, Callable callable)
	{
		BoundObject.Disconnect(signalName, callable);
	}

	/// <summary>
	/// Call the specified method on the bound Spine object
	/// </summary>
	/// <param name="methodName">Name of method to call.</param>
	/// <param name="args">Arguments to pass to the method.</param>
	/// <returns>Value returned by the bound Spine object, cast to the expected type.</returns>
	protected Variant Call(string methodName, params Variant[] args)
	{
		if (!SpineMethods.Contains(methodName))
		{
			throw new InvalidOperationException($"You must add {methodName} to {GetType().Name}.SpineMethods before calling it!");
		}
		Variant result = BoundObject.Call(methodName, args);
		GC.KeepAlive(BoundObject);
		GC.KeepAlive(args);
		return result;
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
