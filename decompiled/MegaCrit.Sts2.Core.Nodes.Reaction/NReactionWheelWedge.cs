using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Reaction;

[ScriptPath("res://src/Core/Nodes/Reaction/NReactionWheelWedge.cs")]
public class NReactionWheelWedge : TextureRect
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
		/// Cached name for the 'OnSelected' method.
		/// </summary>
		public static readonly StringName OnSelected = "OnSelected";

		/// <summary>
		/// Cached name for the 'OnDeselected' method.
		/// </summary>
		public static readonly StringName OnDeselected = "OnDeselected";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : TextureRect.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Reaction' property.
		/// </summary>
		public static readonly StringName Reaction = "Reaction";

		/// <summary>
		/// Cached name for the '_textureRect' field.
		/// </summary>
		public static readonly StringName _textureRect = "_textureRect";

		/// <summary>
		/// Cached name for the '_normal' field.
		/// </summary>
		public static readonly StringName _normal = "_normal";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_defaultPosition' field.
		/// </summary>
		public static readonly StringName _defaultPosition = "_defaultPosition";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : TextureRect.SignalName
	{
	}

	private static readonly Color _defaultColor = new Color("e0f9ff40");

	private static readonly Color _selectedColor = new Color("c2f3ffc0");

	private TextureRect _textureRect;

	private Vector2 _normal;

	private Tween? _tween;

	private Vector2 _defaultPosition;

	public Texture2D Reaction => _textureRect.Texture;

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_defaultPosition = base.Position;
	}

	public void OnSelected()
	{
		Vector2 vector = Vector2.Right.Rotated(base.Rotation);
		_tween?.Kill();
		_tween = CreateTween();
		_tween.SetParallel();
		_tween.TweenProperty(this, "position", _defaultPosition + vector * 25f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "self_modulate", _selectedColor, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void OnDeselected()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.SetParallel();
		_tween.TweenProperty(this, "position", _defaultPosition, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "self_modulate", _defaultColor, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeselected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnSelected && args.Count == 0)
		{
			OnSelected();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeselected && args.Count == 0)
		{
			OnDeselected();
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
		if (method == MethodName.OnSelected)
		{
			return true;
		}
		if (method == MethodName.OnDeselected)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._textureRect)
		{
			_textureRect = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._normal)
		{
			_normal = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._defaultPosition)
		{
			_defaultPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Reaction)
		{
			value = VariantUtils.CreateFrom<Texture2D>(Reaction);
			return true;
		}
		if (name == PropertyName._textureRect)
		{
			value = VariantUtils.CreateFrom(in _textureRect);
			return true;
		}
		if (name == PropertyName._normal)
		{
			value = VariantUtils.CreateFrom(in _normal);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._defaultPosition)
		{
			value = VariantUtils.CreateFrom(in _defaultPosition);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._normal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._defaultPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Reaction, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._textureRect, Variant.From(in _textureRect));
		info.AddProperty(PropertyName._normal, Variant.From(in _normal));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._defaultPosition, Variant.From(in _defaultPosition));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._textureRect, out var value))
		{
			_textureRect = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._normal, out var value2))
		{
			_normal = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._defaultPosition, out var value4))
		{
			_defaultPosition = value4.As<Vector2>();
		}
	}
}
