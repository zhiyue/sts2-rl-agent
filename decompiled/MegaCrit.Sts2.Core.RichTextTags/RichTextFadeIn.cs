using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.RichTextTags;

[GlobalClass]
[Tool]
[ScriptPath("res://src/Core/RichTextTags/RichTextFadeIn.cs")]
public class RichTextFadeIn : AbstractMegaRichTextEffect
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

	private static readonly Variant _speedKey = Variant.From<string>("speed");

	private static readonly Variant _tickKey = Variant.From<string>("tick");

	public new string bbcode = "fade_in";

	protected override string Bbcode => bbcode;

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		Dictionary env = charFx.Env;
		charFx.Offset = Vector2.Zero;
		Variant value;
		double num = ((!env.TryGetValue(_speedKey, out value)) ? 4.0 : value.AsDouble());
		Variant value2;
		double num2 = ((!env.TryGetValue(_tickKey, out value2)) ? 0.009999999776482582 : value2.AsDouble());
		double num3 = charFx.ElapsedTime * num - (double)charFx.RelativeIndex * num2;
		Color color = charFx.Color;
		color.A = Mathf.Clamp((float)num3, 0f, 1f);
		charFx.Color = color;
		if (env.TryGetValue(RichTextUtil.visibleKey, out var value3))
		{
			charFx.Visible = value3.AsBool();
		}
		else
		{
			charFx.Visible = true;
		}
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
