using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NRemoteTargetingIndicator.cs")]
public class NRemoteTargetingIndicator : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName StartDrawingFrom = "StartDrawingFrom";

		public static readonly StringName StopDrawing = "StopDrawing";

		public static readonly StringName UpdateDrawingTo = "UpdateDrawingTo";

		public static readonly StringName DoTargetingCreatureTween = "DoTargetingCreatureTween";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _fromPosition = "_fromPosition";

		public static readonly StringName _toPosition = "_toPosition";

		public static readonly StringName _line = "_line";

		public static readonly StringName _lineBack = "_lineBack";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _isTargetingCreature = "_isTargetingCreature";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const int _segmentCount = 100;

	private const float _defaultAlpha = 0.5f;

	private const float _targetingAlpha = 1f;

	private Player _player;

	private Vector2 _fromPosition;

	private Vector2 _toPosition;

	private Line2D _line;

	private Line2D _lineBack;

	private Tween? _tween;

	private bool _isTargetingCreature;

	public override void _Ready()
	{
		_line = GetNode<Line2D>("Line");
		_lineBack = GetNode<Line2D>("LineBack");
		for (int i = 0; i < 101; i++)
		{
			_line.AddPoint(Vector2.Zero);
			_lineBack.AddPoint(Vector2.Zero);
		}
		StopDrawing();
	}

	public void Initialize(Player player)
	{
		_player = player;
		CharacterModel character = player.Character;
		_line.DefaultColor = character.RemoteTargetingLineColor;
		_lineBack.DefaultColor = character.RemoteTargetingLineOutline;
		Gradient gradient = _line.GetGradient();
		if (gradient != null)
		{
			for (int i = 0; i < gradient.GetPointCount(); i++)
			{
				gradient.SetColor(i, gradient.GetColor(i) * character.RemoteTargetingLineColor);
			}
			_line.SetGradient(gradient);
		}
		Gradient gradient2 = _lineBack.GetGradient();
		if (gradient2 != null)
		{
			for (int j = 0; j < gradient2.GetPointCount(); j++)
			{
				gradient2.SetColor(j, gradient2.GetColor(j) * character.RemoteTargetingLineOutline);
			}
			_lineBack.SetGradient(gradient);
		}
	}

	public override void _Process(double delta)
	{
		Vector2 zero = Vector2.Zero;
		zero.X = _fromPosition.X + (_toPosition.X - _fromPosition.X) * 0.5f;
		zero.Y = _fromPosition.Y - (_toPosition.Y - _fromPosition.Y) * 0.5f;
		for (int i = 0; i < 100; i++)
		{
			Vector2 position = MathHelper.BezierCurve(_fromPosition, _toPosition, zero, (float)i / 101f);
			_line.SetPointPosition(i, position);
			_lineBack.SetPointPosition(i, position);
		}
		_line.SetPointPosition(100, _toPosition);
		_lineBack.SetPointPosition(100, _toPosition);
		bool isTargetingCreature = false;
		foreach (Creature item in _player.Creature.CombatState?.Enemies ?? Array.Empty<Creature>())
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(item);
			if (nCreature != null && nCreature.Hitbox.GetGlobalRect().HasPoint(base.GlobalPosition + _toPosition))
			{
				isTargetingCreature = true;
				break;
			}
		}
		DoTargetingCreatureTween(isTargetingCreature);
	}

	public void StartDrawingFrom(Vector2 from)
	{
		if (!NCombatUi.IsDebugHideMpTargetingUi)
		{
			_fromPosition = from;
			base.Visible = true;
			base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
		}
	}

	public void StopDrawing()
	{
		base.Visible = false;
		base.ProcessMode = ProcessModeEnum.Disabled;
		Color modulate = base.Modulate;
		modulate.A = 0.5f;
		base.Modulate = modulate;
	}

	public void UpdateDrawingTo(Vector2 position)
	{
		_toPosition = position;
	}

	private void DoTargetingCreatureTween(bool isTargetingCreature)
	{
		if (isTargetingCreature != _isTargetingCreature)
		{
			_tween?.Kill();
			_tween = CreateTween();
			if (isTargetingCreature)
			{
				_tween.TweenProperty(this, "modulate:a", 1f, 0.10000000149011612);
			}
			else
			{
				_tween.TweenProperty(this, "modulate:a", 0.5f, 0.25);
			}
			_isTargetingCreature = isTargetingCreature;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartDrawingFrom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "from", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopDrawing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateDrawingTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoTargetingCreatureTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isTargetingCreature", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDrawingFrom && args.Count == 1)
		{
			StartDrawingFrom(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopDrawing && args.Count == 0)
		{
			StopDrawing();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateDrawingTo && args.Count == 1)
		{
			UpdateDrawingTo(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoTargetingCreatureTween && args.Count == 1)
		{
			DoTargetingCreatureTween(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.StartDrawingFrom)
		{
			return true;
		}
		if (method == MethodName.StopDrawing)
		{
			return true;
		}
		if (method == MethodName.UpdateDrawingTo)
		{
			return true;
		}
		if (method == MethodName.DoTargetingCreatureTween)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._fromPosition)
		{
			_fromPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._toPosition)
		{
			_toPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._line)
		{
			_line = VariantUtils.ConvertTo<Line2D>(in value);
			return true;
		}
		if (name == PropertyName._lineBack)
		{
			_lineBack = VariantUtils.ConvertTo<Line2D>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isTargetingCreature)
		{
			_isTargetingCreature = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._fromPosition)
		{
			value = VariantUtils.CreateFrom(in _fromPosition);
			return true;
		}
		if (name == PropertyName._toPosition)
		{
			value = VariantUtils.CreateFrom(in _toPosition);
			return true;
		}
		if (name == PropertyName._line)
		{
			value = VariantUtils.CreateFrom(in _line);
			return true;
		}
		if (name == PropertyName._lineBack)
		{
			value = VariantUtils.CreateFrom(in _lineBack);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._isTargetingCreature)
		{
			value = VariantUtils.CreateFrom(in _isTargetingCreature);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._fromPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._toPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._line, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lineBack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isTargetingCreature, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._fromPosition, Variant.From(in _fromPosition));
		info.AddProperty(PropertyName._toPosition, Variant.From(in _toPosition));
		info.AddProperty(PropertyName._line, Variant.From(in _line));
		info.AddProperty(PropertyName._lineBack, Variant.From(in _lineBack));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._isTargetingCreature, Variant.From(in _isTargetingCreature));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._fromPosition, out var value))
		{
			_fromPosition = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._toPosition, out var value2))
		{
			_toPosition = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._line, out var value3))
		{
			_line = value3.As<Line2D>();
		}
		if (info.TryGetProperty(PropertyName._lineBack, out var value4))
		{
			_lineBack = value4.As<Line2D>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isTargetingCreature, out var value6))
		{
			_isTargetingCreature = value6.As<bool>();
		}
	}
}
