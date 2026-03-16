using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NContinueRunInfo.cs")]
public class NContinueRunInfo : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName AnimShow = "AnimShow";

		public static readonly StringName AnimHide = "AnimHide";

		public static readonly StringName ShowError = "ShowError";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName HasResult = "HasResult";

		public static readonly StringName _visTween = "_visTween";

		public static readonly StringName _initPosition = "_initPosition";

		public static readonly StringName _runInfoContainer = "_runInfoContainer";

		public static readonly StringName _errorContainer = "_errorContainer";

		public static readonly StringName _dateLabel = "_dateLabel";

		public static readonly StringName _goldLabel = "_goldLabel";

		public static readonly StringName _healthLabel = "_healthLabel";

		public static readonly StringName _progressLabel = "_progressLabel";

		public static readonly StringName _ascensionLabel = "_ascensionLabel";

		public static readonly StringName _charIcon = "_charIcon";

		public static readonly StringName _isShown = "_isShown";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Tween? _visTween;

	private Vector2 _initPosition;

	private Control _runInfoContainer;

	private Control _errorContainer;

	private MegaRichTextLabel _dateLabel;

	private MegaRichTextLabel _goldLabel;

	private MegaRichTextLabel _healthLabel;

	private MegaRichTextLabel _progressLabel;

	private MegaRichTextLabel _ascensionLabel;

	private TextureRect _charIcon;

	private bool _isShown;

	public bool HasResult { get; private set; }

	public override void _Ready()
	{
		_initPosition = base.Position;
		base.Modulate = StsColors.transparentWhite;
		_runInfoContainer = GetNode<VBoxContainer>("%RunInfoContainer");
		_errorContainer = GetNode<Control>("%ErrorContainer");
		_dateLabel = GetNode<MegaRichTextLabel>("%DateLabel");
		_goldLabel = GetNode<MegaRichTextLabel>("%GoldLabel");
		_healthLabel = GetNode<MegaRichTextLabel>("%HealthLabel");
		_progressLabel = GetNode<MegaRichTextLabel>("%ProgressLabel");
		_charIcon = GetNode<TextureRect>("%CharacterIcon");
		_ascensionLabel = GetNode<MegaRichTextLabel>("%AscensionLabel");
	}

	public void AnimShow()
	{
		_visTween?.Kill();
		_visTween = CreateTween().SetParallel();
		_visTween.TweenProperty(this, "position", _initPosition + new Vector2(0f, -20f), 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_visTween.TweenProperty(this, "modulate:a", 1f, 0.20000000298023224);
		_isShown = true;
	}

	public void AnimHide()
	{
		if (_isShown)
		{
			_visTween?.Kill();
			_visTween = CreateTween().SetParallel();
			_visTween.TweenProperty(this, "position", _initPosition, 0.20000000298023224);
			_visTween.TweenProperty(this, "modulate:a", 0f, 0.20000000298023224);
			_isShown = false;
		}
	}

	public void SetResult(ReadSaveResult<SerializableRun>? result)
	{
		if (result != null && result.Success && result.SaveData != null)
		{
			ShowInfo(result.SaveData);
		}
		else if (result != null)
		{
			ShowError();
		}
		HasResult = result != null;
	}

	private void ShowInfo(SerializableRun save)
	{
		_errorContainer.Visible = false;
		_runInfoContainer.Visible = true;
		DateTimeFormatInfo dateTimeFormat = LocManager.Instance.CultureInfo.DateTimeFormat;
		DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(save.SaveTime).UtcDateTime, TimeZoneInfo.Local);
		string rawText = new LocString("main_menu_ui", "CONTINUE_RUN_INFO.savedTimeFormat").GetRawText();
		string variable = dateTime.ToString(rawText, dateTimeFormat);
		LocString locString = new LocString("main_menu_ui", "CONTINUE_RUN_INFO.saved");
		locString.Add("LastSavedTime", variable);
		_dateLabel.Text = locString.GetFormattedText();
		if (save.Ascension > 0)
		{
			LocString locString2 = new LocString("main_menu_ui", "CONTINUE_RUN_INFO.ascension");
			_ascensionLabel.Text = $"{locString2.GetFormattedText()} {save.Ascension}";
		}
		else
		{
			_ascensionLabel.Visible = false;
		}
		ActModel byId = ModelDb.GetById<ActModel>(save.Acts[save.CurrentActIndex].Id);
		SerializablePlayer serializablePlayer = save.Players[0];
		_charIcon.Texture = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).IconTexture;
		string formattedText = byId.Title.GetFormattedText();
		string formattedText2 = new LocString("main_menu_ui", "CONTINUE_RUN_INFO.floor").GetFormattedText();
		int num = save.VisitedMapCoords.Count;
		for (int i = 0; i < save.CurrentActIndex; i++)
		{
			num += ModelDb.GetById<ActModel>(save.Acts[i].Id).GetNumberOfFloors(save.Players.Count > 1);
		}
		_progressLabel.Text = $"{formattedText} [blue]- {formattedText2} {num}[/blue]";
		_healthLabel.Text = $"[red]{serializablePlayer.CurrentHp}/{serializablePlayer.MaxHp}[/red]";
		_goldLabel.Text = $"[gold]{serializablePlayer.Gold}[/gold]";
	}

	private void ShowError()
	{
		_runInfoContainer.Visible = false;
		_errorContainer.Visible = true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowError, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.AnimShow && args.Count == 0)
		{
			AnimShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHide && args.Count == 0)
		{
			AnimHide();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowError && args.Count == 0)
		{
			ShowError();
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
		if (method == MethodName.AnimShow)
		{
			return true;
		}
		if (method == MethodName.AnimHide)
		{
			return true;
		}
		if (method == MethodName.ShowError)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.HasResult)
		{
			HasResult = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._visTween)
		{
			_visTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._initPosition)
		{
			_initPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._runInfoContainer)
		{
			_runInfoContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._errorContainer)
		{
			_errorContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			_dateLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._goldLabel)
		{
			_goldLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._healthLabel)
		{
			_healthLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._progressLabel)
		{
			_progressLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			_ascensionLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._charIcon)
		{
			_charIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._isShown)
		{
			_isShown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.HasResult)
		{
			value = VariantUtils.CreateFrom<bool>(HasResult);
			return true;
		}
		if (name == PropertyName._visTween)
		{
			value = VariantUtils.CreateFrom(in _visTween);
			return true;
		}
		if (name == PropertyName._initPosition)
		{
			value = VariantUtils.CreateFrom(in _initPosition);
			return true;
		}
		if (name == PropertyName._runInfoContainer)
		{
			value = VariantUtils.CreateFrom(in _runInfoContainer);
			return true;
		}
		if (name == PropertyName._errorContainer)
		{
			value = VariantUtils.CreateFrom(in _errorContainer);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			value = VariantUtils.CreateFrom(in _dateLabel);
			return true;
		}
		if (name == PropertyName._goldLabel)
		{
			value = VariantUtils.CreateFrom(in _goldLabel);
			return true;
		}
		if (name == PropertyName._healthLabel)
		{
			value = VariantUtils.CreateFrom(in _healthLabel);
			return true;
		}
		if (name == PropertyName._progressLabel)
		{
			value = VariantUtils.CreateFrom(in _progressLabel);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			value = VariantUtils.CreateFrom(in _ascensionLabel);
			return true;
		}
		if (name == PropertyName._charIcon)
		{
			value = VariantUtils.CreateFrom(in _charIcon);
			return true;
		}
		if (name == PropertyName._isShown)
		{
			value = VariantUtils.CreateFrom(in _isShown);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._initPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runInfoContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._errorContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._goldLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._healthLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._progressLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._charIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isShown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasResult, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.HasResult, Variant.From<bool>(HasResult));
		info.AddProperty(PropertyName._visTween, Variant.From(in _visTween));
		info.AddProperty(PropertyName._initPosition, Variant.From(in _initPosition));
		info.AddProperty(PropertyName._runInfoContainer, Variant.From(in _runInfoContainer));
		info.AddProperty(PropertyName._errorContainer, Variant.From(in _errorContainer));
		info.AddProperty(PropertyName._dateLabel, Variant.From(in _dateLabel));
		info.AddProperty(PropertyName._goldLabel, Variant.From(in _goldLabel));
		info.AddProperty(PropertyName._healthLabel, Variant.From(in _healthLabel));
		info.AddProperty(PropertyName._progressLabel, Variant.From(in _progressLabel));
		info.AddProperty(PropertyName._ascensionLabel, Variant.From(in _ascensionLabel));
		info.AddProperty(PropertyName._charIcon, Variant.From(in _charIcon));
		info.AddProperty(PropertyName._isShown, Variant.From(in _isShown));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.HasResult, out var value))
		{
			HasResult = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._visTween, out var value2))
		{
			_visTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._initPosition, out var value3))
		{
			_initPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._runInfoContainer, out var value4))
		{
			_runInfoContainer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._errorContainer, out var value5))
		{
			_errorContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dateLabel, out var value6))
		{
			_dateLabel = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._goldLabel, out var value7))
		{
			_goldLabel = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._healthLabel, out var value8))
		{
			_healthLabel = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._progressLabel, out var value9))
		{
			_progressLabel = value9.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLabel, out var value10))
		{
			_ascensionLabel = value10.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._charIcon, out var value11))
		{
			_charIcon = value11.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._isShown, out var value12))
		{
			_isShown = value12.As<bool>();
		}
	}
}
