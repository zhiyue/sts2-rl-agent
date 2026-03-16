using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public class LiftRestSiteOption : RestSiteOption
{
	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			Girya relic = base.Owner.GetRelic<Girya>();
			int num = 3 - relic.TimesLifted;
			description.Add("LiftsLeft", num);
			return description;
		}
	}

	public override string OptionId => "LIFT";

	public LiftRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		base.Owner.GetRelic<Girya>().TimesLifted++;
		return await Task.FromResult(result: true);
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
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<Girya>());
		if (nRelicFlashVfx == null)
		{
			return Task.CompletedTask;
		}
		nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
		nRelicFlashVfx.Position = Vector2.Zero;
		return Task.CompletedTask;
	}
}
