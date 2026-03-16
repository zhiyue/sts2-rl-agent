using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TheArchitect : EventModel
{
	private const string _locTable = "ancients";

	private static readonly LocString _emptyLocString = new LocString("ancients", "PROCEED.description");

	private static readonly LocString _continueLocString = new LocString("ancients", "THE_ARCHITECT.CONTINUE");

	private static readonly LocString _respondLocString = new LocString("ancients", "THE_ARCHITECT.RESPOND");

	private AncientDialogue? _dialogue;

	private int _currentLineIndex;

	private NSpeechBubbleVfx? _speechBubble;

	private Creature? _architectCreature;

	private int _score;

	private AncientDialogueSet? _dialogueSet;

	public override EventLayoutType LayoutType => EventLayoutType.Combat;

	public override EncounterModel CanonicalEncounter => ModelDb.Encounter<TheArchitectEventEncounter>();

	protected override string LocTable => "ancients";

	private AncientDialogue? Dialogue
	{
		get
		{
			return _dialogue;
		}
		set
		{
			AssertMutable();
			_dialogue = value;
		}
	}

	private int CurrentLineIndex
	{
		get
		{
			return _currentLineIndex;
		}
		set
		{
			AssertMutable();
			_currentLineIndex = value;
		}
	}

	private NSpeechBubbleVfx? SpeechBubble
	{
		get
		{
			return _speechBubble;
		}
		set
		{
			AssertMutable();
			_speechBubble = value;
		}
	}

	private Creature? ArchitectCreature
	{
		get
		{
			return _architectCreature;
		}
		set
		{
			AssertMutable();
			_architectCreature = value;
		}
	}

	private int Score
	{
		get
		{
			return _score;
		}
		set
		{
			AssertMutable();
			_score = value;
		}
	}

	public override IEnumerable<LocString> GameInfoOptions => Array.Empty<LocString>();

	private bool IsOnLastLine
	{
		get
		{
			if (Dialogue != null)
			{
				return CurrentLineIndex >= Dialogue.Lines.Count - 1;
			}
			return true;
		}
	}

	public AncientDialogueSet DialogueSet
	{
		get
		{
			if (_dialogueSet == null)
			{
				_dialogueSet = DefineDialogues();
				_dialogueSet.PopulateLocKeys(base.Id.Entry);
			}
			return _dialogueSet;
		}
	}

	protected override void SetInitialEventState(bool isPreFinished)
	{
		SetEventState(_emptyLocString, GenerateInitialOptionsWrapper());
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		LoadDialogue();
		if (Dialogue == null || Dialogue.Lines.Count == 0)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(CreateProceedOption());
		}
		CurrentLineIndex = 0;
		return new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(CreateOptionForCurrentLine());
	}

	public override void OnRoomEnter()
	{
		StatsManager.RefreshGlobalStats();
		ArchitectCreature = NCombatRoom.Instance?.CreatureNodes.FirstOrDefault((NCreature n) => n.Entity.Side == CombatSide.Enemy)?.Entity;
		Score = ScoreUtility.CalculateScore(base.Owner.RunState, won: true);
		if (LocalContext.IsMe(base.Owner))
		{
			if (ArchitectCreature != null)
			{
				GetArchitectAnimationState()?.SetAnimation("_tracks/head_reading", loop: true, 1);
			}
			AncientDialogue dialogue = Dialogue;
			if (dialogue != null)
			{
				IReadOnlyList<AncientDialogueLine> lines = dialogue.Lines;
				if (lines != null && lines.Count > 0)
				{
					ClearCurrentOptions();
				}
			}
		}
		TaskHelper.RunSafely(PlayCurrentLine());
	}

	public void TriggerVictory()
	{
		if (LocalContext.IsMe(base.Owner))
		{
			NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: false);
		}
	}

	private static string CharKey<T>() where T : CharacterModel
	{
		return ModelDb.Character<T>().Id.Entry;
	}

	private static AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = null;
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "")
				{
					VisitIndex = 0,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 1,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 2,
					EndAttackers = ArchitectAttackers.Both
				}
			}),
			[CharKey<Silent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[4]
			{
				new AncientDialogue("")
				{
					VisitIndex = 0,
					StartAttackers = ArchitectAttackers.Player,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("")
				{
					VisitIndex = 1,
					StartAttackers = ArchitectAttackers.Player,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("")
				{
					VisitIndex = 2,
					StartAttackers = ArchitectAttackers.Player,
					EndAttackers = ArchitectAttackers.Architect
				},
				new AncientDialogue("")
				{
					VisitIndex = 3,
					StartAttackers = ArchitectAttackers.Player,
					EndAttackers = ArchitectAttackers.Architect
				}
			}),
			[CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "", "")
				{
					VisitIndex = 0,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 1,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 2,
					EndAttackers = ArchitectAttackers.Both
				}
			}),
			[CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[4]
			{
				new AncientDialogue("", "")
				{
					VisitIndex = 0,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "")
				{
					VisitIndex = 1,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "")
				{
					VisitIndex = 2,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 3,
					EndAttackers = ArchitectAttackers.Both
				}
			}),
			[CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "", "")
				{
					VisitIndex = 0,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 1,
					EndAttackers = ArchitectAttackers.Both
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 2,
					EndAttackers = ArchitectAttackers.Both
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = Array.Empty<AncientDialogue>();
		return ancientDialogueSet;
	}

	private void LoadDialogue()
	{
		int charVisits = SaveManager.Instance.Progress.GetStatsForCharacter(base.Owner.Character.Id)?.TotalWins ?? 0;
		int wins = SaveManager.Instance.Progress.Wins;
		List<AncientDialogue> items = DialogueSet.GetValidDialogues(base.Owner.Character.Id, charVisits, wins, allowAnyCharacterDialogues: false).ToList();
		Dialogue = base.Rng.NextItem(items);
	}

	private EventOption CreateOptionForCurrentLine()
	{
		AncientDialogueLine ancientDialogueLine = Dialogue.Lines[CurrentLineIndex];
		if (IsOnLastLine)
		{
			return CreateProceedOption();
		}
		return new EventOption(title: (ancientDialogueLine.NextButtonText != null) ? ancientDialogueLine.NextButtonText : ((ancientDialogueLine.Speaker != AncientDialogueSpeaker.Ancient) ? _continueLocString : _respondLocString), eventModel: this, onChosen: AdvanceDialogue, description: _emptyLocString, textKey: $"{base.Id.Entry}.dialogue.{CurrentLineIndex}", hoverTips: Array.Empty<IHoverTip>()).ThatWontSaveToChoiceHistory();
	}

	private EventOption CreateProceedOption()
	{
		return new EventOption(this, WinRun, "PROCEED", false, false).ThatWontSaveToChoiceHistory();
	}

	private async Task AdvanceDialogue()
	{
		CurrentLineIndex++;
		IEnumerable<EventOption> eventOptions;
		if (CurrentLineIndex < Dialogue.Lines.Count)
		{
			await PlayCurrentLine();
			eventOptions = new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(CreateOptionForCurrentLine());
		}
		else
		{
			eventOptions = new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(CreateProceedOption());
		}
		SetEventState(_emptyLocString, eventOptions);
	}

	private async Task WinRun()
	{
		if (LocalContext.IsMe(base.Owner))
		{
			await AnimPlayerAttackIfNecessary(Dialogue.EndAttackers);
			await AnimArchitectAttackIfNecessary(Dialogue.EndAttackers);
			if (base.Owner.RunState.Players.Count > 1)
			{
				NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: true);
			}
			RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
		}
	}

	private async Task<bool> AnimPlayerAttackIfNecessary(ArchitectAttackers attackers)
	{
		if ((attackers != ArchitectAttackers.Player && attackers != ArchitectAttackers.Both) || 1 == 0)
		{
			return false;
		}
		if (ArchitectCreature == null)
		{
			return false;
		}
		List<string> shuffledVfx = base.Owner.Character.GetArchitectAttackVfx();
		base.Rng.Shuffle(shuffledVfx);
		int[] damageParts = DivideWildly(Score, shuffledVfx.Count, base.Rng);
		Control vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
		for (int i = 0; i < shuffledVfx.Count; i++)
		{
			bool isFinalHit = i == shuffledVfx.Count - 1;
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", 0.1f);
			vfxContainer?.AddChildSafely(NDamageNumVfx.Create(ArchitectCreature, damageParts[i], requireInteractable: false));
			vfxContainer?.AddChildSafely(NHitSparkVfx.Create(ArchitectCreature, requireInteractable: false));
			VfxCmd.PlayOnCreatureCenter(ArchitectCreature, shuffledVfx[i]);
			await CreatureCmd.TriggerAnim(ArchitectCreature, "Hit", 0f);
			if (isFinalHit)
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
			}
			else
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
			}
			if (!isFinalHit)
			{
				await Cmd.Wait(0.1f);
			}
		}
		await Cmd.Wait(2f);
		return true;
	}

	private static int[] DivideWildly(int total, int parts, Rng rng)
	{
		if (parts <= 0)
		{
			return Array.Empty<int>();
		}
		if (parts == 1)
		{
			return new int[1] { total };
		}
		double[] array = new double[parts];
		int num = rng.NextInt(parts);
		int num2;
		do
		{
			num2 = rng.NextInt(parts);
		}
		while (num2 == num);
		for (int i = 0; i < parts; i++)
		{
			if (i == num)
			{
				array[i] = rng.NextFloat(2f, 3f);
			}
			else if (i == num2)
			{
				array[i] = rng.NextFloat(0.1f, 0.5f);
			}
			else
			{
				array[i] = rng.NextFloat(0.7f, 1.3f);
			}
		}
		double num3 = array.Sum();
		int[] array2 = new int[parts];
		int num4 = 0;
		for (int j = 0; j < parts - 1; j++)
		{
			array2[j] = Math.Max(1, (int)((double)total * array[j] / num3));
			num4 += array2[j];
		}
		array2[parts - 1] = Math.Max(1, total - num4);
		return array2;
	}

	private async Task AnimArchitectAttackIfNecessary(ArchitectAttackers attackers)
	{
		bool flag = (uint)(attackers - 2) <= 1u;
		if (flag && ArchitectCreature != null)
		{
			await CreatureCmd.TriggerAnim(ArchitectCreature, "Attack", 0.5f);
			VfxCmd.PlayOnCreature(base.Owner.Creature, "vfx/vfx_attack_lightning");
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireBurstVfx.Create(base.Owner.Creature, 1f));
			await Cmd.Wait(0.5f);
		}
	}

	private async Task PlayCurrentLine()
	{
		if (!LocalContext.IsMe(base.Owner))
		{
			return;
		}
		if (SpeechBubble != null)
		{
			TaskHelper.RunSafely(SpeechBubble.AnimOut());
			SpeechBubble = null;
		}
		if (Dialogue == null || CurrentLineIndex >= Dialogue.Lines.Count)
		{
			return;
		}
		AncientDialogueLine line = Dialogue.Lines[CurrentLineIndex];
		if (line.LineText == null)
		{
			return;
		}
		Creature speaker = GetSpeaker(line.Speaker);
		if (speaker == null)
		{
			return;
		}
		if (CurrentLineIndex == 0)
		{
			if (Dialogue.StartAttackers != ArchitectAttackers.None || Dialogue.EndAttackers != ArchitectAttackers.None)
			{
				await Cmd.Wait(0.75f);
			}
			bool flag = await AnimPlayerAttackIfNecessary(Dialogue.StartAttackers);
			if (ArchitectCreature != null)
			{
				if (!flag)
				{
					await Cmd.Wait(1f);
				}
				MegaAnimationState state = GetArchitectAnimationState();
				state?.SetAnimation("_tracks/head_stop_reading", loop: false, 1);
				await Cmd.Wait(0.5f);
				state?.SetAnimation("_tracks/head_normal", loop: true, 1);
			}
			await AnimArchitectAttackIfNecessary(Dialogue.StartAttackers);
		}
		ShowSpeechBubble(line, speaker);
		if (CurrentLineIndex == 0)
		{
			SetEventState(_emptyLocString, new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(CreateOptionForCurrentLine()));
		}
	}

	private void ShowSpeechBubble(AncientDialogueLine line, Creature speaker)
	{
		SpeechBubble = TalkCmd.Play(line.LineText, speaker, double.MaxValue);
	}

	private Creature? GetSpeaker(AncientDialogueSpeaker speaker)
	{
		return speaker switch
		{
			AncientDialogueSpeaker.Ancient => ArchitectCreature, 
			AncientDialogueSpeaker.Character => base.Owner.Creature, 
			_ => null, 
		};
	}

	private MegaAnimationState? GetArchitectAnimationState()
	{
		return NCombatRoom.Instance?.GetCreatureNode(ArchitectCreature)?.SpineController?.GetAnimationState();
	}
}
