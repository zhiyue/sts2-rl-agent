using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Wither : CardModel
{
	private int _fakeUpgradeLevel;

	public override string Title
	{
		get
		{
			string title = base.Title;
			if (FakeUpgradeLevel > 0)
			{
				return $"{title}+{FakeUpgradeLevel}";
			}
			return title;
		}
	}

	private int FakeUpgradeLevel
	{
		get
		{
			return _fakeUpgradeLevel;
		}
		set
		{
			AssertMutable();
			_fakeUpgradeLevel = value;
		}
	}

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(3m, ValueProp.Unpowered | ValueProp.Move));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

	public override bool HasTurnEndInHandEffect => true;

	protected override IEnumerable<string> ExtraRunAssetPaths => NGroundFireVfx.AssetPaths;

	public Wither()
		: base(-1, CardType.Status, CardRarity.Status, TargetType.None)
	{
	}

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.Damage, this);
	}

	public void FakeUpgrade()
	{
		FakeUpgradeLevel++;
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
