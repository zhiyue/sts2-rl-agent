using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class TheScythe : CardModel
{
	private const string _increaseKey = "Increase";

	private const int _baseDamage = 13;

	private int _currentDamage = 13;

	private int _increasedDamage;

	[SavedProperty]
	public int CurrentDamage
	{
		get
		{
			return _currentDamage;
		}
		set
		{
			AssertMutable();
			_currentDamage = value;
			base.DynamicVars.Damage.BaseValue = _currentDamage;
		}
	}

	[SavedProperty]
	public int IncreasedDamage
	{
		get
		{
			return _increasedDamage;
		}
		set
		{
			AssertMutable();
			_increasedDamage = value;
		}
	}

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(CurrentDamage, ValueProp.Move),
		new IntVar("Increase", 3m)
	});

	public TheScythe()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		int intValue = base.DynamicVars["Increase"].IntValue;
		BuffFromPlay(intValue);
		(base.DeckVersion as TheScythe)?.BuffFromPlay(intValue);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Increase"].UpgradeValueBy(1m);
	}

	protected override void AfterDowngraded()
	{
		UpdateDamage();
	}

	private void BuffFromPlay(int extraDamage)
	{
		IncreasedDamage += extraDamage;
		UpdateDamage();
	}

	private void UpdateDamage()
	{
		CurrentDamage = 13 + IncreasedDamage;
	}
}
