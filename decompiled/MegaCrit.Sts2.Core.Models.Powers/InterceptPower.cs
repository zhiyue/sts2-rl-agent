using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class InterceptPower : PowerModel
{
	private class Data
	{
		public readonly List<Creature> coveredCreatures = new List<Creature>();
	}

	private const string _coveringKey = "Covering";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new StringVar("Covering"));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public void AddCoveredCreature(Creature c)
	{
		List<Creature> coveredCreatures = GetInternalData<Data>().coveredCreatures;
		if (!GetInternalData<Data>().coveredCreatures.Contains(c))
		{
			coveredCreatures.Add(c);
		}
		StringVar stringVar = (StringVar)base.DynamicVars["Covering"];
		stringVar.StringValue = "";
		for (int i = 0; i < coveredCreatures.Count; i++)
		{
			stringVar.StringValue += PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, coveredCreatures[i].Player.NetId);
			if (i == coveredCreatures.Count - 2)
			{
				stringVar.StringValue += ", and ";
			}
			else if (i < coveredCreatures.Count - 2)
			{
				stringVar.StringValue += ", ";
			}
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return GetInternalData<Data>().coveredCreatures.Count + 1;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.Remove(this);
		}
	}
}
