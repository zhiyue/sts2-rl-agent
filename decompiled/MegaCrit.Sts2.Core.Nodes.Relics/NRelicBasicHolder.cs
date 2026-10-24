using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Relics;

/// <summary>
/// A basic relic display. In comparison with the top-bar relic holder, this cannot flash and never displays amounts.
/// It automatically supports hovering tweens and showing hovertips, but clicks must be handled externally.
/// Used in run history, multiplayer expanded state, the relic collection, and the relic unlock screen.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Relics/NRelicBasicHolder.cs")]
public class NRelicBasicHolder : NButton
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

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
		/// Cached name for the 'Relic' property.
		/// </summary>
		public static readonly StringName Relic = "Relic";

		/// <summary>
		/// Cached name for the '_relic' field.
		/// </summary>
		public static readonly StringName _relic = "_relic";

		/// <summary>
		/// Cached name for the '_hoverTween' field.
		/// </summary>
		public static readonly StringName _hoverTween = "_hoverTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("relics/relic_basic_holder");

	private NRelic _relic;

	private Tween? _hoverTween;

	private RelicModel _model;

	public NRelic Relic => _relic;

	public static NRelicBasicHolder? Create(RelicModel relic)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRelicBasicHolder nRelicBasicHolder = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRelicBasicHolder>(PackedScene.GenEditState.Disabled);
		nRelicBasicHolder.Name = $"NRelicBasicHolder-{relic.Id}";
		nRelicBasicHolder._model = relic;
		return nRelicBasicHolder;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_relic = GetNode<NRelic>("%Relic");
		_relic.Model = _model;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_hoverTween?.Kill();
	}

	protected override void OnFocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relic.Icon, "scale", Vector2.One * 1.25f, 0.05);
		NHoverTipSet.CreateAndShow(this, _relic.Model.HoverTips)?.SetAlignmentForRelic(_relic);
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relic.Icon, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName._ExitTree)
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
		if (name == PropertyName._relic)
		{
			_relic = VariantUtils.ConvertTo<NRelic>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Relic)
		{
			value = VariantUtils.CreateFrom<NRelic>(Relic);
			return true;
		}
		if (name == PropertyName._relic)
		{
			value = VariantUtils.CreateFrom(in _relic);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relic, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Relic, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._relic, Variant.From(in _relic));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._relic, out var value))
		{
			_relic = value.As<NRelic>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value2))
		{
			_hoverTween = value2.As<Tween>();
		}
	}
}
