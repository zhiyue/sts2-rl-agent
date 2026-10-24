using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Text;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.RichTextTags;

namespace MegaCrit.Sts2.addons.mega_text;

[Tool]
[ScriptPath("res://addons/mega_text/MegaRichTextLabel.cs")]
public class MegaRichTextLabel : RichTextLabel
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : RichTextLabel.MethodName
	{
		/// <summary>
		/// Cached name for the 'DisposeCachedParagraph' method.
		/// </summary>
		public static readonly StringName DisposeCachedParagraph = "DisposeCachedParagraph";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'RefreshFont' method.
		/// </summary>
		public static readonly StringName RefreshFont = "RefreshFont";

		/// <summary>
		/// Cached name for the '_Notification' method.
		/// </summary>
		public new static readonly StringName _Notification = "_Notification";

		/// <summary>
		/// Cached name for the 'InstallEffectsIfNeeded' method.
		/// </summary>
		public static readonly StringName InstallEffectsIfNeeded = "InstallEffectsIfNeeded";

		/// <summary>
		/// Cached name for the 'HasEffect' method.
		/// </summary>
		public static readonly StringName HasEffect = "HasEffect";

		/// <summary>
		/// Cached name for the 'SetTextAutoSize' method.
		/// </summary>
		public static readonly StringName SetTextAutoSize = "SetTextAutoSize";

		/// <summary>
		/// Cached name for the 'AdjustFontSize' method.
		/// </summary>
		public static readonly StringName AdjustFontSize = "AdjustFontSize";

		/// <summary>
		/// Cached name for the 'SetFontSize' method.
		/// </summary>
		public static readonly StringName SetFontSize = "SetFontSize";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : RichTextLabel.PropertyName
	{
		/// <summary>
		/// Cached name for the 'AutoSizeEnabled' property.
		/// </summary>
		public static readonly StringName AutoSizeEnabled = "AutoSizeEnabled";

		/// <summary>
		/// Cached name for the 'MinFontSize' property.
		/// </summary>
		public static readonly StringName MinFontSize = "MinFontSize";

		/// <summary>
		/// Cached name for the 'MaxFontSize' property.
		/// </summary>
		public static readonly StringName MaxFontSize = "MaxFontSize";

		/// <summary>
		/// Cached name for the 'IsVerticallyBound' property.
		/// </summary>
		public static readonly StringName IsVerticallyBound = "IsVerticallyBound";

		/// <summary>
		/// Cached name for the 'IsHorizontallyBound' property.
		/// </summary>
		public static readonly StringName IsHorizontallyBound = "IsHorizontallyBound";

		/// <summary>
		/// Cached name for the 'Text' property.
		/// </summary>
		public new static readonly StringName Text = "Text";

		/// <summary>
		/// Cached name for the '_isAutoSizeEnabled' field.
		/// </summary>
		public static readonly StringName _isAutoSizeEnabled = "_isAutoSizeEnabled";

		/// <summary>
		/// Cached name for the '_minFontSize' field.
		/// </summary>
		public static readonly StringName _minFontSize = "_minFontSize";

		/// <summary>
		/// Cached name for the '_maxFontSize' field.
		/// </summary>
		public static readonly StringName _maxFontSize = "_maxFontSize";

		/// <summary>
		/// Cached name for the '_lastSetSize' field.
		/// </summary>
		public static readonly StringName _lastSetSize = "_lastSetSize";

		/// <summary>
		/// Cached name for the '_isVerticallyBound' field.
		/// </summary>
		public static readonly StringName _isVerticallyBound = "_isVerticallyBound";

		/// <summary>
		/// Cached name for the '_isHorizontallyBound' field.
		/// </summary>
		public static readonly StringName _isHorizontallyBound = "_isHorizontallyBound";

		/// <summary>
		/// Cached name for the '_needsResize' field.
		/// </summary>
		public static readonly StringName _needsResize = "_needsResize";

		/// <summary>
		/// Cached name for the '_effectsInstalled' field.
		/// </summary>
		public static readonly StringName _effectsInstalled = "_effectsInstalled";

		/// <summary>
		/// Cached name for the '_lastAdjustedSize' field.
		/// </summary>
		public static readonly StringName _lastAdjustedSize = "_lastAdjustedSize";

		/// <summary>
		/// Cached name for the '_isAutoSizing' field.
		/// </summary>
		public static readonly StringName _isAutoSizing = "_isAutoSizing";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : RichTextLabel.SignalName
	{
	}

	private static TextParagraph? _cachedParagraph = new TextParagraph();

	private const float _sizeComparisonEpsilon = 0.01f;

	private bool _isAutoSizeEnabled = true;

	private int _minFontSize = 8;

	private int _maxFontSize = 100;

	private int _lastSetSize;

	private bool _isVerticallyBound = true;

	private bool _isHorizontallyBound;

	private bool _needsResize = true;

	private bool _effectsInstalled;

	private Vector2 _lastAdjustedSize;

	private static readonly AbstractMegaRichTextEffect[] _textEffects = new AbstractMegaRichTextEffect[13]
	{
		new RichTextAqua(),
		new RichTextBlue(),
		new RichTextFadeIn(),
		new RichTextFlyIn(),
		new RichTextGold(),
		new RichTextGreen(),
		new RichTextJitter(),
		new RichTextOrange(),
		new RichTextPink(),
		new RichTextPurple(),
		new RichTextRed(),
		new RichTextSine(),
		new RichTextThinkyDots()
	};

	private bool _isAutoSizing;

	[Export(PropertyHint.None, "")]
	public bool AutoSizeEnabled
	{
		get
		{
			return _isAutoSizeEnabled;
		}
		set
		{
			if (value && base.FitContent)
			{
				GD.PushWarning("Auto Size is not compatible with Fit Content, disabling Auto Size...");
				_isAutoSizeEnabled = false;
			}
			else if (AutoSizeEnabled != value)
			{
				_isAutoSizeEnabled = value;
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

	[Export(PropertyHint.None, "")]
	public bool IsVerticallyBound
	{
		get
		{
			return _isVerticallyBound;
		}
		set
		{
			_isVerticallyBound = value;
			if (Engine.IsEditorHint())
			{
				AdjustFontSize();
			}
		}
	}

	[Export(PropertyHint.None, "")]
	public bool IsHorizontallyBound
	{
		get
		{
			return _isHorizontallyBound;
		}
		set
		{
			_isHorizontallyBound = value;
			if (Engine.IsEditorHint())
			{
				AdjustFontSize();
			}
		}
	}

	public new string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			SetTextAutoSize(value);
		}
	}

	/// <summary>
	/// Releases the cached TextParagraph to free text server RIDs at exit.
	/// Uses Dispose() (not Free()) because TextParagraph is RefCounted.
	/// Nulled to guard against AdjustFontSize running during Godot's quit frames.
	/// </summary>
	public static void DisposeCachedParagraph()
	{
		_cachedParagraph?.Dispose();
		_cachedParagraph = null;
	}

	public override void _Ready()
	{
		MegaLabelHelper.AssertThemeFontOverride(this, ThemeConstants.RichTextLabel.NormalFont);
		RefreshFont();
		InstallEffectsIfNeeded();
		AdjustFontSize();
		ParseBbcode(Text);
	}

	public void RefreshFont()
	{
		this.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.RichTextLabel.NormalFont);
		this.ApplyLocaleFontSubstitution(FontType.Bold, ThemeConstants.RichTextLabel.BoldFont);
		this.ApplyLocaleFontSubstitution(FontType.Italic, ThemeConstants.RichTextLabel.ItalicsFont);
	}

	public override void _Notification(int what)
	{
		switch (what)
		{
		case 40:
			if (!(_lastAdjustedSize.DistanceSquaredTo(base.Size) < 0.0001f) && AutoSizeEnabled)
			{
				_needsResize = true;
				AdjustFontSize();
			}
			break;
		case 9001:
			base.CustomEffects.Clear();
			break;
		case 9002:
			InstallEffectsIfNeeded();
			break;
		}
	}

	private void InstallEffectsIfNeeded()
	{
		if ((!_effectsInstalled || base.CustomEffects.Count <= 0) && base.BbcodeEnabled)
		{
			Godot.Collections.Array array = new Godot.Collections.Array();
			AbstractMegaRichTextEffect[] textEffects = _textEffects;
			foreach (AbstractMegaRichTextEffect abstractMegaRichTextEffect in textEffects)
			{
				array.Add(abstractMegaRichTextEffect);
			}
			base.CustomEffects = array;
			_effectsInstalled = true;
		}
	}

	private bool HasEffect(AbstractMegaRichTextEffect effect)
	{
		return base.CustomEffects.Contains(effect);
	}

	/// <summary>
	/// Unfortunately, there's no way to override the setting of text for a Label. So if you want the text size to
	/// automatically adjust after being updated during gameplay, you must use this method instead of setting the
	/// Text property directly.
	/// </summary>
	/// <param name="text"></param>
	public void SetTextAutoSize(string text)
	{
		if (!(base.Text == text))
		{
			base.Text = text;
			InstallEffectsIfNeeded();
			if (AutoSizeEnabled)
			{
				_needsResize = true;
				CallDeferred("AdjustFontSize");
			}
		}
	}

	private void AdjustFontSize()
	{
		TextParagraph cachedParagraph = _cachedParagraph;
		if (!AutoSizeEnabled || _isAutoSizing || cachedParagraph == null || !_needsResize)
		{
			return;
		}
		_isAutoSizing = true;
		try
		{
			_needsResize = true;
			_lastAdjustedSize = base.Size;
			Font themeFont = GetThemeFont(ThemeConstants.RichTextLabel.NormalFont, "RichTextLabel");
			float lineSpacing = GetThemeConstant(ThemeConstants.RichTextLabel.LineSpacing, "RichTextLabel");
			Vector2 size = GetRect().Size;
			List<BbcodeObject> objs = MegaLabelHelper.ParseBbcode(Text);
			if (!MegaLabelHelper.IsTooBig(cachedParagraph, objs, themeFont, MaxFontSize, lineSpacing, size, _isHorizontallyBound, _isVerticallyBound))
			{
				SetFontSize(MaxFontSize);
				return;
			}
			if (_lastSetSize >= MinFontSize && _lastSetSize < MaxFontSize && !MegaLabelHelper.IsTooBig(cachedParagraph, objs, themeFont, _lastSetSize, lineSpacing, size, _isHorizontallyBound, _isVerticallyBound) && MegaLabelHelper.IsTooBig(cachedParagraph, objs, themeFont, _lastSetSize + 1, lineSpacing, size, _isHorizontallyBound, _isVerticallyBound))
			{
				SetFontSize(_lastSetSize);
				return;
			}
			int num = MinFontSize;
			int num2 = MaxFontSize;
			while (num2 >= num)
			{
				int num3 = num + (num2 - num) / 2;
				if (MegaLabelHelper.IsTooBig(cachedParagraph, objs, themeFont, num3, lineSpacing, size, _isHorizontallyBound, _isVerticallyBound))
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
		finally
		{
			_isAutoSizing = false;
		}
	}

	private void SetFontSize(int size)
	{
		if (_lastSetSize != size)
		{
			_lastSetSize = size;
			AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.NormalFontSize, size);
			if (base.BbcodeEnabled)
			{
				AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.BoldFontSize, size);
				AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.BoldItalicsFontSize, size);
				AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.ItalicsFontSize, size);
				AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.MonoFontSize, size);
				ParseBbcode(Text);
			}
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName.DisposeCachedParagraph, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFont, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InstallEffectsIfNeeded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HasEffect, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "effect", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("RichTextEffect"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTextAutoSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AdjustFontSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFontSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DisposeCachedParagraph && args.Count == 0)
		{
			DisposeCachedParagraph();
			ret = default(godot_variant);
			return true;
		}
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
		if (method == MethodName.InstallEffectsIfNeeded && args.Count == 0)
		{
			InstallEffectsIfNeeded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasEffect && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(HasEffect(VariantUtils.ConvertTo<AbstractMegaRichTextEffect>(in args[0])));
			return true;
		}
		if (method == MethodName.SetTextAutoSize && args.Count == 1)
		{
			SetTextAutoSize(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AdjustFontSize && args.Count == 0)
		{
			AdjustFontSize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFontSize && args.Count == 1)
		{
			SetFontSize(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DisposeCachedParagraph && args.Count == 0)
		{
			DisposeCachedParagraph();
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.DisposeCachedParagraph)
		{
			return true;
		}
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
		if (method == MethodName.InstallEffectsIfNeeded)
		{
			return true;
		}
		if (method == MethodName.HasEffect)
		{
			return true;
		}
		if (method == MethodName.SetTextAutoSize)
		{
			return true;
		}
		if (method == MethodName.AdjustFontSize)
		{
			return true;
		}
		if (method == MethodName.SetFontSize)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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
		if (name == PropertyName.IsVerticallyBound)
		{
			IsVerticallyBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsHorizontallyBound)
		{
			IsHorizontallyBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.Text)
		{
			Text = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._isAutoSizeEnabled)
		{
			_isAutoSizeEnabled = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._isVerticallyBound)
		{
			_isVerticallyBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHorizontallyBound)
		{
			_isHorizontallyBound = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._needsResize)
		{
			_needsResize = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._effectsInstalled)
		{
			_effectsInstalled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lastAdjustedSize)
		{
			_lastAdjustedSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isAutoSizing)
		{
			_isAutoSizing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.AutoSizeEnabled)
		{
			from = AutoSizeEnabled;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		int from2;
		if (name == PropertyName.MinFontSize)
		{
			from2 = MinFontSize;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.MaxFontSize)
		{
			from2 = MaxFontSize;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.IsVerticallyBound)
		{
			from = IsVerticallyBound;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsHorizontallyBound)
		{
			from = IsHorizontallyBound;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Text)
		{
			value = VariantUtils.CreateFrom<string>(Text);
			return true;
		}
		if (name == PropertyName._isAutoSizeEnabled)
		{
			value = VariantUtils.CreateFrom(in _isAutoSizeEnabled);
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
		if (name == PropertyName._isVerticallyBound)
		{
			value = VariantUtils.CreateFrom(in _isVerticallyBound);
			return true;
		}
		if (name == PropertyName._isHorizontallyBound)
		{
			value = VariantUtils.CreateFrom(in _isHorizontallyBound);
			return true;
		}
		if (name == PropertyName._needsResize)
		{
			value = VariantUtils.CreateFrom(in _needsResize);
			return true;
		}
		if (name == PropertyName._effectsInstalled)
		{
			value = VariantUtils.CreateFrom(in _effectsInstalled);
			return true;
		}
		if (name == PropertyName._lastAdjustedSize)
		{
			value = VariantUtils.CreateFrom(in _lastAdjustedSize);
			return true;
		}
		if (name == PropertyName._isAutoSizing)
		{
			value = VariantUtils.CreateFrom(in _isAutoSizing);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAutoSizeEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._minFontSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._maxFontSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastSetSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isVerticallyBound, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHorizontallyBound, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._needsResize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._effectsInstalled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._lastAdjustedSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.AutoSizeEnabled, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.MinFontSize, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.MaxFontSize, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsVerticallyBound, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsHorizontallyBound, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAutoSizing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.AutoSizeEnabled, Variant.From<bool>(AutoSizeEnabled));
		info.AddProperty(PropertyName.MinFontSize, Variant.From<int>(MinFontSize));
		info.AddProperty(PropertyName.MaxFontSize, Variant.From<int>(MaxFontSize));
		info.AddProperty(PropertyName.IsVerticallyBound, Variant.From<bool>(IsVerticallyBound));
		info.AddProperty(PropertyName.IsHorizontallyBound, Variant.From<bool>(IsHorizontallyBound));
		info.AddProperty(PropertyName.Text, Variant.From<string>(Text));
		info.AddProperty(PropertyName._isAutoSizeEnabled, Variant.From(in _isAutoSizeEnabled));
		info.AddProperty(PropertyName._minFontSize, Variant.From(in _minFontSize));
		info.AddProperty(PropertyName._maxFontSize, Variant.From(in _maxFontSize));
		info.AddProperty(PropertyName._lastSetSize, Variant.From(in _lastSetSize));
		info.AddProperty(PropertyName._isVerticallyBound, Variant.From(in _isVerticallyBound));
		info.AddProperty(PropertyName._isHorizontallyBound, Variant.From(in _isHorizontallyBound));
		info.AddProperty(PropertyName._needsResize, Variant.From(in _needsResize));
		info.AddProperty(PropertyName._effectsInstalled, Variant.From(in _effectsInstalled));
		info.AddProperty(PropertyName._lastAdjustedSize, Variant.From(in _lastAdjustedSize));
		info.AddProperty(PropertyName._isAutoSizing, Variant.From(in _isAutoSizing));
	}

	/// <inheritdoc />
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
		if (info.TryGetProperty(PropertyName.IsVerticallyBound, out var value4))
		{
			IsVerticallyBound = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsHorizontallyBound, out var value5))
		{
			IsHorizontallyBound = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.Text, out var value6))
		{
			Text = value6.As<string>();
		}
		if (info.TryGetProperty(PropertyName._isAutoSizeEnabled, out var value7))
		{
			_isAutoSizeEnabled = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._minFontSize, out var value8))
		{
			_minFontSize = value8.As<int>();
		}
		if (info.TryGetProperty(PropertyName._maxFontSize, out var value9))
		{
			_maxFontSize = value9.As<int>();
		}
		if (info.TryGetProperty(PropertyName._lastSetSize, out var value10))
		{
			_lastSetSize = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._isVerticallyBound, out var value11))
		{
			_isVerticallyBound = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHorizontallyBound, out var value12))
		{
			_isHorizontallyBound = value12.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._needsResize, out var value13))
		{
			_needsResize = value13.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._effectsInstalled, out var value14))
		{
			_effectsInstalled = value14.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lastAdjustedSize, out var value15))
		{
			_lastAdjustedSize = value15.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isAutoSizing, out var value16))
		{
			_isAutoSizing = value16.As<bool>();
		}
	}
}
