using System.Text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class PotionUsedEntry : CombatHistoryEntry
{
	public PotionModel Potion { get; }

	public Creature? Target { get; }

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(base.Actor.Player.Character.Id.Entry + " drank " + Potion.Id.Entry);
			if (Target != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
				handler.AppendLiteral(" targeting ");
				handler.AppendFormatted(Target.Monster.Id.Entry);
				stringBuilder2.Append(ref handler);
			}
			return stringBuilder.ToString();
		}
	}

	public PotionUsedEntry(PotionModel potion, Creature? target, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(potion.Owner.Creature, roundNumber, currentSide, history)
	{
		Potion = potion;
		Target = target;
	}
}
