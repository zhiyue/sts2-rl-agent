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
[ScriptPath("res://src/Core/RichTextTags/RichTextSine.cs")]
public class RichTextSine : AbstractMegaRichTextEffect
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

	private const float _amplitude = 0.8f;

	private const float _frequency = 0.5f;

	private const float _speed = 1.5f;

	public new string bbcode = "sine";

	protected override string Bbcode => bbcode;

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		if (!ShouldTransformText())
		{
			return false;
		}
		Dictionary env = charFx.Env;
		float num = (float)(charFx.ElapsedTime * 1.5 + (double)((float)charFx.RelativeIndex * 0.1f));
		float y = 0.8f * Mathf.Sin(num * (float)Math.PI * 2f * 0.5f);
		charFx.Offset += new Vector2(0f, y);
		if (env.TryGetValue(RichTextUtil.colorKey, out var value))
		{
			charFx.Color = (Color)value;
		}
		charFx.Visible = !env.ContainsKey(RichTextUtil.visibleKey) || (bool)env[RichTextUtil.visibleKey];
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
