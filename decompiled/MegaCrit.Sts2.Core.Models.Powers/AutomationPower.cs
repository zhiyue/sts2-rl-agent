using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class AutomationPower : PowerModel
{
	private class Data
	{
		public int cardsLeft = 10;
	}

	private const int _baseCardsLeft = 10;

	private const string _baseCardsKey = "BaseCards";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => GetInternalData<Data>().cardsLeft;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("BaseCards", 10m));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner == base.Owner.Player)
		{
			Data data = GetInternalData<Data>();
			data.cardsLeft--;
			InvokeDisplayAmountChanged();
			if (data.cardsLeft <= 0)
			{
				Flash();
				await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);
				data.cardsLeft = 10;
				InvokeDisplayAmountChanged();
			}
		}
	}
}
