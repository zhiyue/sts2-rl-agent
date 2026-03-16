using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class EventCardPool : CardPoolModel
{
	public override string Title => "event";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => new Color("A3A3A3FF");

	public override bool IsColorless => true;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[27]
		{
			ModelDb.Card<Apotheosis>(),
			ModelDb.Card<Apparition>(),
			ModelDb.Card<BrightestFlame>(),
			ModelDb.Card<ByrdSwoop>(),
			ModelDb.Card<Caltrops>(),
			ModelDb.Card<Clash>(),
			ModelDb.Card<Distraction>(),
			ModelDb.Card<DualWield>(),
			ModelDb.Card<Enlightenment>(),
			ModelDb.Card<Entrench>(),
			ModelDb.Card<Exterminate>(),
			ModelDb.Card<FeedingFrenzy>(),
			ModelDb.Card<HelloWorld>(),
			ModelDb.Card<MadScience>(),
			ModelDb.Card<Maul>(),
			ModelDb.Card<Metamorphosis>(),
			ModelDb.Card<NeowsFury>(),
			ModelDb.Card<Outmaneuver>(),
			ModelDb.Card<Peck>(),
			ModelDb.Card<Rebound>(),
			ModelDb.Card<Relax>(),
			ModelDb.Card<RipAndTear>(),
			ModelDb.Card<Squash>(),
			ModelDb.Card<Stack>(),
			ModelDb.Card<ToricToughness>(),
			ModelDb.Card<Wish>(),
			ModelDb.Card<Whistle>()
		};
	}
}
