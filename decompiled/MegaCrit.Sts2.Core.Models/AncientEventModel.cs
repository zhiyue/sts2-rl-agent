using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Models;

public abstract class AncientEventModel : EventModel
{
	private AncientDialogueSet? _dialogueSet;

	private List<EventOption>? _generatedOptions;

	private string? _customDonePage;

	private string? _debugOption;

	public override string LocTable => "ancients";

	public LocString Epithet => L10NLookup(base.Id.Entry + ".epithet");

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

	/// <summary>
	/// The characters for whom the "any character" dialogue options shouldn't be shown.
	/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Events.Tezcatara" />'s "any character" dialogue is friendly, but they don't trust the
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Characters.Defect" />, so we blacklist them so we don't show these friendly dialogue options for them.
	/// </summary>
	public virtual IEnumerable<CharacterModel> AnyCharacterDialogueBlacklist => Array.Empty<CharacterModel>();

	public override Color ButtonColor => new Color(0f, 0f, 0f, 0.35f);

	/// <summary>
	/// The color of the speech bubble in the Ancient events during Ancient Dialogue.
	/// </summary>
	public virtual Color DialogueColor { get; } = new Color("28454f");

	private string? CustomDonePage
	{
		get
		{
			return _customDonePage;
		}
		set
		{
			AssertMutable();
			_customDonePage = value;
		}
	}

	public string? DebugOption
	{
		get
		{
			return _debugOption;
		}
		set
		{
			AssertMutable();
			_debugOption = value;
		}
	}

	/// <summary>
	/// The initial options generated when entering an instance of this Ancient event.
	/// Will always be null in canonical events.
	/// </summary>
	private List<EventOption>? GeneratedOptions
	{
		get
		{
			return _generatedOptions;
		}
		set
		{
			AssertMutable();
			_generatedOptions = value;
		}
	}

	public override EventLayoutType LayoutType => EventLayoutType.Ancient;

	private string MapIconPath => ImageHelper.GetImagePath("packed/map/ancients/ancient_node_" + base.Id.Entry.ToLowerInvariant() + ".png");

	private string MapIconOutlinePath => ImageHelper.GetImagePath("packed/map/ancients/ancient_node_" + base.Id.Entry.ToLowerInvariant() + "_outline.png");

	public Texture2D MapIcon => PreloadManager.Cache.GetCompressedTexture2D(MapIconPath);

	public Texture2D MapIconOutline => PreloadManager.Cache.GetCompressedTexture2D(MapIconOutlinePath);

