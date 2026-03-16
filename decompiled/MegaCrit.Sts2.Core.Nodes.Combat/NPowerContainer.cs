using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPowerContainer.cs")]
public class NPowerContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ConnectCreatureSignals = "ConnectCreatureSignals";

		public static readonly StringName SetCreatureBounds = "SetCreatureBounds";

		public static readonly StringName UpdatePositions = "UpdatePositions";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Creature? _creature;

	private Vector2? _originalPosition;

	private readonly List<NPower> _powerNodes = new List<NPower>();

	public override void _EnterTree()
	{
		base._EnterTree();
		ConnectCreatureSignals();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_creature != null)
		{
			_creature.PowerApplied -= OnPowerApplied;
			_creature.PowerRemoved -= OnPowerRemoved;
		}
	}

	private void ConnectCreatureSignals()
	{
		if (_creature != null)
		{
			_creature.PowerApplied -= OnPowerApplied;
			_creature.PowerRemoved -= OnPowerRemoved;
			_creature.PowerApplied += OnPowerApplied;
			_creature.PowerRemoved += OnPowerRemoved;
		}
	}

	public void SetCreatureBounds(Control bounds)
	{
		base.GlobalPosition = new Vector2(bounds.GlobalPosition.X, base.GlobalPosition.Y);
		base.Size = new Vector2(bounds.Size.X * bounds.Scale.X + 25f, base.Size.Y);
		_originalPosition = base.Position;
		UpdatePositions();
	}

	private void Add(PowerModel power)
	{
		if (power.IsVisible)
		{
			NPower nPower = NPower.Create(power);
			nPower.Container = this;
			_powerNodes.Add(nPower);
			this.AddChildSafely(nPower);
			UpdatePositions();
		}
	}

	private void Remove(PowerModel power)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			NPower nPower = _powerNodes.FirstOrDefault((NPower n) => n.Model == power);
			if (nPower != null)
			{
				_powerNodes.Remove(nPower);
				UpdatePositions();
				nPower.QueueFreeSafely();
			}
		}
	}

	private void UpdatePositions()
	{
		if (_powerNodes.Count != 0)
		{
			float x = _powerNodes[0].Size.X;
			float b = Mathf.CeilToInt(base.Size.X / x);
			b = Mathf.Max(Mathf.CeilToInt((float)_powerNodes.Count / 2f), b);
			for (int i = 0; i < _powerNodes.Count; i++)
			{
				_powerNodes[i].Position = new Vector2(x * ((float)i % b), Mathf.Floor((float)i / b) * x);
			}
			float num = x * Mathf.Min(b, _powerNodes.Count);
			base.Position = (_originalPosition ?? base.Position) + Vector2.Left * Mathf.Max(0f, num - base.Size.X) / 2f;
		}
	}

	public void SetCreature(Creature creature)
	{
		if (_creature != null)
		{
			throw new InvalidOperationException("Creature was already set.");
		}
		_creature = creature;
		ConnectCreatureSignals();
		foreach (PowerModel power in _creature.Powers)
		{
			Add(power);
		}
	}

	private void OnPowerApplied(PowerModel power)
	{
		Add(power);
	}

	private void OnPowerRemoved(PowerModel power)
	{
		Remove(power);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectCreatureSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCreatureBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "bounds", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdatePositions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectCreatureSignals && args.Count == 0)
		{
			ConnectCreatureSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCreatureBounds && args.Count == 1)
		{
			SetCreatureBounds(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePositions && args.Count == 0)
		{
			UpdatePositions();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ConnectCreatureSignals)
		{
			return true;
		}
		if (method == MethodName.SetCreatureBounds)
		{
			return true;
		}
		if (method == MethodName.UpdatePositions)
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
