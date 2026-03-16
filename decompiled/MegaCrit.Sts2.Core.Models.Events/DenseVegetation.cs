using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DenseVegetation : EventModel
{
	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HealVar(0m),
		new HpLossVar(11m)
	});

	public override void CalculateVars()
	{
		base.DynamicVars.Heal.BaseValue = ((base.Owner != null) ? HealRestSiteOption.GetHealAmount(base.Owner) : 0m);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, TrudgeOn, "DENSE_VEGETATION.pages.INITIAL.options.TRUDGE_ON").ThatDoesDamage(base.DynamicVars.HpLoss.BaseValue),
			new EventOption(this, Rest, "DENSE_VEGETATION.pages.INITIAL.options.REST")
		});
	}

	private async Task TrudgeOn()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		Control container = NEventRoom.Instance?.VfxContainer;
		if (LocalContext.IsMe(base.Owner) && container != null)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 vector = new Vector2(container.Size.X * 0.25f, container.Size.Y * 0.6f);
				Vector2 vector2 = new Vector2(MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-100f, 100f), MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat(-200f, 200f));
				Node2D node2D = VfxCmd.PlayNonCombatVfx(container, vector + vector2, "vfx/vfx_attack_slash");
				Node2D node2D2 = VfxCmd.PlayNonCombatVfx(container, vector + vector2, "vfx/events/dense_vegetation_slice_vfx");
				NDebugAudioManager.Instance.Play("slash_attack.mp3", 0.8f, PitchVariance.Medium);
				node2D.RotationDegrees = (0f - MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat()) * 180f;
				node2D2.RotationDegrees = (0f - MegaCrit.Sts2.Core.Random.Rng.Chaotic.NextFloat()) * 180f;
				await Cmd.CustomScaledWait(0.2f, 0.4f);
			}
		}
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(L10NLookup("DENSE_VEGETATION.pages.TRUDGE_ON.description"));
	}

	private async Task Rest()
	{
		await PlayerCmd.MimicRestSiteHeal(base.Owner, playSfx: false);
		if (LocalContext.IsMe(base.Owner))
		{
			int restHandle = NDebugAudioManager.Instance.Play("sleep.tres", 0.8f);
			await Cmd.CustomScaledWait(0.7f, 1.5f);
			NDebugAudioManager.Instance.Stop(restHandle);
			NDebugAudioManager.Instance.Play("hiss.mp3", 0.8f, PitchVariance.Large);
			NGame.Instance.ScreenRumble(ShakeStrength.Medium, ShakeDuration.Normal, RumbleStyle.Rumble);
		}
		SetEventState(L10NLookup("DENSE_VEGETATION.pages.REST.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, Fight, "DENSE_VEGETATION.pages.REST.options.FIGHT")));
	}

	private Task Fight()
	{
		EnterCombatWithoutExitingEvent<DenseVegetationEventEncounter>(Array.Empty<Reward>(), shouldResumeAfterCombat: false);
		return Task.CompletedTask;
	}
}
