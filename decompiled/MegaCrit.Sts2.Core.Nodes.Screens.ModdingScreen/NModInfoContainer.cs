using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

[ScriptPath("res://src/Core/Nodes/Screens/ModdingScreen/NModInfoContainer.cs")]
public class NModInfoContainer : Control
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
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_title' field.
		/// </summary>
		public static readonly StringName _title = "_title";

		/// <summary>
		/// Cached name for the '_image' field.
		/// </summary>
		public static readonly StringName _image = "_image";

		/// <summary>
		/// Cached name for the '_description' field.
		/// </summary>
		public static readonly StringName _description = "_description";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private MegaRichTextLabel _title;

	private TextureRect _image;

	private MegaRichTextLabel _description;

	public override void _Ready()
	{
		_title = GetNode<MegaRichTextLabel>("ModTitle");
		_image = GetNode<TextureRect>("ModImage");
		_description = GetNode<MegaRichTextLabel>("ModDescription");
		_title.Text = "";
		_image.Texture = null;
		_description.Text = "";
	}

	public void Fill(Mod mod)
	{
		_title.Text = mod.manifest?.name ?? "<No Name>";
		string path = "res://" + mod.manifest?.id + "/mod_image.png";
		if (ResourceLoader.Exists(path))
		{
			_image.Texture = PreloadManager.Cache.GetAsset<Texture2D>(path);
		}
		else
		{
			_image.Texture = null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(21, 1, stringBuilder2);
		handler.AppendLiteral("[gold]Author[/gold]: ");
		handler.AppendFormatted(mod.manifest?.author ?? "unknown");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
		handler.AppendLiteral("[gold]Version[/gold]: ");
		handler.AppendFormatted(mod.manifest?.version ?? "unknown");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder.AppendLine();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
		handler.AppendFormatted(mod.manifest?.description ?? "No description");
		stringBuilder5.AppendLine(ref handler);
		List<LocString>? errors = mod.errors;
		if (errors != null && errors.Count > 0)
		{
			stringBuilder.AppendLine();
			foreach (LocString error in mod.errors)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
				handler.AppendLiteral("[red]");
				handler.AppendFormatted(error.GetFormattedText());
				handler.AppendLiteral("[/red]");
				stringBuilder6.AppendLine(ref handler);
			}
		}
		_description.Text = stringBuilder.ToString();
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
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._title)
		{
			_title = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._title)
		{
			value = VariantUtils.CreateFrom(in _title);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._title, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._title, Variant.From(in _title));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._title, out var value))
		{
			_title = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value2))
		{
			_image = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value3))
		{
			_description = value3.As<MegaRichTextLabel>();
		}
	}
}
