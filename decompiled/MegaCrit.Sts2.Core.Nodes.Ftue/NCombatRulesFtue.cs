using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Ftue;

[ScriptPath("res://src/Core/Nodes/Ftue/NCombatRulesFtue.cs")]
public class NCombatRulesFtue : NFtue
{
	public new class MethodName : NFtue.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Start = "Start";

		public static readonly StringName Create = "Create";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ToggleLeft = "ToggleLeft";

		public static readonly StringName ToggleRight = "ToggleRight";
	}

	public new class PropertyName : NFtue.PropertyName
	{
		public static readonly StringName _image1 = "_image1";

		public static readonly StringName _image2 = "_image2";

		public static readonly StringName _image3 = "_image3";

		public static readonly StringName _prevButton = "_prevButton";

		public static readonly StringName _nextButton = "_nextButton";

		public static readonly StringName _pageCount = "_pageCount";

		public static readonly StringName _image = "_image";

		public static readonly StringName _bodyText = "_bodyText";

		public static readonly StringName _header = "_header";

		public static readonly StringName _currentPage = "_currentPage";

		public static readonly StringName _imagePosition = "_imagePosition";

		public static readonly StringName _textPosition = "_textPosition";

		public static readonly StringName _pageTurnTween = "_pageTurnTween";
	}

	public new class SignalName : NFtue.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Texture2D _image1;

	[Export(PropertyHint.None, "")]
	private Texture2D _image2;

	[Export(PropertyHint.None, "")]
	private Texture2D _image3;

	public const string id = "combat_rules_ftue";

	private static readonly string _scenePath = SceneHelper.GetScenePath("ftue/combat_rules_ftue");

	private NButton _prevButton;

	private NButton _nextButton;

	private MegaLabel _pageCount;

	private TextureRect _image;

	private MegaRichTextLabel _bodyText;

	private MegaLabel _header;

	private int _currentPage = 1;

	private const int _totalPages = 3;

	private Vector2 _imagePosition;

	private Vector2 _textPosition;

	private Tween? _pageTurnTween;

	private const double _textTweenSpeed = 0.6;

	private static readonly Vector2 _imageAnimOffset = new Vector2(200f, 0f);

	public override void _Ready()
	{
		_image = GetNode<TextureRect>("Image");
		_bodyText = GetNode<MegaRichTextLabel>("%Description");
		_pageCount = GetNode<MegaLabel>("PageCount");
		_header = GetNode<MegaLabel>("Header");
		_prevButton = GetNode<NButton>("LeftArrow");
		_nextButton = GetNode<NButton>("RightArrow");
		_image.Modulate = Colors.Transparent;
		_bodyText.Modulate = Colors.Transparent;
		_prevButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ToggleLeft));
		_nextButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ToggleRight));
		_prevButton.Visible = false;
		_prevButton.Disable();
		_nextButton.Visible = false;
		_nextButton.Disable();
		_pageCount.Visible = false;
		_header.Visible = false;
	}

	public void Start()
	{
		NModalContainer.Instance?.ShowBackstop();
		_currentPage = 1;
		_imagePosition = _image.Position;
		_bodyText.Text = new LocString("ftues", "TUTORIAL_FTUE_BODY_1").GetFormattedText();
		_bodyText.Modulate = StsColors.transparentWhite;
		_textPosition = _bodyText.Position;
		LocString locString = new LocString("ftues", "COMBAT_BASICS_FTUE_PAGE_COUNT");
		locString.Add("totalPages", 3m);
		locString.Add("currentPage", _currentPage);
		_pageCount.SetTextAutoSize(locString.GetFormattedText());
		_header.SetTextAutoSize(new LocString("ftues", "COMBAT_BASICS_FTUE_HEADER").GetFormattedText());
		_nextButton.Visible = true;
		_nextButton.Enable();
		_pageCount.Visible = true;
		_header.Visible = true;
		_pageTurnTween = CreateTween().SetParallel();
		_pageTurnTween.TweenProperty(_image, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(0f);
		_pageTurnTween.TweenProperty(_bodyText, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
		_pageTurnTween.TweenProperty(_bodyText, "visible_ratio", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine)
			.From(0f);
	}

	public static NCombatRulesFtue? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCombatRulesFtue>(PackedScene.GenEditState.Disabled);
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
			if (inputEvent.IsActionPressed(MegaInput.left) && _prevButton.IsEnabled)
			{
				ToggleLeft(_prevButton);
			}
			if (inputEvent.IsActionPressed(MegaInput.right) && _nextButton.IsEnabled)
			{
				ToggleRight(_nextButton);
			}
		}
	}

	private void ToggleLeft(NButton _)
	{
		_currentPage--;
		switch (_currentPage)
		{
		case 1:
			_prevButton.Visible = false;
			_prevButton.Disable();
			_bodyText.SetTextAutoSize(new LocString("ftues", "TUTORIAL_FTUE_BODY_1").GetFormattedText());
			_image.Texture = _image1;
			break;
		case 2:
			_bodyText.SetTextAutoSize(new LocString("ftues", "TUTORIAL_FTUE_BODY_2").GetFormattedText());
			_image.Texture = _image2;
			break;
		}
		LocString locString = new LocString("ftues", "COMBAT_BASICS_FTUE_PAGE_COUNT");
		locString.Add("totalPages", 3m);
		locString.Add("currentPage", _currentPage);
		_pageCount.SetTextAutoSize(locString.GetFormattedText());
		_pageTurnTween?.Kill();
		_pageTurnTween = CreateTween().SetParallel();
		_pageTurnTween.TweenProperty(_image, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(0.5f);
		_pageTurnTween.TweenProperty(_image, "position", _imagePosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_imagePosition - _imageAnimOffset);
		_pageTurnTween.TweenProperty(_bodyText, "position", _textPosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_textPosition - _imageAnimOffset);
		_pageTurnTween.TweenProperty(_bodyText, "modulate:a", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear)
			.From(0f);
		_pageTurnTween.TweenProperty(_bodyText, "visible_ratio", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine)
			.From(0f);
	}

	private void ToggleRight(NButton _)
	{
		if (_currentPage == 3)
		{
			_pageTurnTween?.Kill();
			SaveManager.Instance.MarkFtueAsComplete("combat_rules_ftue");
			NCombatRoom.Instance.AddChildSafely(NCombatStartBanner.Create());
			CloseFtue();
			return;
		}
		_currentPage++;
		switch (_currentPage)
		{
		case 2:
			_prevButton.Visible = true;
			_prevButton.Enable();
			_bodyText.SetTextAutoSize(new LocString("ftues", "TUTORIAL_FTUE_BODY_2").GetFormattedText());
			_image.Texture = _image2;
			break;
		case 3:
			_bodyText.SetTextAutoSize(new LocString("ftues", "TUTORIAL_FTUE_BODY_3").GetFormattedText());
			_image.Texture = _image3;
			break;
		}
		LocString locString = new LocString("ftues", "COMBAT_BASICS_FTUE_PAGE_COUNT");
		locString.Add("totalPages", 3m);
		locString.Add("currentPage", _currentPage);
		_pageCount.SetTextAutoSize(locString.GetFormattedText());
		_pageTurnTween?.Kill();
		_pageTurnTween = CreateTween().SetParallel();
		_pageTurnTween.TweenProperty(_image, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(0.5f);
		_pageTurnTween.TweenProperty(_image, "position", _imagePosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_imagePosition + _imageAnimOffset);
		_pageTurnTween.TweenProperty(_bodyText, "position", _textPosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_textPosition + _imageAnimOffset);
		_pageTurnTween.TweenProperty(_bodyText, "modulate:a", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear)
			.From(0f);
		_pageTurnTween.TweenProperty(_bodyText, "visible_ratio", 1f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine)
			.From(0f);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleRight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.Start && args.Count == 0)
		{
			Start();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCombatRulesFtue>(Create());
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleLeft && args.Count == 1)
		{
			ToggleLeft(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleRight && args.Count == 1)
		{
			ToggleRight(VariantUtils.ConvertTo<NButton>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NCombatRulesFtue>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Start)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ToggleLeft)
		{
			return true;
		}
		if (method == MethodName.ToggleRight)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._image1)
		{
			_image1 = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._image2)
		{
			_image2 = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._image3)
		{
			_image3 = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			_prevButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			_nextButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._pageCount)
		{
			_pageCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._bodyText)
		{
			_bodyText = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._header)
		{
			_header = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentPage)
		{
			_currentPage = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._imagePosition)
		{
			_imagePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._textPosition)
		{
			_textPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._pageTurnTween)
		{
			_pageTurnTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._image1)
		{
			value = VariantUtils.CreateFrom(in _image1);
			return true;
		}
		if (name == PropertyName._image2)
		{
			value = VariantUtils.CreateFrom(in _image2);
			return true;
		}
		if (name == PropertyName._image3)
		{
			value = VariantUtils.CreateFrom(in _image3);
			return true;
		}
		if (name == PropertyName._prevButton)
		{
			value = VariantUtils.CreateFrom(in _prevButton);
			return true;
		}
		if (name == PropertyName._nextButton)
		{
			value = VariantUtils.CreateFrom(in _nextButton);
			return true;
		}
		if (name == PropertyName._pageCount)
		{
			value = VariantUtils.CreateFrom(in _pageCount);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._bodyText)
		{
			value = VariantUtils.CreateFrom(in _bodyText);
			return true;
		}
		if (name == PropertyName._header)
		{
			value = VariantUtils.CreateFrom(in _header);
			return true;
		}
		if (name == PropertyName._currentPage)
		{
			value = VariantUtils.CreateFrom(in _currentPage);
			return true;
		}
		if (name == PropertyName._imagePosition)
		{
			value = VariantUtils.CreateFrom(in _imagePosition);
			return true;
		}
		if (name == PropertyName._textPosition)
		{
			value = VariantUtils.CreateFrom(in _textPosition);
			return true;
		}
		if (name == PropertyName._pageTurnTween)
		{
			value = VariantUtils.CreateFrom(in _pageTurnTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image1, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image2, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image3, PropertyHint.ResourceType, "Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._prevButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nextButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pageCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bodyText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._header, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentPage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._imagePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._textPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pageTurnTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._image1, Variant.From(in _image1));
		info.AddProperty(PropertyName._image2, Variant.From(in _image2));
		info.AddProperty(PropertyName._image3, Variant.From(in _image3));
		info.AddProperty(PropertyName._prevButton, Variant.From(in _prevButton));
		info.AddProperty(PropertyName._nextButton, Variant.From(in _nextButton));
		info.AddProperty(PropertyName._pageCount, Variant.From(in _pageCount));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._bodyText, Variant.From(in _bodyText));
		info.AddProperty(PropertyName._header, Variant.From(in _header));
		info.AddProperty(PropertyName._currentPage, Variant.From(in _currentPage));
		info.AddProperty(PropertyName._imagePosition, Variant.From(in _imagePosition));
		info.AddProperty(PropertyName._textPosition, Variant.From(in _textPosition));
		info.AddProperty(PropertyName._pageTurnTween, Variant.From(in _pageTurnTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._image1, out var value))
		{
			_image1 = value.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._image2, out var value2))
		{
			_image2 = value2.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._image3, out var value3))
		{
			_image3 = value3.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._prevButton, out var value4))
		{
			_prevButton = value4.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._nextButton, out var value5))
		{
			_nextButton = value5.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._pageCount, out var value6))
		{
			_pageCount = value6.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value7))
		{
			_image = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._bodyText, out var value8))
		{
			_bodyText = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._header, out var value9))
		{
			_header = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentPage, out var value10))
		{
			_currentPage = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._imagePosition, out var value11))
		{
			_imagePosition = value11.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._textPosition, out var value12))
		{
			_textPosition = value12.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._pageTurnTween, out var value13))
		{
			_pageTurnTween = value13.As<Tween>();
		}
	}
}
