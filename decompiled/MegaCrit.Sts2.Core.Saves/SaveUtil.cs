using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Saves;

public static class SaveUtil
{
	/// <summary>
	/// Obtains an enchantment by ID, or falls back to the deprecated event if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static EventModel EventOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<EventModel>(id) ?? ModelDb.Event<DeprecatedEvent>();
	}

	/// <summary>
	/// Obtains an ancient event by ID, or falls back to the deprecated ancient event if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static AncientEventModel AncientEventOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<AncientEventModel>(id) ?? ModelDb.Event<DeprecatedAncientEvent>();
	}

	/// <summary>
	/// Obtains an encounter by ID, or falls back to the deprecated encounter if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static EncounterModel EncounterOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<EncounterModel>(id) ?? ModelDb.Encounter<DeprecatedEncounter>();
	}

	/// <summary>
	/// Obtains a card by ID, or falls back to the deprecated card if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static CardModel CardOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<CardModel>(id) ?? ModelDb.Card<DeprecatedCard>();
	}

	/// <summary>
	/// Obtains a relic by ID, or falls back to the deprecated relic if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static RelicModel RelicOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<RelicModel>(id) ?? ModelDb.Relic<DeprecatedRelic>();
	}

	/// <summary>
	/// Obtains a potion by ID, or falls back to the deprecated potion if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static PotionModel PotionOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<PotionModel>(id) ?? ModelDb.Potion<DeprecatedPotion>();
	}

	/// <summary>
	/// Obtains a modifier by ID, or falls back to the deprecated modifier if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static ModifierModel ModifierOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<ModifierModel>(id) ?? ModelDb.Modifier<DeprecatedModifier>();
	}

	/// <summary>
	/// Obtains an enchantment by ID, or falls back to the deprecated enchantment if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static EnchantmentModel EnchantmentOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<EnchantmentModel>(id) ?? ModelDb.Enchantment<DeprecatedEnchantment>();
	}

	/// <summary>
	/// Obtains a monster by ID, or falls back to the deprecated monster if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static MonsterModel MonsterOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<MonsterModel>(id) ?? ModelDb.Monster<DeprecatedMonster>();
	}

	/// <summary>
	/// Obtains a character by ID, or falls back to the deprecated character if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static CharacterModel CharacterOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<CharacterModel>(id) ?? ModelDb.Character<DeprecatedCharacter>();
	}

	/// <summary>
	/// Obtains a act by ID, or falls back to the deprecated act if it's not found.
	/// This should only be used in save loading where it's okay to fall back to deprecated content.
	/// </summary>
	public static ActModel ActOrDeprecated(ModelId id)
	{
		return ModelDb.GetByIdOrNull<ActModel>(id) ?? ModelDb.Act<DeprecatedAct>();
	}
}
