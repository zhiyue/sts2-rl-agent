using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NMsaaPaginator.cs")]
public class NMsaaPaginator : NPaginator, IResettableSettingNode
{
	public new class MethodName : NPaginator.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetFromSettings = "SetFromSettings";

		public new static readonly StringName OnIndexChanged = "OnIndexChanged";

		public static readonly StringName GetMsaaLabel = "GetMsaaLabel";

		public static readonly StringName GetMsaa = "GetMsaa";
	}

	public new class PropertyName : NPaginator.PropertyName
	{
	}

	public new class SignalName : NPaginator.SignalName
	{
	}

	public override void _Ready()
	{
		ConnectSignals();
		_options.Add("0");
		_options.Add("2");
		_options.Add("4");
		_options.Add("8");
		SetFromSettings();
	}

	public void SetFromSettings()
	{
		int num = _options.IndexOf(SaveManager.Instance.SettingsSave.Msaa.ToString());
		_currentIndex = ((num != -1) ? num : 3);
		_label.SetTextAutoSize(GetMsaaLabel(int.Parse(_options[_currentIndex])));
	}

	protected override void OnIndexChanged(int index)
	{
		_currentIndex = index;
		_label.SetTextAutoSize(GetMsaaLabel(int.Parse(_options[index])));
		SaveManager.Instance.SettingsSave.Msaa = int.Parse(_options[index]);
		Log.Info("MSAA: " + _label.Text);
		RenderingServer.ViewportSetMsaa2D(GetViewport().GetViewportRid(), GetMsaa(SaveManager.Instance.SettingsSave.Msaa));
	}

	private string GetMsaaLabel(int msaaAmount)
	{
		return msaaAmount switch
		{
			2 => "2x", 
			4 => "4x", 
			8 => "8x", 
			_ => new LocString("settings_ui", "MSAA_NONE").GetFormattedText(), 
		};
	}

	private RenderingServer.ViewportMsaa GetMsaa(int index)
	{
		return index switch
		{
			2 => RenderingServer.ViewportMsaa.Msaa2X, 
			4 => RenderingServer.ViewportMsaa.Msaa4X, 
			8 => RenderingServer.ViewportMsaa.Msaa8X, 
			_ => RenderingServer.ViewportMsaa.Disabled, 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFromSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnIndexChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetMsaaLabel, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "msaaAmount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetMsaa, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetFromSettings && args.Count == 0)
		{
			SetFromSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnIndexChanged && args.Count == 1)
		{
			OnIndexChanged(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetMsaaLabel && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetMsaaLabel(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.GetMsaa && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<RenderingServer.ViewportMsaa>(GetMsaa(VariantUtils.ConvertTo<int>(in args[0])));
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
		if (method == MethodName.SetFromSettings)
		{
			return true;
		}
		if (method == MethodName.OnIndexChanged)
		{
			return true;
		}
		if (method == MethodName.GetMsaaLabel)
		{
			return true;
		}
		if (method == MethodName.GetMsaa)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
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
