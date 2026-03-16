using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NFanOfKnivesVfx.cs")]
public class NFanOfKnivesVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _shiv1 = "_shiv1";

		public static readonly StringName _shiv2 = "_shiv2";

		public static readonly StringName _shiv3 = "_shiv3";

		public static readonly StringName _shiv4 = "_shiv4";

		public static readonly StringName _shiv5 = "_shiv5";

		public static readonly StringName _shiv6 = "_shiv6";

		public static readonly StringName _shiv7 = "_shiv7";

		public static readonly StringName _shiv8 = "_shiv8";

		public static readonly StringName _shiv9 = "_shiv9";

		public static readonly StringName _spawnPosition = "_spawnPosition";

		public static readonly StringName _spawnTween = "_spawnTween";

		public static readonly StringName _fanTween = "_fanTween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/fan_of_knives_vfx");

	private const string _fanOfKnivesSfx = "event:/sfx/characters/silent/silent_fan_of_knives";

	private readonly List<Node2D> _shivs = new List<Node2D>();

	private Node2D _shiv1;

	private Node2D _shiv2;

	private Node2D _shiv3;

	private Node2D _shiv4;

	private Node2D _shiv5;

	private Node2D _shiv6;

	private Node2D _shiv7;

	private Node2D _shiv8;

	private Node2D _shiv9;

	private Vector2 _spawnPosition;

	private const double _fanDuration = 0.8;

	private Tween? _spawnTween;

	private Tween? _fanTween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NFanOfKnivesVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NFanOfKnivesVfx nFanOfKnivesVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NFanOfKnivesVfx>(PackedScene.GenEditState.Disabled);
		nFanOfKnivesVfx._spawnPosition = NCombatRoom.Instance.GetCreatureNode(target).VfxSpawnPosition;
		return nFanOfKnivesVfx;
	}

	public override void _Ready()
	{
		_shiv1 = GetNode<Node2D>("ShivFanParticle1");
		_shiv2 = GetNode<Node2D>("ShivFanParticle2");
		_shiv3 = GetNode<Node2D>("ShivFanParticle3");
		_shiv4 = GetNode<Node2D>("ShivFanParticle4");
		_shiv5 = GetNode<Node2D>("ShivFanParticle5");
		_shiv6 = GetNode<Node2D>("ShivFanParticle6");
		_shiv7 = GetNode<Node2D>("ShivFanParticle7");
		_shiv8 = GetNode<Node2D>("ShivFanParticle8");
		_shiv9 = GetNode<Node2D>("ShivFanParticle9");
		_shivs.Add(_shiv1);
		_shivs.Add(_shiv2);
		_shivs.Add(_shiv3);
		_shivs.Add(_shiv4);
		_shivs.Add(_shiv5);
		_shivs.Add(_shiv6);
		_shivs.Add(_shiv7);
		_shivs.Add(_shiv8);
		_shivs.Add(_shiv9);
		foreach (Node2D shiv in _shivs)
		{
			shiv.Scale = Vector2.One * Rng.Chaotic.NextFloat(0.98f, 1.02f);
			shiv.GlobalPosition = _spawnPosition;
		}
		TaskHelper.RunSafely(Animate());
	}

	public override void _ExitTree()
	{
		_fanTween?.Kill();
		_spawnTween?.Kill();
	}

	private async Task Animate()
	{
		SfxCmd.Play("event:/sfx/characters/silent/silent_fan_of_knives");
		_spawnTween = CreateTween().SetParallel();
		foreach (Node2D shiv in _shivs)
		{
			float num = Rng.Chaotic.NextFloat(0.4f, 0.8f);
			_spawnTween.TweenProperty(shiv, "offset:y", -180f, num).From(0f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
			_spawnTween.TweenProperty(shiv, "modulate", Colors.White, num).From(StsColors.transparentBlack);
			_spawnTween.TweenProperty(shiv.GetNode<Node2D>("Shadow"), "offset:y", -180f, num).From(0f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
		}
		_spawnTween.Chain();
		foreach (Node2D shiv2 in _shivs)
		{
			_spawnTween.TweenProperty(shiv2, "modulate", StsColors.transparentWhite, 0.4).SetDelay(Rng.Chaotic.NextDouble(0.25, 0.5));
		}
		_fanTween = CreateTween().SetParallel();
		_fanTween.TweenInterval(0.4000000059604645);
		_fanTween.Chain();
		_fanTween.TweenProperty(_shiv1, "rotation", -1.74533f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv2, "rotation", -1.3089975f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv3, "rotation", -0.872665f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv4, "rotation", -0.4363325f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv6, "rotation", 0.4363325f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv7, "rotation", 0.872665f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv8, "rotation", 1.3089975f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_fanTween.TweenProperty(_shiv9, "rotation", 1.74533f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		await ToSignal(_spawnTween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._shiv1)
		{
			_shiv1 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv2)
		{
			_shiv2 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv3)
		{
			_shiv3 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv4)
		{
			_shiv4 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv5)
		{
			_shiv5 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv6)
		{
			_shiv6 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv7)
		{
			_shiv7 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv8)
		{
			_shiv8 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._shiv9)
		{
			_shiv9 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			_spawnPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._spawnTween)
		{
			_spawnTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._fanTween)
		{
			_fanTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._shiv1)
		{
			value = VariantUtils.CreateFrom(in _shiv1);
			return true;
		}
		if (name == PropertyName._shiv2)
		{
			value = VariantUtils.CreateFrom(in _shiv2);
			return true;
		}
		if (name == PropertyName._shiv3)
		{
			value = VariantUtils.CreateFrom(in _shiv3);
			return true;
		}
		if (name == PropertyName._shiv4)
		{
			value = VariantUtils.CreateFrom(in _shiv4);
			return true;
		}
		if (name == PropertyName._shiv5)
		{
			value = VariantUtils.CreateFrom(in _shiv5);
			return true;
		}
		if (name == PropertyName._shiv6)
		{
			value = VariantUtils.CreateFrom(in _shiv6);
			return true;
		}
		if (name == PropertyName._shiv7)
		{
			value = VariantUtils.CreateFrom(in _shiv7);
			return true;
		}
		if (name == PropertyName._shiv8)
		{
			value = VariantUtils.CreateFrom(in _shiv8);
			return true;
		}
		if (name == PropertyName._shiv9)
		{
			value = VariantUtils.CreateFrom(in _shiv9);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			value = VariantUtils.CreateFrom(in _spawnPosition);
			return true;
		}
		if (name == PropertyName._spawnTween)
		{
			value = VariantUtils.CreateFrom(in _spawnTween);
			return true;
		}
		if (name == PropertyName._fanTween)
		{
			value = VariantUtils.CreateFrom(in _fanTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv4, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv5, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv6, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv7, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv8, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shiv9, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._spawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spawnTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fanTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._shiv1, Variant.From(in _shiv1));
		info.AddProperty(PropertyName._shiv2, Variant.From(in _shiv2));
		info.AddProperty(PropertyName._shiv3, Variant.From(in _shiv3));
		info.AddProperty(PropertyName._shiv4, Variant.From(in _shiv4));
		info.AddProperty(PropertyName._shiv5, Variant.From(in _shiv5));
		info.AddProperty(PropertyName._shiv6, Variant.From(in _shiv6));
		info.AddProperty(PropertyName._shiv7, Variant.From(in _shiv7));
		info.AddProperty(PropertyName._shiv8, Variant.From(in _shiv8));
		info.AddProperty(PropertyName._shiv9, Variant.From(in _shiv9));
		info.AddProperty(PropertyName._spawnPosition, Variant.From(in _spawnPosition));
		info.AddProperty(PropertyName._spawnTween, Variant.From(in _spawnTween));
		info.AddProperty(PropertyName._fanTween, Variant.From(in _fanTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._shiv1, out var value))
		{
			_shiv1 = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv2, out var value2))
		{
			_shiv2 = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv3, out var value3))
		{
			_shiv3 = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv4, out var value4))
		{
			_shiv4 = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv5, out var value5))
		{
			_shiv5 = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv6, out var value6))
		{
			_shiv6 = value6.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv7, out var value7))
		{
			_shiv7 = value7.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv8, out var value8))
		{
			_shiv8 = value8.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._shiv9, out var value9))
		{
			_shiv9 = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._spawnPosition, out var value10))
		{
			_spawnPosition = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._spawnTween, out var value11))
		{
			_spawnTween = value11.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._fanTween, out var value12))
		{
			_fanTween = value12.As<Tween>();
		}
	}
}
