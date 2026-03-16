using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Nightmare : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<string> ExtraRunAssetPaths => NNightmareHandsVfx.AssetPaths;

	public Nightmare()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (TestMode.IsOff)
		{
			NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.3f, 0.8f, 0.66f), new Color(0f, 0f, 4f, 0.33f));
			NGame.Instance.CurrentRunNode.GlobalUi.AddChildSafely(child);
			NGame.Instance.CurrentRunNode.GlobalUi.AddChildSafely(NNightmareHandsVfx.Create());
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		CardModel selectedCard = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
		if (selectedCard != null)
		{
			(await PowerCmd.Apply<NightmarePower>(base.Owner.Creature, 3m, base.Owner.Creature, this)).SetSelectedCard(selectedCard);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
