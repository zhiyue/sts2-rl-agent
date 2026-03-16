using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MegaCrit.Sts2.Core.Saves.Migrations.Shared;

public static class SharedMigrationHelper
{
	public static void MigrateMapPointHistoryRooms(JsonNode? jsonNode)
	{
		if (!(jsonNode is JsonArray jsonArray))
		{
			return;
		}
		foreach (JsonNode item2 in jsonArray)
		{
			if (!(item2 is JsonArray jsonArray2))
			{
				continue;
			}
			foreach (JsonNode item3 in jsonArray2)
			{
				if (!(item3 is JsonObject jsonObject) || !jsonObject.TryGetPropertyValue("room_types", out JsonNode jsonNode2) || !(jsonNode2 is JsonArray jsonArray3))
				{
					continue;
				}
				JsonArray jsonArray4 = new JsonArray();
				jsonObject.Add("rooms", jsonArray4);
				foreach (JsonNode item4 in jsonArray3)
				{
					if (item4 != null)
					{
						JsonObject item = new JsonObject { 
						{
							"room_type",
							item4.DeepClone()
						} };
						jsonArray4.Add((JsonNode?)item);
					}
				}
				jsonObject.Remove("room_types");
				JsonObject jsonObject2 = (JsonObject)jsonArray4[0];
				JsonObject jsonObject3 = (JsonObject)jsonArray4[jsonArray4.Count - 1];
				if (jsonObject.TryGetPropertyValue("model_id", out JsonNode jsonNode3))
				{
					jsonObject2.Add("model_id", jsonNode3.DeepClone());
					jsonObject.Remove("model_id");
				}
				if (jsonObject.TryGetPropertyValue("monster_ids", out JsonNode jsonNode4))
				{
					jsonObject3.Add("monster_ids", jsonNode4.DeepClone());
					jsonObject.Remove("monster_ids");
				}
				if (jsonObject.TryGetPropertyValue("turns_taken", out JsonNode jsonNode5))
				{
					jsonObject3.Add("turns_taken", jsonNode5.DeepClone());
					jsonObject.Remove("turns_taken");
				}
			}
		}
	}

	public static void RecursiveRemoveSchema(JsonNode node, int depth = 0)
	{
		if (node is JsonObject jsonObject)
		{
			if (depth > 0)
			{
				jsonObject.Remove("schema_version");
			}
			{
				foreach (KeyValuePair<string, JsonNode> item in jsonObject)
				{
					if (item.Value != null)
					{
						RecursiveRemoveSchema(item.Value, depth + 1);
					}
				}
				return;
			}
		}
		if (!(node is JsonArray jsonArray))
		{
			return;
		}
		foreach (JsonNode item2 in jsonArray)
		{
			if (item2 != null)
			{
				RecursiveRemoveSchema(item2, depth + 1);
			}
		}
	}

	public static void MigrateMapPointHistoryCardChoices(JsonNode? jsonNode)
	{
		if (!(jsonNode is JsonArray jsonArray))
		{
			return;
		}
		foreach (JsonNode item in jsonArray)
		{
			if (!(item is JsonArray jsonArray2))
			{
				continue;
			}
			foreach (JsonNode item2 in jsonArray2)
			{
				if (!(item2 is JsonObject jsonObject) || !jsonObject.TryGetPropertyValue("player_stats", out JsonNode jsonNode2) || !(jsonNode2 is JsonArray jsonArray3))
				{
					continue;
				}
				foreach (JsonNode item3 in jsonArray3)
				{
					if (!(item3 is JsonObject jsonObject2))
					{
						continue;
					}
					if (jsonObject2.TryGetPropertyValue("cards_gained", out JsonNode jsonNode3) && jsonNode3 is JsonArray jsonArray4)
					{
						JsonArray jsonArray5 = new JsonArray();
						foreach (JsonNode item4 in jsonArray4)
						{
							JsonObject jsonObject3 = new JsonObject();
							jsonObject3["id"] = item4.GetValue<string>();
							jsonObject3["current_upgrade_level"] = 0;
							jsonArray5.Add((JsonNode?)jsonObject3);
						}
						item3["cards_gained"] = jsonArray5;
					}
					if (jsonObject2.TryGetPropertyValue("cards_removed", out JsonNode jsonNode4) && jsonNode4 is JsonArray jsonArray6)
					{
						JsonArray jsonArray7 = new JsonArray();
						foreach (JsonNode item5 in jsonArray6)
						{
							JsonObject jsonObject4 = new JsonObject();
							jsonObject4["id"] = item5.GetValue<string>();
							jsonObject4["current_upgrade_level"] = 0;
							jsonArray7.Add((JsonNode?)jsonObject4);
						}
						item3["cards_removed"] = jsonArray7;
					}
					if (jsonObject2.TryGetPropertyValue("card_choices", out JsonNode jsonNode5))
					{
						if (!(jsonNode5 is JsonArray jsonArray8))
						{
							continue;
						}
						foreach (JsonNode item6 in jsonArray8)
						{
							if (item6 is JsonObject jsonObject5)
							{
								JsonObject jsonObject6 = new JsonObject();
								jsonObject6["id"] = jsonObject5["choice"].GetValue<string>();
								jsonObject6["current_upgrade_level"] = 0;
								jsonObject5["card"] = jsonObject6;
								jsonObject5.Remove("choice");
							}
						}
					}
					if (jsonObject2.TryGetPropertyValue("cards_enchanted", out JsonNode jsonNode6))
					{
						if (!(jsonNode6 is JsonArray jsonArray9))
						{
							continue;
						}
						foreach (JsonNode item7 in jsonArray9)
						{
							if (item7 is JsonObject jsonObject7)
							{
								JsonObject jsonObject8 = new JsonObject();
								jsonObject8["id"] = jsonObject7["card"].GetValue<string>();
								jsonObject8["current_upgrade_level"] = 0;
								jsonObject7["card"] = jsonObject8;
							}
						}
					}
					if (!jsonObject2.TryGetPropertyValue("cards_transformed", out JsonNode jsonNode7) || !(jsonNode7 is JsonArray jsonArray10))
					{
						continue;
					}
					foreach (JsonNode item8 in jsonArray10)
					{
						if (item8 is JsonObject jsonObject9)
						{
							JsonObject jsonObject10 = new JsonObject();
							jsonObject10["id"] = jsonObject9["original_card"].GetValue<string>();
							jsonObject10["current_upgrade_level"] = 0;
							jsonObject9["original_card"] = jsonObject10;
							JsonObject jsonObject11 = new JsonObject();
							jsonObject11["id"] = jsonObject9["final_card"].GetValue<string>();
							jsonObject11["current_upgrade_level"] = 0;
							jsonObject9["final_card"] = jsonObject11;
						}
					}
				}
			}
		}
	}
}
