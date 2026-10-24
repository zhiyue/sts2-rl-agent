using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Represents the JSON file which defines mod manifests.
/// </summary>
public class ModManifest
{
	[JsonPropertyName("id")]
	public string? id;

	[JsonPropertyName("name")]
	public string? name;

	[JsonPropertyName("author")]
	public string? author;

	[JsonPropertyName("description")]
	public string? description;

	[JsonPropertyName("version")]
	public string? version;

	[JsonPropertyName("has_pck")]
	public bool hasPck;

	[JsonPropertyName("has_dll")]
	public bool hasDll;

	[JsonPropertyName("dependencies")]
	public List<ModDependency>? dependencies;

	[JsonPropertyName("affects_gameplay")]
	public bool affectsGameplay = true;

	[JsonPropertyName("min_game_version")]
	public string? minGameVersion;

	public static ModManifest? ReadFromStream(Stream stream, out List<LocString>? errors)
	{
		errors = null;
		JsonNode jsonNode = JsonNode.Parse(stream);
		if (jsonNode == null)
		{
			return null;
		}
		JsonNode jsonNode2 = jsonNode["dependencies"];
		if (jsonNode2 != null && jsonNode2.GetValueKind() == JsonValueKind.Array)
		{
			JsonNode? jsonNode3 = jsonNode2.AsArray().FirstOrDefault();
			if (jsonNode3 != null && jsonNode3.GetValueKind() == JsonValueKind.String)
			{
				Log.Error("Detected old-style dependencies without min version specified! It works for now but this will be removed in a future release. Let the mod author know.");
				LocString locString = new LocString("main_menu_ui", "MOD_ERROR.MIGRATION_REQUIRED");
				locString.Add("id", jsonNode["id"]?.GetValue<string>() ?? "<null>");
				if (errors == null)
				{
					errors = new List<LocString>();
				}
				errors.Add(locString);
				JsonArray jsonArray = new JsonArray();
				foreach (JsonNode item in jsonNode2.AsArray())
				{
					if (item != null)
					{
						jsonArray.Add(new JsonObject(new global::_003C_003Ez__ReadOnlyArray<KeyValuePair<string, JsonNode>>(new KeyValuePair<string, JsonNode>[2]
						{
							new KeyValuePair<string, JsonNode>("id", item.GetValue<string>()),
							new KeyValuePair<string, JsonNode>("min_version", null)
						})));
					}
				}
				jsonNode["dependencies"] = jsonArray;
			}
		}
		return jsonNode.Deserialize(JsonSerializationUtility.GetTypeInfo<ModManifest>());
	}
}
