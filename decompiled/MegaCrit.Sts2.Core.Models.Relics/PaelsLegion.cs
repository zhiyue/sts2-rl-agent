using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaelsLegion : RelicModel
{
	private const string _turnsKey = "Turns";

	private string _skin = SkinOptions[0];

	private int _cooldown;

	private bool _triggeredBlockLastTurn;

	private CardPlay? _affectedCardPlay;

	public override bool AddsPet => true;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override bool ShowCounter => DisplayAmount > 0;

	public static string[] SkinOptions => new string[4] { "eyes", "horns", "spikes", "wings" };

	[SavedProperty]
	public string Skin
	{
		get
		{
			return _skin;
		}
		set
		{
			AssertMutable();
			_skin = value;
		}
	}

	public override int DisplayAmount
	{
		get
		{
			if (!CombatManager.Instance.IsInProgress)
			{
				return -1;
			}
			if (base.IsCanonical)
			{
				return -1;
			}
			if (_cooldown <= 0)
			{
				return -1;
			}
			return _cooldown;
		}
	}

	private int Cooldown
	{
		get
		{
			return _cooldown;
		}
		set
		{
			AssertMutable();
			_cooldown = value;
			InvokeDisplayAmountChanged();
		}
	}

	private bool TriggeredBlockLastTurn
	{
		get
		{
			return _triggeredBlockLastTurn;
		}
		set
		{
			AssertMutable();
			_triggeredBlockLastTurn = value;
			InvokeDisplayAmountChanged();
		}
	}

	private CardPlay? AffectedCardPlay
	{
		get
		{
			return _affectedCardPlay;
		}
		set
		{
			AssertMutable();
			_affectedCardPlay = value;
			InvokeDisplayAmountChanged();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Turns", 2m));

	public override async Task AfterObtained()
	{
		Skin = new Rng((uint)(base.Owner.NetId + base.Owner.RunState.Rng.Seed)).NextItem(SkinOptions);
		if (CombatManager.Instance.IsInProgress)
		{
			await SummonPet();
		}
	}

	public override async Task BeforeCombatStart()
	{
		await SummonPet();
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!props.IsCardOrMonsterMove())
		{
			return 1m;
		}
		if (cardSource == null)
		{
			return 1m;
		}
		if (target != base.Owner.Creature)
		{
			return 1m;
		}
		if (Cooldown > 0)
		{
			return 1m;
		}
		return 2m;
	}

	public override Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (modifiedAmount <= 0m)
		{
			return Task.CompletedTask;
		}
		if (cardPlay == null)
		{
			return Task.CompletedTask;
		}
		if (AffectedCardPlay != null && AffectedCardPlay != cardPlay)
		{
			return Task.CompletedTask;
		}
		AffectedCardPlay = cardPlay;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (AffectedCardPlay != null && AffectedCardPlay == cardPlay)
		{
			Flash();
			AffectedCardPlay = null;
			Cooldown = base.DynamicVars["Turns"].IntValue;
			base.Status = RelicStatus.Normal;
			MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion paelsLegion = (MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion)base.Owner.PlayerCombatState.GetPet<MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion>().Monster;
			await CreatureCmd.TriggerAnim(paelsLegion.Creature, "BlockTrigger", 0.15f);
			TriggeredBlockLastTurn = true;
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Creature.Side)
		{
			bool flag = Cooldown > 0;
			Cooldown--;
			if (Cooldown <= 0)
			{
				base.Status = RelicStatus.Active;
				InvokeDisplayAmountChanged();
			}
			MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion paelsLegion = (MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion)base.Owner.PlayerCombatState.GetPet<MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion>().Monster;
			if (Cooldown > 0 && TriggeredBlockLastTurn)
			{
				await CreatureCmd.TriggerAnim(paelsLegion.Creature, "SleepTrigger", 0.15f);
			}
			else if (Cooldown <= 0 && flag)
			{
				await CreatureCmd.TriggerAnim(paelsLegion.Creature, "WakeUpTrigger", 0.15f);
			}
			TriggeredBlockLastTurn = false;
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		Cooldown = 0;
		TriggeredBlockLastTurn = false;
		AffectedCardPlay = null;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task SummonPet()
	{
		await PlayerCmd.AddPet<MegaCrit.Sts2.Core.Models.Monsters.PaelsLegion>(base.Owner);
	}
}
