using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NDoomSubEmitterVfx.cs")]
public class NDoomSubEmitterVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName FireSpear = "FireSpear";

		public static readonly StringName FireAllSpears = "FireAllSpears";

		public static readonly StringName ShowOrHide = "ShowOrHide";

		public static readonly StringName UpdateWidth = "UpdateWidth";

		public static readonly StringName SetVisibility = "SetVisibility";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName CurScaleX = "CurScaleX";

		public static readonly StringName _scalableLayers = "_scalableLayers";

		public static readonly StringName _spears = "_spears";

		public static readonly StringName _verticalShrinkingLayer = "_verticalShrinkingLayer";

		public static readonly StringName _particlesToKeepDense = "_particlesToKeepDense";

		public static readonly StringName _baseScales = "_baseScales";

		public static readonly StringName _indeces = "_indeces";

		public static readonly StringName _baseSpearRegionWidth = "_baseSpearRegionWidth";

		public static readonly StringName _dumbHackBecauseOfHowTexturerectsWork = "_dumbHackBecauseOfHowTexturerectsWork";

		public static readonly StringName _rotationHackForSameDumbReason = "_rotationHackForSameDumbReason";

		public static readonly StringName _baseParticleDensity = "_baseParticleDensity";

		public static readonly StringName _spearFixedHScale = "_spearFixedHScale";

		public static readonly StringName _spearAngleIntensity = "_spearAngleIntensity";

		public static readonly StringName _minSpearSize = "_minSpearSize";

		public static readonly StringName _maxSpearSize = "_maxSpearSize";

		public static readonly StringName _minSpearTime = "_minSpearTime";

		public static readonly StringName _maxSpearTime = "_maxSpearTime";

		public static readonly StringName _outerMargin = "_outerMargin";

		public static readonly StringName _innerMargin = "_innerMargin";

		public static readonly StringName _time = "_time";

		public static readonly StringName _isOn = "_isOn";

		public static readonly StringName _curScaleX = "_curScaleX";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Array<Node2D> _scalableLayers;

	[Export(PropertyHint.None, "")]
	private Array<TextureRect> _spears;

	[Export(PropertyHint.None, "")]
	private Node2D _verticalShrinkingLayer;

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _particlesToKeepDense;

	private Array<Vector2> _baseScales;

	private Array<int> _indeces;

	private float _baseSpearRegionWidth = 140f;

	private float _dumbHackBecauseOfHowTexturerectsWork = -20f;

	private float _rotationHackForSameDumbReason = 4f;

	private int _baseParticleDensity = 300;

	private float _spearFixedHScale = 0.4f;

	private float _spearAngleIntensity = 0.15f;

	private float _minSpearSize = 1f;

	private float _maxSpearSize = 1.5f;

	private float _minSpearTime = 0.3f;

	private float _maxSpearTime = 0.6f;

	private float _outerMargin = 0.2f;

	private float _innerMargin = -0.2f;

	private double _time = 2.0;

	private bool _isOn;

	private float _curScaleX = 1f;

	private Tween? _tween;

	public float CurScaleX
	{
		get
		{
			return _curScaleX;
		}
		set
		{
			_curScaleX = value;
			UpdateWidth(_curScaleX);
		}
	}

	public override void _Ready()
	{
		_baseScales = new Array<Vector2>();
		_indeces = new Array<int>();
		foreach (Node2D scalableLayer in _scalableLayers)
		{
			_baseScales.Add(scalableLayer.Scale);
			_indeces.Add(scalableLayer.GetIndex());
		}
		_isOn = false;
		SetVisibility(isOn: false);
		UpdateWidth(0f);
		ShowOrHide(0f, 0f);
	}

	private void FireSpear(TextureRect textureRect = null)
	{
		Vector2 position = textureRect.Position;
		position.X = Rng.Chaotic.NextFloat(_baseSpearRegionWidth * -0.5f, _baseSpearRegionWidth * 0.5f) + _dumbHackBecauseOfHowTexturerectsWork;
		textureRect.Position = position;
		textureRect.RotationDegrees = textureRect.Position.X * _spearAngleIntensity + _rotationHackForSameDumbReason;
		textureRect.Modulate = Colors.Transparent;
		float x = _spearFixedHScale / _curScaleX;
		textureRect.Scale = new Vector2(x, Rng.Chaotic.NextFloat(_minSpearSize, _maxSpearSize));
		Vector2 vector = new Vector2(x, 0f);
		float num = Rng.Chaotic.NextFloat(_minSpearTime, _maxSpearTime);
		Tween tween = textureRect.CreateTween();
		tween.TweenProperty(textureRect, "scale", vector, num).From(new Vector2(x, Rng.Chaotic.NextFloat(_minSpearSize, _maxSpearSize)));
		if (_isOn)
		{
			tween.TweenCallback(Callable.From(delegate
			{
				FireSpear(textureRect);
			}));
		}
		Tween tween2 = textureRect.CreateTween();
		float a = Rng.Chaotic.NextFloat(0.4f, 0.7f);
		tween2.TweenProperty(textureRect, "modulate", new Color(1f, 1f, 1f, a), 0.20000000298023224).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
	}

	private void FireAllSpears()
	{
		foreach (TextureRect spear in _spears)
		{
			FireSpear(spear);
		}
	}

	public void ShowOrHide(float widthScale, float tweenTime)
	{
		_isOn = widthScale > 0.1f;
		float num = (_isOn ? 0f : 0.3f);
		float num2 = widthScale;
		Tween.EaseType ease;
		if (_isOn)
		{
			SetVisibility(isOn: true);
			ease = Tween.EaseType.Out;
			int amount = (((int)widthScale * _baseParticleDensity <= 1) ? 1 : ((int)widthScale * _baseParticleDensity));
			_particlesToKeepDense.Amount = amount;
		}
		else
		{
			ease = Tween.EaseType.In;
			num2 = 0f;
			tweenTime = _curScaleX * 0.15f;
		}
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "CurScaleX", num2, tweenTime).SetDelay(num).SetEase(ease)
			.SetTrans(Tween.TransitionType.Cubic);
		if (!_isOn)
		{
			_tween.TweenCallback(Callable.From(delegate
			{
				SetVisibility(isOn: false);
			}));
		}
	}

	private void UpdateWidth(float width)
	{
		_curScaleX = width;
		int num = 0;
		foreach (Node2D scalableLayer in _scalableLayers)
		{
			Vector2 vector = _baseScales[num];
			Vector2 scale = ((scalableLayer != _verticalShrinkingLayer || !(width < 1f)) ? new Vector2(vector.X * _curScaleX, vector.Y) : new Vector2(vector.X * _curScaleX, vector.Y * width / 1f));
			scalableLayer.Scale = scale;
			num++;
		}
	}

	private void SetVisibility(bool isOn)
	{
		int num = 0;
		foreach (Node2D scalableLayer in _scalableLayers)
		{
			if (scalableLayer is GpuParticles2D gpuParticles2D)
			{
				if (isOn)
				{
					gpuParticles2D.Restart();
				}
				else
				{
					gpuParticles2D.Emitting = false;
				}
			}
			else if (isOn)
			{
				MoveChild(scalableLayer, _indeces[num]);
			}
			num++;
		}
		if (isOn)
		{
			FireAllSpears();
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FireSpear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "textureRect", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("TextureRect"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FireAllSpears, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowOrHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "widthScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "tweenTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateWidth, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "width", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isOn", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.FireSpear && args.Count == 1)
		{
			FireSpear(VariantUtils.ConvertTo<TextureRect>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FireAllSpears && args.Count == 0)
		{
			FireAllSpears();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowOrHide && args.Count == 2)
		{
			ShowOrHide(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateWidth && args.Count == 1)
		{
			UpdateWidth(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetVisibility && args.Count == 1)
		{
			SetVisibility(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.FireSpear)
		{
			return true;
		}
		if (method == MethodName.FireAllSpears)
		{
			return true;
		}
		if (method == MethodName.ShowOrHide)
		{
			return true;
		}
		if (method == MethodName.UpdateWidth)
		{
			return true;
		}
		if (method == MethodName.SetVisibility)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CurScaleX)
		{
			CurScaleX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._scalableLayers)
		{
			_scalableLayers = VariantUtils.ConvertToArray<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._spears)
		{
			_spears = VariantUtils.ConvertToArray<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._verticalShrinkingLayer)
		{
			_verticalShrinkingLayer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._particlesToKeepDense)
		{
			_particlesToKeepDense = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._baseScales)
		{
			_baseScales = VariantUtils.ConvertToArray<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._indeces)
		{
			_indeces = VariantUtils.ConvertToArray<int>(in value);
			return true;
		}
		if (name == PropertyName._baseSpearRegionWidth)
		{
			_baseSpearRegionWidth = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._dumbHackBecauseOfHowTexturerectsWork)
		{
			_dumbHackBecauseOfHowTexturerectsWork = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rotationHackForSameDumbReason)
		{
			_rotationHackForSameDumbReason = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._baseParticleDensity)
		{
			_baseParticleDensity = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._spearFixedHScale)
		{
			_spearFixedHScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._spearAngleIntensity)
		{
			_spearAngleIntensity = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._minSpearSize)
		{
			_minSpearSize = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxSpearSize)
		{
			_maxSpearSize = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._minSpearTime)
		{
			_minSpearTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxSpearTime)
		{
			_maxSpearTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._outerMargin)
		{
			_outerMargin = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._innerMargin)
		{
			_innerMargin = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._isOn)
		{
			_isOn = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._curScaleX)
		{
			_curScaleX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurScaleX)
		{
			value = VariantUtils.CreateFrom<float>(CurScaleX);
			return true;
		}
		if (name == PropertyName._scalableLayers)
		{
			value = VariantUtils.CreateFromArray(_scalableLayers);
			return true;
		}
		if (name == PropertyName._spears)
		{
			value = VariantUtils.CreateFromArray(_spears);
			return true;
		}
		if (name == PropertyName._verticalShrinkingLayer)
		{
			value = VariantUtils.CreateFrom(in _verticalShrinkingLayer);
			return true;
		}
		if (name == PropertyName._particlesToKeepDense)
		{
			value = VariantUtils.CreateFrom(in _particlesToKeepDense);
			return true;
		}
		if (name == PropertyName._baseScales)
		{
			value = VariantUtils.CreateFromArray(_baseScales);
			return true;
		}
		if (name == PropertyName._indeces)
		{
			value = VariantUtils.CreateFromArray(_indeces);
			return true;
		}
		if (name == PropertyName._baseSpearRegionWidth)
		{
			value = VariantUtils.CreateFrom(in _baseSpearRegionWidth);
			return true;
		}
		if (name == PropertyName._dumbHackBecauseOfHowTexturerectsWork)
		{
			value = VariantUtils.CreateFrom(in _dumbHackBecauseOfHowTexturerectsWork);
			return true;
		}
		if (name == PropertyName._rotationHackForSameDumbReason)
		{
			value = VariantUtils.CreateFrom(in _rotationHackForSameDumbReason);
			return true;
		}
		if (name == PropertyName._baseParticleDensity)
		{
			value = VariantUtils.CreateFrom(in _baseParticleDensity);
			return true;
		}
		if (name == PropertyName._spearFixedHScale)
		{
			value = VariantUtils.CreateFrom(in _spearFixedHScale);
			return true;
		}
		if (name == PropertyName._spearAngleIntensity)
		{
			value = VariantUtils.CreateFrom(in _spearAngleIntensity);
			return true;
		}
		if (name == PropertyName._minSpearSize)
		{
			value = VariantUtils.CreateFrom(in _minSpearSize);
			return true;
		}
		if (name == PropertyName._maxSpearSize)
		{
			value = VariantUtils.CreateFrom(in _maxSpearSize);
			return true;
		}
		if (name == PropertyName._minSpearTime)
		{
			value = VariantUtils.CreateFrom(in _minSpearTime);
			return true;
		}
		if (name == PropertyName._maxSpearTime)
		{
			value = VariantUtils.CreateFrom(in _maxSpearTime);
			return true;
		}
		if (name == PropertyName._outerMargin)
		{
			value = VariantUtils.CreateFrom(in _outerMargin);
			return true;
		}
		if (name == PropertyName._innerMargin)
		{
			value = VariantUtils.CreateFrom(in _innerMargin);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		if (name == PropertyName._isOn)
		{
			value = VariantUtils.CreateFrom(in _isOn);
			return true;
		}
		if (name == PropertyName._curScaleX)
		{
			value = VariantUtils.CreateFrom(in _curScaleX);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._scalableLayers, PropertyHint.TypeString, "24/34:Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._spears, PropertyHint.TypeString, "24/34:TextureRect", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalShrinkingLayer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particlesToKeepDense, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._baseScales, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._indeces, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseSpearRegionWidth, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._dumbHackBecauseOfHowTexturerectsWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rotationHackForSameDumbReason, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._baseParticleDensity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._spearFixedHScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._spearAngleIntensity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minSpearSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxSpearSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minSpearTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxSpearTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._outerMargin, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._innerMargin, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isOn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._curScaleX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.CurScaleX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CurScaleX, Variant.From<float>(CurScaleX));
		info.AddProperty(PropertyName._scalableLayers, Variant.CreateFrom(_scalableLayers));
		info.AddProperty(PropertyName._spears, Variant.CreateFrom(_spears));
		info.AddProperty(PropertyName._verticalShrinkingLayer, Variant.From(in _verticalShrinkingLayer));
		info.AddProperty(PropertyName._particlesToKeepDense, Variant.From(in _particlesToKeepDense));
		info.AddProperty(PropertyName._baseScales, Variant.CreateFrom(_baseScales));
		info.AddProperty(PropertyName._indeces, Variant.CreateFrom(_indeces));
		info.AddProperty(PropertyName._baseSpearRegionWidth, Variant.From(in _baseSpearRegionWidth));
		info.AddProperty(PropertyName._dumbHackBecauseOfHowTexturerectsWork, Variant.From(in _dumbHackBecauseOfHowTexturerectsWork));
		info.AddProperty(PropertyName._rotationHackForSameDumbReason, Variant.From(in _rotationHackForSameDumbReason));
		info.AddProperty(PropertyName._baseParticleDensity, Variant.From(in _baseParticleDensity));
		info.AddProperty(PropertyName._spearFixedHScale, Variant.From(in _spearFixedHScale));
		info.AddProperty(PropertyName._spearAngleIntensity, Variant.From(in _spearAngleIntensity));
		info.AddProperty(PropertyName._minSpearSize, Variant.From(in _minSpearSize));
		info.AddProperty(PropertyName._maxSpearSize, Variant.From(in _maxSpearSize));
		info.AddProperty(PropertyName._minSpearTime, Variant.From(in _minSpearTime));
		info.AddProperty(PropertyName._maxSpearTime, Variant.From(in _maxSpearTime));
		info.AddProperty(PropertyName._outerMargin, Variant.From(in _outerMargin));
		info.AddProperty(PropertyName._innerMargin, Variant.From(in _innerMargin));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._isOn, Variant.From(in _isOn));
		info.AddProperty(PropertyName._curScaleX, Variant.From(in _curScaleX));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CurScaleX, out var value))
		{
			CurScaleX = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scalableLayers, out var value2))
		{
			_scalableLayers = value2.AsGodotArray<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._spears, out var value3))
		{
			_spears = value3.AsGodotArray<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._verticalShrinkingLayer, out var value4))
		{
			_verticalShrinkingLayer = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._particlesToKeepDense, out var value5))
		{
			_particlesToKeepDense = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._baseScales, out var value6))
		{
			_baseScales = value6.AsGodotArray<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._indeces, out var value7))
		{
			_indeces = value7.AsGodotArray<int>();
		}
		if (info.TryGetProperty(PropertyName._baseSpearRegionWidth, out var value8))
		{
			_baseSpearRegionWidth = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._dumbHackBecauseOfHowTexturerectsWork, out var value9))
		{
			_dumbHackBecauseOfHowTexturerectsWork = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rotationHackForSameDumbReason, out var value10))
		{
			_rotationHackForSameDumbReason = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._baseParticleDensity, out var value11))
		{
			_baseParticleDensity = value11.As<int>();
		}
		if (info.TryGetProperty(PropertyName._spearFixedHScale, out var value12))
		{
			_spearFixedHScale = value12.As<float>();
		}
		if (info.TryGetProperty(PropertyName._spearAngleIntensity, out var value13))
		{
			_spearAngleIntensity = value13.As<float>();
		}
		if (info.TryGetProperty(PropertyName._minSpearSize, out var value14))
		{
			_minSpearSize = value14.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxSpearSize, out var value15))
		{
			_maxSpearSize = value15.As<float>();
		}
		if (info.TryGetProperty(PropertyName._minSpearTime, out var value16))
		{
			_minSpearTime = value16.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxSpearTime, out var value17))
		{
			_maxSpearTime = value17.As<float>();
		}
		if (info.TryGetProperty(PropertyName._outerMargin, out var value18))
		{
			_outerMargin = value18.As<float>();
		}
		if (info.TryGetProperty(PropertyName._innerMargin, out var value19))
		{
			_innerMargin = value19.As<float>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value20))
		{
			_time = value20.As<double>();
		}
		if (info.TryGetProperty(PropertyName._isOn, out var value21))
		{
			_isOn = value21.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._curScaleX, out var value22))
		{
			_curScaleX = value22.As<float>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value23))
		{
			_tween = value23.As<Tween>();
		}
	}
}
