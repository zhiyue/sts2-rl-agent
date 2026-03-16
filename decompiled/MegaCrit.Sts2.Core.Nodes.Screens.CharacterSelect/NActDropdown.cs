using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NActDropdown.cs")]
public class NActDropdown : NDropdown
{
	public new class MethodName : NDropdown.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName PopulateOptions = "PopulateOptions";

		public static readonly StringName OnDropdownItemSelected = "OnDropdownItemSelected";

		public static readonly StringName GetDropdownContainer = "GetDropdownContainer";
	}

	public new class PropertyName : NDropdown.PropertyName
	{
		public static readonly StringName CurrentOption = "CurrentOption";

		public static readonly StringName _currentOptionIndex = "_currentOptionIndex";
	}

	public new class SignalName : NDropdown.SignalName
	{
	}

	private static readonly string[] _options = new string[3] { "random", "overgrowth", "underdocks" };

	private int _currentOptionIndex = _options.IndexOf("random");

	public string CurrentOption => _options[_currentOptionIndex];

	public override void _Ready()
	{
		ConnectSignals();
		PopulateOptions();
	}

	protected override void OnFocus()
	{
		_currentOptionHighlight.Modulate = new Color("afcdde");
	}

	protected override void OnUnfocus()
	{
		_currentOptionHighlight.Modulate = Colors.White;
	}

	private void PopulateOptions()
	{
		List<NDropdownItem> list = GetDropdownItems().ToList();
		for (int i = 0; i < _options.Length; i++)
		{
			NDropdownItem nDropdownItem = list[i];
			string text = _options[i];
			nDropdownItem.Connect(NDropdownItem.SignalName.Selected, Callable.From<NDropdownItem>(OnDropdownItemSelected));
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted(char.ToUpperInvariant(text[0]));
			string text2 = text;
			defaultInterpolatedStringHandler.AppendFormatted(text2.Substring(1, text2.Length - 1));
			nDropdownItem.Text = defaultInterpolatedStringHandler.ToStringAndClear();
		}
		GetDropdownContainer().GetParent<NDropdownContainer>().RefreshLayout();
	}

	private void OnDropdownItemSelected(NDropdownItem item)
	{
		CloseDropdown();
		_currentOptionIndex = GetDropdownItems().ToList().IndexOf(item);
		_currentOptionLabel.SetTextAutoSize(item.Text);
	}

	private Control GetDropdownContainer()
	{
		return GetNode<Control>("DropdownContainer/VBoxContainer");
	}

	private IEnumerable<NDropdownItem> GetDropdownItems()
	{
		return GetDropdownContainer().GetChildren().OfType<NDropdownItem>();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PopulateOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDropdownItemSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "item", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetDropdownContainer, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PopulateOptions && args.Count == 0)
		{
			PopulateOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected && args.Count == 1)
		{
			OnDropdownItemSelected(VariantUtils.ConvertTo<NDropdownItem>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetDropdownContainer && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Control>(GetDropdownContainer());
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.PopulateOptions)
		{
			return true;
		}
		if (method == MethodName.OnDropdownItemSelected)
		{
			return true;
		}
		if (method == MethodName.GetDropdownContainer)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._currentOptionIndex)
		{
			_currentOptionIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurrentOption)
		{
			value = VariantUtils.CreateFrom<string>(CurrentOption);
			return true;
		}
		if (name == PropertyName._currentOptionIndex)
		{
			value = VariantUtils.CreateFrom(in _currentOptionIndex);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentOptionIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.CurrentOption, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._currentOptionIndex, Variant.From(in _currentOptionIndex));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._currentOptionIndex, out var value))
		{
			_currentOptionIndex = value.As<int>();
		}
	}
}
