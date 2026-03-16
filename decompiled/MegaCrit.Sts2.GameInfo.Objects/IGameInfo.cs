using System.Text.Json.Serialization;

namespace MegaCrit.Sts2.GameInfo.Objects;

[JsonDerivedType(typeof(AncientChoiceInfo))]
[JsonDerivedType(typeof(CardInfo))]
[JsonDerivedType(typeof(DailyMods))]
[JsonDerivedType(typeof(EncounterInfo))]
[JsonDerivedType(typeof(EventInfo))]
[JsonDerivedType(typeof(Keywords))]
[JsonDerivedType(typeof(NeowBonusInfo))]
[JsonDerivedType(typeof(PotionInfo))]
[JsonDerivedType(typeof(RelicInfo))]
[JsonDerivedType(typeof(EnchantmentInfo))]
public interface IGameInfo
{
	[JsonPropertyName("name")]
	string Name { get; }

	[JsonPropertyName("bot_keyword")]
	string BotKeyword { get; }

	[JsonPropertyName("bot_text")]
	string BotText { get; }
}
