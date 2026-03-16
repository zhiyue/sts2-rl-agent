using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class EternalFeather : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(5),
		new HealVar(3m)
	});

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is RestSiteRoom)
		{
			Flash();
			int num = PileType.Deck.GetPile(base.Owner).Cards.Count / base.DynamicVars.Cards.IntValue;
			decimal healAmount = base.DynamicVars.Heal.BaseValue * (decimal)num;
			await CreatureCmd.Heal(base.Owner.Creature, healAmount);
			if (LocalContext.IsMe(base.Owner))
			{
				PlayerFullscreenHealVfx.Play(base.Owner, healAmount, NRestSiteRoom.Instance);
			}
		}
	}
}