	public IEnumerable<string> MapNodeAssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { MapIconPath, MapIconOutlinePath });

	public virtual string AmbientBgm => "";

	public bool HasAmbientBgm => AmbientBgm != "";

	/// <summary>
	/// The icon used for this Ancient in the Run History screen.
	/// Also used in other spots, like <see cref="T:MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueLine" />.
	/// </summary>
	public Texture2D RunHistoryIcon => PreloadManager.Cache.GetCompressedTexture2D(ImageHelper.GetRoomIconPath(MapPointType.Ancient, RoomType.Event, base.Id));

	public Texture2D RunHistoryIconOutline => PreloadManager.Cache.GetCompressedTexture2D(RunHistoryIconOutlinePath);

	private string RunHistoryIconOutlinePath => ImageHelper.GetImagePath("ui/run_history/" + base.Id.Entry.ToLowerInvariant() + "_outline.png");

	/// <summary>
	/// The amount of health the player healed upon entering this ancient.
	/// </summary>
	public int HealedAmount { get; private set; }

	public abstract IEnumerable<EventOption> AllPossibleOptions { get; }

	protected virtual Color EventButtonColor { get; set; } = new Color("00000059");

	public override LocString InitialDescription
	{
		get
		{
			if (!RunManager.Instance.IsInProgress || Hook.ShouldAllowAncient(base.Owner.RunState, base.Owner, this))
			{
				return base.InitialDescription;
			}
			return new LocString("relics", "WAX_CHOKER.blockMessage");
		}
	}

	/// <summary>
	/// Define the dialogue structure for this ancient. Each subclass must implement this
	/// to specify dialogues for all categories (first meeting, repeating, etc.).
	/// </summary>
	protected abstract AncientDialogueSet DefineDialogues();

	/// <summary>
	/// Convenience method to get a character's dictionary key from its type.
	/// Usage: CharKey&lt;Ironclad&gt;() returns "IRONCLAD".
	/// </summary>
	protected static string CharKey<T>() where T : CharacterModel
	{
		return ModelDb.Character<T>().Id.Entry;
	}

	protected override async Task BeforeEventStarted(bool isPreFinished)
	{
		if (!isPreFinished)
		{
			if (this is Neow)
			{
				base.Owner.Creature.SetCurrentHpInternal(0m);
			}
			int oldHp = base.Owner.Creature.CurrentHp;
			decimal amount = base.Owner.Creature.MaxHp - base.Owner.Creature.CurrentHp;
			if (RunManager.Instance.HasAscension(AscensionLevel.WearyTraveler))
			{
				amount *= 0.8m;
			}
			await CreatureCmd.Heal(base.Owner.Creature, amount, playAnim: false);
			if (NRun.Instance != null && this is Neow)
			{
				TaskHelper.RunSafely(NRun.Instance.GlobalUi.TopBar.Hp.LerpAtNeow());
			}
			HealedAmount = base.Owner.Creature.CurrentHp - oldHp;
		}
	}

	protected sealed override IReadOnlyList<EventOption> GenerateInitialOptionsWrapper()
	{
		if (Hook.ShouldAllowAncient(base.Owner.RunState, base.Owner, this))
		{
			GeneratedOptions = GenerateInitialOptions().ToList();
			if (DebugOption != null)
			{
				GeneratedOptions.RemoveAt(0);
				GeneratedOptions.Insert(0, AllPossibleOptions.First((EventOption c) => c.TextKey.Contains(DebugOption)));
			}
			ReplaceNullOptions(GeneratedOptions);
			return GeneratedOptions;
		}
		return new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, NEventRoom.Proceed, "PROCEED", false, true));
	}

	protected override void SetInitialEventState(bool isPreFinished)
	{
		IReadOnlyList<EventOption> readOnlyList = GenerateInitialOptionsWrapper();
		if (readOnlyList.Count == 0 || isPreFinished)
		{
			StartPreFinished();
		}
		else
		{
			SetEventState(InitialDescription, readOnlyList);
		}
	}

	private void UpdateRunHistory()
	{
		if (!RunManager.Instance.IsInProgress)
		{
			return;
		}
		foreach (EventOption generatedOption in GeneratedOptions)
		{
			AncientChoiceHistoryEntry item = new AncientChoiceHistoryEntry(generatedOption.Title, generatedOption.WasChosen);
			base.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(base.Owner.NetId).AncientChoices.Add(item);
		}
	}

	public void StartPreFinished()
	{
		if (CustomDonePage == null)
		{
			SetEventFinished(L10NLookup(base.Id.Entry + ".pages.DONE.description"));
		}
		else
		{
			SetEventFinished(L10NLookup(CustomDonePage));
		}
	}

	protected void Done()
	{
		UpdateRunHistory();
		if (CustomDonePage == null)
		{
			SetEventFinished(L10NLookup(base.Id.Entry + ".pages.DONE.description"));
		}
		else
		{
			SetEventFinished(L10NLookup(CustomDonePage));
		}
	}

	/// <summary>
	/// Generate an event option from a relic.
	/// This overload also generates an onChosen callback that makes the player obtain the specified relic and then
	/// finishes the event.
	/// </summary>
	/// <param name="pageName">Name of the page that this option is in, for metrics/gameinfo and loc. INITIAL by default.</param>
	/// <param name="customDonePage">custom page path that this choice goes to next</param>
	/// <typeparam name="T">Relic whose title, description, and HoverTips should be used for the event option.</typeparam>
	protected EventOption RelicOption<T>(string pageName = "INITIAL", string? customDonePage = null) where T : RelicModel
	{
		return RelicOption(ModelDb.Relic<T>().ToMutable(), pageName);
	}

	/// <summary>
	/// Generate an event option from a relic.
	/// This overload also generates an onChosen callback that makes the player obtain the specified relic and then
	/// finishes the event.
	/// See <see cref="M:MegaCrit.Sts2.Core.Models.AncientEventModel.RelicOption``1(System.String,System.String)" /> for more details.
	/// </summary>
	protected EventOption RelicOption(RelicModel relic, string pageName = "INITIAL", string? customDonePage = null)
	{
		return RelicOption(relic, OnChosen, pageName);
		async Task OnChosen()
		{
			await RelicCmd.Obtain(relic, base.Owner);
			CustomDonePage = customDonePage;
			Done();
		}
	}
}
