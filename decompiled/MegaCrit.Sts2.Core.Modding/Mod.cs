using System.Reflection;

namespace MegaCrit.Sts2.Core.Modding;

public class Mod
{
	public ModSource modSource;

	public required string pckName;

	public bool wasLoaded;

	public ModManifest? manifest;

	public Assembly? assembly;

	public bool? assemblyLoadedSuccessfully;
}
