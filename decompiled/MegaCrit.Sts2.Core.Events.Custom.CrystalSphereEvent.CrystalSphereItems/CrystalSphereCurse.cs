using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems;

public class CrystalSphereCurse : CrystalSphereItem
{
	public override Vector2I Size => new Vector2I(2, 2);

	public override bool IsGood => false;

	public override async Task RevealItem(Player owner)
	{
		await base.RevealItem(owner);
		CardModel cardModel = await CardPileCmd.AddCurseToDeck<Doubt>(owner);
		if (cardModel != null)
		{
			RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(cardModel);
		}
	}

	public override SerializableCrystalSphereItem ToSerializable()
	{
		return new SerializableCrystalSphereItem
		{
			type = CrystalSphereItemType.Curse
		};
	}
}
