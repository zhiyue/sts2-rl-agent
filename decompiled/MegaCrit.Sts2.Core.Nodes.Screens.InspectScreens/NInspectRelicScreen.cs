using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;

[ScriptPath("res://src/Core/Nodes/Screens/InspectScreens/NInspectRelicScreen.cs")]
public class NInspectRelicScreen : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnRightButtonPressed = "OnRightButtonPressed";

		public static readonly StringName OnLeftButtonPressed = "OnLeftButtonPressed";

		public static readonly StringName SetRelic = "SetRelic";

		public static readonly StringName UpdateRelicDisplay = "UpdateRelicDisplay";

		public static readonly StringName SetRarityVisuals = "SetRarityVisuals";

		public static readonly StringName Close = "Close";

		public static readonly StringName OnBackstopPressed = "OnBackstopPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _popup = "_popup";

		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _nameLabel = "_nameLabel";

		public static readonly StringName _rarityLabel = "_rarityLabel";

		public static readonly StringName _description = "_description";

		public static readonly StringName _flavor = "_flavor";

		public static readonly StringName _relicImage = "_relicImage";

		public static readonly StringName _frameHsv = "_frameHsv";

		public static readonly StringName _leftButton = "_leftButton";

		public static readonly StringName _rightButton = "_rightButton";

		public static readonly StringName _hoverTipRect = "_hoverTipRect";

		public static readonly StringName _screenTween = "_screenTween";

		public static readonly StringName _popupTween = "_popupTween";

		public static readonly StringName _popupPosition = "_popupPosition";

		public static readonly StringName _leftButtonX = "_leftButtonX";

		public static readonly StringName _rightButtonX = "_rightButtonX";

		public static readonly StringName _index = "_index";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/inspect_relic_screen/inspect_relic_screen");

	private Control _popup;

	private Control _backstop;

	private MegaLabel _nameLabel;

	private MegaLabel _rarityLabel;

	private MegaRichTextLabel _description;

	private MegaRichTextLabel _flavor;

	private TextureRect _relicImage;

	private ShaderMaterial _frameHsv;

	private NGoldArrowButton _leftButton;

	private NGoldArrowButton _rightButton;

	private Control _hoverTipRect;

	private Tween? _screenTween;

	private Tween? _popupTween;

	private Vector2 _popupPosition;

	private float _leftButtonX;

	private float _rightButtonX;

	private const double _arrowButtonDelay = 0.1;

	private IReadOnlyList<RelicModel> _relics;

	private int _index;

	private HashSet<RelicModel> _allUnlockedRelics = new HashSet<RelicModel>();

	public static string[] AssetPaths => new string[1] { _scenePath };

	public Control? DefaultFocusedControl => null;

	public static NInspectRelicScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NInspectRelicScreen>(PackedScene.GenEditState.Disabled);
	}

	public void Open(IReadOnlyList<RelicModel> relics, RelicModel relic)
	{
		Log.Info($"Inspecting Relic: {relic.Title}");
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		_allUnlockedRelics.Clear();
		_allUnlockedRelics.UnionWith(unlockState.Relics);
		_relics = relics.ToList();
		_index = relics.IndexOf(relic);
		SetRelic(_index);
		base.Visible = true;
		_popup.Modulate = StsColors.transparentBlack;
		_leftButton.Modulate = StsColors.transparentBlack;
		_rightButton.Modulate = StsColors.transparentBlack;
		_leftButton.Enable();
		_rightButton.Enable();
		_backstop.Visible = true;
		_backstop.MouseFilter = MouseFilterEnum.Stop;
		_leftButton.MouseFilter = MouseFilterEnum.Stop;
		_rightButton.MouseFilter = MouseFilterEnum.Stop;
		_screenTween?.Kill();
		_screenTween = CreateTween().SetParallel();
		_screenTween.TweenProperty(_backstop, "modulate:a", 0.9f, 0.25);
		_screenTween.TweenProperty(_leftButton, "position:x", _leftButtonX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_leftButtonX + 100f)
			.SetDelay(0.1);
		_screenTween.TweenProperty(_leftButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_screenTween.TweenProperty(_rightButton, "position:x", _rightButtonX, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_rightButtonX - 100f)
			.SetDelay(0.1);
		_screenTween.TweenProperty(_rightButton, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_popupTween?.Kill();
		_popupTween = CreateTween().SetParallel();
		_popupTween.TweenProperty(_popup, "modulate", Colors.White, 0.25);
		_popupTween.TweenProperty(_popup, "position", _popupPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(_popupPosition + new Vector2(0f, 200f));
		ActiveScreenContext.Instance.Update();
		NHotkeyManager.Instance.AddBlockingScreen(this);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.cancel, Close);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
	}

	public override void _Ready()
	{
		_popup = GetNode<Control>("%Popup");
		_backstop = GetNode<Control>("%Backstop");
		_nameLabel = GetNode<MegaLabel>("%RelicName");
		_rarityLabel = GetNode<MegaLabel>("%Rarity");
		_description = GetNode<MegaRichTextLabel>("%RelicDescription");
		_flavor = GetNode<MegaRichTextLabel>("%FlavorText");
		_relicImage = GetNode<TextureRect>("%RelicImage");
		_frameHsv = (ShaderMaterial)GetNode<Control>("%Frame").Material;
		_leftButton = GetNode<NGoldArrowButton>("%LeftArrow");
		_rightButton = GetNode<NGoldArrowButton>("%RightArrow");
		_popupPosition = _popup.Position;
		_hoverTipRect = GetNode<Control>("HoverTipRect");
		_backstop = GetNode<NButton>("Backstop");
		_backstop.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackstopPressed));
		_leftButton = GetNode<NGoldArrowButton>("LeftArrow");
		_leftButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnLeftButtonPressed));
		_rightButton = GetNode<NGoldArrowButton>("RightArrow");
		_rightButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnRightButtonPressed));
		_leftButtonX = _leftButton.Position.X;
		_rightButtonX = _rightButton.Position.X;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsVisibleInTree() || NDevConsole.Instance.Visible)
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		if ((!(control is TextEdit) && !(control is LineEdit)) || 1 == 0)
		{
			if (inputEvent.IsActionPressed(MegaInput.left))
			{
				OnLeftButtonPressed(_leftButton);
			}
			if (inputEvent.IsActionPressed(MegaInput.right))
			{
				OnRightButtonPressed(_rightButton);
			}
		}
	}

	private void OnRightButtonPressed(NButton button)
	{
		SetRelic(_index + 1);
		_popup.Modulate = Colors.White;
		_popupTween?.Kill();
		_popupTween = CreateTween().SetParallel();
		_popupTween.TweenProperty(_popup, "position", _popupPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_popupPosition + new Vector2(100f, 0f));
	}

	private void OnLeftButtonPressed(NButton button)
	{
		SetRelic(_index - 1);
		_popup.Modulate = Colors.White;
		_popupTween?.Kill();
		_popupTween = CreateTween().SetParallel();
		_popupTween.TweenProperty(_popup, "position", _popupPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_popupPosition + new Vector2(-100f, 0f));
	}

	private void SetRelic(int index)
	{
		_index = Math.Clamp(index, 0, _relics.Count - 1);
		_leftButton.Visible = _index > 0;
		_leftButton.MouseFilter = (MouseFilterEnum)((_index > 0) ? 0 : 2);
		_rightButton.Visible = _index < _relics.Count - 1;
		_rightButton.MouseFilter = (MouseFilterEnum)((_index < _relics.Count - 1) ? 0 : 2);
		UpdateRelicDisplay();
	}

	private void UpdateRelicDisplay()
	{
		RelicModel relicModel = _relics[_index];
		if (!_allUnlockedRelics.Contains(relicModel.CanonicalInstance))
		{
			_nameLabel.SetTextAutoSize(new LocString("inspect_relic_screen", "LOCKED_TITLE").GetFormattedText());
			_rarityLabel.SetTextAutoSize(string.Empty);
			_relicImage.SelfModulate = Colors.White;
			_description.SetTextAutoSize(new LocString("inspect_relic_screen", "LOCKED_DESCRIPTION").GetFormattedText());
			_flavor.Text = string.Empty;
			SetRarityVisuals(RelicRarity.Common);
			_relicImage.Texture = PreloadManager.Cache.GetTexture2D(ImageHelper.GetImagePath("packed/common_ui/locked_model.png"));
		}
		else if (!SaveManager.Instance.IsRelicSeen(relicModel))
		{
			_nameLabel.SetTextAutoSize(new LocString("inspect_relic_screen", "UNDISCOVERED_TITLE").GetFormattedText());
			_rarityLabel.SetTextAutoSize(string.Empty);
			_relicImage.SelfModulate = StsColors.ninetyPercentBlack;
			_description.SetTextAutoSize(new LocString("inspect_relic_screen", "UNDISCOVERED_DESCRIPTION").GetFormattedText());
			_flavor.Text = string.Empty;
			SetRarityVisuals(relicModel.Rarity);
			_relicImage.Texture = relicModel.BigIcon;
		}
		else
		{
			_nameLabel.SetTextAutoSize(relicModel.Title.GetFormattedText());
			LocString locString = new LocString("gameplay_ui", "RELIC_RARITY." + relicModel.Rarity.ToString().ToUpperInvariant());
			_rarityLabel.SetTextAutoSize(locString.GetFormattedText());
			_relicImage.SelfModulate = Colors.White;
			_description.SetTextAutoSize(relicModel.DynamicDescription.GetFormattedText());
			_flavor.SetTextAutoSize(relicModel.Flavor.GetFormattedText());
			SetRarityVisuals(relicModel.Rarity);
			_relicImage.Texture = relicModel.BigIcon;
		}
		NHoverTipSet.Clear();
		if (SaveManager.Instance.IsRelicSeen(relicModel))
		{
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, relicModel.HoverTipsExcludingRelic);
			nHoverTipSet.SetAlignment(_hoverTipRect, HoverTip.GetHoverTipAlignment(this));
		}
	}

	private void SetRarityVisuals(RelicRarity rarity)
	{
		Vector3 vector;
		switch (rarity)
		{
		case RelicRarity.None:
		case RelicRarity.Starter:
		case RelicRarity.Common:
			_rarityLabel.Modulate = StsColors.cream;
			vector = new Vector3(0.95f, 0.25f, 0.9f);
			break;
		case RelicRarity.Uncommon:
			_rarityLabel.Modulate = StsColors.blue;
			vector = new Vector3(0.426f, 0.8f, 1.1f);
			break;
		case RelicRarity.Rare:
			_rarityLabel.Modulate = StsColors.gold;
			vector = new Vector3(1f, 0.8f, 1.15f);
			break;
		case RelicRarity.Shop:
			_rarityLabel.Modulate = StsColors.blue;
			vector = new Vector3(0.525f, 2.5f, 0.85f);
			break;
		case RelicRarity.Event:
			_rarityLabel.Modulate = StsColors.green;
			vector = new Vector3(0.23f, 0.75f, 0.9f);
			break;
		case RelicRarity.Ancient:
			_rarityLabel.Modulate = StsColors.red;
			vector = new Vector3(0.875f, 3f, 0.9f);
			break;
		default:
			Log.Error("Unspecified relic rarity: " + rarity);
			throw new ArgumentOutOfRangeException();
		}
		_frameHsv.SetShaderParameter(_h, vector.X);
		_frameHsv.SetShaderParameter(_s, vector.Y);
		_frameHsv.SetShaderParameter(_v, vector.Z);
	}

	public void Close()
	{
		if (base.Visible)
		{
			NHotkeyManager.Instance.RemoveBlockingScreen(this);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.cancel, Close);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.pauseAndBack, Close);
			_backstop.MouseFilter = MouseFilterEnum.Ignore;
			_leftButton.MouseFilter = MouseFilterEnum.Ignore;
			_rightButton.MouseFilter = MouseFilterEnum.Ignore;
			_leftButton.Disable();
			_rightButton.Disable();
			_screenTween?.Kill();
			_screenTween = CreateTween().SetParallel();
			_screenTween.TweenProperty(_backstop, "modulate:a", 0f, 0.25);
			_screenTween.TweenProperty(_leftButton, "modulate:a", 0f, 0.1);
			_screenTween.TweenProperty(_rightButton, "modulate:a", 0f, 0.1);
			_screenTween.TweenProperty(_popup, "modulate", StsColors.transparentWhite, 0.1);
			_screenTween.Chain().TweenCallback(Callable.From(delegate
			{
				base.Visible = false;
				ActiveScreenContext.Instance.Update();
			}));
			NHoverTipSet.Clear();
		}
	}

	private void OnBackstopPressed(NButton _)
	{
		Close();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRightButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnLeftButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetRelic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateRelicDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetRarityVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
			ret = VariantUtils.CreateFrom<NInspectRelicScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRightButtonPressed && args.Count == 1)
		{
			OnRightButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLeftButtonPressed && args.Count == 1)
		{
			OnLeftButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetRelic && args.Count == 1)
		{
			SetRelic(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateRelicDisplay && args.Count == 0)
		{
			UpdateRelicDisplay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetRarityVisuals && args.Count == 1)
		{
			SetRarityVisuals(VariantUtils.ConvertTo<RelicRarity>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 0)
		{
			Close();
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
			ret = VariantUtils.CreateFrom<NInspectRelicScreen>(Create());
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnRightButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnLeftButtonPressed)
		{
			return true;
		}
		if (method == MethodName.SetRelic)
		{
			return true;
		}
		if (method == MethodName.UpdateRelicDisplay)
		{
			return true;
		}
		if (method == MethodName.SetRarityVisuals)
		{
			return true;
		}
		if (method == MethodName.Close)
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
		if (name == PropertyName._popup)
		{
			_popup = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			_nameLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._rarityLabel)
		{
			_rarityLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._flavor)
		{
			_flavor = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicImage)
		{
			_relicImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._frameHsv)
		{
			_frameHsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._leftButton)
		{
			_leftButton = VariantUtils.ConvertTo<NGoldArrowButton>(in value);
			return true;
		}
		if (name == PropertyName._rightButton)
		{
			_rightButton = VariantUtils.ConvertTo<NGoldArrowButton>(in value);
			return true;
		}
		if (name == PropertyName._hoverTipRect)
		{
			_hoverTipRect = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			_screenTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._popupTween)
		{
			_popupTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._popupPosition)
		{
			_popupPosition = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._popup)
		{
			value = VariantUtils.CreateFrom(in _popup);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._nameLabel)
		{
			value = VariantUtils.CreateFrom(in _nameLabel);
			return true;
		}
		if (name == PropertyName._rarityLabel)
		{
			value = VariantUtils.CreateFrom(in _rarityLabel);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._flavor)
		{
			value = VariantUtils.CreateFrom(in _flavor);
			return true;
		}
		if (name == PropertyName._relicImage)
		{
			value = VariantUtils.CreateFrom(in _relicImage);
			return true;
		}
		if (name == PropertyName._frameHsv)
		{
			value = VariantUtils.CreateFrom(in _frameHsv);
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
		if (name == PropertyName._screenTween)
		{
			value = VariantUtils.CreateFrom(in _screenTween);
			return true;
		}
		if (name == PropertyName._popupTween)
		{
			value = VariantUtils.CreateFrom(in _popupTween);
			return true;
		}
		if (name == PropertyName._popupPosition)
		{
			value = VariantUtils.CreateFrom(in _popupPosition);
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
		if (name == PropertyName._index)
		{
			value = VariantUtils.CreateFrom(in _index);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._popup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rarityLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flavor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._frameHsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTipRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._popupTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._popupPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._leftButtonX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rightButtonX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._index, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._popup, Variant.From(in _popup));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._nameLabel, Variant.From(in _nameLabel));
		info.AddProperty(PropertyName._rarityLabel, Variant.From(in _rarityLabel));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._flavor, Variant.From(in _flavor));
		info.AddProperty(PropertyName._relicImage, Variant.From(in _relicImage));
		info.AddProperty(PropertyName._frameHsv, Variant.From(in _frameHsv));
		info.AddProperty(PropertyName._leftButton, Variant.From(in _leftButton));
		info.AddProperty(PropertyName._rightButton, Variant.From(in _rightButton));
		info.AddProperty(PropertyName._hoverTipRect, Variant.From(in _hoverTipRect));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
		info.AddProperty(PropertyName._popupTween, Variant.From(in _popupTween));
		info.AddProperty(PropertyName._popupPosition, Variant.From(in _popupPosition));
		info.AddProperty(PropertyName._leftButtonX, Variant.From(in _leftButtonX));
		info.AddProperty(PropertyName._rightButtonX, Variant.From(in _rightButtonX));
		info.AddProperty(PropertyName._index, Variant.From(in _index));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._popup, out var value))
		{
			_popup = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value2))
		{
			_backstop = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._nameLabel, out var value3))
		{
			_nameLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._rarityLabel, out var value4))
		{
			_rarityLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value5))
		{
			_description = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._flavor, out var value6))
		{
			_flavor = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicImage, out var value7))
		{
			_relicImage = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._frameHsv, out var value8))
		{
			_frameHsv = value8.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._leftButton, out var value9))
		{
			_leftButton = value9.As<NGoldArrowButton>();
		}
		if (info.TryGetProperty(PropertyName._rightButton, out var value10))
		{
			_rightButton = value10.As<NGoldArrowButton>();
		}
		if (info.TryGetProperty(PropertyName._hoverTipRect, out var value11))
		{
			_hoverTipRect = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value12))
		{
			_screenTween = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._popupTween, out var value13))
		{
			_popupTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._popupPosition, out var value14))
		{
			_popupPosition = value14.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._leftButtonX, out var value15))
		{
			_leftButtonX = value15.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rightButtonX, out var value16))
		{
			_rightButtonX = value16.As<float>();
		}
		if (info.TryGetProperty(PropertyName._index, out var value17))
		{
			_index = value17.As<int>();
		}
	}
}
