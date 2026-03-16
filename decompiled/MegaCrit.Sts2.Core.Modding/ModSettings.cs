using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Modding;

public class ModSettings
{
	[JsonPropertyName("mods_enabled")]
	public bool PlayerAgreedToModLoading { get; set; }

	[JsonPropertyName("disabled_mods")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<DisabledMod> DisabledMods { get; set; } = new List<DisabledMod>();

	public bool IsModDisabled(Mod mod)
	{
		return IsModDisabled(mod.pckName, mod.modSource);
	}

	public bool IsModDisabled(string pckName, ModSource source)
	{
		return DisabledMods.Any((DisabledMod m) => m.Name == pckName && m.Source == source);
	}
}
