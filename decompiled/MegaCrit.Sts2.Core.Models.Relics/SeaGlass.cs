using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public class SeaGlass : RelicModel
{
	private const string _characterKey = "Character";

	private ModelId? _characterId;

	public override LocString Title
	{
		get
		{
			if (Character == null)
			{
				return new LocString("relics", base.Id.Entry + ".title");
			}
			return new LocString("relics", base.Id.Entry + "." + Character.Id.Entry + ".title");
		}
	}

	[SavedProperty]
	public ModelId? CharacterId
	{
		get
		{
			return _characterId;
		}
		set
		{
			AssertMutable();
			_characterId = value;
			((StringVar)base.DynamicVars["Character"]).StringValue = Character.Title.GetFormattedText();
		}
	}

	private CharacterModel? Character
	{
		get
		{
			if (!(CharacterId != null))
			{
				return null;
			}
			return ModelDb.GetById<CharacterModel>(CharacterId);
		}
	}

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(15),
		new StringVar("Character")
	});

	public override async Task AfterObtained()
	{
		if (CharacterId == null)
		{
			Log.Error("Sea Glass was obtained without a character ID assigned! This could be a bug, or the player could have used the console. Defaulting to Ironclad");
			CharacterId = ModelDb.Character<Ironclad>().Id;
		}
		int cardCount = base.DynamicVars.Cards.IntValue / 3;
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(Character.CardPool), (CardModel c) => c.Rarity == CardRarity.Common).WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
		CardCreationOptions options2 = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(Character.CardPool), (CardModel c) => c.Rarity == CardRarity.Uncommon).WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
		CardCreationOptions options3 = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(Character.CardPool), (CardModel c) => c.Rarity == CardRarity.Rare).WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
		List<CardCreationResult> first = CardFactory.CreateForReward(base.Owner, cardCount, options).ToList();
		List<CardCreationResult> second = CardFactory.CreateForReward(base.Owner, cardCount, options2).ToList();
		List<CardCreationResult> second2 = CardFactory.CreateForReward(base.Owner, cardCount, options3).ToList();
		List<CardCreationResult> list = first.Concat(second).Concat(second2).ToList();
		foreach (CardModel item in await CardSelectCmd.FromSimpleGridForRewards(prefs: new CardSelectorPrefs(RelicModel.L10NLookup(base.Id.Entry + ".selectionScreenPrompt"), 0, list.Count), context: new BlockingPlayerChoiceContext(), cards: list, player: base.Owner))
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
		}
	}
}
