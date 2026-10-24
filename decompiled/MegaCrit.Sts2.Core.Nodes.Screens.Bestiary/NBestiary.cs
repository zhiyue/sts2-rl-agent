using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

/// <summary>
/// Screen that has a list of monsters that you can click on to view their name, description, hp, some stats, and
/// a list of their moves which you can click on to play the associated animation and sfx.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiary.cs")]
public class NBestiary : NSubmenu
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSubmenu.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		/// <summary>
		/// Cached name for the 'CreateEntries' method.
		/// </summary>
		public static readonly StringName CreateEntries = "CreateEntries";

		/// <summary>
		/// Cached name for the 'OnMonsterClicked' method.
		/// </summary>
		public static readonly StringName OnMonsterClicked = "OnMonsterClicked";

		/// <summary>
		/// Cached name for the 'SelectMonster' method.
		/// </summary>
		public static readonly StringName SelectMonster = "SelectMonster";

		/// <summary>
		/// Cached name for the 'OnMoveButtonClicked' method.
		/// </summary>
		public static readonly StringName OnMoveButtonClicked = "OnMoveButtonClicked";

		/// <summary>
		/// Cached name for the 'GetSideCenter' method.
		/// </summary>
		public static readonly StringName GetSideCenter = "GetSideCenter";

		/// <summary>
		/// Cached name for the 'GetSideFloor' method.
		/// </summary>
		public static readonly StringName GetSideFloor = "GetSideFloor";

		/// <summary>
		/// Cached name for the 'CanBeShown' method.
		/// </summary>
		public static readonly StringName CanBeShown = "CanBeShown";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSubmenu.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the 'BackVfxContainer' property.
		/// </summary>
		public static readonly StringName BackVfxContainer = "BackVfxContainer";

		/// <summary>
		/// Cached name for the 'VfxContainer' property.
		/// </summary>
		public static readonly StringName VfxContainer = "VfxContainer";

		/// <summary>
		/// Cached name for the 'Layout' property.
		/// </summary>
		public static readonly StringName Layout = "Layout";

		/// <summary>
		/// Cached name for the '_monsterNameLabel' field.
		/// </summary>
		public static readonly StringName _monsterNameLabel = "_monsterNameLabel";

		/// <summary>
		/// Cached name for the '_epithet' field.
		/// </summary>
		public static readonly StringName _epithet = "_epithet";

		/// <summary>
		/// Cached name for the '_sidebar' field.
		/// </summary>
		public static readonly StringName _sidebar = "_sidebar";

		/// <summary>
		/// Cached name for the '_bestiaryList' field.
		/// </summary>
		public static readonly StringName _bestiaryList = "_bestiaryList";

		/// <summary>
		/// Cached name for the '_selectionArrow' field.
		/// </summary>
		public static readonly StringName _selectionArrow = "_selectionArrow";

		/// <summary>
		/// Cached name for the '_arrowTween' field.
		/// </summary>
		public static readonly StringName _arrowTween = "_arrowTween";

		/// <summary>
		/// Cached name for the '_initSelectionArrow' field.
		/// </summary>
		public static readonly StringName _initSelectionArrow = "_initSelectionArrow";

		/// <summary>
		/// Cached name for the '_layoutContainer' field.
		/// </summary>
		public static readonly StringName _layoutContainer = "_layoutContainer";

		/// <summary>
		/// Cached name for the '_currentLayout' field.
		/// </summary>
		public static readonly StringName _currentLayout = "_currentLayout";

		/// <summary>
		/// Cached name for the '_descriptionLabel' field.
		/// </summary>
		public static readonly StringName _descriptionLabel = "_descriptionLabel";

		/// <summary>
		/// Cached name for the '_moveList' field.
		/// </summary>
		public static readonly StringName _moveList = "_moveList";

		/// <summary>
		/// Cached name for the '_moveContainer' field.
		/// </summary>
		public static readonly StringName _moveContainer = "_moveContainer";

		/// <summary>
		/// Cached name for the '_selectedEntry' field.
		/// </summary>
		public static readonly StringName _selectedEntry = "_selectedEntry";

		/// <summary>
		/// Cached name for the '_previousScreenshakeTarget' field.
		/// </summary>
		public static readonly StringName _previousScreenshakeTarget = "_previousScreenshakeTarget";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary");

	private MegaRichTextLabel _monsterNameLabel;

	private MegaLabel _epithet;

	private NScrollableContainer _sidebar;

	private VBoxContainer _bestiaryList;

	private static readonly LocString _locked = new LocString("bestiary", "LOCKED.monsterTitle");

	private Control _selectionArrow;

	private Tween? _arrowTween;

	private static readonly Vector2 _arrowOffset = new Vector2(-38f, 117f);

	private bool _initSelectionArrow = true;

	private Control _layoutContainer;

	private NBestiaryLayout? _currentLayout;

	private static readonly LocString _placeholderDesc = new LocString("bestiary", "DESCRIPTION.placeholder");

	private MegaRichTextLabel _descriptionLabel;

	private Control _moveList;

	private Control _moveContainer;

	private HashSet<ModelId> _discoveredMonsterIds;

	private HashSet<ModelId> _discoveredEncounterIds;

	private NBestiaryEntry? _selectedEntry;

	private Control? _previousScreenshakeTarget;

	private Tween? _tween;

	public static NBestiary? Instance { get; private set; }

	public static string[] AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_scenePath);
			list.AddRange(NBestiaryEntry.AssetPaths);
			return list.ToArray();
		}
	}

	protected override Control? InitialFocusedControl => _bestiaryList.GetChildren().OfType<NBestiaryEntry>().FirstOrDefault();

	public Control BackVfxContainer { get; private set; }

	public Control VfxContainer { get; private set; }

	public Control? Layout => _currentLayout;

	public static NBestiary? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBestiary>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		GetNode<MegaLabel>("%MoveHeader").SetTextAutoSize(new LocString("bestiary", "ACTIONS.header").GetFormattedText());
		GetNode<MegaRichTextLabel>("%ConstructionLabel").SetTextAutoSize(new LocString("bestiary", "UNDER_CONSTRUCTION").GetRawText());
		_sidebar = GetNode<NScrollableContainer>("%Sidebar");
		_bestiaryList = GetNode<VBoxContainer>("%BestiaryList");
		_monsterNameLabel = GetNode<MegaRichTextLabel>("%MonsterName");
		_layoutContainer = GetNode<Control>("%LayoutContainer");
		_epithet = GetNode<MegaLabel>("%Epithet");
		_descriptionLabel = GetNode<MegaRichTextLabel>("%Description");
		_moveContainer = GetNode<Control>("%MoveContainer");
		_selectionArrow = GetNode<Control>("%SelectionArrow");
		_moveList = GetNode<Control>("%MoveList");
		VfxContainer = GetNode<Control>("%VfxContainer");
		BackVfxContainer = GetNode<Control>("%BackVfxContainer");
	}

	/// <summary>
	/// On screen open. When the player opens the Bestiary.
	/// </summary>
	public override void OnSubmenuOpened()
	{
		Instance = this;
		_previousScreenshakeTarget = NGame.Instance?.ScreenshakeTarget;
		CreateEntries();
	}

	/// <summary>
	/// Called when the Bestiary is closed (Back button)
	/// </summary>
	public override void OnSubmenuClosed()
	{
		_initSelectionArrow = true;
		_selectedEntry = null;
		Instance = null;
		_currentLayout?.Cleanup();
		if (_previousScreenshakeTarget != null)
		{
			NGame.Instance?.SetScreenShakeTarget(_previousScreenshakeTarget);
		}
		else
		{
			NGame.Instance?.ClearScreenShakeTarget();
		}
		_bestiaryList.FreeChildren();
	}

	/// <summary>
	/// Initializes the list of monsters based on your save file.
	/// </summary>
	private void CreateEntries()
	{
		_discoveredMonsterIds = (from e in SaveManager.Instance.Progress.EnemyStats.Values
			where e.TotalWins > 0
			select e.Id).ToHashSet();
		_discoveredEncounterIds = (from e in SaveManager.Instance.Progress.EncounterStats.Values
			where e.TotalWins > 0
			select e.Id).ToHashSet();
		foreach (ActModel act in ModelDb.Acts)
		{
			AddAct(act);
		}
		Control node = _sidebar.GetNode<Control>("Content");
		Vector2 position = node.Position;
		position.Y = 0f;
		node.Position = position;
		_sidebar.InstantlyScrollToTop();
		NBestiaryEntry nBestiaryEntry = _bestiaryList.GetChildren().OfType<NBestiaryEntry>().FirstOrDefault((NBestiaryEntry e) => e.IsDiscovered && e.IsEnabled);
		if (nBestiaryEntry == null)
		{
			Log.Error("Should not be possible as the Compendium + Bestiary isn't unlocked by default!");
		}
		else
		{
			SelectMonster(nBestiaryEntry);
		}
	}

	private void AddAct(ActModel act)
	{
		if (!SaveManager.Instance.Progress.DiscoveredActs.Contains(act.Id))
		{
			return;
		}
		_bestiaryList.AddChildSafely(NBestiaryActDivider.Create(act));
		HashSet<ModelId> hashSet = new HashSet<ModelId>();
		List<BestiaryEntry> list = new List<BestiaryEntry>();
		foreach (EncounterModel allEncounter in act.AllEncounters)
		{
			foreach (MonsterModel allPossibleMonster in allEncounter.AllPossibleMonsters)
			{
				if (hashSet.Add(allPossibleMonster.Id) && allPossibleMonster.ShouldShowInCompendium)
				{
					list.Add(BestiaryEntry.FromMonster(allPossibleMonster, allEncounter, allEncounter.RoomType));
				}
			}
		}
		if (act is Hive)
		{
			list.Add(BestiaryEntry.FromEncounter(ModelDb.Encounter<DecimillipedeElite>(), RoomType.Elite));
		}
		list.Sort(delegate(BestiaryEntry e1, BestiaryEntry e2)
		{
			if (e1.roomType != e2.roomType)
			{
				return e1.roomType.CompareTo(e2.roomType);
			}
			if (e1.roomType == RoomType.Boss)
			{
				int num = string.Compare(e1.GetEncounterTitle(), e2.GetEncounterTitle(), StringComparison.CurrentCulture);
				if (num != 0)
				{
					return num;
				}
				return string.Compare(e1.GetEntryTitle(), e2.GetEntryTitle(), StringComparison.CurrentCulture);
			}
			return string.Compare(e1.GetEntryTitle(), e2.GetEntryTitle(), StringComparison.CurrentCulture);
		});
		foreach (BestiaryEntry item in list)
		{
			NBestiaryEntry nBestiaryEntry = NBestiaryEntry.Create(item, item.IsDiscovered(_discoveredMonsterIds, _discoveredEncounterIds));
			_bestiaryList.AddChildSafely(nBestiaryEntry);
			nBestiaryEntry.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryEntry>(OnMonsterClicked));
		}
	}

	/// <summary>
	/// A player clicked on a monster in the list on the right.
	/// </summary>
	private void OnMonsterClicked(NBestiaryEntry entry)
	{
		SelectMonster(entry);
	}

	/// <summary>
	/// Loads a specific monster's bestiary entry.
	/// </summary>
	private void SelectMonster(NBestiaryEntry entry)
	{
		if (entry == _selectedEntry)
		{
			return;
		}
		_moveList.FreeChildren();
		_selectedEntry = entry;
		if (entry.IsUnderConstruction)
		{
			_monsterNameLabel.Text = entry.Entry.GetEntryTitle();
			_descriptionLabel.Text = _placeholderDesc.GetFormattedText();
			_currentLayout?.Cleanup();
			_currentLayout?.QueueFreeSafely();
			_moveContainer.Visible = false;
		}
		else if (!entry.IsDiscovered)
		{
			_monsterNameLabel.Text = _locked.GetFormattedText();
			_descriptionLabel.Text = _placeholderDesc.GetFormattedText();
			_currentLayout?.Cleanup();
			_currentLayout?.QueueFreeSafely();
			_moveContainer.Visible = false;
		}
		else
		{
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_descriptionLabel.Text = _placeholderDesc.GetFormattedText();
			_descriptionLabel.Modulate = StsColors.transparentWhite;
			_monsterNameLabel.Text = entry.Entry.GetEntryTitle();
			_monsterNameLabel.SelfModulate = StsColors.transparentWhite;
			_epithet.Modulate = StsColors.transparentWhite;
			_moveContainer.Modulate = StsColors.transparentWhite;
			_tween.TweenProperty(_monsterNameLabel, "position:y", 88f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(24f);
			_tween.TweenProperty(_monsterNameLabel, "self_modulate:a", 1f, 0.5);
			_tween.TweenProperty(_epithet, "modulate:a", 1f, 0.5).SetDelay(0.2);
			_tween.TweenProperty(_descriptionLabel, "position:y", 894f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(958f);
			_tween.TweenProperty(_descriptionLabel, "modulate:a", 1f, 0.5);
			_tween.TweenProperty(_moveContainer, "position:x", 242f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(210f)
				.SetDelay(0.2);
			_tween.TweenProperty(_moveContainer, "modulate:a", 1f, 0.5).SetDelay(0.2);
			_currentLayout?.Cleanup();
			if (!entry.Entry.CanReuseLayout(_currentLayout))
			{
				_currentLayout?.QueueFreeSafely();
				_currentLayout = entry.Entry.CreateLayoutNode(this);
				_layoutContainer.AddChildSafely(_currentLayout);
				NGame.Instance?.SetScreenShakeTarget(_currentLayout);
			}
			List<BestiaryMonsterMove> list = _currentLayout.Setup(entry.Entry, _tween);
			_moveContainer.Visible = true;
			for (int i = 0; i < list.Count; i++)
			{
				if (i >= 9)
				{
					Log.Error("Hotkeys for monster Actions beyond 9 are not supported!");
				}
				NBestiaryMoveButton nBestiaryMoveButton = NBestiaryMoveButton.Create(list[i], $"mega_select_card_{i + 1}");
				_moveList.AddChildSafely(nBestiaryMoveButton);
				nBestiaryMoveButton.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryMoveButton>(OnMoveButtonClicked));
			}
		}
		if (_initSelectionArrow)
		{
			Control selectionArrow = _selectionArrow;
			Color modulate = _selectionArrow.Modulate;
			modulate.A = 0f;
			selectionArrow.Modulate = modulate;
			_initSelectionArrow = false;
			TaskHelper.RunSafely(InitializeSelectorArrow(entry));
		}
		else
		{
			Control selectionArrow2 = _selectionArrow;
			Color modulate = _selectionArrow.Modulate;
			modulate.A = 1f;
			selectionArrow2.Modulate = modulate;
			_arrowTween?.Kill();
			_arrowTween = CreateTween().SetParallel();
			_arrowTween.TweenProperty(_selectionArrow, "position", entry.Position + _arrowOffset, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	private async Task InitializeSelectorArrow(NBestiaryEntry entry)
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_selectionArrow.Position = entry.Position + _arrowOffset;
		_arrowTween?.Kill();
		_arrowTween = CreateTween().SetParallel();
		_arrowTween.TweenProperty(_selectionArrow, "modulate:a", 1f, 0.2);
	}

	private void OnMoveButtonClicked(NButton button)
	{
		NBestiaryMoveButton nBestiaryMoveButton = (NBestiaryMoveButton)button;
		PlayMoveAnim(_currentLayout?.GetCreatures() ?? Array.Empty<NCreature>(), nBestiaryMoveButton.Move);
	}

	private void PlayMoveAnim(IEnumerable<NCreature> creatures, BestiaryMonsterMove move)
	{
		foreach (NCreature creature in creatures)
		{
			if (move.stateId != null)
			{
				MonsterModel monsterModel = creature?.Entity.Monster;
				if (monsterModel == null)
				{
					throw new InvalidOperationException($"Non-monster creature {creature} is in the bestiary!");
				}
				monsterModel.SetMoveImmediate((MoveState)monsterModel.MoveStateMachine.States[move.stateId], forceTransition: true);
				TaskHelper.RunSafely(monsterModel.PerformMove());
			}
			else if (move.nonStateMove != null)
			{
				TaskHelper.RunSafely(move.nonStateMove(Array.Empty<Creature>()));
			}
			else if (move.action != null)
			{
				TaskHelper.RunSafely(move.action());
			}
			else if (move.animId != null)
			{
				creature?.Visuals.SpineBody?.GetAnimationState().SetAnimation(move.animId, loop: false);
				if (move.animId != "die")
				{
					creature?.Visuals.SpineBody?.GetAnimationState().AddAnimation("idle_loop");
				}
				if (move.sfx != null)
				{
					NAudioManager.Instance.PlayOneShot(move.sfx);
				}
			}
			if (move.stopSfxLoops)
			{
				creature?.StopAllSfxLoops();
			}
		}
	}

	public NCreature? GetCreatureNode(Creature? creature)
	{
		foreach (NCreature item in _currentLayout?.GetCreatures() ?? Array.Empty<NCreature>())
		{
			if (item.Entity == creature)
			{
				return item;
			}
		}
		return null;
	}

	public Vector2 GetSideCenter()
	{
		if (_currentLayout == null)
		{
			Log.Error("Tried to get current side center, but we're not showing anything!");
			return Vector2.Zero;
		}
		Vector2 zero = Vector2.Zero;
		int num = 0;
		foreach (NCreature creature in _currentLayout.GetCreatures())
		{
			zero += creature.VfxSpawnPosition;
			num++;
		}
		return zero / num;
	}

	public Vector2 GetSideFloor()
	{
		if (_currentLayout == null)
		{
			Log.Error("Tried to get current side floor, but we're not showing anything!");
			return Vector2.Zero;
		}
		Vector2 zero = Vector2.Zero;
		int num = 0;
		foreach (NCreature creature in _currentLayout.GetCreatures())
		{
			zero += creature.GetBottomOfHitbox();
			num++;
		}
		return zero / num;
	}

	public static bool CanBeShown()
	{
		if (SaveManager.Instance.Progress.DiscoveredActs.Count == 0)
		{
			return false;
		}
		return SaveManager.Instance.Progress.EnemyStats.Values.Any((EnemyStats e) => e.TotalWins > 0);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMonsterClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SelectMonster, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMoveButtonClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetSideCenter, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetSideFloor, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CanBeShown, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NBestiary>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateEntries && args.Count == 0)
		{
			CreateEntries();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMonsterClicked && args.Count == 1)
		{
			OnMonsterClicked(VariantUtils.ConvertTo<NBestiaryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SelectMonster && args.Count == 1)
		{
			SelectMonster(VariantUtils.ConvertTo<NBestiaryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked && args.Count == 1)
		{
			OnMoveButtonClicked(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSideCenter && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetSideCenter());
			return true;
		}
		if (method == MethodName.GetSideFloor && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetSideFloor());
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanBeShown());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NBestiary>(Create());
			return true;
		}
		if (method == MethodName.CanBeShown && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanBeShown());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.CreateEntries)
		{
			return true;
		}
		if (method == MethodName.OnMonsterClicked)
		{
			return true;
		}
		if (method == MethodName.SelectMonster)
		{
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked)
		{
			return true;
		}
		if (method == MethodName.GetSideCenter)
		{
			return true;
		}
		if (method == MethodName.GetSideFloor)
		{
			return true;
		}
		if (method == MethodName.CanBeShown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.BackVfxContainer)
		{
			BackVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.VfxContainer)
		{
			VfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._monsterNameLabel)
		{
			_monsterNameLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._epithet)
		{
			_epithet = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._sidebar)
		{
			_sidebar = VariantUtils.ConvertTo<NScrollableContainer>(in value);
			return true;
		}
		if (name == PropertyName._bestiaryList)
		{
			_bestiaryList = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			_selectionArrow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._arrowTween)
		{
			_arrowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._initSelectionArrow)
		{
			_initSelectionArrow = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._layoutContainer)
		{
			_layoutContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._currentLayout)
		{
			_currentLayout = VariantUtils.ConvertTo<NBestiaryLayout>(in value);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			_descriptionLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._moveList)
		{
			_moveList = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._moveContainer)
		{
			_moveContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			_selectedEntry = VariantUtils.ConvertTo<NBestiaryEntry>(in value);
			return true;
		}
		if (name == PropertyName._previousScreenshakeTarget)
		{
			_previousScreenshakeTarget = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.InitialFocusedControl)
		{
			from = InitialFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.BackVfxContainer)
		{
			from = BackVfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.VfxContainer)
		{
			from = VfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Layout)
		{
			from = Layout;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._monsterNameLabel)
		{
			value = VariantUtils.CreateFrom(in _monsterNameLabel);
			return true;
		}
		if (name == PropertyName._epithet)
		{
			value = VariantUtils.CreateFrom(in _epithet);
			return true;
		}
		if (name == PropertyName._sidebar)
		{
			value = VariantUtils.CreateFrom(in _sidebar);
			return true;
		}
		if (name == PropertyName._bestiaryList)
		{
			value = VariantUtils.CreateFrom(in _bestiaryList);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			value = VariantUtils.CreateFrom(in _selectionArrow);
			return true;
		}
		if (name == PropertyName._arrowTween)
		{
			value = VariantUtils.CreateFrom(in _arrowTween);
			return true;
		}
		if (name == PropertyName._initSelectionArrow)
		{
			value = VariantUtils.CreateFrom(in _initSelectionArrow);
			return true;
		}
		if (name == PropertyName._layoutContainer)
		{
			value = VariantUtils.CreateFrom(in _layoutContainer);
			return true;
		}
		if (name == PropertyName._currentLayout)
		{
			value = VariantUtils.CreateFrom(in _currentLayout);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			value = VariantUtils.CreateFrom(in _descriptionLabel);
			return true;
		}
		if (name == PropertyName._moveList)
		{
			value = VariantUtils.CreateFrom(in _moveList);
			return true;
		}
		if (name == PropertyName._moveContainer)
		{
			value = VariantUtils.CreateFrom(in _moveContainer);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			value = VariantUtils.CreateFrom(in _selectedEntry);
			return true;
		}
		if (name == PropertyName._previousScreenshakeTarget)
		{
			value = VariantUtils.CreateFrom(in _previousScreenshakeTarget);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epithet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sidebar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiaryList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._initSelectionArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layoutContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentLayout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previousScreenshakeTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BackVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Layout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.BackVfxContainer, Variant.From<Control>(BackVfxContainer));
		info.AddProperty(PropertyName.VfxContainer, Variant.From<Control>(VfxContainer));
		info.AddProperty(PropertyName._monsterNameLabel, Variant.From(in _monsterNameLabel));
		info.AddProperty(PropertyName._epithet, Variant.From(in _epithet));
		info.AddProperty(PropertyName._sidebar, Variant.From(in _sidebar));
		info.AddProperty(PropertyName._bestiaryList, Variant.From(in _bestiaryList));
		info.AddProperty(PropertyName._selectionArrow, Variant.From(in _selectionArrow));
		info.AddProperty(PropertyName._arrowTween, Variant.From(in _arrowTween));
		info.AddProperty(PropertyName._initSelectionArrow, Variant.From(in _initSelectionArrow));
		info.AddProperty(PropertyName._layoutContainer, Variant.From(in _layoutContainer));
		info.AddProperty(PropertyName._currentLayout, Variant.From(in _currentLayout));
		info.AddProperty(PropertyName._descriptionLabel, Variant.From(in _descriptionLabel));
		info.AddProperty(PropertyName._moveList, Variant.From(in _moveList));
		info.AddProperty(PropertyName._moveContainer, Variant.From(in _moveContainer));
		info.AddProperty(PropertyName._selectedEntry, Variant.From(in _selectedEntry));
		info.AddProperty(PropertyName._previousScreenshakeTarget, Variant.From(in _previousScreenshakeTarget));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.BackVfxContainer, out var value))
		{
			BackVfxContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.VfxContainer, out var value2))
		{
			VfxContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._monsterNameLabel, out var value3))
		{
			_monsterNameLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._epithet, out var value4))
		{
			_epithet = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._sidebar, out var value5))
		{
			_sidebar = value5.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._bestiaryList, out var value6))
		{
			_bestiaryList = value6.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._selectionArrow, out var value7))
		{
			_selectionArrow = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._arrowTween, out var value8))
		{
			_arrowTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._initSelectionArrow, out var value9))
		{
			_initSelectionArrow = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._layoutContainer, out var value10))
		{
			_layoutContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentLayout, out var value11))
		{
			_currentLayout = value11.As<NBestiaryLayout>();
		}
		if (info.TryGetProperty(PropertyName._descriptionLabel, out var value12))
		{
			_descriptionLabel = value12.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._moveList, out var value13))
		{
			_moveList = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._moveContainer, out var value14))
		{
			_moveContainer = value14.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._selectedEntry, out var value15))
		{
			_selectedEntry = value15.As<NBestiaryEntry>();
		}
		if (info.TryGetProperty(PropertyName._previousScreenshakeTarget, out var value16))
		{
			_previousScreenshakeTarget = value16.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value17))
		{
			_tween = value17.As<Tween>();
		}
	}
}
