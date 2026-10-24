using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HellraiserPower : PowerModel
{
	private class Data
	{
		public readonly HashSet<CardModel> autoPlayingCards = new HashSet<CardModel>();

		public int infiniteAutoPlaysThisTurn;

		public bool showedCapReachedMessage;
	}

	private const int _infiniteAutoPlayCap = 9;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner.Creature != base.Owner || !card.Tags.Contains(CardTag.Strike))
		{
			return;
		}
		Data data = GetInternalData<Data>();
		bool flag = true;
		if (base.Owner.CombatState.HittableEnemies.All((Creature c) => c.HpDisplay.IsInfinite()))
		{
			if (data.infiniteAutoPlaysThisTurn >= 9)
			{
				flag = false;
				if (!data.showedCapReachedMessage)
				{
					ThinkCmd.Play(new LocString("powers", "HELLRAISER_POWER.infiniteAutoPlayCapReached"), base.Owner);
					data.showedCapReachedMessage = true;
				}
			}
			data.infiniteAutoPlaysThisTurn++;
		}
		else
		{
			ResetInfiniteAutoPlayData();
		}
		if (flag)
		{
			data.autoPlayingCards.Add(card);
			await CardCmd.AutoPlay(choiceContext, card, null);
			data.autoPlayingCards.Remove(card);
		}
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		ResetInfiniteAutoPlayData();
		return Task.CompletedTask;
	}

	public override Task BeforeAttack(AttackCommand command)
	{
		if (!GetInternalData<Data>().autoPlayingCards.Contains(command.ModelSource))
		{
			return Task.CompletedTask;
		}
		command.WithHitFx("vfx/hellraiser_attack_vfx", command.HitSfx, command.TmpHitSfx).WithAttackerAnim("Cast", command.Attacker.Player.Character.CastAnimDelay).SpawningHitVfxOnEachCreature()
			.WithHitVfxSpawnedAtBase();
		return Task.CompletedTask;
	}

	private void ResetInfiniteAutoPlayData()
	{
		Data internalData = GetInternalData<Data>();
		internalData.infiniteAutoPlaysThisTurn = 0;
		internalData.showedCapReachedMessage = false;
	}
}
