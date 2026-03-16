using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NRegentCharacterSelectBg.cs")]
public class NRegentCharacterSelectBg : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetSkin = "SetSkin";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _sphereGuardianHover = "_sphereGuardianHover";

		public static readonly StringName _decaHover = "_decaHover";

		public static readonly StringName _sentryHover = "_sentryHover";

		public static readonly StringName _sneckoHover = "_sneckoHover";

		public static readonly StringName _cultistHover = "_cultistHover";

		public static readonly StringName _shapesHover = "_shapesHover";

		public static readonly StringName _amongusHover = "_amongusHover";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaSprite _spineController;

	private Control _sphereGuardianHover;

	private Control _decaHover;

	private Control _sentryHover;

	private Control _sneckoHover;

	private Control _cultistHover;

	private Control _shapesHover;

	private Control _amongusHover;

	public override void _Ready()
	{
		_spineController = new MegaSprite(GetNode("SpineSprite"));
		_sphereGuardianHover = GetNode<Control>("SphereGuardianHover");
		_sphereGuardianHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("spheric guardian constellation");
		}));
		_sphereGuardianHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_decaHover = GetNode<Control>("DecaHover");
		_decaHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("deca outline");
		}));
		_decaHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_sentryHover = GetNode<Control>("SentryHover");
		_sentryHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("sentry constellation");
		}));
		_sentryHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_sneckoHover = GetNode<Control>("SneckoHover");
		_sneckoHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("snecko constellation");
		}));
		_sneckoHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_cultistHover = GetNode<Control>("CultistHover");
		_cultistHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("cultist constellation");
		}));
		_cultistHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_shapesHover = GetNode<Control>("ShapesHover");
		_shapesHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("shapes constellation");
		}));
		_shapesHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
		_amongusHover = GetNode<Control>("AmongusHover");
		_amongusHover.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
		{
			SetSkin("amongus constellation");
		}));
		_amongusHover.Connect(Control.SignalName.MouseExited, Callable.From(delegate
		{
			SetSkin("normal");
		}));
	}

	private void SetSkin(string skinName)
	{
		MegaSkeleton skeleton = _spineController.GetSkeleton();
		skeleton.SetSkin(skeleton.GetData().FindSkin(skinName));
		skeleton.SetSlotsToSetupPose();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSkin, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "skinName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetSkin && args.Count == 1)
		{
			SetSkin(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName.SetSkin)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._sphereGuardianHover)
		{
			_sphereGuardianHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._decaHover)
		{
			_decaHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._sentryHover)
		{
			_sentryHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._sneckoHover)
		{
			_sneckoHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cultistHover)
		{
			_cultistHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._shapesHover)
		{
			_shapesHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._amongusHover)
		{
			_amongusHover = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._sphereGuardianHover)
		{
			value = VariantUtils.CreateFrom(in _sphereGuardianHover);
			return true;
		}
		if (name == PropertyName._decaHover)
		{
			value = VariantUtils.CreateFrom(in _decaHover);
			return true;
		}
		if (name == PropertyName._sentryHover)
		{
			value = VariantUtils.CreateFrom(in _sentryHover);
			return true;
		}
		if (name == PropertyName._sneckoHover)
		{
			value = VariantUtils.CreateFrom(in _sneckoHover);
			return true;
		}
		if (name == PropertyName._cultistHover)
		{
			value = VariantUtils.CreateFrom(in _cultistHover);
			return true;
		}
		if (name == PropertyName._shapesHover)
		{
			value = VariantUtils.CreateFrom(in _shapesHover);
			return true;
		}
		if (name == PropertyName._amongusHover)
		{
			value = VariantUtils.CreateFrom(in _amongusHover);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sphereGuardianHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._decaHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sentryHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sneckoHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cultistHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shapesHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._amongusHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._sphereGuardianHover, Variant.From(in _sphereGuardianHover));
		info.AddProperty(PropertyName._decaHover, Variant.From(in _decaHover));
		info.AddProperty(PropertyName._sentryHover, Variant.From(in _sentryHover));
		info.AddProperty(PropertyName._sneckoHover, Variant.From(in _sneckoHover));
		info.AddProperty(PropertyName._cultistHover, Variant.From(in _cultistHover));
		info.AddProperty(PropertyName._shapesHover, Variant.From(in _shapesHover));
		info.AddProperty(PropertyName._amongusHover, Variant.From(in _amongusHover));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._sphereGuardianHover, out var value))
		{
			_sphereGuardianHover = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._decaHover, out var value2))
		{
			_decaHover = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._sentryHover, out var value3))
		{
			_sentryHover = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._sneckoHover, out var value4))
		{
			_sneckoHover = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cultistHover, out var value5))
		{
			_cultistHover = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._shapesHover, out var value6))
		{
			_shapesHover = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._amongusHover, out var value7))
		{
			_amongusHover = value7.As<Control>();
		}
	}
}
