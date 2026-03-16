using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TouchOfOrobas : RelicModel
{
	private const string _starterRelicKey = "StarterRelic";

	private const string _upgradedRelicKey = "UpgradedRelic";

	private ModelId? _starterRelic;

	private ModelId? _upgradedRelic;

	private List<IHoverTip> _extraHoverTips = new List<IHoverTip>();

	public override RelicRarity Rarity => RelicRarity.Ancient;

	private static Dictionary<ModelId, RelicModel> RefinementUpgrades => new Dictionary<ModelId, RelicModel>
	{
		{
			ModelDb.Relic<BurningBlood>().Id,
			ModelDb.Relic<BlackBlood>()
		},
		{
			ModelDb.Relic<RingOfTheSnake>().Id,
			ModelDb.Relic<RingOfTheDrake>()
		},
		{
			ModelDb.Relic<DivineRight>().Id,
			ModelDb.Relic<DivineDestiny>()
		},
		{
			ModelDb.Relic<BoundPhylactery>().Id,
			ModelDb.Relic<PhylacteryUnbound>()
		},
		{
			ModelDb.Relic<CrackedCore>().Id,
			ModelDb.Relic<InfusedCore>()
		}
	};

	[SavedProperty]
	public ModelId? StarterRelic
	{
		get
		{
			return _starterRelic;
		}
		set
		{
			AssertMutable();
			if (_starterRelic != null)
			{
				throw new InvalidOperationException("Recursive Core setup called twice!");
			}
			_starterRelic = value;
			if (_starterRelic != null)
			{
				RelicModel relicModel = SaveUtil.RelicOrDeprecated(_starterRelic);
				_extraHoverTips.AddRange(relicModel.HoverTips);
				((StringVar)base.DynamicVars["StarterRelic"]).StringValue = relicModel.Title.GetFormattedText();
			}
		}
	}

	[SavedProperty]
	public ModelId? UpgradedRelic
	{
		get
		{
			return _upgradedRelic;
		}
		set
		{
			AssertMutable();
			if (_upgradedRelic != null)
			{
				throw new InvalidOperationException("Recursive Core setup called twice!");
			}
			_upgradedRelic = value;
			if (_upgradedRelic != null)
			{
				RelicModel relicModel = SaveUtil.RelicOrDeprecated(_upgradedRelic);
				_extraHoverTips.AddRange(relicModel.HoverTips);
				((StringVar)base.DynamicVars["UpgradedRelic"]).StringValue = relicModel.Title.GetFormattedText();
			}
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => _extraHoverTips;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("StarterRelic"),
		new StringVar("UpgradedRelic")
	});

	protected override void AfterCloned()
	{
		base.AfterCloned();
		_extraHoverTips = new List<IHoverTip>();
	}

	private RelicModel? GetStarterRelic(Player p)
	{
		return p.Relics.FirstOrDefault((RelicModel r) => r.Rarity == RelicRarity.Starter);
	}

	public RelicModel GetUpgradedStarterRelic(RelicModel starterRelic)
	{
		if (RefinementUpgrades.TryGetValue(starterRelic.Id, out RelicModel value))
		{
			return value;
		}
		return ModelDb.Relic<Circlet>().ToMutable();
	}

	public bool SetupForPlayer(Player player)
	{
		AssertMutable();
		RelicModel starterRelic = GetStarterRelic(player);
		if (starterRelic != null)
		{
			StarterRelic = starterRelic.Id;
			UpgradedRelic = GetUpgradedStarterRelic(starterRelic).Id;
			return true;
		}
		return false;
	}

	public void SetupForTests(ModelId starterRelic, ModelId upgradedRelic)
	{
		AssertMutable();
		StarterRelic = starterRelic;
		UpgradedRelic = upgradedRelic;
	}

	public override async Task AfterObtained()
	{
		ModelId id = StarterRelic ?? base.Owner.Relics.First((RelicModel r) => r.Rarity == RelicRarity.Starter).Id;
		RelicModel relicById = base.Owner.GetRelicById(id);
		ModelId id2 = UpgradedRelic ?? GetUpgradedStarterRelic(relicById).Id;
		RelicModel replace = ModelDb.GetById<RelicModel>(id2).ToMutable();
		await RelicCmd.Replace(relicById, replace);
	}
}
