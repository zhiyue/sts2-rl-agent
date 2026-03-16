using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SurroundedPower : PowerModel
{
	public enum Direction
	{
		Right,
		Left
	}

	private Direction _facing;

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Direction Facing
	{
		get
		{
			return _facing;
		}
		private set
		{
			AssertMutable();
			_facing = value;
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer == null)
		{
			return 1m;
		}
		if (target != base.Owner)
		{
			return 1m;
		}
		switch (Facing)
		{
		case Direction.Right:
			if (!dealer.HasPower<BackAttackLeftPower>())
			{
				return 1m;
			}
			break;
		case Direction.Left:
			if (!dealer.HasPower<BackAttackRightPower>())
			{
				return 1m;
			}
			break;
		}
		return 1.5m;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Target != null && cardPlay.Card.Owner == base.Owner.Player)
		{
			await UpdateDirection(cardPlay.Target);
		}
	}

	public override async Task BeforePotionUsed(PotionModel potion, Creature? target)
	{
		if (CombatManager.Instance.IsInProgress && target != null && potion.Owner == base.Owner.Player)
		{
			await UpdateDirection(target);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented && creature.Side != base.Owner.Side)
		{
			IReadOnlyList<Creature> hittableEnemies = base.Owner.CombatState.HittableEnemies;
			if (hittableEnemies.Count != 0 && (hittableEnemies.All((Creature e) => e.HasPower<BackAttackLeftPower>()) || hittableEnemies.All((Creature e) => e.HasPower<BackAttackRightPower>())))
			{
				await UpdateDirection(hittableEnemies[0]);
			}
		}
	}

	private async Task UpdateDirection(Creature target)
	{
		switch (Facing)
		{
		case Direction.Right:
			if (target.HasPower<BackAttackLeftPower>())
			{
				await FaceDirection(Direction.Left);
			}
			break;
		case Direction.Left:
			if (target.HasPower<BackAttackRightPower>())
			{
				await FaceDirection(Direction.Right);
			}
			break;
		}
	}

	private async Task FaceDirection(Direction direction)
	{
		Facing = direction;
		Creature owner = base.Owner;
		IReadOnlyList<Creature> pets = base.Owner.Pets;
		int num = 0;
		Creature[] array = new Creature[1 + pets.Count];
		array[num] = owner;
		num++;
		foreach (Creature item in pets)
		{
			array[num] = item;
			num++;
		}
		IEnumerable<Creature> source = new global::_003C_003Ez__ReadOnlyArray<Creature>(array);
		IEnumerable<Node2D> enumerable = source.Select((Creature c) => NCombatRoom.Instance?.GetCreatureNode(c)?.Body);
		foreach (Node2D item2 in enumerable)
		{
			await FlipScale(item2);
		}
	}

	private Task FlipScale(Node2D? body)
	{
		if (body == null)
		{
			return Task.CompletedTask;
		}
		float x = body.Scale.X;
		if ((Facing == Direction.Right && x < 0f) || (Facing == Direction.Left && x > 0f))
		{
			body.Scale *= new Vector2(-1f, 1f);
		}
		return Task.CompletedTask;
	}
}
