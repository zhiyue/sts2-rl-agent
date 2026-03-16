using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.RichTextTags;

[GlobalClass]
[Tool]
[ScriptPath("res://src/Core/RichTextTags/RichTextJitter.cs")]
public class RichTextJitter : AbstractMegaRichTextEffect
{
	public new class MethodName : AbstractMegaRichTextEffect.MethodName
	{
		public new static readonly StringName _ProcessCustomFX = "_ProcessCustomFX";
	}

	public new class PropertyName : AbstractMegaRichTextEffect.PropertyName
	{
		public new static readonly StringName Bbcode = "Bbcode";

		public new static readonly StringName bbcode = "bbcode";

		public static readonly StringName _fastNoise = "_fastNoise";
	}

	public new class SignalName : AbstractMegaRichTextEffect.SignalName
	{
	}

	private const float _amplitude = 3f;

	private const float _speed = 600f;

	public new string bbcode = "jitter";

	private FastNoiseLite _fastNoise;

	protected override string Bbcode => bbcode;

	public RichTextJitter()
	{
		_fastNoise = new FastNoiseLite();
		_fastNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_fastNoise.FractalOctaves = 8;
		_fastNoise.FractalGain = 0.8f;
	}

	public override bool _ProcessCustomFX(CharFXTransform charFx)
	{
		Dictionary env = charFx.Env;
		_fastNoise.Seed = (charFx.RelativeIndex + 1) * 131;
		float noise1D = _fastNoise.GetNoise1D((float)charFx.ElapsedTime * 600f);
		_fastNoise.Seed = (charFx.RelativeIndex + 1) * 737;
		float noise1D2 = _fastNoise.GetNoise1D((float)charFx.ElapsedTime * 600f);
		charFx.Offset += new Vector2(noise1D, noise1D2) * 3f;
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
		if (name == PropertyName._fastNoise)
		{
			_fastNoise = VariantUtils.ConvertTo<FastNoiseLite>(in value);
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
		if (name == PropertyName._fastNoise)
		{
			value = VariantUtils.CreateFrom(in _fastNoise);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fastNoise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.bbcode, Variant.From(in bbcode));
		info.AddProperty(PropertyName._fastNoise, Variant.From(in _fastNoise));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.bbcode, out var value))
		{
			bbcode = value.As<string>();
		}
		if (info.TryGetProperty(PropertyName._fastNoise, out var value2))
		{
			_fastNoise = value2.As<FastNoiseLite>();
		}
	}
}
