using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class Trial : EventModel
{
	private static readonly string _trialMerchantVfx = SceneHelper.GetScenePath("vfx/events/trial_merchant_vfx");

	private static readonly string _trialNobleVfx = SceneHelper.GetScenePath("vfx/events/trial_noble_vfx");

	private static readonly string _trialNondescriptVfx = SceneHelper.GetScenePath("vfx/events/trial_nondescript_vfx");

	private const string _entrantNumberKey = "EntrantNumber";

	private const string _trialResultKey = "TrialResult";

	private const string _trialStoryKey = "TrialStory";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("EntrantNumber", -1m));

	private static string TrialStartedPath => ImageHelper.GetImagePath("events/trial_started.png");

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Accept, "TRIAL.pages.INITIAL.options.ACCEPT"),
			new EventOption(this, Reject, "TRIAL.pages.INITIAL.options.REJECT")
		});
	}

	public override IEnumerable<string> GetAssetPaths(IRunState runState)
	{
		List<string> list = new List<string>();
		list.AddRange(base.GetAssetPaths(runState));
		list.Add(TrialStartedPath);
		list.Add(_trialMerchantVfx);
		list.Add(_trialNobleVfx);
		list.Add(_trialNondescriptVfx);
		return new _003C_003Ez__ReadOnlyList<string>(list);
	}

	private Task Accept()
	{
		if (LocalContext.IsMe(base.Owner))
		{
			NEventRoom.Instance.Layout.RemoveNodesOnPortrait();
		}
		string portraitPath;
		string entryName;
		EventOption[] eventOptions;
		switch (base.Rng.NextInt(3))
		{
		case 0:
			portraitPath = _trialMerchantVfx;
			entryName = "TRIAL.pages.MERCHANT.description";
			eventOptions = new EventOption[2]
			{
				new EventOption(this, MerchantGuilty, "TRIAL.pages.MERCHANT.options.GUILTY", HoverTipFactory.FromCardWithCardHoverTips<Regret>()),
				new EventOption(this, MerchantInnocent, "TRIAL.pages.MERCHANT.options.INNOCENT", HoverTipFactory.FromCardWithCardHoverTips<Shame>())
			};
			break;
		case 1:
			portraitPath = _trialNobleVfx;
			entryName = "TRIAL.pages.NOBLE.description";
			eventOptions = new EventOption[2]
			{
				new EventOption(this, NobleGuilty, "TRIAL.pages.NOBLE.options.GUILTY"),
				new EventOption(this, NobleInnocent, "TRIAL.pages.NOBLE.options.INNOCENT", HoverTipFactory.FromCardWithCardHoverTips<Regret>())
			};
			break;
		case 2:
			portraitPath = _trialNondescriptVfx;
			entryName = "TRIAL.pages.NONDESCRIPT.description";
			eventOptions = new EventOption[2]
			{
				new EventOption(this, NondescriptGuilty, "TRIAL.pages.NONDESCRIPT.options.GUILTY", HoverTipFactory.FromCardWithCardHoverTips<Doubt>()),
				new EventOption(this, NondescriptInnocent, "TRIAL.pages.NONDESCRIPT.options.INNOCENT", HoverTipFactory.FromCardWithCardHoverTips<Doubt>().Concat(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Transform))))
			};
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		AddVfxAnchoredToPortrait(portraitPath);
		if (LocalContext.IsMe(base.Owner))
		{
			NEventRoom.Instance.SetPortrait(PreloadManager.Cache.GetTexture2D(TrialStartedPath));
		}
		LocString locString = L10NLookup("TRIAL.trialFormat");
		locString.Add(new StringVar("TrialStory", L10NLookup(entryName).GetRawText()));
		SetEventState(locString, eventOptions);
		return Task.CompletedTask;
	}

	private Task Reject()
	{
		EventOption[] eventOptions = new EventOption[2]
		{
			new EventOption(this, Accept, "TRIAL.pages.REJECT.options.ACCEPT"),
			new EventOption(this, DoubleDown, "TRIAL.pages.REJECT.options.DOUBLE_DOWN", false, true).ThatDoesDamage(9999m)
		};
		LocString description = L10NLookup("TRIAL.pages.REJECT.description");
		SetEventState(description, eventOptions);
		return Task.CompletedTask;
	}

	private Task DoubleDown()
	{
		NModalContainer.Instance.Add(NAbandonRunConfirmPopup.Create(null));
		return Task.CompletedTask;
	}

	private void AddVfxAnchoredToPortrait(string portraitPath)
	{
		if (LocalContext.IsMe(base.Owner))
		{
			Node2D node2D = PreloadManager.Cache.GetScene(portraitPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			node2D.Position = new Vector2(292f, 68f);
			NEventRoom.Instance.Layout.AddVfxAnchoredToPortrait(node2D);
		}
	}

	private async Task MerchantGuilty()
	{
		await CardPileCmd.AddCurseToDeck<Regret>(base.Owner);
		for (int i = 0; i < 2; i++)
		{
			await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable(), base.Owner);
		}
		SetTrialFinished("TRIAL.pages.MERCHANT_GUILTY.description");
	}

	private async Task MerchantInnocent()
	{
		await CardPileCmd.AddCurseToDeck<Shame>(base.Owner);
		foreach (CardModel item in await CardSelectCmd.FromDeckForUpgrade(prefs: new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 2), player: base.Owner))
		{
			CardCmd.Upgrade(item);
		}
		SetTrialFinished("TRIAL.pages.MERCHANT_INNOCENT.description");
	}

	private async Task NobleGuilty()
	{
		await CreatureCmd.Heal(base.Owner.Creature, 10m);
		SetTrialFinished("TRIAL.pages.NOBLE_GUILTY.description");
	}

	private async Task NobleInnocent()
	{
		await CardPileCmd.AddCurseToDeck<Regret>(base.Owner);
		await PlayerCmd.GainGold(300m, base.Owner);
		SetTrialFinished("TRIAL.pages.NOBLE_INNOCENT.description");
	}

	private async Task NondescriptGuilty()
	{
		await CardPileCmd.AddCurseToDeck<Doubt>(base.Owner);
		List<Reward> list = new List<Reward>();
		for (int i = 0; i < 2; i++)
		{
			list.Add(new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool)), 3, base.Owner));
		}
		await RewardsCmd.OfferCustom(base.Owner, list);
		SetTrialFinished("TRIAL.pages.NONDESCRIPT_GUILTY.description");
	}

	private async Task NondescriptInnocent()
	{
		await CardPileCmd.AddCurseToDeck<Doubt>(base.Owner);
		List<CardModel> list = (await CardSelectCmd.FromDeckForTransformation(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 2), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			await CardCmd.TransformToRandom(item, base.Owner.RunState.Rng.Niche, CardPreviewStyle.EventLayout);
		}
		SetTrialFinished("TRIAL.pages.NONDESCRIPT_INNOCENT.description");
	}

	private void SetTrialFinished(string trialResultLoc)
	{
		LocString locString = L10NLookup("TRIAL.trialResult");
		locString.Add(new StringVar("TrialResult", L10NLookup(trialResultLoc).GetRawText()));
		SetEventFinished(locString);
	}

	public override void CalculateVars()
	{
		if (base.DynamicVars["EntrantNumber"].BaseValue == -1m)
		{
			base.DynamicVars["EntrantNumber"].BaseValue = MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextInt(101, 999);
		}
	}
}
