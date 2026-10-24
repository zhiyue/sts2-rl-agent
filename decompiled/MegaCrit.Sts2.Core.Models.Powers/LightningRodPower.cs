using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class LightningRodPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>()
	});

	/// <remarks>
	/// We do this in AfterEnergyReset instead of BeforeSideTurnStart so the player will still get benefits from orbs
	/// that might be evoked to make room for the new Lightning Orb. Specifically:
	///
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Orbs.PlasmaOrb" />'s energy gain could be lost to energy reset if this triggered earlier.
	/// - <see cref="T:MegaCrit.Sts2.Core.Models.Orbs.FrostOrb" />'s block gain could be lost to block clear if this triggered earlier.
	/// </remarks>
	public override async Task AfterEnergyReset(Player player)
	{
		if (player == base.Owner.Player)
		{
			await OrbCmd.Channel<LightningOrb>(new ThrowingPlayerChoiceContext(), base.Owner.Player);
			await PowerCmd.Decrement(this);
		}
	}
}
