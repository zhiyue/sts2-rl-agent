using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NModalContainer.cs")]
public class NModalContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Add = "Add";

		public static readonly StringName Clear = "Clear";

		public static readonly StringName ShowBackstop = "ShowBackstop";

		public static readonly StringName HideBackstop = "HideBackstop";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _backstopTween = "_backstopTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private ColorRect _backstop;

	private Tween? _backstopTween;

	public static NModalContainer? Instance { get; private set; }

	public IScreenContext? OpenModal { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			Log.Error("NModalContainer already exists.");
			this.QueueFreeSafely();
		}
		else
		{
			Instance = this;
			_backstop = GetNode<ColorRect>("Backstop");
		}
	}

	public void Add(Node modalToCreate, bool showBackstop = true)
	{
		if (OpenModal != null)
		{
			Log.Warn("There's another modal already open.");
			return;
		}
		OpenModal = (IScreenContext)modalToCreate;
		this.AddChildSafely(modalToCreate);
		ActiveScreenContext.Instance.Update();
		if (showBackstop)
		{
			ShowBackstop();
		}
	}

	public void Clear()
	{
		foreach (Node child in GetChildren())
		{
			if (child != _backstop)
			{
				child.QueueFreeSafely();
			}
		}
		OpenModal = null;
		ActiveScreenContext.Instance.Update();
		HideBackstop();
	}

	public void ShowBackstop()
	{
		base.MouseFilter = MouseFilterEnum.Stop;
		_backstop.Visible = true;
		_backstopTween?.Kill();
		_backstopTween = CreateTween();
		_backstopTween.TweenProperty(_backstop, "color:a", 0.85f, 0.3);
	}

	public void HideBackstop()
	{
		base.MouseFilter = MouseFilterEnum.Ignore;
		_backstopTween?.Kill();
		_backstopTween = CreateTween();
		_backstopTween.TweenProperty(_backstop, "color:a", 0f, 0.3);
		_backstopTween.TweenCallback(Callable.From(() => _backstop.Visible = false));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Add, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "modalToCreate", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "showBackstop", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Clear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Add && args.Count == 2)
		{
			Add(VariantUtils.ConvertTo<Node>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Clear && args.Count == 0)
		{
			Clear();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowBackstop && args.Count == 0)
		{
			ShowBackstop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideBackstop && args.Count == 0)
		{
			HideBackstop();
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
		if (method == MethodName.Add)
		{
			return true;
		}
		if (method == MethodName.Clear)
		{
			return true;
		}
		if (method == MethodName.ShowBackstop)
		{
			return true;
		}
		if (method == MethodName.HideBackstop)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			_backstopTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			value = VariantUtils.CreateFrom(in _backstopTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstopTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._backstopTween, Variant.From(in _backstopTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._backstop, out var value))
		{
			_backstop = value.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._backstopTween, out var value2))
		{
			_backstopTween = value2.As<Tween>();
		}
	}
}
