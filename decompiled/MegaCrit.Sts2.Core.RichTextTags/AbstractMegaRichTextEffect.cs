using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.RichTextTags;

[ScriptPath("res://src/Core/RichTextTags/AbstractMegaRichTextEffect.cs")]
public abstract class AbstractMegaRichTextEffect : RichTextEffect
{
	public new class MethodName : RichTextEffect.MethodName
	{
		public static readonly StringName ShouldTransformText = "ShouldTransformText";
	}

	public new class PropertyName : RichTextEffect.PropertyName
	{
		public static readonly StringName bbcode = "bbcode";

		public static readonly StringName Bbcode = "Bbcode";
	}

	public new class SignalName : RichTextEffect.SignalName
	{
	}

	public string bbcode => Bbcode;

	protected abstract string Bbcode { get; }

	protected bool ShouldTransformText()
	{
		if (Engine.IsEditorHint())
		{
			return true;
		}
		return SaveManager.Instance.PrefsSave.TextEffectsEnabled;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.ShouldTransformText, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ShouldTransformText && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(ShouldTransformText());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ShouldTransformText)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.bbcode)
		{
			from = bbcode;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Bbcode)
		{
			from = Bbcode;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
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
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
