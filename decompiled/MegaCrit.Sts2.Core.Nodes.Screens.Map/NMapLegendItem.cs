using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

/// <summary>
/// A single line on the map legend. Has some hover logic, will eventually require controller
/// support so players can navigate to this and read what each map icon represents.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapLegendItem.cs")]
public class NMapLegendItem : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetMapPointType' method.
		/// </summary>
		public static readonly StringName SetMapPointType = "SetMapPointType";

		/// <summary>
		/// Cached name for the 'SetLocalizedFields' method.
		/// </summary>
		public static readonly StringName SetLocalizedFields = "SetLocalizedFields";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the '_icon' field.
		/// </summary>
		public static readonly StringName _icon = "_icon";

		/// <summary>
		/// Cached name for the '_scaleDownTween' field.
		/// </summary>
		public static readonly StringName _scaleDownTween = "_scaleDownTween";

		/// <summary>
		/// Cached name for the '_pointType' field.
		/// </summary>
		public static readonly StringName _pointType = "_pointType";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private TextureRect _icon;

	private HoverTip _hoverTip;

	private Tween? _scaleDownTween;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.25f;

	private const float _unhoverAnimDur = 0.5f;

	private MapPointType _pointType;

	public override void _Ready()
	{
		ConnectSignals();
		SetLocalizedFields(base.Name);
		SetMapPointType(base.Name);
		_icon = GetNode<TextureRect>("Icon");
	}

	/// <summary>
	/// Helper method which grabs the Node's name and finds the appropriate map point type.
	/// </summary>
	/// <param name="name">Name of this Godot node.</param>
	private void SetMapPointType(string name)
	{
		_pointType = name switch
		{
			"UnknownLegendItem" => MapPointType.Unknown, 
			"MerchantLegendItem" => MapPointType.Shop, 
			"TreasureLegendItem" => MapPointType.Treasure, 
			"RestSiteLegendItem" => MapPointType.RestSite, 
			"EnemyLegendItem" => MapPointType.Monster, 
			"EliteLegendItem" => MapPointType.Elite, 
			_ => throw new ArgumentOutOfRangeException("Unknown Node " + name + " when setting MapLegend localization."), 
		};
	}

	/// <summary>
	/// Helper method which grabs the Node's name and then finds the appropriate
	/// LocString for this LegendItem's label and HoverTip text fields.
	/// </summary>
	/// <param name="name">Name of this Godot node.</param>
	private void SetLocalizedFields(string name)
	{
		string text = name switch
		{
			"UnknownLegendItem" => "LEGEND_UNKNOWN", 
			"MerchantLegendItem" => "LEGEND_MERCHANT", 
			"TreasureLegendItem" => "LEGEND_TREASURE", 
			"RestSiteLegendItem" => "LEGEND_REST", 
			"EnemyLegendItem" => "LEGEND_ENEMY", 
			"EliteLegendItem" => "LEGEND_ELITE", 
			_ => throw new ArgumentOutOfRangeException("Unknown Node " + name + " when setting MapLegend localization."), 
		};
		GetNode<MegaLabel>("MegaLabel").SetTextAutoSize(new LocString("map", text + ".title").GetFormattedText());
		_hoverTip = new HoverTip(new LocString("map", text + ".hoverTip.title"), new LocString("map", text + ".hoverTip.description"));
	}

	protected override void OnFocus()
	{
		_scaleDownTween?.Kill();
		_icon.Scale = _hoverScale;
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		Control parent = GetParent<Control>();
		nHoverTipSet?.SetGlobalPosition(parent.GlobalPosition + new Vector2(parent.Size.X - nHoverTipSet.Size.X, parent.Size.Y));
		NMapScreen.Instance.HighlightPointType(_pointType);
	}

	protected override void OnUnfocus()
	{
		_scaleDownTween = CreateTween().SetParallel();
		_scaleDownTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).From(_hoverScale).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		NMapScreen.Instance.HighlightPointType(MapPointType.Unassigned);
		NHoverTipSet.Remove(this);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMapPointType, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetLocalizedFields, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMapPointType && args.Count == 1)
		{
			SetMapPointType(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLocalizedFields && args.Count == 1)
		{
			SetLocalizedFields(VariantUtils.ConvertTo<string>(in args[0]));
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetMapPointType)
		{
			return true;
		}
		if (method == MethodName.SetLocalizedFields)
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._scaleDownTween)
		{
			_scaleDownTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._pointType)
		{
			_pointType = VariantUtils.ConvertTo<MapPointType>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._scaleDownTween)
		{
			value = VariantUtils.CreateFrom(in _scaleDownTween);
			return true;
		}
		if (name == PropertyName._pointType)
		{
			value = VariantUtils.CreateFrom(in _pointType);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleDownTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._pointType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._scaleDownTween, Variant.From(in _scaleDownTween));
		info.AddProperty(PropertyName._pointType, Variant.From(in _pointType));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._scaleDownTween, out var value2))
		{
			_scaleDownTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._pointType, out var value3))
		{
			_pointType = value3.As<MapPointType>();
		}
	}
}
