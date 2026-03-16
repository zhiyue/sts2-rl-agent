using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NTopBar.cs")]
public class NTopBar : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ToggleAnimState = "ToggleAnimState";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName DebugHideTopBar = "DebugHideTopBar";

		public static readonly StringName AnimHide = "AnimHide";

		public static readonly StringName AnimShow = "AnimShow";

		public static readonly StringName MaxPotionsChanged = "MaxPotionsChanged";

		public static readonly StringName UpdateNavigation = "UpdateNavigation";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Map = "Map";

		public static readonly StringName Deck = "Deck";

		public static readonly StringName Pause = "Pause";

		public static readonly StringName PotionContainer = "PotionContainer";

		public static readonly StringName RoomIcon = "RoomIcon";

		public static readonly StringName FloorIcon = "FloorIcon";

		public static readonly StringName BossIcon = "BossIcon";

		public static readonly StringName Gold = "Gold";

		public static readonly StringName Hp = "Hp";

		public static readonly StringName Portrait = "Portrait";

		public static readonly StringName PortraitTip = "PortraitTip";

		public static readonly StringName Timer = "Timer";

		public static readonly StringName TrailContainer = "TrailContainer";

		public static readonly StringName _capstoneContainer = "_capstoneContainer";

		public static readonly StringName _modifiersContainer = "_modifiersContainer";

		public static readonly StringName _ascensionIcon = "_ascensionIcon";

		public static readonly StringName _ascensionLabel = "_ascensionLabel";

		public static readonly StringName _ascensionHsv = "_ascensionHsv";

		public static readonly StringName _hideTween = "_hideTween";

		public static readonly StringName _isDebugHidden = "_isDebugHidden";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NCapstoneContainer _capstoneContainer;

	private static readonly StringName _fontOutlineTheme = "font_outline_color";

	private static readonly StringName _h = new StringName("h");

	private static readonly StringName _v = new StringName("v");

	private static readonly Color _redLabelOutline = new Color("593400");

	private static readonly Color _blueLabelOutline = new Color("004759");

	private Control _modifiersContainer;

	private Control _ascensionIcon;

	private MegaLabel _ascensionLabel;

	private ShaderMaterial _ascensionHsv;

	private Tween? _hideTween;

	private bool _isDebugHidden;

	private Player? _player;

	public NTopBarMapButton Map { get; private set; }

	public NTopBarDeckButton Deck { get; private set; }

	public NTopBarPauseButton Pause { get; private set; }

	public NPotionContainer PotionContainer { get; private set; }

	public NTopBarRoomIcon RoomIcon { get; private set; }

	public NTopBarFloorIcon FloorIcon { get; private set; }

	public NTopBarBossIcon BossIcon { get; private set; }

	public NTopBarGold Gold { get; private set; }

	public NTopBarHp Hp { get; private set; }

	public NTopBarPortrait Portrait { get; private set; }

	public NTopBarPortraitTip PortraitTip { get; private set; }

	public NRunTimer Timer { get; private set; }

	public Node TrailContainer { get; private set; }

	public override void _Ready()
	{
		TrailContainer = GetNode<Node>("%TrailContainer");
		Map = GetNode<NTopBarMapButton>("%Map");
		Deck = GetNode<NTopBarDeckButton>("%Deck");
		Pause = GetNode<NTopBarPauseButton>("%PauseButton");
		PotionContainer = GetNode<NPotionContainer>("%PotionContainer");
		RoomIcon = GetNode<NTopBarRoomIcon>("%RoomIcon");
		FloorIcon = GetNode<NTopBarFloorIcon>("%FloorIcon");
		BossIcon = GetNode<NTopBarBossIcon>("%BossIcon");
		Gold = GetNode<NTopBarGold>("%TopBarGold");
		Hp = GetNode<NTopBarHp>("%TopBarHp");
		Portrait = GetNode<NTopBarPortrait>("%TopBarPortrait");
		PortraitTip = GetNode<NTopBarPortraitTip>("%TopBarPortraitTip");
		Timer = GetNode<NRunTimer>("%TimerContainer");
		_ascensionIcon = GetNode<Control>("%AscensionIcon");
		_ascensionLabel = GetNode<MegaLabel>("%AscensionLabel");
		_ascensionHsv = (ShaderMaterial)_ascensionIcon.Material;
		_modifiersContainer = GetNode<Control>("%Modifiers");
		_capstoneContainer = GetParent().GetNode<NCapstoneContainer>("%CapstoneScreenContainer");
		_capstoneContainer.Connect(Node.SignalName.ChildEnteredTree, Callable.From<Node>(ToggleAnimState));
		_capstoneContainer.Connect(Node.SignalName.ChildExitingTree, Callable.From<Node>(ToggleAnimState));
	}

	public void Initialize(IRunState runState)
	{
		if (runState.AscensionLevel > 0)
		{
			if (runState.Players.Count > 1)
			{
				_ascensionHsv.SetShaderParameter(_h, 0.52f);
				_ascensionHsv.SetShaderParameter(_v, 1.2f);
				_ascensionLabel.AddThemeColorOverride(_fontOutlineTheme, _blueLabelOutline);
			}
			else
			{
				_ascensionHsv.SetShaderParameter(_h, 1f);
				_ascensionHsv.SetShaderParameter(_v, 1f);
				_ascensionLabel.AddThemeColorOverride(_fontOutlineTheme, _redLabelOutline);
			}
			_ascensionIcon.Visible = true;
			_ascensionLabel.SetTextAutoSize(runState.AscensionLevel.ToString());
		}
		_modifiersContainer.Visible = runState.Modifiers.Count > 0;
		foreach (ModifierModel modifier in runState.Modifiers)
		{
			NTopBarModifier child = NTopBarModifier.Create(modifier);
			_modifiersContainer.AddChildSafely(child);
		}
		_player = LocalContext.GetMe(runState);
		Deck.Initialize(_player);
		RoomIcon.Initialize(runState);
		FloorIcon.Initialize(runState);
		BossIcon.Initialize(runState);
		Gold.Initialize(_player);
		Hp.Initialize(_player);
		Pause.Initialize(runState);
		Portrait.Initialize(_player);
		PortraitTip.Initialize(runState);
		PotionContainer.Initialize(runState);
		_player.RelicObtained += OnRelicsUpdated;
		_player.RelicRemoved += OnRelicsUpdated;
		_player.MaxPotionCountChanged += MaxPotionsChanged;
		Callable.From(UpdateNavigation).CallDeferred();
	}

	public override void _ExitTree()
	{
		if (_player != null)
		{
			_player.RelicObtained -= OnRelicsUpdated;
			_player.RelicRemoved -= OnRelicsUpdated;
			_player.MaxPotionCountChanged -= MaxPotionsChanged;
		}
	}

	private void ToggleAnimState(Node _)
	{
		Pause.ToggleAnimState();
		Deck.ToggleAnimState();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideTopBar))
		{
			DebugHideTopBar();
		}
	}

	private void DebugHideTopBar()
	{
		if (!_isDebugHidden)
		{
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create("Hide Top Bar"));
			AnimHide();
		}
		else
		{
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create("Show Top Bar"));
			AnimShow();
		}
		_isDebugHidden = !_isDebugHidden;
	}

	public void AnimHide()
	{
		base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Disabled;
		_hideTween?.Kill();
		_hideTween = CreateTween();
		_hideTween.TweenProperty(this, "position:y", -100f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void AnimShow()
	{
		base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Enabled;
		_hideTween?.Kill();
		_hideTween = CreateTween();
		_hideTween.TweenProperty(this, "position:y", 0f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	private void OnRelicsUpdated(RelicModel _)
	{
		Callable.From(UpdateNavigation).CallDeferred();
	}

	private void MaxPotionsChanged(int _)
	{
		Callable.From(UpdateNavigation).CallDeferred();
	}

	private void UpdateNavigation()
	{
		Control control = NRun.Instance.GlobalUi.RelicInventory.RelicNodes.FirstOrDefault();
		if (control != null)
		{
			Gold.FocusNeighborBottom = control.GetPath();
			Hp.FocusNeighborBottom = control.GetPath();
			FloorIcon.FocusNeighborBottom = control.GetPath();
			RoomIcon.FocusNeighborBottom = control.GetPath();
			BossIcon.FocusNeighborBottom = control.GetPath();
			Gold.FocusNeighborTop = Gold.GetPath();
			Hp.FocusNeighborTop = Hp.GetPath();
			FloorIcon.FocusNeighborTop = FloorIcon.GetPath();
			RoomIcon.FocusNeighborTop = RoomIcon.GetPath();
			BossIcon.FocusNeighborTop = BossIcon.GetPath();
			Hp.FocusNeighborLeft = Hp.GetPath();
			Hp.FocusNeighborRight = Gold.GetPath();
			Gold.FocusNeighborLeft = Hp.GetPath();
			Gold.FocusNeighborRight = PotionContainer.FirstPotionControl?.GetPath();
			RoomIcon.FocusNeighborLeft = PotionContainer.LastPotionControl?.GetPath();
			RoomIcon.FocusNeighborRight = FloorIcon.GetPath();
			FloorIcon.FocusNeighborLeft = RoomIcon.GetPath();
			FloorIcon.FocusNeighborRight = BossIcon.GetPath();
			BossIcon.FocusNeighborLeft = FloorIcon.GetPath();
			BossIcon.FocusNeighborRight = BossIcon.GetPath();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleAnimState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugHideTopBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MaxPotionsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleAnimState && args.Count == 1)
		{
			ToggleAnimState(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugHideTopBar && args.Count == 0)
		{
			DebugHideTopBar();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHide && args.Count == 0)
		{
			AnimHide();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimShow && args.Count == 0)
		{
			AnimShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MaxPotionsChanged && args.Count == 1)
		{
			MaxPotionsChanged(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ToggleAnimState)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.DebugHideTopBar)
		{
			return true;
		}
		if (method == MethodName.AnimHide)
		{
			return true;
		}
		if (method == MethodName.AnimShow)
		{
			return true;
		}
		if (method == MethodName.MaxPotionsChanged)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Map)
		{
			Map = VariantUtils.ConvertTo<NTopBarMapButton>(in value);
			return true;
		}
		if (name == PropertyName.Deck)
		{
			Deck = VariantUtils.ConvertTo<NTopBarDeckButton>(in value);
			return true;
		}
		if (name == PropertyName.Pause)
		{
			Pause = VariantUtils.ConvertTo<NTopBarPauseButton>(in value);
			return true;
		}
		if (name == PropertyName.PotionContainer)
		{
			PotionContainer = VariantUtils.ConvertTo<NPotionContainer>(in value);
			return true;
		}
		if (name == PropertyName.RoomIcon)
		{
			RoomIcon = VariantUtils.ConvertTo<NTopBarRoomIcon>(in value);
			return true;
		}
		if (name == PropertyName.FloorIcon)
		{
			FloorIcon = VariantUtils.ConvertTo<NTopBarFloorIcon>(in value);
			return true;
		}
		if (name == PropertyName.BossIcon)
		{
			BossIcon = VariantUtils.ConvertTo<NTopBarBossIcon>(in value);
			return true;
		}
		if (name == PropertyName.Gold)
		{
			Gold = VariantUtils.ConvertTo<NTopBarGold>(in value);
			return true;
		}
		if (name == PropertyName.Hp)
		{
			Hp = VariantUtils.ConvertTo<NTopBarHp>(in value);
			return true;
		}
		if (name == PropertyName.Portrait)
		{
			Portrait = VariantUtils.ConvertTo<NTopBarPortrait>(in value);
			return true;
		}
		if (name == PropertyName.PortraitTip)
		{
			PortraitTip = VariantUtils.ConvertTo<NTopBarPortraitTip>(in value);
			return true;
		}
		if (name == PropertyName.Timer)
		{
			Timer = VariantUtils.ConvertTo<NRunTimer>(in value);
			return true;
		}
		if (name == PropertyName.TrailContainer)
		{
			TrailContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._capstoneContainer)
		{
			_capstoneContainer = VariantUtils.ConvertTo<NCapstoneContainer>(in value);
			return true;
		}
		if (name == PropertyName._modifiersContainer)
		{
			_modifiersContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._ascensionIcon)
		{
			_ascensionIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			_ascensionLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._ascensionHsv)
		{
			_ascensionHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._hideTween)
		{
			_hideTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isDebugHidden)
		{
			_isDebugHidden = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Map)
		{
			value = VariantUtils.CreateFrom<NTopBarMapButton>(Map);
			return true;
		}
		if (name == PropertyName.Deck)
		{
			value = VariantUtils.CreateFrom<NTopBarDeckButton>(Deck);
			return true;
		}
		if (name == PropertyName.Pause)
		{
			value = VariantUtils.CreateFrom<NTopBarPauseButton>(Pause);
			return true;
		}
		if (name == PropertyName.PotionContainer)
		{
			value = VariantUtils.CreateFrom<NPotionContainer>(PotionContainer);
			return true;
		}
		if (name == PropertyName.RoomIcon)
		{
			value = VariantUtils.CreateFrom<NTopBarRoomIcon>(RoomIcon);
			return true;
		}
		if (name == PropertyName.FloorIcon)
		{
			value = VariantUtils.CreateFrom<NTopBarFloorIcon>(FloorIcon);
			return true;
		}
		if (name == PropertyName.BossIcon)
		{
			value = VariantUtils.CreateFrom<NTopBarBossIcon>(BossIcon);
			return true;
		}
		if (name == PropertyName.Gold)
		{
			value = VariantUtils.CreateFrom<NTopBarGold>(Gold);
			return true;
		}
		if (name == PropertyName.Hp)
		{
			value = VariantUtils.CreateFrom<NTopBarHp>(Hp);
			return true;
		}
		if (name == PropertyName.Portrait)
		{
			value = VariantUtils.CreateFrom<NTopBarPortrait>(Portrait);
			return true;
		}
		if (name == PropertyName.PortraitTip)
		{
			value = VariantUtils.CreateFrom<NTopBarPortraitTip>(PortraitTip);
			return true;
		}
		if (name == PropertyName.Timer)
		{
			value = VariantUtils.CreateFrom<NRunTimer>(Timer);
			return true;
		}
		if (name == PropertyName.TrailContainer)
		{
			value = VariantUtils.CreateFrom<Node>(TrailContainer);
			return true;
		}
		if (name == PropertyName._capstoneContainer)
		{
			value = VariantUtils.CreateFrom(in _capstoneContainer);
			return true;
		}
		if (name == PropertyName._modifiersContainer)
		{
			value = VariantUtils.CreateFrom(in _modifiersContainer);
			return true;
		}
		if (name == PropertyName._ascensionIcon)
		{
			value = VariantUtils.CreateFrom(in _ascensionIcon);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			value = VariantUtils.CreateFrom(in _ascensionLabel);
			return true;
		}
		if (name == PropertyName._ascensionHsv)
		{
			value = VariantUtils.CreateFrom(in _ascensionHsv);
			return true;
		}
		if (name == PropertyName._hideTween)
		{
			value = VariantUtils.CreateFrom(in _hideTween);
			return true;
		}
		if (name == PropertyName._isDebugHidden)
		{
			value = VariantUtils.CreateFrom(in _isDebugHidden);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._capstoneContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Map, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Deck, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Pause, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PotionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RoomIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FloorIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BossIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Gold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hp, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PortraitTip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TrailContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hideTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDebugHidden, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Map, Variant.From<NTopBarMapButton>(Map));
		info.AddProperty(PropertyName.Deck, Variant.From<NTopBarDeckButton>(Deck));
		info.AddProperty(PropertyName.Pause, Variant.From<NTopBarPauseButton>(Pause));
		info.AddProperty(PropertyName.PotionContainer, Variant.From<NPotionContainer>(PotionContainer));
		info.AddProperty(PropertyName.RoomIcon, Variant.From<NTopBarRoomIcon>(RoomIcon));
		info.AddProperty(PropertyName.FloorIcon, Variant.From<NTopBarFloorIcon>(FloorIcon));
		info.AddProperty(PropertyName.BossIcon, Variant.From<NTopBarBossIcon>(BossIcon));
		info.AddProperty(PropertyName.Gold, Variant.From<NTopBarGold>(Gold));
		info.AddProperty(PropertyName.Hp, Variant.From<NTopBarHp>(Hp));
		info.AddProperty(PropertyName.Portrait, Variant.From<NTopBarPortrait>(Portrait));
		info.AddProperty(PropertyName.PortraitTip, Variant.From<NTopBarPortraitTip>(PortraitTip));
		info.AddProperty(PropertyName.Timer, Variant.From<NRunTimer>(Timer));
		info.AddProperty(PropertyName.TrailContainer, Variant.From<Node>(TrailContainer));
		info.AddProperty(PropertyName._capstoneContainer, Variant.From(in _capstoneContainer));
		info.AddProperty(PropertyName._modifiersContainer, Variant.From(in _modifiersContainer));
		info.AddProperty(PropertyName._ascensionIcon, Variant.From(in _ascensionIcon));
		info.AddProperty(PropertyName._ascensionLabel, Variant.From(in _ascensionLabel));
		info.AddProperty(PropertyName._ascensionHsv, Variant.From(in _ascensionHsv));
		info.AddProperty(PropertyName._hideTween, Variant.From(in _hideTween));
		info.AddProperty(PropertyName._isDebugHidden, Variant.From(in _isDebugHidden));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Map, out var value))
		{
			Map = value.As<NTopBarMapButton>();
		}
		if (info.TryGetProperty(PropertyName.Deck, out var value2))
		{
			Deck = value2.As<NTopBarDeckButton>();
		}
		if (info.TryGetProperty(PropertyName.Pause, out var value3))
		{
			Pause = value3.As<NTopBarPauseButton>();
		}
		if (info.TryGetProperty(PropertyName.PotionContainer, out var value4))
		{
			PotionContainer = value4.As<NPotionContainer>();
		}
		if (info.TryGetProperty(PropertyName.RoomIcon, out var value5))
		{
			RoomIcon = value5.As<NTopBarRoomIcon>();
		}
		if (info.TryGetProperty(PropertyName.FloorIcon, out var value6))
		{
			FloorIcon = value6.As<NTopBarFloorIcon>();
		}
		if (info.TryGetProperty(PropertyName.BossIcon, out var value7))
		{
			BossIcon = value7.As<NTopBarBossIcon>();
		}
		if (info.TryGetProperty(PropertyName.Gold, out var value8))
		{
			Gold = value8.As<NTopBarGold>();
		}
		if (info.TryGetProperty(PropertyName.Hp, out var value9))
		{
			Hp = value9.As<NTopBarHp>();
		}
		if (info.TryGetProperty(PropertyName.Portrait, out var value10))
		{
			Portrait = value10.As<NTopBarPortrait>();
		}
		if (info.TryGetProperty(PropertyName.PortraitTip, out var value11))
		{
			PortraitTip = value11.As<NTopBarPortraitTip>();
		}
		if (info.TryGetProperty(PropertyName.Timer, out var value12))
		{
			Timer = value12.As<NRunTimer>();
		}
		if (info.TryGetProperty(PropertyName.TrailContainer, out var value13))
		{
			TrailContainer = value13.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._capstoneContainer, out var value14))
		{
			_capstoneContainer = value14.As<NCapstoneContainer>();
		}
		if (info.TryGetProperty(PropertyName._modifiersContainer, out var value15))
		{
			_modifiersContainer = value15.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ascensionIcon, out var value16))
		{
			_ascensionIcon = value16.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLabel, out var value17))
		{
			_ascensionLabel = value17.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._ascensionHsv, out var value18))
		{
			_ascensionHsv = value18.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._hideTween, out var value19))
		{
			_hideTween = value19.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isDebugHidden, out var value20))
		{
			_isDebugHidden = value20.As<bool>();
		}
	}
}
