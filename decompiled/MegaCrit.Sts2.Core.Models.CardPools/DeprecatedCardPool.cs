using Godot;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.CardPools;

public sealed class DeprecatedCardPool : CardPoolModel
{
	public override string Title => "token";

	public override string EnergyColorName => "colorless";

	public override string CardFrameMaterialPath => "card_frame_colorless";

	public override Color DeckEntryCardColor => Colors.White;

	public override bool IsColorless => true;

	protected override CardModel[] GenerateAllCards()
	{
		return new CardModel[1] { ModelDb.Card<DeprecatedCard>() };
	}
}
