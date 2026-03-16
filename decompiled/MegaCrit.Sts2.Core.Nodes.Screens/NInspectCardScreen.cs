using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NInspectCardScreen.cs")]
public class NInspectCardScreen : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Close = "Close";

		public static readonly StringName OnRightButtonReleased = "OnRightButtonReleased";

		public static readonly StringName OnLeftButtonReleased = "OnLeftButtonReleased";

		public static readonly StringName ToggleShowUpgrade = "ToggleShowUpgrade";

		public static readonly StringName UpdateCardDisplay = "UpdateCardDisplay";

		public static readonly StringName SetCard = "SetCard";

		public static readonly StringName OnBackstopPressed = "OnBackstopPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsShowingUpgradedCard = "IsShowingUpgradedCard";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _card = "_card";

		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _upgradeTickbox = "_upgradeTickbox";

		public static readonly StringName _leftButton = "_leftButton";

		public static readonly StringName _rightButton = "_rightButton";

		public static readonly StringName _hoverTipRect = "_hoverTipRect";

		public static readonly StringName _index = "_index";

		public static readonly StringName _openTween = "_openTween";

		public static readonly StringName _cardTween = "_cardTween";

		public static readonly StringName _cardPosition = "_cardPosition";

		public static readonly StringName _leftButtonX = "_leftButtonX";

		public static readonly StringName _rightButtonX = "_rightButtonX";

		public static readonly StringName _viewAllUpgraded = "_viewAllUpgraded";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/inspect_card_screen");

	private NCard _card;

	private NButton _backstop;

	private NTickbox _upgradeTickbox;

	private NButton _leftButton;

	private NButton _rightButton;

	private Control _hoverTipRect;

	private List<CardModel>? _cards;

	private int _index;

	private Tween? _openTween;

	private Tween? _cardTween;

	private Vector2 _cardPosition;

	private float _leftButtonX;

	private float _rightButtonX;

	private const double _arrowButtonDelay = 0.1;

	private bool _viewAllUpgraded;

	public static string[] AssetPaths => new string[1] { _scenePath };

	private bool IsShowingUpgradedCard => _upgradeTickbox.IsTicked;

	public Control? DefaultFocusedControl => null;

	public static NInspectCardScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NInspectCardScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_card = GetNode<NCard>("Card");
		_cardPosition = _card.Position;
		_hoverTipRect = GetNode<Control>("HoverTipRect");
		_backstop = GetNode<NButton>("Backstop");
		_backstop.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackstopPressed));
		_leftButton = GetNode<NButton>("LeftArrow");
		_leftButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			OnLeftButtonReleased();
		}));
		_rightButton = GetNode<NButton>("RightArrow");
		_rightButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			OnRightButtonReleased();
		}));
		_leftButtonX = _leftButton.Position.X;
		_rightButtonX = _rightButton.Position.X;
		_upgradeTickbox = GetNode<NTickbox>("%Upgrade");
		_upgradeTickbox.IsTicked = false;
		_upgradeTickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowUpgrade));
		GetNode<MegaLabel>("%ShowUpgradeLabel").SetTextAutoSize(new LocString("card_selection", "VIEW_UPGRADES").GetFormattedText());
		_rightButton.Disable();
		_leftButton.Disable();
		_upgradeTickbox.Disable();
		Close();
	}

	public void Open(List<CardModel> cards, int index, bool viewAllUpgraded = false)
	{
		_cards = cards;
		base.Visible = true;
		base.MouseFilter = MouseFilterEnum.Stop;
		_viewAllUpgraded = viewAllUpgraded;
		SetCard(index);
		_card.Scale = Vector2.One * 1.75f;
		_card.Modulate = StsColors.transparentBlack;
		_leftButton.Modulate = StsColors.transparentBlack;
		_rightButton.Modulate = StsColors.transparentBlack;
		_rightButton.Enable();
		_leftButton.Enable();
		_openTween?.Kill();
		_openTween = CreateTween().SetParallel();
		_openTween.TweenProperty(_backstop, "modulate:a", 0.9f, 0.25);
		_openTween.TweenProperty(this, "modulate:a", 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(0f);
		_openTween.TweenProperty(_leftButton, "position:x", _leftButtonX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_leftButtonX + 100f)
			.SetDelay(0.1);
		_openTween.TweenProperty(_leftButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_openTween.TweenProperty(_rightButton, "position:x", _rightButtonX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_rightButtonX - 100f)
			.SetDelay(0.1);
		_openTween.TweenProperty(_rightButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_cardTween?.Kill();
		_cardTween = CreateTween().SetParallel();
		_cardTween.TweenProperty(_card, "modulate", Colors.White, 0.25);
		_cardTween.TweenProperty(_card, "scale", Vector2.One * 2f, 0.15).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring)
			.SetDelay(0.1);
		_upgradeTickbox.Enable();
		ActiveScreenContext.Instance.Update();
		NHotkeyManager.Instance.AddBlockingScreen(this);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.cancel, Close);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.left, OnLeftButtonReleased);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.right, OnRightButtonReleased);
	}

	public void Close()
	{
		if (base.Visible)
		{
			base.MouseFilter = MouseFilterEnum.Ignore;
			_leftButton.MouseFilter = MouseFilterEnum.Ignore;
			_rightButton.MouseFilter = MouseFilterEnum.Ignore;
			_rightButton.Disable();
			_leftButton.Disable();
			_upgradeTickbox.Disable();
			NHoverTipSet.Clear();
			SetProcessUnhandledInput(enable: false);
			_openTween?.Kill();
			_openTween = CreateTween().SetParallel();
			_openTween.TweenProperty(_backstop, "modulate:a", 0f, 0.25);
			_openTween.TweenProperty(_leftButton, "modulate:a", 0f, 0.1);
			_openTween.TweenProperty(_rightButton, "modulate:a", 0f, 0.1);
			_openTween.TweenProperty(_card, "modulate", StsColors.transparentWhite, 0.1);
			_openTween.Chain().TweenCallback(Callable.From(delegate
			{
				base.Visible = false;
				ActiveScreenContext.Instance.Update();
			}));
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.cancel, Close);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.left, OnLeftButtonReleased);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.right, OnRightButtonReleased);
			NHotkeyManager.Instance.RemoveBlockingScreen(this);
		}
	}

	private void OnRightButtonReleased()
	{
		if (_rightButton.Visible)
		{
			SetCard(_index + 1);
			_card.Modulate = Colors.White;
			_openTween?.Kill();
			_openTween = CreateTween().SetParallel();
			_openTween.TweenProperty(_card, "position", _cardPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(_cardPosition + new Vector2(100f, 0f));
		}
	}

	private void OnLeftButtonReleased()
	{
		if (_leftButton.Visible)
		{
			SetCard(_index - 1);
			_card.Modulate = Colors.White;
			_openTween?.Kill();
			_openTween = CreateTween().SetParallel();
			_openTween.TweenProperty(_card, "position", _cardPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(_cardPosition + new Vector2(-100f, 0f));
		}
	}

	private void ToggleShowUpgrade(NTickbox _)
	{
		_viewAllUpgraded = false;
		UpdateCardDisplay();
	}

	private void UpdateCardDisplay()
	{
		CardModel cardModel = _cards[_index];
		CardModel cardModel2 = (CardModel)_cards[_index].MutableClone();
		if (IsShowingUpgradedCard)
		{
			if (!cardModel.IsUpgraded && cardModel.IsUpgradable)
			{
				cardModel2.UpgradePreviewType = CardUpgradePreviewType.Deck;
				cardModel2.UpgradeInternal();
			}
			_card.Model = cardModel2;
			_card.ShowUpgradePreview();
		}
		else
		{
			if (cardModel2.IsUpgraded)
			{
				CardCmd.Downgrade(cardModel2);
			}
			_card.Model = cardModel2;
			_card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		}
		NHoverTipSet.Clear();
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, cardModel2.HoverTips);
		nHoverTipSet.SetAlignment(_hoverTipRect, HoverTip.GetHoverTipAlignment(this));
	}

	private void SetCard(int index)
	{
		_index = Math.Clamp(index, 0, _cards.Count - 1);
		_leftButton.Visible = _index > 0;
		_leftButton.MouseFilter = (MouseFilterEnum)(_leftButton.Visible ? 0 : 2);
		_rightButton.Visible = _index < _cards.Count - 1;
		_rightButton.MouseFilter = (MouseFilterEnum)(_rightButton.Visible ? 0 : 2);
		_upgradeTickbox.Visible = _cards[_index].MaxUpgradeLevel > 0;
		_upgradeTickbox.MouseFilter = (MouseFilterEnum)((_cards[_index].MaxUpgradeLevel > 0) ? 0 : 2);
		if (_cards[_index].IsUpgraded || _viewAllUpgraded)
		{
			_upgradeTickbox.IsTicked = true;
		}
		else
		{
			_upgradeTickbox.IsTicked = false;
		}
		UpdateCardDisplay();
	}

	private void OnBackstopPressed(NButton _)
	{
		Close();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRightButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnLeftButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleShowUpgrade, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCardDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnBackstopPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NInspectCardScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRightButtonReleased && args.Count == 0)
		{
			OnRightButtonReleased();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLeftButtonReleased && args.Count == 0)
		{
			OnLeftButtonReleased();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleShowUpgrade && args.Count == 1)
		{
			ToggleShowUpgrade(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCardDisplay && args.Count == 0)
		{
			UpdateCardDisplay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCard && args.Count == 1)
		{
			SetCard(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBackstopPressed && args.Count == 1)
		{
			OnBackstopPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NInspectCardScreen>(Create());
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
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.OnRightButtonReleased)
		{
			return true;
		}
		if (method == MethodName.OnLeftButtonReleased)
		{
			return true;
		}
		if (method == MethodName.ToggleShowUpgrade)
		{
			return true;
		}
		if (method == MethodName.UpdateCardDisplay)
		{
			return true;
		}
		if (method == MethodName.SetCard)
		{
			return true;
		}
		if (method == MethodName.OnBackstopPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._card)
		{
			_card = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._upgradeTickbox)
		{
			_upgradeTickbox = VariantUtils.ConvertTo<NTickbox>(in value);
			return true;
		}
		if (name == PropertyName._leftButton)
		{
			_leftButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._rightButton)
		{
			_rightButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._hoverTipRect)
		{
			_hoverTipRect = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._openTween)
		{
			_openTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cardPosition)
		{
			_cardPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._leftButtonX)
		{
			_leftButtonX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rightButtonX)
		{
			_rightButtonX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._viewAllUpgraded)
		{
			_viewAllUpgraded = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsShowingUpgradedCard)
		{
			value = VariantUtils.CreateFrom<bool>(IsShowingUpgradedCard);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._card)
		{
			value = VariantUtils.CreateFrom(in _card);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._upgradeTickbox)
		{
			value = VariantUtils.CreateFrom(in _upgradeTickbox);
			return true;
		}
		if (name == PropertyName._leftButton)
		{
			value = VariantUtils.CreateFrom(in _leftButton);
			return true;
		}
		if (name == PropertyName._rightButton)
		{
			value = VariantUtils.CreateFrom(in _rightButton);
			return true;
		}
		if (name == PropertyName._hoverTipRect)
		{
			value = VariantUtils.CreateFrom(in _hoverTipRect);
			return true;
		}
		if (name == PropertyName._index)
		{
			value = VariantUtils.CreateFrom(in _index);
			return true;
		}
		if (name == PropertyName._openTween)
		{
			value = VariantUtils.CreateFrom(in _openTween);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
			return true;
		}
		if (name == PropertyName._cardPosition)
		{
			value = VariantUtils.CreateFrom(in _cardPosition);
			return true;
		}
		if (name == PropertyName._leftButtonX)
		{
			value = VariantUtils.CreateFrom(in _leftButtonX);
			return true;
		}
		if (name == PropertyName._rightButtonX)
		{
			value = VariantUtils.CreateFrom(in _rightButtonX);
			return true;
		}
		if (name == PropertyName._viewAllUpgraded)
		{
			value = VariantUtils.CreateFrom(in _viewAllUpgraded);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._card, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upgradeTickbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTipRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._openTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._cardPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._leftButtonX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rightButtonX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._viewAllUpgraded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgradedCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._card, Variant.From(in _card));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._upgradeTickbox, Variant.From(in _upgradeTickbox));
		info.AddProperty(PropertyName._leftButton, Variant.From(in _leftButton));
		info.AddProperty(PropertyName._rightButton, Variant.From(in _rightButton));
		info.AddProperty(PropertyName._hoverTipRect, Variant.From(in _hoverTipRect));
		info.AddProperty(PropertyName._index, Variant.From(in _index));
		info.AddProperty(PropertyName._openTween, Variant.From(in _openTween));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
		info.AddProperty(PropertyName._cardPosition, Variant.From(in _cardPosition));
		info.AddProperty(PropertyName._leftButtonX, Variant.From(in _leftButtonX));
		info.AddProperty(PropertyName._rightButtonX, Variant.From(in _rightButtonX));
		info.AddProperty(PropertyName._viewAllUpgraded, Variant.From(in _viewAllUpgraded));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._card, out var value))
		{
			_card = value.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value2))
		{
			_backstop = value2.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._upgradeTickbox, out var value3))
		{
			_upgradeTickbox = value3.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._leftButton, out var value4))
		{
			_leftButton = value4.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._rightButton, out var value5))
		{
			_rightButton = value5.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._hoverTipRect, out var value6))
		{
			_hoverTipRect = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._index, out var value7))
		{
			_index = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._openTween, out var value8))
		{
			_openTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value9))
		{
			_cardTween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cardPosition, out var value10))
		{
			_cardPosition = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._leftButtonX, out var value11))
		{
			_leftButtonX = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rightButtonX, out var value12))
		{
			_rightButtonX = value12.As<float>();
		}
		if (info.TryGetProperty(PropertyName._viewAllUpgraded, out var value13))
		{
			_viewAllUpgraded = value13.As<bool>();
		}
	}
}
