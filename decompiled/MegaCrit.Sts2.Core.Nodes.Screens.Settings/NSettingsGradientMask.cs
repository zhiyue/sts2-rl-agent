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
	public new class MethodName : TextureRect.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnResized = "OnResized";
	}

	public new class PropertyName : TextureRect.PropertyName
	{
		public static readonly StringName _tabContainer = "_tabContainer";

		public static readonly StringName _texture = "_texture";
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnResized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnResized && args.Count == 0)
		{
			OnResized();
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
		if (method == MethodName.OnResized)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tabContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._texture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tabContainer, Variant.From(in _tabContainer));
		info.AddProperty(PropertyName._texture, Variant.From(in _texture));
	}

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
