using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NSearchBar.cs")]
public class NSearchBar : Control
{
	[Signal]
	public delegate void QueryChangedEventHandler(string query);

	[Signal]
	public delegate void QuerySubmittedEventHandler(string query);

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName TextUpdated = "TextUpdated";

		public static readonly StringName TextSubmitted = "TextSubmitted";

		public static readonly StringName ClearText = "ClearText";

		public static readonly StringName Normalize = "Normalize";

		public static readonly StringName RemoveHtmlTags = "RemoveHtmlTags";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Text = "Text";

		public static readonly StringName TextArea = "TextArea";

		public static readonly StringName _textArea = "_textArea";

		public static readonly StringName _clearButton = "_clearButton";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName QueryChanged = "QueryChanged";

		public static readonly StringName QuerySubmitted = "QuerySubmitted";
	}

	private LineEdit _textArea;

	private NButton _clearButton;

	private QueryChangedEventHandler backing_QueryChanged;

	private QuerySubmittedEventHandler backing_QuerySubmitted;

	public string Text => _textArea.Text;

	public LineEdit TextArea => _textArea;

	public event QueryChangedEventHandler QueryChanged
	{
		add
		{
			backing_QueryChanged = (QueryChangedEventHandler)Delegate.Combine(backing_QueryChanged, value);
		}
		remove
		{
			backing_QueryChanged = (QueryChangedEventHandler)Delegate.Remove(backing_QueryChanged, value);
		}
	}

	public event QuerySubmittedEventHandler QuerySubmitted
	{
		add
		{
			backing_QuerySubmitted = (QuerySubmittedEventHandler)Delegate.Combine(backing_QuerySubmitted, value);
		}
		remove
		{
			backing_QuerySubmitted = (QuerySubmittedEventHandler)Delegate.Remove(backing_QuerySubmitted, value);
		}
	}

	[GeneratedRegex("[\\t\\r\\n]")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex NonSpaceWhitespaceCharacters()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__NonSpaceWhitespaceCharacters_5.Instance;
	}

	[GeneratedRegex("\\s{2,}")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex ConsecutiveSpaces()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__ConsecutiveSpaces_6.Instance;
	}

	[GeneratedRegex("<.*?>")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex HtmlTags()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__HtmlTags_7.Instance;
	}

	public override void _Ready()
	{
		_textArea = GetNode<LineEdit>("TextArea");
		_textArea.Connect(LineEdit.SignalName.TextChanged, Callable.From<string>(TextUpdated));
		_textArea.Connect(LineEdit.SignalName.TextSubmitted, Callable.From<string>(TextSubmitted));
		_textArea.SetPlaceholder(new LocString("card_library", "SEARCH_PLACEHOLDER").GetRawText());
		_clearButton = GetNode<NButton>("ClearButton");
		_clearButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)ClearText));
	}

	private void TextUpdated(string _)
	{
		EmitSignal(SignalName.QueryChanged, _textArea.Text);
	}

	private void TextSubmitted(string _)
	{
		EmitSignal(SignalName.QuerySubmitted, _textArea.Text);
	}

	private void ClearText(NButton _)
	{
		ClearText();
	}

	public void ClearText()
	{
		_textArea.TryGrabFocus();
		if (!string.IsNullOrWhiteSpace(_textArea.Text))
		{
			_textArea.Text = "";
			EmitSignal(SignalName.QueryChanged, _textArea.Text);
		}
	}

	public static string Normalize(string text)
	{
		string input = NonSpaceWhitespaceCharacters().Replace(text.Trim(), " ");
		return ConsecutiveSpaces().Replace(input, " ").ToLowerInvariant();
	}

	public static string RemoveHtmlTags(string text)
	{
		return HtmlTags().Replace(text, string.Empty);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TextUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TextSubmitted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Normalize, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveHtmlTags, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.TextUpdated && args.Count == 1)
		{
			TextUpdated(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TextSubmitted && args.Count == 1)
		{
			TextSubmitted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearText && args.Count == 1)
		{
			ClearText(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearText && args.Count == 0)
		{
			ClearText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Normalize && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(Normalize(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveHtmlTags && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(RemoveHtmlTags(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Normalize && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(Normalize(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveHtmlTags && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(RemoveHtmlTags(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.TextUpdated)
		{
			return true;
		}
		if (method == MethodName.TextSubmitted)
		{
			return true;
		}
		if (method == MethodName.ClearText)
		{
			return true;
		}
		if (method == MethodName.Normalize)
		{
			return true;
		}
		if (method == MethodName.RemoveHtmlTags)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._textArea)
		{
			_textArea = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._clearButton)
		{
			_clearButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Text)
		{
			value = VariantUtils.CreateFrom<string>(Text);
			return true;
		}
		if (name == PropertyName.TextArea)
		{
			value = VariantUtils.CreateFrom<LineEdit>(TextArea);
			return true;
		}
		if (name == PropertyName._textArea)
		{
			value = VariantUtils.CreateFrom(in _textArea);
			return true;
		}
		if (name == PropertyName._clearButton)
		{
			value = VariantUtils.CreateFrom(in _clearButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textArea, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._clearButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.Text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TextArea, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._textArea, Variant.From(in _textArea));
		info.AddProperty(PropertyName._clearButton, Variant.From(in _clearButton));
		info.AddSignalEventDelegate(SignalName.QueryChanged, backing_QueryChanged);
		info.AddSignalEventDelegate(SignalName.QuerySubmitted, backing_QuerySubmitted);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._textArea, out var value))
		{
			_textArea = value.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._clearButton, out var value2))
		{
			_clearButton = value2.As<NButton>();
		}
		if (info.TryGetSignalEventDelegate<QueryChangedEventHandler>(SignalName.QueryChanged, out var value3))
		{
			backing_QueryChanged = value3;
		}
		if (info.TryGetSignalEventDelegate<QuerySubmittedEventHandler>(SignalName.QuerySubmitted, out var value4))
		{
			backing_QuerySubmitted = value4;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.QueryChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "query", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.QuerySubmitted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "query", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalQueryChanged(string query)
	{
		EmitSignal(SignalName.QueryChanged, query);
	}

	protected void EmitSignalQuerySubmitted(string query)
	{
		EmitSignal(SignalName.QuerySubmitted, query);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.QueryChanged && args.Count == 1)
		{
			backing_QueryChanged?.Invoke(VariantUtils.ConvertTo<string>(in args[0]));
		}
		else if (signal == SignalName.QuerySubmitted && args.Count == 1)
		{
			backing_QuerySubmitted?.Invoke(VariantUtils.ConvertTo<string>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.QueryChanged)
		{
			return true;
		}
		if (signal == SignalName.QuerySubmitted)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
