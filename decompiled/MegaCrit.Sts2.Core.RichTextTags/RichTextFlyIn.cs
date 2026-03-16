using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.RichTextTags;

[GlobalClass]
[Tool]
[ScriptPath("res://src/Core/RichTextTags/RichTextFlyIn.cs")]
public class RichTextFlyIn : AbstractMegaRichTextEffect
{
	public new class MethodName : AbstractMegaRichTextEffect.MethodName
	{
		public new static readonly StringName _ProcessCustomFX = "_ProcessCustomFX";
	}

	public new class PropertyName : AbstractMegaRichTextEffect.PropertyName
	{
		public new static readonly StringName Bbcode = "Bbcode";

		public new static readonly StringName bbcode = "bbcode";
	}

	public new class SignalName : AbstractMegaRichTextEffect.SignalName
	{
	}

	private static readonly Variant _xOffsetKey = Variant.From<string>("offset_x");

	private static readonly Variant _yOffsetKey = Variant.From<string>("offset_y");

	public new string bbcode = "fly_in";

	protected override string Bbcode => bbcode;

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		if (Engine.IsEditorHint())
		{
			return false;
		}
		Dictionary env = charFx.Env;
		Vector2 zero = Vector2.Zero;
		if (env.TryGetValue(_xOffsetKey, out var value))
		{
			zero.X = (float)value.AsDouble();
		}
		if (env.TryGetValue(_yOffsetKey, out var value2))
		{
			zero.Y = (float)value2.AsDouble();
		}
		double num = charFx.ElapsedTime * 3.0 - (double)((float)charFx.RelativeIndex * 0.015f);
		Color color = charFx.Color;
		color.A = Mathf.Clamp((float)num, 0f, 1f);
		charFx.Color = color;
		if (ShouldTransformText())
		{
			Vector2 vector = new Vector2(charFx.Transform.X.X, charFx.Transform.Y.Y);
			Vector2 vector2 = zero.Lerp(vector, Ease.QuadOut(color.A));
			Vector2 offset = vector2 - vector;
			charFx.Transform = charFx.Transform.TranslatedLocal(offset);
			charFx.Transform = charFx.Transform.RotatedLocal(Ease.QuadOut(1f - color.A) * Mathf.DegToRad(20f) * ((offset.X < 0f) ? 1f : (-1f)));
		}
		return true;
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ProcessCustomFX)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.bbcode, Variant.From(in bbcode));
	}

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
