using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockResetCombatOnShufflePower : PowerModel
{
	private bool _hasReset;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	private bool HasReset
	{
		get
		{
			return _hasReset;
		}
		set
		{
			AssertMutable();
			_hasReset = value;
		}
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (!HasReset && oldPileType == PileType.Discard)
		{
			CardPile? pile = card.Pile;
			if (pile != null && pile.Type == PileType.Draw)
			{
				HasReset = true;
				CombatManager.Instance.Reset(graceful: true);
				return Task.CompletedTask;
			}
		}
		return Task.CompletedTask;
	}
}
