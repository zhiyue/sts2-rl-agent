using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCreatureVisuals.cs")]
public class NCreatureVisuals : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetScaleAndHue = "SetScaleAndHue";

		public static readonly StringName IsPlayingHurtAnimation = "IsPlayingHurtAnimation";

		public static readonly StringName TryApplyLiquidOverlay = "TryApplyLiquidOverlay";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName Body = "Body";

		public static readonly StringName Bounds = "Bounds";

		public static readonly StringName IntentPosition = "IntentPosition";

		public static readonly StringName OrbPosition = "OrbPosition";

		public static readonly StringName TalkPosition = "TalkPosition";

		public static readonly StringName HasSpineAnimation = "HasSpineAnimation";

		public static readonly StringName VfxSpawnPosition = "VfxSpawnPosition";

		public static readonly StringName DefaultScale = "DefaultScale";

		public static readonly StringName _hue = "_hue";

		public static readonly StringName _liquidOverlayTimer = "_liquidOverlayTimer";

		public static readonly StringName _savedNormalMaterial = "_savedNormalMaterial";

		public static readonly StringName _currentLiquidOverlayMaterial = "_currentLiquidOverlayMaterial";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _overlayInfluence = new StringName("overlay_influence");

	private static readonly StringName _h = new StringName("h");

	private static readonly StringName _tint = new StringName("tint");

	private const double _baseLiquidOverlayDuration = 1.0;

	private float _hue = 1f;

	private double _liquidOverlayTimer;

	private Material? _savedNormalMaterial;

	private ShaderMaterial? _currentLiquidOverlayMaterial;

	public Node2D Body { get; private set; }

	public Control Bounds { get; private set; }

	public Marker2D IntentPosition { get; private set; }

	public Marker2D OrbPosition { get; private set; }

	public Marker2D? TalkPosition { get; private set; }

	public bool HasSpineAnimation
	{
		get
		{
			if (GodotObject.IsInstanceValid(Body))
			{
				return Body.GetClass() == "SpineSprite";
			}
			return false;
		}
	}

	public MegaSprite? SpineBody { get; private set; }

	public Marker2D VfxSpawnPosition { get; private set; }

	public float DefaultScale { get; set; } = 1f;

	public override void _Ready()
	{
		Body = GetNode<Node2D>("%Visuals");
		Bounds = GetNode<Control>("%Bounds");
		IntentPosition = GetNode<Marker2D>("%IntentPos");
		VfxSpawnPosition = GetNode<Marker2D>("%CenterPos");
		OrbPosition = (HasNode("%OrbPos") ? GetNode<Marker2D>("%OrbPos") : IntentPosition);
		TalkPosition = (HasNode("%TalkPos") ? GetNode<Marker2D>("%TalkPos") : null);
		if (HasSpineAnimation)
		{
			SpineBody = new MegaSprite(Body);
		}
		_savedNormalMaterial = null;
		_currentLiquidOverlayMaterial = null;
	}

	public void SetUpSkin(MonsterModel model)
	{
		if (SpineBody?.GetSkeleton() != null)
		{
			model.SetupSkins(this);
		}
	}

	public void SetScaleAndHue(float scale, float hue)
	{
		DefaultScale = scale;
		base.Scale = Vector2.One * scale;
		_hue = hue;
		if (!Mathf.IsEqualApprox(hue, 0f) && SpineBody != null)
		{
			Material normalMaterial = SpineBody.GetNormalMaterial();
			ShaderMaterial shaderMaterial;
			if (normalMaterial == null)
			{
				Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/hsv.tres");
				shaderMaterial = (ShaderMaterial)material.Duplicate();
				SpineBody.SetNormalMaterial(shaderMaterial);
			}
			else
			{
				shaderMaterial = (ShaderMaterial)normalMaterial;
			}
			shaderMaterial.SetShaderParameter(_h, hue);
		}
	}

	public bool IsPlayingHurtAnimation()
	{
		if (SpineBody?.GetSkeleton() != null)
		{
			return SpineBody.GetAnimationState().GetCurrent(0).GetAnimation()
				.GetName()
				.Equals("hurt");
		}
		return false;
	}

	public void TryApplyLiquidOverlay(Color tint)
	{
		if (_currentLiquidOverlayMaterial != null)
		{
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_liquidOverlayTimer = 1.0;
		}
		else
		{
			TaskHelper.RunSafely(ApplyLiquidOverlayInternal(tint));
		}
	}

	private async Task ApplyLiquidOverlayInternal(Color tint)
	{
		if (SpineBody != null)
		{
			_savedNormalMaterial = SpineBody.GetNormalMaterial();
			Material material = (ShaderMaterial)PreloadManager.Cache.GetMaterial("res://materials/vfx/potion/potion_liquid_overlay.tres");
			_currentLiquidOverlayMaterial = (ShaderMaterial)material.Duplicate();
			_currentLiquidOverlayMaterial.SetShaderParameter(_tint, tint);
			_currentLiquidOverlayMaterial.SetShaderParameter(_h, _hue);
			_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1f);
			SpineBody.SetNormalMaterial(_currentLiquidOverlayMaterial);
			_liquidOverlayTimer = 1.0;
			while (_liquidOverlayTimer > 0.0)
			{
				double num = (1.0 - _liquidOverlayTimer) / 1.0;
				_currentLiquidOverlayMaterial.SetShaderParameter(_overlayInfluence, 1.0 - num);
				_liquidOverlayTimer -= GetProcessDeltaTime();
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
			SpineBody.SetNormalMaterial(_savedNormalMaterial);
			_currentLiquidOverlayMaterial = null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetScaleAndHue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "hue", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsPlayingHurtAnimation, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryApplyLiquidOverlay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetScaleAndHue && args.Count == 2)
		{
			SetScaleAndHue(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsPlayingHurtAnimation && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsPlayingHurtAnimation());
			return true;
		}
		if (method == MethodName.TryApplyLiquidOverlay && args.Count == 1)
		{
			TryApplyLiquidOverlay(VariantUtils.ConvertTo<Color>(in args[0]));
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
		if (method == MethodName.SetScaleAndHue)
		{
			return true;
		}
		if (method == MethodName.IsPlayingHurtAnimation)
		{
			return true;
		}
		if (method == MethodName.TryApplyLiquidOverlay)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Body)
		{
			Body = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName.Bounds)
		{
			Bounds = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.IntentPosition)
		{
			IntentPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.OrbPosition)
		{
			OrbPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.TalkPosition)
		{
			TalkPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.VfxSpawnPosition)
		{
			VfxSpawnPosition = VariantUtils.ConvertTo<Marker2D>(in value);
			return true;
		}
		if (name == PropertyName.DefaultScale)
		{
			DefaultScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._hue)
		{
			_hue = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._liquidOverlayTimer)
		{
			_liquidOverlayTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._savedNormalMaterial)
		{
			_savedNormalMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._currentLiquidOverlayMaterial)
		{
			_currentLiquidOverlayMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Body)
		{
			value = VariantUtils.CreateFrom<Node2D>(Body);
			return true;
		}
		if (name == PropertyName.Bounds)
		{
			value = VariantUtils.CreateFrom<Control>(Bounds);
			return true;
		}
		Marker2D from;
		if (name == PropertyName.IntentPosition)
		{
			from = IntentPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.OrbPosition)
		{
			from = OrbPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.TalkPosition)
		{
			from = TalkPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasSpineAnimation)
		{
			value = VariantUtils.CreateFrom<bool>(HasSpineAnimation);
			return true;
		}
		if (name == PropertyName.VfxSpawnPosition)
		{
			from = VfxSpawnPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DefaultScale)
		{
			value = VariantUtils.CreateFrom<float>(DefaultScale);
			return true;
		}
		if (name == PropertyName._hue)
		{
			value = VariantUtils.CreateFrom(in _hue);
			return true;
		}
		if (name == PropertyName._liquidOverlayTimer)
		{
			value = VariantUtils.CreateFrom(in _liquidOverlayTimer);
			return true;
		}
		if (name == PropertyName._savedNormalMaterial)
		{
			value = VariantUtils.CreateFrom(in _savedNormalMaterial);
			return true;
		}
		if (name == PropertyName._currentLiquidOverlayMaterial)
		{
			value = VariantUtils.CreateFrom(in _currentLiquidOverlayMaterial);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Bounds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.IntentPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.OrbPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TalkPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasSpineAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.DefaultScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._hue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._liquidOverlayTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._savedNormalMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentLiquidOverlayMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Body, Variant.From<Node2D>(Body));
		info.AddProperty(PropertyName.Bounds, Variant.From<Control>(Bounds));
		info.AddProperty(PropertyName.IntentPosition, Variant.From<Marker2D>(IntentPosition));
		info.AddProperty(PropertyName.OrbPosition, Variant.From<Marker2D>(OrbPosition));
		info.AddProperty(PropertyName.TalkPosition, Variant.From<Marker2D>(TalkPosition));
		info.AddProperty(PropertyName.VfxSpawnPosition, Variant.From<Marker2D>(VfxSpawnPosition));
		info.AddProperty(PropertyName.DefaultScale, Variant.From<float>(DefaultScale));
		info.AddProperty(PropertyName._hue, Variant.From(in _hue));
		info.AddProperty(PropertyName._liquidOverlayTimer, Variant.From(in _liquidOverlayTimer));
		info.AddProperty(PropertyName._savedNormalMaterial, Variant.From(in _savedNormalMaterial));
		info.AddProperty(PropertyName._currentLiquidOverlayMaterial, Variant.From(in _currentLiquidOverlayMaterial));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Body, out var value))
		{
			Body = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName.Bounds, out var value2))
		{
			Bounds = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.IntentPosition, out var value3))
		{
			IntentPosition = value3.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.OrbPosition, out var value4))
		{
			OrbPosition = value4.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.TalkPosition, out var value5))
		{
			TalkPosition = value5.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.VfxSpawnPosition, out var value6))
		{
			VfxSpawnPosition = value6.As<Marker2D>();
		}
		if (info.TryGetProperty(PropertyName.DefaultScale, out var value7))
		{
			DefaultScale = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._hue, out var value8))
		{
			_hue = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._liquidOverlayTimer, out var value9))
		{
			_liquidOverlayTimer = value9.As<double>();
		}
		if (info.TryGetProperty(PropertyName._savedNormalMaterial, out var value10))
		{
			_savedNormalMaterial = value10.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._currentLiquidOverlayMaterial, out var value11))
		{
			_currentLiquidOverlayMaterial = value11.As<ShaderMaterial>();
		}
	}
}
