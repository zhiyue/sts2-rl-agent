using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Modding;

public class ModSettings
{
	[JsonPropertyName("mods_enabled")]
	public bool PlayerAgreedToModLoading { get; set; }

	[JsonPropertyName("mod_list")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotCollectionEmptyOrNull)]
	public List<SettingsSaveMod> ModList { get; set; } = new List<SettingsSaveMod>();

	public bool IsModDisabled(Mod mod)
	{
		return IsModDisabled(mod.manifest?.id, mod.modSource);
	}

	public bool IsModDisabled(string? id, ModSource source)
	{
		return ModList.Any((SettingsSaveMod m) => m.Id == id && m.Source == source && !m.IsEnabled);
	}
}
