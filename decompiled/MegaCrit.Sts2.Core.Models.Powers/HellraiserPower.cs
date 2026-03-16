using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HellraiserPower : PowerModel
{
	private HashSet<CardModel>? _autoplayingCards;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	private HashSet<CardModel> AutoplayingCards
	{
		get
		{
			AssertMutable();
			if (_autoplayingCards == null)
			{
				_autoplayingCards = new HashSet<CardModel>();
			}
			return _autoplayingCards;
		}
	}

	public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner.Creature == base.Owner && card.Tags.Contains(CardTag.Strike))
		{
			AutoplayingCards.Add(card);
			await CardCmd.AutoPlay(choiceContext, card, null);
			AutoplayingCards.Remove(card);
		}
	}

	public override Task BeforeAttack(AttackCommand command)
	{
		if (!AutoplayingCards.Contains(command.ModelSource))
		{
			return Task.CompletedTask;
		}
		command.WithHitFx("vfx/hellraiser_attack_vfx", command.HitSfx, command.TmpHitSfx).WithAttackerAnim("Cast", command.Attacker.Player.Character.CastAnimDelay).SpawningHitVfxOnEachCreature()
			.WithHitVfxSpawnedAtBase();
		return Task.CompletedTask;
	}
}
