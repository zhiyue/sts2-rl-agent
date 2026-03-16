using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapMarker.cs")]
public class NMapMarker : TextureRect
{
	public new class MethodName : TextureRect.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ResetMapPoint = "ResetMapPoint";

		public static readonly StringName HideMapPoint = "HideMapPoint";

		public static readonly StringName SetMapPoint = "SetMapPoint";
	}

	public new class PropertyName : TextureRect.PropertyName
	{
		public static readonly StringName _tween = "_tween";

		public static readonly StringName _posOffset = "_posOffset";

		public static readonly StringName _isEnabled = "_isEnabled";
	}

	public new class SignalName : TextureRect.SignalName
	{
	}

	private Tween? _tween;

	private Vector2 _posOffset;

	private bool _isEnabled;

	public override void _Ready()
	{
		_posOffset = new Vector2((0f - base.Size.X) / 2f, -35f);
	}

	public void Initialize(Player player)
	{
		_isEnabled = player.RunState.Players.Count == 1;
		base.Texture = player.Character.MapMarker;
		base.Visible = false;
	}

	public void ResetMapPoint()
	{
		base.Visible = false;
	}

	public void HideMapPoint()
	{
		if (_isEnabled)
		{
			_tween?.FastForwardToCompletion();
			_tween = CreateTween();
			_tween.TweenProperty(this, "scale", Vector2.Zero, 0.20000000298023224).From(Vector2.One);
			_tween.TweenCallback(Callable.From(delegate
			{
				base.Visible = false;
			}));
		}
	}

	public void SetMapPoint(NMapPoint node)
	{
		if (_isEnabled)
		{
			_tween?.FastForwardToCompletion();
			if (!base.Visible)
			{
				base.Visible = true;
				Vector2 vector = new Vector2(node.Size.X / 2f, 0f);
				base.Position = node.Position + vector + _posOffset;
				_tween = CreateTween();
				_tween.TweenProperty(this, "scale", Vector2.One, 0.20000000298023224).From(Vector2.Down);
				_tween.Parallel().TweenProperty(this, "position", base.Position + Vector2.Up * 25f, 0.20000000298023224).SetEase(Tween.EaseType.In)
					.SetTrans(Tween.TransitionType.Sine)
					.FromCurrent();
				_tween.TweenProperty(this, "position", base.Position, 0.75).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetMapPoint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideMapPoint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMapPoint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.ResetMapPoint && args.Count == 0)
		{
			ResetMapPoint();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideMapPoint && args.Count == 0)
		{
			HideMapPoint();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMapPoint && args.Count == 1)
		{
			SetMapPoint(VariantUtils.ConvertTo<NMapPoint>(in args[0]));
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
		if (method == MethodName.ResetMapPoint)
		{
			return true;
		}
		if (method == MethodName.HideMapPoint)
		{
			return true;
		}
		if (method == MethodName.SetMapPoint)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			_posOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isEnabled)
		{
			_isEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			value = VariantUtils.CreateFrom(in _posOffset);
			return true;
		}
		if (name == PropertyName._isEnabled)
		{
			value = VariantUtils.CreateFrom(in _isEnabled);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._posOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._posOffset, Variant.From(in _posOffset));
		info.AddProperty(PropertyName._isEnabled, Variant.From(in _isEnabled));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._posOffset, out var value2))
		{
			_posOffset = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isEnabled, out var value3))
		{
			_isEnabled = value3.As<bool>();
		}
	}
}
