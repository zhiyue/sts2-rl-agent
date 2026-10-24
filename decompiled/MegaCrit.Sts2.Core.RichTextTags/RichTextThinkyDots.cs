using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.RichTextTags;

[GlobalClass]
[Tool]
[ScriptPath("res://src/Core/RichTextTags/RichTextThinkyDots.cs")]
public class RichTextThinkyDots : AbstractMegaRichTextEffect
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : AbstractMegaRichTextEffect.MethodName
	{
		/// <summary>
		/// Cached name for the '_ProcessCustomFX' method.
		/// </summary>
		public new static readonly StringName _ProcessCustomFX = "_ProcessCustomFX";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : AbstractMegaRichTextEffect.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Bbcode' property.
		/// </summary>
		public new static readonly StringName Bbcode = "Bbcode";

		/// <summary>
		/// Cached name for the 'bbcode' field.
		/// </summary>
		public new static readonly StringName bbcode = "bbcode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : AbstractMegaRichTextEffect.SignalName
	{
	}

	private const float _amplitude = 1.5f;

	private const float _frequency = 0.4f;

	private const float _speed = 1f;

	private const float _spacing = 4f;

	public new string bbcode = "thinky_dots";

	protected override string Bbcode => bbcode;

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		if (!ShouldTransformText())
		{
			return false;
		}
		Dictionary env = charFx.Env;
		charFx.Offset = Vector2.Zero;
		float val = (float)(charFx.ElapsedTime * 1.0 - (double)((float)charFx.RelativeIndex * 0.1f));
		val = Math.Max(val, 0f);
		float num = val % 4.4f;
		float a = ((!(num < 0.4f)) ? 0f : (1.5f * Mathf.Sin(num / 0.4f * (float)Math.PI)));
		charFx.Offset += new Vector2(0f, 0f - Mathf.Max(a, 0f));
		if (env.TryGetValue(RichTextUtil.colorKey, out var value))
		{
			charFx.Color = (Color)value;
		}
		charFx.Visible = !env.ContainsKey(RichTextUtil.visibleKey) || (bool)env[RichTextUtil.visibleKey];
		return true;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._ProcessCustomFX, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "charFx", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("CharFXTransform"), exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ProcessCustomFX && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(_ProcessCustomFX(VariantUtils.ConvertTo<CharFXTransform>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ProcessCustomFX)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.bbcode)
		{
			bbcode = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Bbcode)
		{
			value = VariantUtils.CreateFrom<string>(Bbcode);
			return true;
		}
		if (name == PropertyName.bbcode)
		{
			value = VariantUtils.CreateFrom(in bbcode);
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
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.bbcode, Variant.From(in bbcode));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.bbcode, out var value))
		{
			bbcode = value.As<string>();
		}
	}
}
