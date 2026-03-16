using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class ByrdonisEgg : CardModel
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public ByrdonisEgg()
		: base(-1, CardType.Quest, CardRarity.Quest, TargetType.None)
	{
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		options.Add(new HatchRestSiteOption(player));
		return true;
	}
}
