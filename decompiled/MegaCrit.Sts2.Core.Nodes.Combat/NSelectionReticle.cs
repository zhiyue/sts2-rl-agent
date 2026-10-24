using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NSelectionReticle.cs")]
public class NSelectionReticle : Control
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

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnSelect' method.
		/// </summary>
		public static readonly StringName OnSelect = "OnSelect";

		/// <summary>
		/// Cached name for the 'OnDeselect' method.
		/// </summary>
		public static readonly StringName OnDeselect = "OnDeselect";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsSelected' property.
		/// </summary>
		public static readonly StringName IsSelected = "IsSelected";

		/// <summary>
		/// Cached name for the '_currentTween' field.
		/// </summary>
		public static readonly StringName _currentTween = "_currentTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Tween? _currentTween;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	public bool IsSelected { get; private set; }

	public override void _Ready()
	{
		base.Modulate = Colors.Transparent;
		base.PivotOffset = base.Size * 0.5f;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_cancelToken.Cancel();
	}

	public void OnSelect()
	{
		if (!NCombatUi.IsDebugHideTargetingUi)
		{
			_currentTween?.Kill();
			_currentTween = CreateTween().SetParallel();
			_currentTween.TweenProperty(this, "modulate:a", 1f, 0.20000000298023224);
			_currentTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
				.From(Vector2.One * 0.9f);
			base.Modulate = Colors.White;
			base.Scale = Vector2.One;
			IsSelected = true;
		}
	}

	public void OnDeselect()
	{
		if (!_cancelToken.IsCancellationRequested)
		{
			_currentTween?.Kill();
			if (this.IsValid() && IsInsideTree())
			{
				_currentTween = CreateTween()?.SetParallel();
				_currentTween?.TweenProperty(this, "modulate:a", 0f, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				_currentTween?.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				IsSelected = false;
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
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSelect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelect && args.Count == 0)
		{
			OnSelect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeselect && args.Count == 0)
		{
			OnDeselect();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnSelect)
		{
			return true;
		}
		if (method == MethodName.OnDeselect)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsSelected)
		{
			IsSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			_currentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsSelected)
		{
			value = VariantUtils.CreateFrom<bool>(IsSelected);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			value = VariantUtils.CreateFrom(in _currentTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsSelected, Variant.From<bool>(IsSelected));
		info.AddProperty(PropertyName._currentTween, Variant.From(in _currentTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsSelected, out var value))
		{
			IsSelected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._currentTween, out var value2))
		{
			_currentTween = value2.As<Tween>();
		}
	}
}
