using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class TokenCardPool : CardPoolModel
{
	public override string Title => "token";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => Colors.White;

	public override bool IsColorless => true;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[14]
		{
			ModelDb.Card<Disintegration>(),
			ModelDb.Card<Fuel>(),
			ModelDb.Card<GiantRock>(),
			ModelDb.Card<Luminesce>(),
			ModelDb.Card<MindRot>(),
			ModelDb.Card<MinionDiveBomb>(),
			ModelDb.Card<MinionSacrifice>(),
			ModelDb.Card<MinionStrike>(),
			ModelDb.Card<Shiv>(),
			ModelDb.Card<Sloth>(),
			ModelDb.Card<Soul>(),
			ModelDb.Card<SovereignBlade>(),
			ModelDb.Card<SweepingGaze>(),
			ModelDb.Card<WasteAway>()
		};
	}
}
