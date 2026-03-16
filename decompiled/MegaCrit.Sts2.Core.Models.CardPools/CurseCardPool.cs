using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class CurseCardPool : CardPoolModel
{
	public override string Title => "curse";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_curse";

	public override Color DeckEntryCardColor => new Color("585B61FF");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[18]
		{
			ModelDb.Card<AscendersBane>(),
			ModelDb.Card<BadLuck>(),
			ModelDb.Card<Clumsy>(),
			ModelDb.Card<CurseOfTheBell>(),
			ModelDb.Card<Debt>(),
			ModelDb.Card<Decay>(),
			ModelDb.Card<Doubt>(),
			ModelDb.Card<Enthralled>(),
			ModelDb.Card<Folly>(),
			ModelDb.Card<Greed>(),
			ModelDb.Card<Guilty>(),
			ModelDb.Card<Injury>(),
			ModelDb.Card<Normality>(),
			ModelDb.Card<PoorSleep>(),
			ModelDb.Card<Regret>(),
			ModelDb.Card<Shame>(),
			ModelDb.Card<SporeMind>(),
			ModelDb.Card<Writhe>()
		};
	}
}
