using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class GeneticAlgorithm : CardModel
{
	private const string _increaseKey = "Increase";

	private int _currentBlock = 1;

	private int _increasedBlock;

	public override bool GainsBlock => true;

	[SavedProperty]
	public int CurrentBlock
	{
		get
		{
			return _currentBlock;
		}
		set
		{
			AssertMutable();
			_currentBlock = value;
			base.DynamicVars.Block.BaseValue = _currentBlock;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(CurrentBlock, ValueProp.Move),
		new IntVar("Increase", 3m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	[SavedProperty]
	public int IncreasedBlock
	{
		get
		{
			return _increasedBlock;
		}
		set
		{
			AssertMutable();
			_increasedBlock = value;
		}
	}

	public GeneticAlgorithm()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		int intValue = base.DynamicVars["Increase"].IntValue;
		BuffFromPlay(intValue);
		(base.DeckVersion as GeneticAlgorithm)?.BuffFromPlay(intValue);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Increase"].UpgradeValueBy(1m);
	}

	protected override void AfterDowngraded()
	{
		UpdateBlock();
	}

	private void BuffFromPlay(int extraBlock)
	{
		IncreasedBlock += extraBlock;
		UpdateBlock();
	}

	private void UpdateBlock()
	{
		CurrentBlock = 1 + IncreasedBlock;
	}
}
