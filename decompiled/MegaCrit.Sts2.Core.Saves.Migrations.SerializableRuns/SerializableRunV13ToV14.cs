using System.Collections.Generic;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;

[Migration(typeof(SerializableRun), 13, 14)]
public class SerializableRunV13ToV14 : MigrationBase<SerializableRun>
{
	protected override void ApplyMigration(MigratingData saveData)
	{
		Log.Info("SerializableRun migration v13 -> v14: Migrating SerializableRun.CardPoolId to CardPoolIds");
		if (!(saveData.GetRawNode("pre_finished_room") is JsonObject jsonObject) || !(jsonObject["extra_rewards"] is JsonObject jsonObject2))
		{
			return;
		}
		foreach (var (_, jsonNode2) in jsonObject2)
		{
			if (!(jsonNode2 is JsonArray jsonArray))
			{
				continue;
			}
			foreach (JsonNode item in jsonArray)
			{
				if (!(item is JsonObject jsonObject3))
				{
					continue;
				}
				if (!jsonObject3.ContainsKey("card_pools"))
				{
					jsonObject3["card_pools"] = new JsonArray();
				}
				if (jsonObject3.TryGetPropertyValue("card_pool", out JsonNode jsonNode3))
				{
					string text2 = jsonNode3?.GetValue<string>();
					jsonObject3.Remove("card_pool");
					if (text2 != null && !(text2 == ModelId.none.ToString()))
					{
						((JsonArray)jsonObject3["card_pools"]).Add(text2);
					}
				}
			}
		}
	}
}
