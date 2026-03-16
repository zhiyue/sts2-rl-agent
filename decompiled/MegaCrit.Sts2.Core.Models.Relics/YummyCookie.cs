using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class YummyCookie : RelicModel
{
	private static CharacterModel? _cachedRandomCharacter;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override string IconBaseName
	{
		get
		{
			CharacterModel characterModel;
			if (base.IsCanonical || base.Owner == null)
			{
				if (_cachedRandomCharacter == null)
				{
					_cachedRandomCharacter = Rng.Chaotic.NextItem(ModelDb.AllCharacters);
				}
				characterModel = _cachedRandomCharacter;
			}
			else
			{
				characterModel = base.Owner.Character;
			}
			if (!(characterModel is Ironclad))
			{
				if (!(characterModel is Silent))
				{
					if (!(characterModel is Regent))
					{
						if (!(characterModel is Necrobinder))
						{
							if (characterModel is Defect)
							{
								return "yummy_cookie_defect";
							}
							return "yummy_cookie_ironclad";
						}
						return "yummy_cookie_necro";
					}
					return "yummy_cookie_regent";
				}
				return "yummy_cookie_silent";
			}
			return "yummy_cookie_ironclad";
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(4));

	protected override void AfterCloned()
	{
		base.AfterCloned();
		RelicIconChanged();
	}

	public override async Task AfterObtained()
	{
		List<CardModel> list = (await CardSelectCmd.FromDeckForUpgrade(prefs: new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			CardCmd.Upgrade(item);
		}
	}
}
