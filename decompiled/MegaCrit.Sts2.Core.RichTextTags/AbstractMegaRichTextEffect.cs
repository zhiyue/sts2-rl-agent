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
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : RichTextEffect.MethodName
	{
		/// <summary>
		/// Cached name for the 'ShouldTransformText' method.
		/// </summary>
		public static readonly StringName ShouldTransformText = "ShouldTransformText";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : RichTextEffect.PropertyName
	{
		/// <summary>
		/// Cached name for the 'bbcode' property.
		/// </summary>
		public static readonly StringName bbcode = "bbcode";

		/// <summary>
		/// Cached name for the 'Bbcode' property.
		/// </summary>
		public static readonly StringName Bbcode = "Bbcode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.ShouldTransformText, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ShouldTransformText)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
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
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
