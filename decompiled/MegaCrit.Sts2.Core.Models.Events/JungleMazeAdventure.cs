using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class JungleMazeAdventure : EventModel
{
	private const string _soloGoldKey = "SoloGold";

	private const string _soloHpKey = "SoloHp";

	private const string _joinForcesGoldKey = "JoinForcesGold";

	private static readonly List<(string, string)> _fx;

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DynamicVar("SoloGold", 150m),
		new DamageVar("SoloHp", 18m, ValueProp.Unblockable | ValueProp.Unpowered),
		new DynamicVar("JoinForcesGold", 50m)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, DontNeedHelp, "JUNGLE_MAZE_ADVENTURE.pages.INITIAL.options.SOLO_QUEST").ThatDoesDamage(base.DynamicVars["SoloHp"].BaseValue),
			new EventOption(this, SafetyInNumbers, "JUNGLE_MAZE_ADVENTURE.pages.INITIAL.options.JOIN_FORCES")
		});
	}

	public override void CalculateVars()
	{
		base.DynamicVars["SoloGold"].BaseValue += (decimal)base.Rng.NextFloat(-15f, 15f);
		base.DynamicVars["JoinForcesGold"].BaseValue += (decimal)base.Rng.NextFloat(-15f, 15f);
	}

	private async Task DontNeedHelp()
	{
		List<(string, string)> shuffledFx = _fx.ToList().StableShuffle(base.Rng);
		for (int i = 0; i < 3; i++)
		{
			Control control = NEventRoom.Instance?.VfxContainer;
			if (LocalContext.IsMe(base.Owner) && control != null)
			{
				VfxCmd.PlayNonCombatVfx(control, new Vector2(control.Size.X * 0.25f, control.Size.Y * 0.5f), shuffledFx[i].Item1);
				NDebugAudioManager.Instance.Play(shuffledFx[i].Item2);
			}
			if (i < 2)
			{
				await Cmd.CustomScaledWait(0.25f, 0.5f);
			}
		}
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["SoloHp"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		await PlayerCmd.GainGold(base.DynamicVars["SoloGold"].BaseValue, base.Owner);
		SetEventFinished(L10NLookup("JUNGLE_MAZE_ADVENTURE.pages.SOLO_QUEST.description"));
	}

	private async Task SafetyInNumbers()
	{
		NDebugAudioManager.Instance.Play("hey.mp3");
		await Cmd.CustomScaledWait(0f, 0.2f);
		await PlayerCmd.GainGold(base.DynamicVars["JoinForcesGold"].BaseValue, base.Owner);
		SetEventFinished(L10NLookup("JUNGLE_MAZE_ADVENTURE.pages.JOIN_FORCES.description"));
	}

	static JungleMazeAdventure()
	{
		int num = 3;
		List<(string, string)> list = new List<(string, string)>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<(string, string)> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = ("vfx/vfx_attack_blunt", "blunt_attack.mp3");
		num2++;
		span[num2] = ("vfx/vfx_attack_slash", "slash_attack.mp3");
		num2++;
		span[num2] = ("vfx/vfx_heavy_blunt", "heavy_attack.mp3");
		_fx = list;
	}
}
