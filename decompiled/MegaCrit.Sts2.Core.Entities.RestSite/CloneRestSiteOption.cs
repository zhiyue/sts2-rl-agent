using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public class CloneRestSiteOption : RestSiteOption
{
	public override string OptionId => "CLONE";

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			description.Add("EnchantmentName", ModelDb.Enchantment<Clone>().Title.GetFormattedText());
			return description;
		}
	}

	public CloneRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		IEnumerable<CardModel> enumerable = base.Owner.Deck.Cards.Where((CardModel c) => c.Enchantment is Clone).ToList();
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		foreach (CardModel item in enumerable)
		{
			CardModel card = base.Owner.RunState.CloneCard(item);
			List<CardPileAddResult> list = results;
			list.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(results, 1.2f, CardPreviewStyle.MessyLayout);
		return true;
	}

	public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
		return Task.CompletedTask;
	}

	public override Task DoRemotePostSelectVfx()
	{
		NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First((NRestSiteCharacter c) => c.Player == base.Owner);
		nRestSiteCharacter?.Shake();
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<PaelsGrowth>());
		if (nRelicFlashVfx == null)
		{
			return Task.CompletedTask;
		}
		nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
		nRelicFlashVfx.Position = Vector2.Zero;
		return Task.CompletedTask;
	}
}
