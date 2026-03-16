using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace MegaCrit.Sts2.addons.mega_text;

[Tool]
[ScriptPath("res://addons/mega_text/MegaLabel.cs")]
public class MegaLabel : Label
{
	public new class MethodName : Label.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshFont = "RefreshFont";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName SetTextAutoSize = "SetTextAutoSize";

		public static readonly StringName SetFontSize = "SetFontSize";

		public static readonly StringName AdjustFontSize = "AdjustFontSize";
	}

	public new class PropertyName : Label.PropertyName
	{
		public static readonly StringName AutoSizeEnabled = "AutoSizeEnabled";

		public static readonly StringName MinFontSize = "MinFontSize";

		public static readonly StringName MaxFontSize = "MaxFontSize";

		public static readonly StringName _autoSizeEnabled = "_autoSizeEnabled";

		public static readonly StringName _minFontSize = "_minFontSize";

		public static readonly StringName _maxFontSize = "_maxFontSize";

		public static readonly StringName _lastSetSize = "_lastSetSize";

		public static readonly StringName _lastAdjustedSize = "_lastAdjustedSize";
	}

	public new class SignalName : Label.SignalName
	{
	}

	private static readonly TextParagraph _cachedParagraph = new TextParagraph();

	private const float _sizeComparisonEpsilon = 0.01f;

	private bool _autoSizeEnabled = true;

	private int _minFontSize = 8;

	private int _maxFontSize = 100;

	private int _lastSetSize;

	private Vector2 _lastAdjustedSize;

	[Export(PropertyHint.None, "")]
	public bool AutoSizeEnabled
	{
		get
		{
			return _autoSizeEnabled;
		}
		set
		{
			if (_autoSizeEnabled != value)
			{
				_autoSizeEnabled = value;
				if (Engine.IsEditorHint())
				{
					AdjustFontSize();
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public int MinFontSize
	{
		get
		{
			return _minFontSize;
		}
		set
		{
			if (_minFontSize != value)
			{
				_minFontSize = value;
				if (Engine.IsEditorHint())
				{
					AdjustFontSize();
				}
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public int MaxFontSize
	{
		get
		{
			return _maxFontSize;
		}
		set
		{
			if (_maxFontSize != value)
			{
				_maxFontSize = value;
				if (Engine.IsEditorHint())
				{
					AdjustFontSize();
				}
			}
		}
	}

	public override void _Ready()
	{
		MegaLabelHelper.AssertThemeFontOverride(this, ThemeConstants.Label.font);
		RefreshFont();
		AdjustFontSize();
	}

	public void RefreshFont()
	{
		this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.Label.font);
	}

	public override void _Notification(int what)
	{
		if ((long)what == 40 && !(_lastAdjustedSize.DistanceSquaredTo(base.Size) < 0.0001f))
		{
			AdjustFontSize();
		}
	}

	public void SetTextAutoSize(string text)
	{
		if (!(base.Text == text))
		{
			base.Text = text;
			AdjustFontSize();
		}
	}

	private void SetFontSize(int size)
	{
		if (_lastSetSize != size)
		{
			_lastSetSize = size;
			if (HasThemeFont(ThemeConstants.Label.font))
			{
				AddThemeFontSizeOverride(ThemeConstants.Label.fontSize, size);
			}
		}
	}

	private void AdjustFontSize()
	{
		if (!AutoSizeEnabled)
		{
			return;
		}
		_lastAdjustedSize = base.Size;
		Font themeFont = GetThemeFont(ThemeConstants.Label.font, "Label");
		float lineSpacing = GetThemeConstant(ThemeConstants.Label.lineSpacing, "Label");
		Vector2 size = GetRect().Size;
		bool wrap = base.AutowrapMode != TextServer.AutowrapMode.Off;
		if (!MegaLabelHelper.IsTooBig(_cachedParagraph, base.Text, themeFont, MaxFontSize, lineSpacing, wrap, size))
		{
			SetFontSize(MaxFontSize);
			return;
		}
		if (_lastSetSize >= MinFontSize && _lastSetSize < MaxFontSize && !MegaLabelHelper.IsTooBig(_cachedParagraph, base.Text, themeFont, _lastSetSize, lineSpacing, wrap, size) && MegaLabelHelper.IsTooBig(_cachedParagraph, base.Text, themeFont, _lastSetSize + 1, lineSpacing, wrap, size))
		{
			SetFontSize(_lastSetSize);
			return;
		}
		int num = MinFontSize;
		int num2 = MaxFontSize;
		while (num2 >= num)
		{
			int num3 = num + (num2 - num) / 2;
			if (num3 == MaxFontSize || MegaLabelHelper.IsTooBig(_cachedParagraph, base.Text, themeFont, num3, lineSpacing, wrap, size))
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		SetFontSize(Math.Min(num, num2));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFont, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTextAutoSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetFontSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AdjustFontSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshFont && args.Count == 0)
		{
			RefreshFont();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTextAutoSize && args.Count == 1)
		{
			SetTextAutoSize(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFontSize && args.Count == 1)
		{
			SetFontSize(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AdjustFontSize && args.Count == 0)
		{
			AdjustFontSize();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.RefreshFont)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.SetTextAutoSize)
		{
			return true;
		}
		if (method == MethodName.SetFontSize)
		{
			return true;
		}
		if (method == MethodName.AdjustFontSize)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.AutoSizeEnabled)
		{
			AutoSizeEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.MinFontSize)
		{
			MinFontSize = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName.MaxFontSize)
		{
			MaxFontSize = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._autoSizeEnabled)
		{
			_autoSizeEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._minFontSize)
		{
			_minFontSize = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._maxFontSize)
		{
			_maxFontSize = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._lastSetSize)
		{
			_lastSetSize = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._lastAdjustedSize)
		{
			_lastAdjustedSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.AutoSizeEnabled)
		{
			value = VariantUtils.CreateFrom<bool>(AutoSizeEnabled);
			return true;
		}
		int from;
		if (name == PropertyName.MinFontSize)
		{
			from = MinFontSize;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MaxFontSize)
		{
			from = MaxFontSize;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._autoSizeEnabled)
		{
			value = VariantUtils.CreateFrom(in _autoSizeEnabled);
			return true;
		}
		if (name == PropertyName._minFontSize)
		{
			value = VariantUtils.CreateFrom(in _minFontSize);
			return true;
		}
		if (name == PropertyName._maxFontSize)
		{
			value = VariantUtils.CreateFrom(in _maxFontSize);
			return true;
		}
		if (name == PropertyName._lastSetSize)
		{
			value = VariantUtils.CreateFrom(in _lastSetSize);
			return true;
		}
		if (name == PropertyName._lastAdjustedSize)
		{
			value = VariantUtils.CreateFrom(in _lastAdjustedSize);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._autoSizeEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._minFontSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._maxFontSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastSetSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._lastAdjustedSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.AutoSizeEnabled, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.MinFontSize, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.MaxFontSize, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.AutoSizeEnabled, Variant.From<bool>(AutoSizeEnabled));
		info.AddProperty(PropertyName.MinFontSize, Variant.From<int>(MinFontSize));
		info.AddProperty(PropertyName.MaxFontSize, Variant.From<int>(MaxFontSize));
		info.AddProperty(PropertyName._autoSizeEnabled, Variant.From(in _autoSizeEnabled));
		info.AddProperty(PropertyName._minFontSize, Variant.From(in _minFontSize));
		info.AddProperty(PropertyName._maxFontSize, Variant.From(in _maxFontSize));
		info.AddProperty(PropertyName._lastSetSize, Variant.From(in _lastSetSize));
		info.AddProperty(PropertyName._lastAdjustedSize, Variant.From(in _lastAdjustedSize));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.AutoSizeEnabled, out var value))
		{
			AutoSizeEnabled = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.MinFontSize, out var value2))
		{
			MinFontSize = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName.MaxFontSize, out var value3))
		{
			MaxFontSize = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName._autoSizeEnabled, out var value4))
		{
			_autoSizeEnabled = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._minFontSize, out var value5))
		{
			_minFontSize = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName._maxFontSize, out var value6))
		{
			_maxFontSize = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastSetSize, out var value7))
		{
			_lastSetSize = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastAdjustedSize, out var value8))
		{
			_lastAdjustedSize = value8.As<Vector2>();
		}
	}
}
