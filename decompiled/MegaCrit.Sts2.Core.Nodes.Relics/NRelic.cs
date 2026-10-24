using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Relics;

/// <summary>
/// Only contains the visuals (the image) of a relic.
/// Does NOT handle input, that is to be done by a RelicHolder.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Relics/NRelic.cs")]
public class NRelic : Control
{
	public enum IconSize
	{
		Small,
		Large
	}

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Reload' method.
		/// </summary>
		public static readonly StringName Reload = "Reload";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Icon' property.
		/// </summary>
		public static readonly StringName Icon = "Icon";

		/// <summary>
		/// Cached name for the 'Outline' property.
		/// </summary>
		public static readonly StringName Outline = "Outline";

		/// <summary>
		/// Cached name for the '_iconSize' field.
		/// </summary>
		public static readonly StringName _iconSize = "_iconSize";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	public const string relicMatPath = "res://materials/ui/relic_mat.tres";

	private static readonly string _scenePath = SceneHelper.GetScenePath("relics/relic");

	private RelicModel? _model;

	private IconSize _iconSize;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://materials/ui/relic_mat.tres", _scenePath });

	public TextureRect Icon { get; private set; }

	public TextureRect Outline { get; private set; }

	public RelicModel Model
	{
		get
		{
			return _model ?? throw new InvalidOperationException("Model was accessed before it was set.");
		}
		set
		{
			if (_model != value)
			{
				RelicModel model = _model;
				_model = value;
				this.ModelChanged?.Invoke(model, _model);
			}
			Reload();
		}
	}

	public event Action<RelicModel?, RelicModel?>? ModelChanged;

	public static NRelic? Create(RelicModel relic, IconSize iconSize)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRelic nRelic = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRelic>(PackedScene.GenEditState.Disabled);
		nRelic.Name = $"NRelic-{relic.Id}";
		nRelic.Model = relic;
		nRelic._iconSize = iconSize;
		return nRelic;
	}

	public override void _Ready()
	{
		Icon = GetNode<TextureRect>("%Icon");
		Outline = GetNode<TextureRect>("%Outline");
		Reload();
	}

	private void Reload()
	{
		if (IsNodeReady() && _model != null)
		{
			Model.UpdateTexture(Icon);
			switch (_iconSize)
			{
			case IconSize.Small:
				Icon.Texture = Model.Icon;
				Outline.Visible = true;
				Outline.Texture = Model.IconOutline;
				break;
			case IconSize.Large:
				Icon.Texture = Model.BigIcon;
				Outline.Visible = false;
				break;
			default:
				throw new ArgumentOutOfRangeException("_iconSize", _iconSize, null);
			}
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Icon)
		{
			Icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName.Outline)
		{
			Outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconSize)
		{
			_iconSize = VariantUtils.ConvertTo<IconSize>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		TextureRect from;
		if (name == PropertyName.Icon)
		{
			from = Icon;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Outline)
		{
			from = Outline;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._iconSize)
		{
			value = VariantUtils.CreateFrom(in _iconSize);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._iconSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Icon, Variant.From<TextureRect>(Icon));
		info.AddProperty(PropertyName.Outline, Variant.From<TextureRect>(Outline));
		info.AddProperty(PropertyName._iconSize, Variant.From(in _iconSize));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Icon, out var value))
		{
			Icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName.Outline, out var value2))
		{
			Outline = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconSize, out var value3))
		{
			_iconSize = value3.As<IconSize>();
		}
	}
}
