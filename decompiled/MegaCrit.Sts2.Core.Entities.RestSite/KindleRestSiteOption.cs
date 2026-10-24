using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public class KindleRestSiteOption : RestSiteOption
{
	public override string OptionId => "KINDLE";

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			description.Add("RelicName", ModelDb.Relic<PumpkinCandle>().Title.GetFormattedText());
			description.Add("RekindleAmount", 5m);
			return description;
		}
	}

	public KindleRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override Task<bool> OnSelect()
	{
		base.Owner.GetRelic<PumpkinCandle>()?.Rekindle();
		return Task.FromResult(result: true);
	}

	public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		PlayKindleVfx();
		return Task.CompletedTask;
	}

	public override Task DoRemotePostSelectVfx()
	{
		PlayKindleVfx();
		return Task.CompletedTask;
	}

	private void PlayKindleVfx()
	{
		SfxCmd.Play("event:/sfx/characters/attack_fire");
		NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First((NRestSiteCharacter c) => c.Player == base.Owner);
		nRestSiteCharacter?.Shake();
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<PumpkinCandle>());
		if (nRelicFlashVfx != null)
		{
			nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
			nRelicFlashVfx.Position = Vector2.Zero;
		}
	}
}
