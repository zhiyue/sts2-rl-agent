using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/LocalizedTexture.cs")]
public class LocalizedTexture : Resource
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Resource.MethodName
	{
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Resource.PropertyName
	{
		/// <summary>
		/// Cached name for the '_textures' field.
		/// </summary>
		public static readonly StringName _textures = "_textures";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Resource.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Godot.Collections.Dictionary<string, Texture2D> _textures = new Godot.Collections.Dictionary<string, Texture2D>();

	public bool TryGetTexture(out Texture2D? texture)
	{
		texture = null;
		if (SaveManager.Instance.SettingsSave == null)
		{
			return false;
		}
		string language = SaveManager.Instance.SettingsSave.Language;
		if (string.IsNullOrEmpty(language))
		{
			return false;
		}
		if (!_textures.TryGetValue(language, out Texture2D value))
		{
			return false;
		}
		texture = value;
		return true;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._textures)
		{
			_textures = VariantUtils.ConvertToDictionary<string, Texture2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._textures)
		{
			value = VariantUtils.CreateFromDictionary(_textures);
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
		list.Add(new PropertyInfo(Variant.Type.Dictionary, PropertyName._textures, PropertyHint.TypeString, "4/0:;24/17:Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._textures, Variant.CreateFrom(_textures));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._textures, out var value))
		{
			_textures = value.AsGodotDictionary<string, Texture2D>();
		}
	}
}
