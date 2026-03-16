using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ArchaicTooth : RelicModel
{
	private const string _starterCardKey = "StarterCard";

	private const string _ancientCardKey = "AncientCard";

	private SerializableCard? _serializableStarterCard;

	private SerializableCard? _serializableAncientCard;

	private List<IHoverTip> _extraHoverTips = new List<IHoverTip>();

	private static Dictionary<ModelId, CardModel> TranscendenceUpgrades => new Dictionary<ModelId, CardModel>
	{
		{
			ModelDb.Card<Bash>().Id,
			ModelDb.Card<Break>()
		},
		{
			ModelDb.Card<Neutralize>().Id,
			ModelDb.Card<Suppress>()
		},
		{
			ModelDb.Card<Unleash>().Id,
			ModelDb.Card<Protector>()
		},
		{
			ModelDb.Card<FallingStar>().Id,
			ModelDb.Card<MeteorShower>()
		},
		{
			ModelDb.Card<Dualcast>().Id,
			ModelDb.Card<Quadcast>()
		}
	};

	public static List<CardModel> TranscendenceCards => TranscendenceUpgrades.Values.ToList();

	public override RelicRarity Rarity => RelicRarity.Ancient;

	[SavedProperty]
	public SerializableCard? StarterCard
	{
		get
		{
			return _serializableStarterCard;
		}
		private set
		{
			AssertMutable();
			_serializableStarterCard = value;
			UpdateHoverTips();
		}
	}

	[SavedProperty]
	public SerializableCard? AncientCard
	{
		get
		{
			return _serializableAncientCard;
		}
		private set
		{
			AssertMutable();
			_serializableAncientCard = value;
			UpdateHoverTips();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("StarterCard"),
		new StringVar("AncientCard")
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => _extraHoverTips;

	protected override void AfterCloned()
	{
		base.AfterCloned();
		_extraHoverTips = new List<IHoverTip>();
	}

	public bool SetupForPlayer(Player player)
	{
		AssertMutable();
		CardModel transcendenceStarterCard = GetTranscendenceStarterCard(player);
		if (transcendenceStarterCard != null)
		{
			StarterCard = transcendenceStarterCard.ToSerializable();
			AncientCard = GetTranscendenceTransformedCard(transcendenceStarterCard).ToSerializable();
			UpdateHoverTips();
			return true;
		}
		return false;
	}

	public void SetupForTests(SerializableCard starterCard, SerializableCard ancientCard)
	{
		AssertMutable();
		StarterCard = starterCard;
		AncientCard = ancientCard;
		UpdateHoverTips();
	}

	private void UpdateHoverTips()
	{
		_extraHoverTips.Clear();
		if (StarterCard != null)
		{
			CardModel cardModel = CardModel.FromSerializable(StarterCard);
			_extraHoverTips.AddRange(cardModel.HoverTips);
			_extraHoverTips.Add(HoverTipFactory.FromCard(cardModel));
			((StringVar)base.DynamicVars["StarterCard"]).StringValue = cardModel.Title;
		}
		if (AncientCard != null)
		{
			CardModel cardModel2 = CardModel.FromSerializable(AncientCard);
			_extraHoverTips.AddRange(cardModel2.HoverTips);
			_extraHoverTips.Add(HoverTipFactory.FromCard(cardModel2));
			((StringVar)base.DynamicVars["AncientCard"]).StringValue = cardModel2.Title;
		}
	}

	private CardModel? GetTranscendenceStarterCard(Player player)
	{
		return player.Deck.Cards.FirstOrDefault((CardModel c) => TranscendenceUpgrades.ContainsKey(c.Id));
	}

	private CardModel GetTranscendenceTransformedCard(CardModel starterCard)
	{
		if (TranscendenceUpgrades.TryGetValue(starterCard.Id, out CardModel value))
		{
			CardModel cardModel = starterCard.Owner.RunState.CreateCard(value, starterCard.Owner);
			if (starterCard.IsUpgraded)
			{
				CardCmd.Upgrade(cardModel);
			}
			if (starterCard.Enchantment != null)
			{
				EnchantmentModel enchantmentModel = (EnchantmentModel)starterCard.Enchantment.MutableClone();
				CardCmd.Enchant(enchantmentModel, cardModel, enchantmentModel.Amount);
			}
			return cardModel;
		}
		return base.Owner.RunState.CreateCard<Doubt>(starterCard.Owner);
	}

	public override async Task AfterObtained()
	{
		CardModel transcendenceStarterCard = GetTranscendenceStarterCard(base.Owner);
		await CardCmd.Transform(transcendenceStarterCard, GetTranscendenceTransformedCard(transcendenceStarterCard));
	}
}
