using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NCardPileScreen.cs")]
public class NCardPileScreen : Control, ICapstoneScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnPileContentsChanged = "OnPileContentsChanged";

		public static readonly StringName OnReturnButtonPressed = "OnReturnButtonPressed";

		public static readonly StringName AfterCapstoneOpened = "AfterCapstoneOpened";

		public static readonly StringName AfterCapstoneClosed = "AfterCapstoneClosed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _background = "_background";

		public static readonly StringName _grid = "_grid";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _bottomLabel = "_bottomLabel";

		public static readonly StringName _currentTween = "_currentTween";

		public static readonly StringName _closeHotkeys = "_closeHotkeys";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private ColorRect _background;

	private NCardGrid _grid;

	private NButton _backButton;

	private MegaRichTextLabel _bottomLabel;

	private Tween? _currentTween;

	private string[] _closeHotkeys = Array.Empty<string>();

	private static string ScenePath => SceneHelper.GetScenePath("/screens/card_pile_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public NetScreenType ScreenType => NetScreenType.CardPile;

	public CardPile Pile { get; private set; }

	public bool UseSharedBackstop => true;

	public Control? DefaultFocusedControl => _grid.DefaultFocusedControl;

	public override void _Ready()
	{
		_bottomLabel = GetNode<MegaRichTextLabel>("%BottomLabel");
		switch (Pile.Type)
		{
		case PileType.Draw:
			_bottomLabel.Text = "[center]" + new LocString("gameplay_ui", "DRAW_PILE_INFO").GetFormattedText();
			break;
		case PileType.Discard:
			_bottomLabel.Text = "[center]" + new LocString("gameplay_ui", "DISCARD_PILE_INFO").GetFormattedText();
			break;
		case PileType.Exhaust:
			_bottomLabel.Text = "[center]" + new LocString("gameplay_ui", "EXHAUST_PILE_INFO").GetFormattedText();
			break;
		default:
			_bottomLabel.Visible = false;
			Log.Info("CardPileScreen has no info text.");
			break;
		}
		_backButton = GetNode<NButton>("BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnReturnButtonPressed));
		_backButton.Enable();
		_grid = GetNode<NCardGrid>("CardGrid");
		OnPileContentsChanged();
		_grid.InsetForTopBar();
		_background = GetNode<ColorRect>("Background");
		_background.Modulate = StsColors.transparentBlack;
		_currentTween = CreateTween();
		_currentTween.TweenProperty(_background, "modulate", StsColors.screenBackdrop, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Pile.ContentsChanged += OnPileContentsChanged;
		string[] closeHotkeys = _closeHotkeys;
		foreach (string hotkey in closeHotkeys)
		{
			NHotkeyManager.Instance.PushHotkeyReleasedBinding(hotkey, NCapstoneContainer.Instance.Close);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Pile.ContentsChanged -= OnPileContentsChanged;
		string[] closeHotkeys = _closeHotkeys;
		foreach (string hotkey in closeHotkeys)
		{
			NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(hotkey, NCapstoneContainer.Instance.Close);
		}
	}

	public static NCardPileScreen ShowScreen(CardPile pile, string[] closeHotkeys)
	{
		NDebugAudioManager.Instance?.Play("map_open.mp3");
		NCardPileScreen nCardPileScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardPileScreen>(PackedScene.GenEditState.Disabled);
		nCardPileScreen.Name = $"{"NCardPileScreen"}-{pile.Type}";
		nCardPileScreen.Pile = pile;
		nCardPileScreen._closeHotkeys = closeHotkeys;
		NCapstoneContainer.Instance.Open(nCardPileScreen);
		return nCardPileScreen;
	}

	private void OnPileContentsChanged()
	{
		List<CardModel> list = Pile.Cards.ToList();
		if (Pile.Type == PileType.Draw)
		{
			list.Sort((CardModel c1, CardModel c2) => (c1.Rarity != c2.Rarity) ? c1.Rarity.CompareTo(c2.Rarity) : string.Compare(c1.Id.Entry, c2.Id.Entry, StringComparison.Ordinal));
		}
		NCardGrid grid = _grid;
		PileType type = Pile.Type;
		int num = 1;
		List<SortingOrders> list2 = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list2, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list2);
		int index = 0;
		span[index] = SortingOrders.Ascending;
		grid.SetCards(list, type, list2);
	}

	private void OnReturnButtonPressed(NButton _)
	{
		NCapstoneContainer.Instance.Close();
	}

	public void AfterCapstoneOpened()
	{
		base.Visible = true;
	}

	public void AfterCapstoneClosed()
	{
		base.Visible = false;
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPileContentsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnReturnButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPileContentsChanged && args.Count == 0)
		{
			OnPileContentsChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReturnButtonPressed && args.Count == 1)
		{
			OnReturnButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened && args.Count == 0)
		{
			AfterCapstoneOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed && args.Count == 0)
		{
			AfterCapstoneClosed();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnPileContentsChanged)
		{
			return true;
		}
		if (method == MethodName.OnReturnButtonPressed)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._background)
		{
			_background = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._grid)
		{
			_grid = VariantUtils.ConvertTo<NCardGrid>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			_bottomLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			_currentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._closeHotkeys)
		{
			_closeHotkeys = VariantUtils.ConvertTo<string[]>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._background)
		{
			value = VariantUtils.CreateFrom(in _background);
			return true;
		}
		if (name == PropertyName._grid)
		{
			value = VariantUtils.CreateFrom(in _grid);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			value = VariantUtils.CreateFrom(in _bottomLabel);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			value = VariantUtils.CreateFrom(in _currentTween);
			return true;
		}
		if (name == PropertyName._closeHotkeys)
		{
			value = VariantUtils.CreateFrom(in _closeHotkeys);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._grid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName._closeHotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._background, Variant.From(in _background));
		info.AddProperty(PropertyName._grid, Variant.From(in _grid));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._bottomLabel, Variant.From(in _bottomLabel));
		info.AddProperty(PropertyName._currentTween, Variant.From(in _currentTween));
		info.AddProperty(PropertyName._closeHotkeys, Variant.From(in _closeHotkeys));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._background, out var value))
		{
			_background = value.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._grid, out var value2))
		{
			_grid = value2.As<NCardGrid>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value3))
		{
			_backButton = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._bottomLabel, out var value4))
		{
			_bottomLabel = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentTween, out var value5))
		{
			_currentTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._closeHotkeys, out var value6))
		{
			_closeHotkeys = value6.As<string[]>();
		}
	}
}
