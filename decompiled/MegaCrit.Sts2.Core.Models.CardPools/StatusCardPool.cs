using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class StatusCardPool : CardPoolModel
{
	public override string Title => "status";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => Colors.White;

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[11]
		{
			ModelDb.Card<Beckon>(),
			ModelDb.Card<Burn>(),
			ModelDb.Card<Dazed>(),
			ModelDb.Card<Debris>(),
			ModelDb.Card<FranticEscape>(),
			ModelDb.Card<Infection>(),
			ModelDb.Card<Slimed>(),
			ModelDb.Card<Soot>(),
			ModelDb.Card<Toxic>(),
			ModelDb.Card<Void>(),
			ModelDb.Card<Wound>()
		};
	}
}
