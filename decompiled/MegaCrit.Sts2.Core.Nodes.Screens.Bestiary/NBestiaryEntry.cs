using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

/// <summary>
/// The buttons in the monster list in the Bestiary entry. A text button.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiaryEntry.cs")]
public class NBestiaryEntry : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'HoveredSfx' property.
		/// </summary>
		public new static readonly StringName HoveredSfx = "HoveredSfx";

		/// <summary>
		/// Cached name for the 'IsDiscovered' property.
		/// </summary>
		public static readonly StringName IsDiscovered = "IsDiscovered";

		/// <summary>
		/// Cached name for the 'IsUnderConstruction' property.
		/// </summary>
		public static readonly StringName IsUnderConstruction = "IsUnderConstruction";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_highlight' field.
		/// </summary>
		public static readonly StringName _highlight = "_highlight";

		/// <summary>
		/// Cached name for the '_underConstructionIcon' field.
		/// </summary>
		public static readonly StringName _underConstructionIcon = "_underConstructionIcon";

		/// <summary>
		/// Cached name for the '_defaultColor' field.
		/// </summary>
		public static readonly StringName _defaultColor = "_defaultColor";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private MegaRichTextLabel _label;

	private Control _highlight;

	private TextureRect _underConstructionIcon;

	private Color _defaultColor;

	private Tween? _tween;

	protected override string? HoveredSfx => null;

	private static string ScenePath => SceneHelper.GetScenePath("screens/bestiary/bestiary_entry");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public BestiaryEntry Entry { get; private set; }

	public bool IsDiscovered { get; private set; }

	public bool IsUnderConstruction => _underConstructionIcon.Visible;

	public static NBestiaryEntry Create(BestiaryEntry entry, bool isDiscovered)
	{
		NBestiaryEntry nBestiaryEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NBestiaryEntry>(PackedScene.GenEditState.Disabled);
		nBestiaryEntry.Entry = entry;
		nBestiaryEntry.IsDiscovered = isDiscovered;
		return nBestiaryEntry;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_label = GetNode<MegaRichTextLabel>("%Label");
		_highlight = GetNode<Control>("%Highlight");
		_underConstructionIcon = GetNode<TextureRect>("%UnderConstructionIcon");
		if (!IsDiscovered)
		{
			Disable();
			_label.Text = new LocString("bestiary", "UNSEEN.monsterName").GetRawText();
			_label.SelfModulate = StsColors.gray;
			_underConstructionIcon.Visible = false;
		}
		else
		{
			_label.Text = Entry.GetEntryTitle();
			_defaultColor = Entry.roomType switch
			{
				RoomType.Boss => StsColors.red, 
				RoomType.Elite => StsColors.purple, 
				_ => StsColors.cream, 
			};
			_label.SelfModulate = _defaultColor;
			_underConstructionIcon.Visible = false;
		}
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_label, "scale", Vector2.One * 1.1f, 0.05);
		if (NControllerManager.Instance.IsUsingController)
		{
			_highlight.Visible = true;
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_label, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_highlight.Visible = false;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsDiscovered)
		{
			IsDiscovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			_highlight = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._underConstructionIcon)
		{
			_underConstructionIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._defaultColor)
		{
			_defaultColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.HoveredSfx)
		{
			value = VariantUtils.CreateFrom<string>(HoveredSfx);
			return true;
		}
		bool from;
		if (name == PropertyName.IsDiscovered)
		{
			from = IsDiscovered;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsUnderConstruction)
		{
			from = IsUnderConstruction;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._highlight)
		{
			value = VariantUtils.CreateFrom(in _highlight);
			return true;
		}
		if (name == PropertyName._underConstructionIcon)
		{
			value = VariantUtils.CreateFrom(in _underConstructionIcon);
			return true;
		}
		if (name == PropertyName._defaultColor)
		{
			value = VariantUtils.CreateFrom(in _defaultColor);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.HoveredSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._underConstructionIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._defaultColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsDiscovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUnderConstruction, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsDiscovered, Variant.From<bool>(IsDiscovered));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._highlight, Variant.From(in _highlight));
		info.AddProperty(PropertyName._underConstructionIcon, Variant.From(in _underConstructionIcon));
		info.AddProperty(PropertyName._defaultColor, Variant.From(in _defaultColor));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsDiscovered, out var value))
		{
			IsDiscovered = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._highlight, out var value3))
		{
			_highlight = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._underConstructionIcon, out var value4))
		{
			_underConstructionIcon = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._defaultColor, out var value5))
		{
			_defaultColor = value5.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
