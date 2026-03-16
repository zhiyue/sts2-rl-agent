using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.HoverTips;

public static class HoverTipFactory
{
	private static readonly Dictionary<CardKeyword, HoverTip> _keywordHoverTips = new Dictionary<CardKeyword, HoverTip>();

	private static readonly Dictionary<ModelId, HoverTip> _potionHoverTips = new Dictionary<ModelId, HoverTip>();

	public static IEnumerable<IHoverTip> FromEnchantment<T>(int amount = 1) where T : EnchantmentModel
	{
		EnchantmentModel enchantmentModel = ModelDb.Enchantment<T>().ToMutable();
		enchantmentModel.Amount = amount;
		enchantmentModel.RecalculateValues();
		return enchantmentModel.HoverTips;
	}

	public static IEnumerable<IHoverTip> FromAffliction<T>(int amount = 1) where T : AfflictionModel
	{
		AfflictionModel afflictionModel = ModelDb.Affliction<T>().ToMutable();
		afflictionModel.Amount = amount;
		return afflictionModel.HoverTips;
	}

	public static IHoverTip FromKeyword(CardKeyword keyword)
	{
		if (!_keywordHoverTips.ContainsKey(keyword))
		{
			_keywordHoverTips[keyword] = new HoverTip(keyword.GetTitle(), keyword.GetDescription());
		}
		return _keywordHoverTips[keyword];
	}

	public static IHoverTip FromPower<T>() where T : PowerModel
	{
		PowerModel model = ModelDb.Power<T>();
		return FromPower(model);
	}

	public static IHoverTip FromPower(PowerModel model)
	{
		return model.DumbHoverTip;
	}

	public static IEnumerable<IHoverTip> FromPowerWithPowerHoverTips<T>() where T : PowerModel
	{
		return new IHoverTip[1] { FromPower<T>() }.Concat(ModelDb.Power<T>().HoverTips);
	}

	public static IHoverTip FromPotion<T>() where T : PotionModel
	{
		PotionModel model = ModelDb.Potion<T>();
		return FromPotion(model);
	}

	public static IHoverTip FromPotion(PotionModel model)
	{
		_potionHoverTips.TryAdd(model.Id, model.HoverTip);
		return _potionHoverTips[model.Id];
	}

	public static IHoverTip FromOrb<T>() where T : OrbModel
	{
		return ModelDb.Orb<T>().DumbHoverTip;
	}

	public static IEnumerable<IHoverTip> FromRelic<T>() where T : RelicModel
	{
		RelicModel relic = ModelDb.Relic<T>();
		return FromRelic(relic);
	}

	public static IEnumerable<IHoverTip> FromRelicExcludingItself<T>() where T : RelicModel
	{
		RelicModel relic = ModelDb.Relic<T>();
		return FromRelicExcludingItself(relic);
	}

	public static IEnumerable<IHoverTip> FromRelic(RelicModel relic)
	{
		return relic.HoverTips;
	}

	public static IEnumerable<IHoverTip> FromRelicExcludingItself(RelicModel relic)
	{
		return relic.HoverTipsExcludingRelic;
	}

	public static IEnumerable<IHoverTip> FromCardWithCardHoverTips<T>(bool inheritsUpgrades = false) where T : CardModel
	{
		return new IHoverTip[1] { FromCard<T>() }.Concat(ModelDb.Card<T>().HoverTips);
	}

	public static IHoverTip FromCard<T>(bool upgrade = false) where T : CardModel
	{
		return FromCard(ModelDb.Card<T>(), upgrade);
	}

	public static IHoverTip FromCard(CardModel card, bool upgrade = false)
	{
		if (upgrade)
		{
			card = (CardModel)card.MutableClone();
			card.UpgradeInternal();
			card.FinalizeUpgradeInternal();
		}
		return new CardHoverTip(card);
	}

	public static IHoverTip Static(StaticHoverTip tip, params DynamicVar[] vars)
	{
		string text = StringHelper.Slugify(tip.ToString());
		LocString locString = L10NStatic(text + ".title");
		LocString locString2 = L10NStatic(text + ".description");
		foreach (DynamicVar dynamicVar in vars)
		{
			locString.Add(dynamicVar);
			locString2.Add(dynamicVar);
		}
		return new HoverTip(locString, locString2);
	}

	public static IHoverTip ForEnergy(CardModel card)
	{
		return ForEnergyWithIconPath(EnergyIconHelper.GetPath(card));
	}

	public static IHoverTip ForEnergy(PotionModel potion)
	{
		return ForEnergyWithIconPath(EnergyIconHelper.GetPath(potion));
	}

	public static IHoverTip ForEnergy(PowerModel power)
	{
		return ForEnergyWithIconPath(EnergyIconHelper.GetPath(power));
	}

	public static IHoverTip ForEnergy(Player player)
	{
		return ForEnergyWithIconPath(EnergyIconHelper.GetPath(player.Character.CardPool));
	}

	public static IHoverTip ForEnergy(RelicModel relic)
	{
		return ForEnergyWithIconPath(EnergyIconHelper.GetPath(relic));
	}

	private static IHoverTip ForEnergyWithIconPath(string path)
	{
		string localCharacterEnergyIconPrefix = RunManager.Instance.GetLocalCharacterEnergyIconPrefix();
		return new HoverTip(L10NStatic("ENERGY.title"), L10NStatic("ENERGY.description"), PreloadManager.Cache.GetTexture2D((localCharacterEnergyIconPrefix != null) ? EnergyIconHelper.GetPath(localCharacterEnergyIconPrefix) : path));
	}

	public static IEnumerable<IHoverTip> FromForge()
	{
		List<IHoverTip> list = new List<IHoverTip>();
		list.Add(Static(StaticHoverTip.Forge));
		list.AddRange(FromCardWithCardHoverTips<SovereignBlade>());
		return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
	}

	private static LocString L10NStatic(string entry)
	{
		return new LocString("static_hover_tips", entry);
	}
}
