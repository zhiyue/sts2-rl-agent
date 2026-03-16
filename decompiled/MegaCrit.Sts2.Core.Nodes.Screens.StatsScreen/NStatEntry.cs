using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NStatEntry.cs")]
public class NStatEntry : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName SetTopText = "SetTopText";

		public static readonly StringName SetBottomText = "SetBottomText";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _tween = "_tween";

		public static readonly StringName _imgUrl = "_imgUrl";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _topLabel = "_topLabel";

		public static readonly StringName _bottomLabel = "_bottomLabel";

		public static readonly StringName _controllerFocusReticle = "_controllerFocusReticle";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private HoverTip? _hoverTip;

	private Tween? _tween;

	private string? _imgUrl;

	private TextureRect _icon;

	private MegaRichTextLabel _topLabel;

	private MegaRichTextLabel _bottomLabel;

	private NSelectionReticle _controllerFocusReticle;

	private static string ScenePath => SceneHelper.GetScenePath("screens/stats_screen/stats_screen_section");

	public static NStatEntry Create(string imgUrl)
	{
		NStatEntry nStatEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NStatEntry>(PackedScene.GenEditState.Disabled);
		nStatEntry._imgUrl = imgUrl;
		return nStatEntry;
	}

	public void SetTopText(string text)
	{
		_topLabel.Visible = true;
		_topLabel.SetTextAutoSize(text);
	}

	public void SetBottomText(string text)
	{
		_bottomLabel.Visible = true;
		_bottomLabel.SetTextAutoSize(text);
	}

	public override void _Ready()
	{
		SetPivotOffset(new Vector2(50f, base.Size.Y * 0.5f));
		_icon = GetNode<TextureRect>("%Icon");
		if (_imgUrl != null)
		{
			_icon.Texture = PreloadManager.Cache.GetTexture2D(_imgUrl);
		}
		_topLabel = GetNode<MegaRichTextLabel>("%TopLabel");
		_bottomLabel = GetNode<MegaRichTextLabel>("%BottomLabel");
		_controllerFocusReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		ConnectSignals();
	}

	public void SetHoverTip(HoverTip hoverTip)
	{
		_hoverTip = hoverTip;
	}

	protected override void OnFocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.05);
		if (_hoverTip.HasValue)
		{
			if (base.GlobalPosition.X < GetViewport().GetVisibleRect().Size.X * 0.4f)
			{
				NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
				nHoverTipSet.GlobalPosition = new Vector2(base.GlobalPosition.X - 392f, base.GlobalPosition.Y);
			}
			else
			{
				NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
				nHoverTipSet.GlobalPosition = new Vector2(base.GlobalPosition.X + 532f, base.GlobalPosition.Y);
			}
		}
		if (NControllerManager.Instance.IsUsingController)
		{
			_controllerFocusReticle.OnSelect();
		}
	}

	protected override void OnUnfocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		if (_hoverTip.HasValue)
		{
			NHoverTipSet.Remove(this);
		}
		_controllerFocusReticle.OnDeselect();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "imgUrl", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTopText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetBottomText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NStatEntry>(Create(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.SetTopText && args.Count == 1)
		{
			SetTopText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetBottomText && args.Count == 1)
		{
			SetBottomText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NStatEntry>(Create(VariantUtils.ConvertTo<string>(in args[0])));
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
		if (method == MethodName.SetTopText)
		{
			return true;
		}
		if (method == MethodName.SetBottomText)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._imgUrl)
		{
			_imgUrl = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._topLabel)
		{
			_topLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			_bottomLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._controllerFocusReticle)
		{
			_controllerFocusReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._imgUrl)
		{
			value = VariantUtils.CreateFrom(in _imgUrl);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._topLabel)
		{
			value = VariantUtils.CreateFrom(in _topLabel);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			value = VariantUtils.CreateFrom(in _bottomLabel);
			return true;
		}
		if (name == PropertyName._controllerFocusReticle)
		{
			value = VariantUtils.CreateFrom(in _controllerFocusReticle);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._imgUrl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._topLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerFocusReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._imgUrl, Variant.From(in _imgUrl));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._topLabel, Variant.From(in _topLabel));
		info.AddProperty(PropertyName._bottomLabel, Variant.From(in _bottomLabel));
		info.AddProperty(PropertyName._controllerFocusReticle, Variant.From(in _controllerFocusReticle));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._imgUrl, out var value2))
		{
			_imgUrl = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._topLabel, out var value4))
		{
			_topLabel = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._bottomLabel, out var value5))
		{
			_bottomLabel = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._controllerFocusReticle, out var value6))
		{
			_controllerFocusReticle = value6.As<NSelectionReticle>();
		}
	}
}
