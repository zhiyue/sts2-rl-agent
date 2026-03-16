using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class QuestCardPool : CardPoolModel
{
	public override string Title => "quest";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_quest";

	public override Color DeckEntryCardColor => new Color("24476A");

	public override Color EnergyOutlineColor => new Color("431E14");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[3]
		{
			ModelDb.Card<ByrdonisEgg>(),
			ModelDb.Card<LanternKey>(),
			ModelDb.Card<SpoilsMap>()
		};
	}
}
