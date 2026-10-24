using System;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Declares a class as the main entry point for the mod.
/// If this is present, then upon loading the mod, we'll call the method named initializerMethod within the class.
/// Otherwise, we'll create a harmony instance for the mod and call Harmony.PatchAll.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ModInitializerAttribute : Attribute
{
	public string initializerMethod;

	public ModInitializerAttribute(string initializerMethod)
	{
		this.initializerMethod = initializerMethod;
	}
}
