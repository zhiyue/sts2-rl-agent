using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiary.cs")]
public class NBestiary : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName CreateEntries = "CreateEntries";

		public static readonly StringName OnMonsterClicked = "OnMonsterClicked";

		public static readonly StringName SelectMonster = "SelectMonster";

		public static readonly StringName OnMoveButtonFocused = "OnMoveButtonFocused";

		public static readonly StringName OnMoveButtonUnfocused = "OnMoveButtonUnfocused";

		public static readonly StringName OnMoveButtonClicked = "OnMoveButtonClicked";

		public static readonly StringName PlayIdleAnim = "PlayIdleAnim";

		public static readonly StringName PlayMoveAnim = "PlayMoveAnim";

		public static readonly StringName OnMoveAnimCompleted = "OnMoveAnimCompleted";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _monsterNameLabel = "_monsterNameLabel";

		public static readonly StringName _epithet = "_epithet";

		public static readonly StringName _sidebar = "_sidebar";

		public static readonly StringName _bestiaryList = "_bestiaryList";

		public static readonly StringName _monsterVisualsContainer = "_monsterVisualsContainer";

		public static readonly StringName _descriptionLabel = "_descriptionLabel";

		public static readonly StringName _selectionArrow = "_selectionArrow";

		public static readonly StringName _moveList = "_moveList";

		public static readonly StringName _moveContainer = "_moveContainer";

		public static readonly StringName _arrowTween = "_arrowTween";

		public static readonly StringName _arrowPosReset = "_arrowPosReset";

		public static readonly StringName _monsterVisuals = "_monsterVisuals";

		public static readonly StringName _selectedEntry = "_selectedEntry";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _isPlayingMoveAnim = "_isPlayingMoveAnim";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary");

	private MegaRichTextLabel _monsterNameLabel;

	private MegaLabel _epithet;

	private NScrollableContainer _sidebar;

	private VBoxContainer _bestiaryList;

	private static readonly LocString _locked = new LocString("bestiary", "LOCKED.monsterTitle");

	private Control _monsterVisualsContainer;

	private static readonly LocString _placeholderDesc = new LocString("bestiary", "DESCRIPTION.placeholder");

	private MegaRichTextLabel _descriptionLabel;

	private Control _selectionArrow;

	private Control _moveList;

	private Control _moveContainer;

	private Tween? _arrowTween;

	private static readonly Vector2 _arrowOffset = new Vector2(-34f, 4f);

	private bool _arrowPosReset = true;

	private NCreatureVisuals? _monsterVisuals;

	private MegaSprite? _animController;

	private MegaAnimationState? _animState;

	private NBestiaryEntry? _selectedEntry;

	private Tween? _tween;

	private bool _isPlayingMoveAnim;

	protected override Control? InitialFocusedControl => _bestiaryList.GetChildren().OfType<NBestiaryEntry>().FirstOrDefault();

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
		_sidebar = GetNode<NScrollableContainer>("%Sidebar");
		_bestiaryList = GetNode<VBoxContainer>("%BestiaryList");
		_monsterNameLabel = GetNode<MegaRichTextLabel>("%MonsterName");
		_epithet = GetNode<MegaLabel>("%Epithet");
		_descriptionLabel = GetNode<MegaRichTextLabel>("%Description");
		_moveContainer = GetNode<Control>("%MoveContainer");
		_selectionArrow = GetNode<Control>("%SelectionArrow");
		_monsterVisualsContainer = GetNode<Control>("%MonsterVisualsContainer");
		_moveList = GetNode<Control>("%MoveList");
	}

	public override void OnSubmenuOpened()
	{
		CreateEntries();
	}

	public override void OnSubmenuClosed()
	{
		_selectedEntry = null;
		_monsterVisuals?.QueueFreeSafely();
		_monsterVisuals = null;
		foreach (Node child in _bestiaryList.GetChildren())
		{
			child.QueueFreeSafely();
		}
	}

	private void CreateEntries()
	{
		HashSet<ModelId> hashSet = (from e in SaveManager.Instance.Progress.EnemyStats.Values
			where e.TotalWins > 0
			select e.Id).ToHashSet();
		foreach (MonsterModel item in ModelDb.Monsters.OrderBy((MonsterModel m) => m.Id.Entry))
		{
			bool flag = hashSet.Contains(item.Id);
			NBestiaryEntry nBestiaryEntry = NBestiaryEntry.Create(item, !flag);
			_bestiaryList.AddChildSafely(nBestiaryEntry);
			nBestiaryEntry.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryEntry>(OnMonsterClicked));
		}
		_sidebar.InstantlyScrollToTop();
		SelectMonster(_bestiaryList.GetChild<NBestiaryEntry>(0));
	}

	private void OnMonsterClicked(NBestiaryEntry entry)
	{
		SelectMonster(entry);
	}

	private void SelectMonster(NBestiaryEntry entry)
	{
		if (entry == _selectedEntry)
		{
			return;
		}
		_moveList.FreeChildren();
		_arrowPosReset = true;
		_selectedEntry?.Deselect();
		_selectedEntry = entry;
		_selectedEntry.Select();
		MonsterModel monster = _selectedEntry.Monster;
		if (entry.IsLocked)
		{
			_monsterNameLabel.Text = _locked.GetFormattedText();
			_descriptionLabel.Text = _placeholderDesc.GetFormattedText();
			_monsterVisuals?.QueueFreeSafely();
			_monsterVisuals = null;
			_moveContainer.Visible = false;
			return;
		}
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_descriptionLabel.Text = _placeholderDesc.GetFormattedText();
		_descriptionLabel.Modulate = StsColors.transparentWhite;
		_monsterNameLabel.Text = monster.Title.GetFormattedText();
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
		_monsterVisuals?.QueueFreeSafely();
		_monsterVisuals = monster.CreateVisuals();
		_monsterVisualsContainer.AddChildSafely(_monsterVisuals);
		_monsterVisuals.Position = new Vector2(0f, _monsterVisuals.Bounds.Size.Y * 0.5f);
		_monsterVisuals.Modulate = StsColors.transparentBlack;
		_tween.TweenProperty(_monsterVisuals, "modulate", Colors.White, 0.25);
		_isPlayingMoveAnim = false;
		if (_monsterVisuals.HasSpineAnimation)
		{
			_moveContainer.Visible = true;
			_animController = _monsterVisuals.SpineBody;
			_animState = _animController.GetAnimationState();
			monster.GenerateAnimator(_animController);
			_monsterVisuals.SetUpSkin(monster);
			PlayIdleAnim();
			{
				foreach (BestiaryMonsterMove item in monster.MonsterMoveList(_monsterVisuals))
				{
					NBestiaryMoveButton nBestiaryMoveButton = NBestiaryMoveButton.Create(item);
					_moveList.AddChildSafely(nBestiaryMoveButton);
					nBestiaryMoveButton.Connect(NClickableControl.SignalName.Focused, Callable.From<NBestiaryMoveButton>(OnMoveButtonFocused));
					nBestiaryMoveButton.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NBestiaryMoveButton>(OnMoveButtonUnfocused));
					nBestiaryMoveButton.Connect(NClickableControl.SignalName.Released, Callable.From<NBestiaryMoveButton>(OnMoveButtonClicked));
				}
				return;
			}
		}
		_moveContainer.Visible = false;
	}

	private void OnMoveButtonFocused(NBestiaryMoveButton button)
	{
		if (_arrowPosReset)
		{
			_selectionArrow.GlobalPosition = button.GlobalPosition + _arrowOffset;
			_arrowPosReset = false;
		}
		_arrowTween?.Kill();
		_arrowTween = CreateTween().SetParallel();
		_arrowTween.TweenProperty(_selectionArrow, "modulate:a", 1f, 0.05);
		_arrowTween.TweenProperty(_selectionArrow, "global_position", button.GlobalPosition + _arrowOffset, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void OnMoveButtonUnfocused(NBestiaryMoveButton button)
	{
		_arrowTween?.Kill();
		_arrowTween = CreateTween().SetParallel();
		_arrowTween.TweenProperty(_selectionArrow, "modulate:a", 0f, 0.25);
	}

	private void OnMoveButtonClicked(NButton button)
	{
		NBestiaryMoveButton nBestiaryMoveButton = (NBestiaryMoveButton)button;
		PlayMoveAnim(nBestiaryMoveButton.Move.animId);
		nBestiaryMoveButton.PlaySfx();
	}

	private void PlayIdleAnim()
	{
		if (_monsterVisuals != null && _monsterVisuals.HasSpineAnimation)
		{
			_isPlayingMoveAnim = false;
			_animState.SetAnimation("idle_loop");
		}
	}

	private void PlayMoveAnim(string animId)
	{
		if (_monsterVisuals != null && _monsterVisuals.HasSpineAnimation)
		{
			_animState.SetAnimation(animId, loop: false);
			if (!_isPlayingMoveAnim)
			{
				_animController.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>(OnMoveAnimCompleted));
			}
			_isPlayingMoveAnim = true;
		}
	}

	private void OnMoveAnimCompleted(GodotObject _, GodotObject __, GodotObject ___)
	{
		if (_isPlayingMoveAnim)
		{
			_isPlayingMoveAnim = false;
			_animController.DisconnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>(OnMoveAnimCompleted));
			PlayIdleAnim();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
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
		list.Add(new MethodInfo(MethodName.OnMoveButtonFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMoveButtonUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMoveButtonClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayIdleAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayMoveAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "animId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMoveAnimCompleted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		return list;
	}

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
		if (method == MethodName.OnMoveButtonFocused && args.Count == 1)
		{
			OnMoveButtonFocused(VariantUtils.ConvertTo<NBestiaryMoveButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMoveButtonUnfocused && args.Count == 1)
		{
			OnMoveButtonUnfocused(VariantUtils.ConvertTo<NBestiaryMoveButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked && args.Count == 1)
		{
			OnMoveButtonClicked(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayIdleAnim && args.Count == 0)
		{
			PlayIdleAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayMoveAnim && args.Count == 1)
		{
			PlayMoveAnim(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMoveAnimCompleted && args.Count == 3)
		{
			OnMoveAnimCompleted(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
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
		ret = default(godot_variant);
		return false;
	}

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
		if (method == MethodName.OnMoveButtonFocused)
		{
			return true;
		}
		if (method == MethodName.OnMoveButtonUnfocused)
		{
			return true;
		}
		if (method == MethodName.OnMoveButtonClicked)
		{
			return true;
		}
		if (method == MethodName.PlayIdleAnim)
		{
			return true;
		}
		if (method == MethodName.PlayMoveAnim)
		{
			return true;
		}
		if (method == MethodName.OnMoveAnimCompleted)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._monsterVisualsContainer)
		{
			_monsterVisualsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			_descriptionLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			_selectionArrow = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._arrowTween)
		{
			_arrowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._arrowPosReset)
		{
			_arrowPosReset = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._monsterVisuals)
		{
			_monsterVisuals = VariantUtils.ConvertTo<NCreatureVisuals>(in value);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			_selectedEntry = VariantUtils.ConvertTo<NBestiaryEntry>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isPlayingMoveAnim)
		{
			_isPlayingMoveAnim = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
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
		if (name == PropertyName._monsterVisualsContainer)
		{
			value = VariantUtils.CreateFrom(in _monsterVisualsContainer);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			value = VariantUtils.CreateFrom(in _descriptionLabel);
			return true;
		}
		if (name == PropertyName._selectionArrow)
		{
			value = VariantUtils.CreateFrom(in _selectionArrow);
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
		if (name == PropertyName._arrowTween)
		{
			value = VariantUtils.CreateFrom(in _arrowTween);
			return true;
		}
		if (name == PropertyName._arrowPosReset)
		{
			value = VariantUtils.CreateFrom(in _arrowPosReset);
			return true;
		}
		if (name == PropertyName._monsterVisuals)
		{
			value = VariantUtils.CreateFrom(in _monsterVisuals);
			return true;
		}
		if (name == PropertyName._selectedEntry)
		{
			value = VariantUtils.CreateFrom(in _selectedEntry);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._isPlayingMoveAnim)
		{
			value = VariantUtils.CreateFrom(in _isPlayingMoveAnim);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epithet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sidebar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiaryList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterVisualsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._arrowPosReset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._monsterVisuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedEntry, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPlayingMoveAnim, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._monsterNameLabel, Variant.From(in _monsterNameLabel));
		info.AddProperty(PropertyName._epithet, Variant.From(in _epithet));
		info.AddProperty(PropertyName._sidebar, Variant.From(in _sidebar));
		info.AddProperty(PropertyName._bestiaryList, Variant.From(in _bestiaryList));
		info.AddProperty(PropertyName._monsterVisualsContainer, Variant.From(in _monsterVisualsContainer));
		info.AddProperty(PropertyName._descriptionLabel, Variant.From(in _descriptionLabel));
		info.AddProperty(PropertyName._selectionArrow, Variant.From(in _selectionArrow));
		info.AddProperty(PropertyName._moveList, Variant.From(in _moveList));
		info.AddProperty(PropertyName._moveContainer, Variant.From(in _moveContainer));
		info.AddProperty(PropertyName._arrowTween, Variant.From(in _arrowTween));
		info.AddProperty(PropertyName._arrowPosReset, Variant.From(in _arrowPosReset));
		info.AddProperty(PropertyName._monsterVisuals, Variant.From(in _monsterVisuals));
		info.AddProperty(PropertyName._selectedEntry, Variant.From(in _selectedEntry));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._isPlayingMoveAnim, Variant.From(in _isPlayingMoveAnim));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._monsterNameLabel, out var value))
		{
			_monsterNameLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._epithet, out var value2))
		{
			_epithet = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._sidebar, out var value3))
		{
			_sidebar = value3.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._bestiaryList, out var value4))
		{
			_bestiaryList = value4.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._monsterVisualsContainer, out var value5))
		{
			_monsterVisualsContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._descriptionLabel, out var value6))
		{
			_descriptionLabel = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._selectionArrow, out var value7))
		{
			_selectionArrow = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._moveList, out var value8))
		{
			_moveList = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._moveContainer, out var value9))
		{
			_moveContainer = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._arrowTween, out var value10))
		{
			_arrowTween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._arrowPosReset, out var value11))
		{
			_arrowPosReset = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._monsterVisuals, out var value12))
		{
			_monsterVisuals = value12.As<NCreatureVisuals>();
		}
		if (info.TryGetProperty(PropertyName._selectedEntry, out var value13))
		{
			_selectedEntry = value13.As<NBestiaryEntry>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value14))
		{
			_tween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isPlayingMoveAnim, out var value15))
		{
			_isPlayingMoveAnim = value15.As<bool>();
		}
	}
}
