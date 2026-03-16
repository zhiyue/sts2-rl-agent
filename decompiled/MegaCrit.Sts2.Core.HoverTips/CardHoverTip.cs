using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.HoverTips;

public class CardHoverTip : IHoverTip
{
	public CardModel Card { get; }

	public string Id { get; }

	public bool IsSmart => true;

	public bool IsDebuff => false;

	public bool IsInstanced => false;

	public AbstractModel CanonicalModel => Card.CanonicalInstance;

	public CardHoverTip(CardModel card)
	{
		if (!card.IsMutable)
		{
			Card = card.ToMutable();
		}
		else
		{
			Card = card;
		}
		Id = Card.Id.ToString();
		if (card.IsUpgraded)
		{
			Id += "+";
		}
	}
}
