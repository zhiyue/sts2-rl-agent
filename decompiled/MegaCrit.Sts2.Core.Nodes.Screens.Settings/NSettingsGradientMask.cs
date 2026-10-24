using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NSettingsGradientMask.cs")]
public class NSettingsGradientMask : TextureRect
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : TextureRect.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnResized' method.
		/// </summary>
		public static readonly StringName OnResized = "OnResized";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : TextureRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_tabContainer' field.
		/// </summary>
		public static readonly StringName _tabContainer = "_tabContainer";

		/// <summary>
		/// Cached name for the '_texture' field.
		/// </summary>
		public static readonly StringName _texture = "_texture";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : TextureRect.SignalName
	{
	}

	private const float _fadeOffset = -8f;

	private const float _fadeSize = 16f;

	private NSettingsTabManager _tabContainer;

	private GradientTexture2D _texture;

	public override void _Ready()
	{
		NSettingsScreen ancestorOfType = this.GetAncestorOfType<NSettingsScreen>();
		_tabContainer = ancestorOfType.GetNode<NSettingsTabManager>("%SettingsTabManager");
		_texture = (GradientTexture2D)base.Texture;
		Connect(Control.SignalName.Resized, Callable.From(OnResized));
		OnResized();
	}

	private void OnResized()
	{
		float num = 1f - (_tabContainer.Position.Y + _tabContainer.Size.Y + -8f + 16f) / base.Size.Y;
		float offset = num + 16f / base.Size.Y;
		_texture.Gradient.SetOffset(2, num);
		_texture.Gradient.SetOffset(3, offset);
		_texture.Gradient.SetColor(2, Colors.White);
		_texture.Gradient.SetColor(3, StsColors.transparentWhite);
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
		list.Add(new MethodInfo(MethodName.OnResized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnResized && args.Count == 0)
		{
			OnResized();
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
		if (method == MethodName.OnResized)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tabContainer)
		{
			_tabContainer = VariantUtils.ConvertTo<NSettingsTabManager>(in value);
			return true;
		}
		if (name == PropertyName._texture)
		{
			_texture = VariantUtils.ConvertTo<GradientTexture2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tabContainer)
		{
			value = VariantUtils.CreateFrom(in _tabContainer);
			return true;
		}
		if (name == PropertyName._texture)
		{
			value = VariantUtils.CreateFrom(in _texture);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tabContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._texture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tabContainer, Variant.From(in _tabContainer));
		info.AddProperty(PropertyName._texture, Variant.From(in _texture));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tabContainer, out var value))
		{
			_tabContainer = value.As<NSettingsTabManager>();
		}
		if (info.TryGetProperty(PropertyName._texture, out var value2))
		{
			_texture = value2.As<GradientTexture2D>();
		}
	}
}
