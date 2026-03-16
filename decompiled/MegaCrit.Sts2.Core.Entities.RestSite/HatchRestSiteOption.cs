using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Entities.RestSite;

public class HatchRestSiteOption : RestSiteOption
{
	public override string OptionId => "HATCH";

	public HatchRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		await RelicCmd.Obtain<Byrdpip>(base.Owner);
		return true;
	}

	public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		SfxCmd.Play("event:/sfx/byrdpip/byrdpip_attack");
		return Task.CompletedTask;
	}

	public override Task DoRemotePostSelectVfx()
	{
		SfxCmd.Play("event:/sfx/byrdpip/byrdpip_attack");
		NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First((NRestSiteCharacter c) => c.Player == base.Owner);
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<Byrdpip>());
		if (nRelicFlashVfx == null)
		{
			return Task.CompletedTask;
		}
		nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
		nRelicFlashVfx.Position = Vector2.Zero;
		return Task.CompletedTask;
	}
}
