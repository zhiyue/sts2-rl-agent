using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NAncientBgContainer.cs")]
public class NAncientBgContainer : Control
{
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
		/// Cached name for the 'OnWindowChange' method.
		/// </summary>
		public static readonly StringName OnWindowChange = "OnWindowChange";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_window' field.
		/// </summary>
		public static readonly StringName _window = "_window";

		/// <summary>
		/// Cached name for the '_pos43' field.
		/// </summary>
		public static readonly StringName _pos43 = "_pos43";

		/// <summary>
		/// Cached name for the '_scale43' field.
		/// </summary>
		public static readonly StringName _scale43 = "_scale43";

		/// <summary>
		/// Cached name for the '_pos169' field.
		/// </summary>
		public static readonly StringName _pos169 = "_pos169";

		/// <summary>
		/// Cached name for the '_scale169' field.
		/// </summary>
		public static readonly StringName _scale169 = "_scale169";

		/// <summary>
		/// Cached name for the '_pos219' field.
		/// </summary>
		public static readonly StringName _pos219 = "_pos219";

		/// <summary>
		/// Cached name for the '_scale219' field.
		/// </summary>
		public static readonly StringName _scale219 = "_scale219";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Window _window;

	private const float _ratioMin = 1.3333f;

	private const float _ratioNormal = 1.7777f;

	private const float _ratioMax = 2.3333f;

	private Vector2 _pos43 = new Vector2(-140f, 110f);

	private Vector2 _scale43 = new Vector2(1f, 1f);

	private Vector2 _pos169 = new Vector2(0f, 40f);

	private Vector2 _scale169 = new Vector2(0.89f, 0.89f);

	private Vector2 _pos219 = new Vector2(330f, 40f);

	private Vector2 _scale219 = new Vector2(1f, 1f);

	public override void _Ready()
	{
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
	}

	/// <summary>
	/// A great way to resize bg assets. Interpolates from 16:9 to other aspect ratios.
	/// TODO: Adopt for normal events??
	/// </summary>
	private void OnWindowChange()
	{
		float num = Mathf.Clamp(base.Size.X / base.Size.Y, 1.3333f, 2.3333f);
		base.PivotOffset = base.Size * 0.5f;
		if (num < 1.7777f)
		{
			float weight = Mathf.InverseLerp(1.3333f, 1.7777f, num);
			base.Position = _pos43.Lerp(_pos169, weight);
			base.Scale = _scale43.Lerp(_scale169, weight);
		}
		else
		{
			float weight2 = Mathf.InverseLerp(1.7777f, 2.3333f, num);
			base.Position = _pos169.Lerp(_pos219, weight2);
			base.Scale = _scale169.Lerp(_scale219, weight2);
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
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
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
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName._pos43)
		{
			_pos43 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scale43)
		{
			_scale43 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._pos169)
		{
			_pos169 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scale169)
		{
			_scale169 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._pos219)
		{
			_pos219 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scale219)
		{
			_scale219 = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		if (name == PropertyName._pos43)
		{
			value = VariantUtils.CreateFrom(in _pos43);
			return true;
		}
		if (name == PropertyName._scale43)
		{
			value = VariantUtils.CreateFrom(in _scale43);
			return true;
		}
		if (name == PropertyName._pos169)
		{
			value = VariantUtils.CreateFrom(in _pos169);
			return true;
		}
		if (name == PropertyName._scale169)
		{
			value = VariantUtils.CreateFrom(in _scale169);
			return true;
		}
		if (name == PropertyName._pos219)
		{
			value = VariantUtils.CreateFrom(in _pos219);
			return true;
		}
		if (name == PropertyName._scale219)
		{
			value = VariantUtils.CreateFrom(in _scale219);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._pos43, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._scale43, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._pos169, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._scale169, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._pos219, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._scale219, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName._pos43, Variant.From(in _pos43));
		info.AddProperty(PropertyName._scale43, Variant.From(in _scale43));
		info.AddProperty(PropertyName._pos169, Variant.From(in _pos169));
		info.AddProperty(PropertyName._scale169, Variant.From(in _scale169));
		info.AddProperty(PropertyName._pos219, Variant.From(in _pos219));
		info.AddProperty(PropertyName._scale219, Variant.From(in _scale219));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._window, out var value))
		{
			_window = value.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._pos43, out var value2))
		{
			_pos43 = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scale43, out var value3))
		{
			_scale43 = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._pos169, out var value4))
		{
			_pos169 = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scale169, out var value5))
		{
			_scale169 = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._pos219, out var value6))
		{
			_pos219 = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scale219, out var value7))
		{
			_scale219 = value7.As<Vector2>();
		}
	}
}
