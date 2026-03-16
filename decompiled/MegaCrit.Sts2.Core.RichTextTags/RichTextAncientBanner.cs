using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.RichTextTags;

[GlobalClass]
[Tool]
[ScriptPath("res://src/Core/RichTextTags/RichTextAncientBanner.cs")]
public class RichTextAncientBanner : AbstractMegaRichTextEffect
{
	public new class MethodName : AbstractMegaRichTextEffect.MethodName
	{
		public new static readonly StringName _ProcessCustomFX = "_ProcessCustomFX";
	}

	public new class PropertyName : AbstractMegaRichTextEffect.PropertyName
	{
		public static readonly StringName Rotation = "Rotation";

		public static readonly StringName Spacing = "Spacing";

		public static readonly StringName CenterCharacter = "CenterCharacter";

		public new static readonly StringName Bbcode = "Bbcode";

		public new static readonly StringName bbcode = "bbcode";
	}

	public new class SignalName : AbstractMegaRichTextEffect.SignalName
	{
	}

	public new string bbcode = "ancient_banner";

	public float Rotation { get; set; }

	public float Spacing { get; set; }

	public float CenterCharacter { get; set; }

	protected override string Bbcode => bbcode;

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		if (ShouldTransformText())
		{
			float num = (float)charFx.RelativeIndex + 0.5f - CenterCharacter;
			Transform2D transform = charFx.Transform;
			Vector2 x = charFx.Transform.X;
			x.X = Rotation;
			transform.X = x;
			charFx.Transform = transform;
			transform = charFx.Transform;
			x = charFx.Transform.Origin;
			x.X = charFx.Transform.Origin.X + num * Spacing;
			transform.Origin = x;
			charFx.Transform = transform;
		}
		else
		{
			double num2 = charFx.ElapsedTime * 3.0 - (double)((float)charFx.RelativeIndex * 0.015f);
			Color color = charFx.Color;
			color.A = Mathf.Clamp((float)num2, 0f, 1f);
			charFx.Color = color;
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
		if (name == PropertyName.Rotation)
		{
			Rotation = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName.Spacing)
		{
			Spacing = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName.CenterCharacter)
		{
			CenterCharacter = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
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
		float from;
		if (name == PropertyName.Rotation)
		{
			from = Rotation;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Spacing)
		{
			from = Spacing;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CenterCharacter)
		{
			from = CenterCharacter;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.Rotation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.Spacing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.CenterCharacter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Bbcode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Rotation, Variant.From<float>(Rotation));
		info.AddProperty(PropertyName.Spacing, Variant.From<float>(Spacing));
		info.AddProperty(PropertyName.CenterCharacter, Variant.From<float>(CenterCharacter));
		info.AddProperty(PropertyName.bbcode, Variant.From(in bbcode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Rotation, out var value))
		{
			Rotation = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName.Spacing, out var value2))
		{
			Spacing = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName.CenterCharacter, out var value3))
		{
			CenterCharacter = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName.bbcode, out var value4))
		{
			bbcode = value4.As<string>();
		}
	}
}
