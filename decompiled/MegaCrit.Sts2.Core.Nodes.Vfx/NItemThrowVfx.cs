using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NItemThrowVfx.cs")]
public class NItemThrowVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _itemSprite = "_itemSprite";

		public static readonly StringName _flightTime = "_flightTime";

		public static readonly StringName _heightMultiplier = "_heightMultiplier";

		public static readonly StringName _horizontalCurve = "_horizontalCurve";

		public static readonly StringName _verticalCurve = "_verticalCurve";

		public static readonly StringName _rotationMultiplier = "_rotationMultiplier";

		public static readonly StringName _rotationInfluenceCurve = "_rotationInfluenceCurve";

		public static readonly StringName _sourcePosition = "_sourcePosition";

		public static readonly StringName _targetPosition = "_targetPosition";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_item_throw");

	private const float _baseItemSize = 80f;

	[Export(PropertyHint.None, "")]
	private Sprite2D? _itemSprite;

	[Export(PropertyHint.None, "")]
	private float _flightTime;

	[Export(PropertyHint.None, "")]
	private float _heightMultiplier;

	[Export(PropertyHint.None, "")]
	private Curve? _horizontalCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _verticalCurve;

	[Export(PropertyHint.None, "")]
	private float _rotationMultiplier;

	[Export(PropertyHint.None, "")]
	private Curve? _rotationInfluenceCurve;

	private Vector2 _sourcePosition;

	private Vector2 _targetPosition;

	public static NItemThrowVfx? Create(Vector2 sourcePosition, Vector2 targetPosition, Texture2D? itemTexture, Vector2? scale = null)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NItemThrowVfx nItemThrowVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NItemThrowVfx>(PackedScene.GenEditState.Disabled);
		nItemThrowVfx._sourcePosition = sourcePosition;
		nItemThrowVfx._targetPosition = targetPosition;
		if (nItemThrowVfx._itemSprite != null)
		{
			nItemThrowVfx._itemSprite.Visible = false;
			nItemThrowVfx._itemSprite.Scale = scale ?? Vector2.One;
			if (itemTexture != null)
			{
				nItemThrowVfx._itemSprite.Texture = itemTexture;
				nItemThrowVfx._itemSprite.Scale *= 80f / (float)itemTexture.GetWidth();
			}
		}
		return nItemThrowVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(ThrowItem());
	}

	private async Task ThrowItem()
	{
		_itemSprite.Visible = true;
		_itemSprite.GlobalPosition = _sourcePosition;
		_itemSprite.RotationDegrees = Rng.Chaotic.NextFloat(360f);
		double timer = 0.0;
		while (timer < (double)_flightTime)
		{
			double processDeltaTime = GetProcessDeltaTime();
			float offset = (float)(timer / (double)_flightTime);
			float weight = _horizontalCurve.Sample(offset);
			float num = _verticalCurve.Sample(offset);
			float num2 = _rotationInfluenceCurve.Sample(offset);
			_itemSprite.Rotate((float)Mathf.DegToRad((double)(num2 * _rotationMultiplier) * processDeltaTime));
			Vector2 globalPosition = _sourcePosition.Lerp(_targetPosition, weight) + Vector2.Up * num * _heightMultiplier;
			_itemSprite.GlobalPosition = globalPosition;
			timer += processDeltaTime;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_itemSprite.Visible = false;
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._itemSprite)
		{
			_itemSprite = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._flightTime)
		{
			_flightTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._heightMultiplier)
		{
			_heightMultiplier = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._horizontalCurve)
		{
			_horizontalCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._verticalCurve)
		{
			_verticalCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._rotationMultiplier)
		{
			_rotationMultiplier = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rotationInfluenceCurve)
		{
			_rotationInfluenceCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._sourcePosition)
		{
			_sourcePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			_targetPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._itemSprite)
		{
			value = VariantUtils.CreateFrom(in _itemSprite);
			return true;
		}
		if (name == PropertyName._flightTime)
		{
			value = VariantUtils.CreateFrom(in _flightTime);
			return true;
		}
		if (name == PropertyName._heightMultiplier)
		{
			value = VariantUtils.CreateFrom(in _heightMultiplier);
			return true;
		}
		if (name == PropertyName._horizontalCurve)
		{
			value = VariantUtils.CreateFrom(in _horizontalCurve);
			return true;
		}
		if (name == PropertyName._verticalCurve)
		{
			value = VariantUtils.CreateFrom(in _verticalCurve);
			return true;
		}
		if (name == PropertyName._rotationMultiplier)
		{
			value = VariantUtils.CreateFrom(in _rotationMultiplier);
			return true;
		}
		if (name == PropertyName._rotationInfluenceCurve)
		{
			value = VariantUtils.CreateFrom(in _rotationInfluenceCurve);
			return true;
		}
		if (name == PropertyName._sourcePosition)
		{
			value = VariantUtils.CreateFrom(in _sourcePosition);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			value = VariantUtils.CreateFrom(in _targetPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._itemSprite, PropertyHint.NodeType, "Sprite2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._flightTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._heightMultiplier, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._horizontalCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rotationMultiplier, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rotationInfluenceCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._sourcePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._itemSprite, Variant.From(in _itemSprite));
		info.AddProperty(PropertyName._flightTime, Variant.From(in _flightTime));
		info.AddProperty(PropertyName._heightMultiplier, Variant.From(in _heightMultiplier));
		info.AddProperty(PropertyName._horizontalCurve, Variant.From(in _horizontalCurve));
		info.AddProperty(PropertyName._verticalCurve, Variant.From(in _verticalCurve));
		info.AddProperty(PropertyName._rotationMultiplier, Variant.From(in _rotationMultiplier));
		info.AddProperty(PropertyName._rotationInfluenceCurve, Variant.From(in _rotationInfluenceCurve));
		info.AddProperty(PropertyName._sourcePosition, Variant.From(in _sourcePosition));
		info.AddProperty(PropertyName._targetPosition, Variant.From(in _targetPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._itemSprite, out var value))
		{
			_itemSprite = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._flightTime, out var value2))
		{
			_flightTime = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._heightMultiplier, out var value3))
		{
			_heightMultiplier = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._horizontalCurve, out var value4))
		{
			_horizontalCurve = value4.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._verticalCurve, out var value5))
		{
			_verticalCurve = value5.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._rotationMultiplier, out var value6))
		{
			_rotationMultiplier = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rotationInfluenceCurve, out var value7))
		{
			_rotationInfluenceCurve = value7.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._sourcePosition, out var value8))
		{
			_sourcePosition = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetPosition, out var value9))
		{
			_targetPosition = value9.As<Vector2>();
		}
	}
}
