using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Thrash : CardModel
{
	private decimal _extraDamage;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(4m, ValueProp.Move));

	private decimal ExtraDamage
	{
		get
		{
			return _extraDamage;
		}
		set
		{
			AssertMutable();
			_extraDamage = value;
		}
	}

	public Thrash()
		: base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_thrash")
			.Execute(choiceContext);
		CardPile pile = PileType.Hand.GetPile(base.Owner);
		CardModel cardModel = base.Owner.RunState.Rng.CombatCardSelection.NextItem(pile.Cards.Where((CardModel c) => c.Type == CardType.Attack));
		if (cardModel != null)
		{
			decimal damage = default(decimal);
			if (cardModel.DynamicVars.ContainsKey("CalculatedDamage"))
			{
				damage = cardModel.DynamicVars.CalculatedDamage.Calculate(null);
			}
			else if (cardModel.DynamicVars.ContainsKey("Damage"))
			{
				damage = cardModel.DynamicVars.Damage.BaseValue;
			}
			else if (cardModel.DynamicVars.ContainsKey("OstyDamage"))
			{
				damage = cardModel.DynamicVars.OstyDamage.BaseValue;
			}
			else
			{
				Log.Warn(base.Id.Entry + " exhausted attack card " + cardModel.Id.Entry + " that did not have an appropriate damage var!");
			}
			damage = Hook.ModifyDamage(base.Owner.RunState, base.Owner.Creature.CombatState, null, base.Owner.Creature, damage, ValueProp.Move, cardModel, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
			base.DynamicVars.Damage.BaseValue += damage;
			ExtraDamage += damage;
			await CardCmd.Exhaust(choiceContext, cardModel);
		}
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamage;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
