using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NMessyCardPreviewContainer.cs")]
public class NMessyCardPreviewContainer : Control
{
	public class PoissonDiscSampler
	{
		private struct GridPos
		{
			public readonly int x;

			public readonly int y;

			public GridPos(Vector2 sample, float cellSize)
			{
				x = (int)(sample.X / cellSize);
				y = (int)(sample.Y / cellSize);
			}
		}

		private const int _maxAttempts = 30;

		private readonly Rect2 _rect;

		private readonly float _radius2;

		private readonly float _cellSize;

		private readonly Vector2[,] _grid;

		private readonly List<Vector2> _activeSamples = new List<Vector2>();

		public PoissonDiscSampler(float width, float height, float radius)
		{
			_rect = new Rect2(0f, 0f, width, height);
			_radius2 = radius * radius;
			_cellSize = radius / Mathf.Sqrt(2f);
			_grid = new Vector2[Mathf.CeilToInt(width / _cellSize), Mathf.CeilToInt(height / _cellSize)];
		}

		public IEnumerator<Vector2> Samples()
		{
			yield return AddSample(_rect.Size / 2f);
			while (_activeSamples.Count > 0)
			{
				int i = (int)(Rng.Chaotic.NextFloat() * (float)_activeSamples.Count);
				Vector2 vector = _activeSamples[i];
				bool found = false;
				for (int j = 0; j < 30; j++)
				{
					float s = (float)Math.PI * 2f * Rng.Chaotic.NextFloat();
					float num = Mathf.Sqrt(Rng.Chaotic.NextFloat() * 3f * _radius2 + _radius2);
					Vector2 vector2 = vector + num * new Vector2(Mathf.Cos(s), Mathf.Sin(s));
					if (_rect.HasPoint(vector2) && IsFarEnough(vector2))
					{
						found = true;
						yield return AddSample(vector2);
						break;
					}
				}
				if (!found)
				{
					_activeSamples[i] = _activeSamples[_activeSamples.Count - 1];
					_activeSamples.RemoveAt(_activeSamples.Count - 1);
				}
			}
		}

		private bool IsFarEnough(Vector2 sample)
		{
			GridPos gridPos = new GridPos(sample, _cellSize);
			int num = Mathf.Max(gridPos.x - 2, 0);
			int num2 = Mathf.Max(gridPos.y - 2, 0);
			int num3 = Mathf.Min(gridPos.x + 2, _grid.GetLength(0) - 1);
			int num4 = Mathf.Min(gridPos.y + 2, _grid.GetLength(1) - 1);
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num; j <= num3; j++)
				{
					Vector2 vector = _grid[j, i];
					if (vector != Vector2.Zero)
					{
						Vector2 vector2 = vector - sample;
						if (vector2.X * vector2.X + vector2.Y * vector2.Y < _radius2)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private Vector2 AddSample(Vector2 sample)
		{
			_activeSamples.Add(sample);
			GridPos gridPos = new GridPos(sample, _cellSize);
			_grid[gridPos.x, gridPos.y] = sample;
			return sample;
		}
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName PositionNewChild = "PositionNewChild";

		public static readonly StringName ResetSamples = "ResetSamples";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _resetNewCardTimer = "_resetNewCardTimer";

		public static readonly StringName _currentMaxPosition = "_currentMaxPosition";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _spacing = 150f;

	private const int _resetNewCardMsec = 2000;

	private ulong _resetNewCardTimer;

	private float _currentMaxPosition;

	private IEnumerator<Vector2>? _samples;

	public override void _Ready()
	{
		Connect(Node.SignalName.ChildEnteredTree, Callable.From<Node>(PositionNewChild));
	}

	private void PositionNewChild(Node node)
	{
		if (Time.GetTicksMsec() > _resetNewCardTimer)
		{
			_currentMaxPosition = 0f;
			ResetSamples();
		}
		_resetNewCardTimer = Time.GetTicksMsec() + 2000;
		if (!_samples.MoveNext())
		{
			ResetSamples();
		}
		Vector2 current = _samples.Current;
		if (node is Control control)
		{
			control.Position = current;
		}
		else if (node is Node2D node2D)
		{
			node2D.Position = current;
		}
	}

	private void ResetSamples()
	{
		PoissonDiscSampler poissonDiscSampler = new PoissonDiscSampler(base.Size.X, base.Size.Y, 150f);
		_samples = poissonDiscSampler.Samples();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PositionNewChild, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ResetSamples, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PositionNewChild && args.Count == 1)
		{
			PositionNewChild(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetSamples && args.Count == 0)
		{
			ResetSamples();
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
		if (method == MethodName.PositionNewChild)
		{
			return true;
		}
		if (method == MethodName.ResetSamples)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._resetNewCardTimer)
		{
			_resetNewCardTimer = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._currentMaxPosition)
		{
			_currentMaxPosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._resetNewCardTimer)
		{
			value = VariantUtils.CreateFrom(in _resetNewCardTimer);
			return true;
		}
		if (name == PropertyName._currentMaxPosition)
		{
			value = VariantUtils.CreateFrom(in _currentMaxPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._resetNewCardTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._currentMaxPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._resetNewCardTimer, Variant.From(in _resetNewCardTimer));
		info.AddProperty(PropertyName._currentMaxPosition, Variant.From(in _currentMaxPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._resetNewCardTimer, out var value))
		{
			_resetNewCardTimer = value.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._currentMaxPosition, out var value2))
		{
			_currentMaxPosition = value2.As<float>();
		}
	}
}
