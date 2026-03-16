using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSpeechBubbleVfx.cs")]
public class NSpeechBubbleVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetSpeechBubbleColor = "SetSpeechBubbleColor";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName CreateInternal = "CreateInternal";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName SecondsToDisplay = "SecondsToDisplay";

		public static readonly StringName _container = "_container";

		public static readonly StringName _label = "_label";

		public static readonly StringName _contents = "_contents";

		public static readonly StringName _bubble = "_bubble";

		public static readonly StringName _shadow = "_shadow";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _startPos = "_startPos";

		public static readonly StringName _vfxColor = "_vfxColor";

		public static readonly StringName _style = "_style";

		public static readonly StringName _side = "_side";

		public static readonly StringName _text = "_text";

		public static readonly StringName _elapsedTime = "_elapsedTime";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private Control _container;

	private MegaRichTextLabel _label;

	private Node2D _contents;

	private Sprite2D _bubble;

	private Sprite2D _shadow;

	private ShaderMaterial _hsv;

	private const string _path = "res://scenes/vfx/vfx_speech_bubble.tscn";

	private Tween? _tween;

	private const float _spawnProportionToTopOfHitbox = 0.75f;

	private const float _spawnProportionToEdgeOfHitbox = 0.75f;

	private Vector2 _startPos;

	private VfxColor _vfxColor;

	private DialogueStyle _style;

	private DialogueSide _side;

	private string _text;

	private float _elapsedTime = 3.14f;

	private const float _waveFrequency = 4.5f;

	private const float _waveAmplitude = 2f;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/vfx_speech_bubble.tscn");

	public double SecondsToDisplay { get; private set; }

	public static NSpeechBubbleVfx? Create(string text, Creature speaker, double secondsToDisplay, VfxColor vfxColor = VfxColor.White)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSpeechBubbleVfx nSpeechBubbleVfx = CreateInternal(text, speaker.Side switch
		{
			CombatSide.Player => DialogueSide.Left, 
			CombatSide.Enemy => DialogueSide.Right, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, secondsToDisplay);
		nSpeechBubbleVfx._startPos = GetCreatureSpeechPosition(speaker);
		nSpeechBubbleVfx._vfxColor = vfxColor;
		return nSpeechBubbleVfx;
	}

	public static NSpeechBubbleVfx? Create(string text, DialogueSide side, Vector2 globalPosition, double secondsToDisplay, VfxColor vfxColor = VfxColor.White)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSpeechBubbleVfx nSpeechBubbleVfx = CreateInternal(text, side, secondsToDisplay);
		nSpeechBubbleVfx._startPos = globalPosition;
		nSpeechBubbleVfx._vfxColor = vfxColor;
		return nSpeechBubbleVfx;
	}

	public override void _Ready()
	{
		_contents = GetNode<Node2D>("%Contents");
		_bubble = GetNode<Sprite2D>("%Bubble");
		_shadow = GetNode<Sprite2D>("%Shadow");
		_container = GetNode<Control>("%Container");
		_label = GetNode<MegaRichTextLabel>("%Text");
		_hsv = (ShaderMaterial)_bubble.Material;
		SetSpeechBubbleColor();
		base.GlobalPosition = _startPos;
		if (_side == DialogueSide.Right)
		{
			_container.Position = new Vector2(0f - _container.Size.X - _container.Position.X, _container.Position.Y);
			_bubble.FlipH = true;
			_shadow.FlipH = true;
		}
		TaskHelper.RunSafely(AnimateSpeechBubble());
	}

	private void SetSpeechBubbleColor()
	{
		switch (_vfxColor)
		{
		case VfxColor.Blue:
			_hsv.SetShaderParameter(_h, 0.05f);
			_hsv.SetShaderParameter(_s, 1.3f);
			_hsv.SetShaderParameter(_v, 0.55f);
			break;
		case VfxColor.Green:
			_hsv.SetShaderParameter(_h, 0.8f);
			_hsv.SetShaderParameter(_s, 1.5f);
			_hsv.SetShaderParameter(_v, 0.6f);
			break;
		case VfxColor.Purple:
			_hsv.SetShaderParameter(_h, 0.3f);
			_hsv.SetShaderParameter(_s, 0.6f);
			_hsv.SetShaderParameter(_v, 0.5f);
			break;
		case VfxColor.Red:
			_hsv.SetShaderParameter(_h, 0.48f);
			_hsv.SetShaderParameter(_s, 2f);
			_hsv.SetShaderParameter(_v, 0.5f);
			break;
		case VfxColor.Black:
			_hsv.SetShaderParameter(_h, 1f);
			_hsv.SetShaderParameter(_s, 0.25f);
			_hsv.SetShaderParameter(_v, 0.3f);
			break;
		default:
			_hsv.SetShaderParameter(_h, 1f);
			_hsv.SetShaderParameter(_s, 0.9f);
			_hsv.SetShaderParameter(_v, 0.5f);
			break;
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task AnimateSpeechBubble()
	{
		_tween = CreateTween().SetParallel();
		float value = 60f * ((_side == DialogueSide.Left) ? (-1f) : 1f);
		_label.Text = $"[center][fly_in offset_x={value} offset_y=40]{_text}[/fly_in][/center]";
		_tween.TweenProperty(this, "modulate:a", 1f, 0.4).From(0f);
		_tween.TweenProperty(_label, "visible_ratio", 1f, 0.4).From(0f);
		_tween.TweenProperty(_bubble, "scale", new Vector2(0.75f, 0.75f), 0.5).From(new Vector2(0.25f, 0.25f)).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "rotation_degrees", 0f, 0.3).From(7f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Sine);
		_tween.Chain();
		double time = Math.Max(SecondsToDisplay - 1.0, 1.0);
		_tween.TweenInterval(time);
		_tween.Chain();
		await AnimOutInternal();
	}

	public async Task AnimOut()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		await AnimOutInternal();
	}

	private async Task AnimOutInternal()
	{
		_tween.TweenProperty(this, "modulate", StsColors.transparentBlack, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	private static NSpeechBubbleVfx CreateInternal(string text, DialogueSide side, double secondsToDisplay)
	{
		NSpeechBubbleVfx nSpeechBubbleVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/vfx_speech_bubble.tscn").Instantiate<NSpeechBubbleVfx>(PackedScene.GenEditState.Disabled);
		nSpeechBubbleVfx._side = side;
		nSpeechBubbleVfx._text = text;
		nSpeechBubbleVfx.SecondsToDisplay = secondsToDisplay;
		return nSpeechBubbleVfx;
	}

	public override void _Process(double delta)
	{
		_elapsedTime += (float)delta * 4.5f;
		_contents.Position = new Vector2(0f, Mathf.Sin(_elapsedTime) * 2f);
	}

	private static Vector2 GetCreatureSpeechPosition(Creature speaker)
	{
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(speaker);
		if (creatureNode.Visuals.TalkPosition != null)
		{
			return creatureNode.Visuals.TalkPosition.GlobalPosition;
		}
		Vector2 result = creatureNode.VfxSpawnPosition + new Vector2(0f, (0f - creatureNode.Hitbox.Size.Y) * 0.5f * 0.75f);
		if (speaker.Side == CombatSide.Player)
		{
			result.X += creatureNode.Hitbox.Size.X * 0.75f;
		}
		else
		{
			result.X -= creatureNode.Hitbox.Size.X * 0.75f;
		}
		return result;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "side", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "globalPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "secondsToDisplay", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "vfxColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSpeechBubbleColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateInternal, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "side", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "secondsToDisplay", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 5)
		{
			ret = VariantUtils.CreateFrom<NSpeechBubbleVfx>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<DialogueSide>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<double>(in args[3]), VariantUtils.ConvertTo<VfxColor>(in args[4])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSpeechBubbleColor && args.Count == 0)
		{
			SetSpeechBubbleColor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateInternal && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NSpeechBubbleVfx>(CreateInternal(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<DialogueSide>(in args[1]), VariantUtils.ConvertTo<double>(in args[2])));
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 5)
		{
			ret = VariantUtils.CreateFrom<NSpeechBubbleVfx>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<DialogueSide>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]), VariantUtils.ConvertTo<double>(in args[3]), VariantUtils.ConvertTo<VfxColor>(in args[4])));
			return true;
		}
		if (method == MethodName.CreateInternal && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NSpeechBubbleVfx>(CreateInternal(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<DialogueSide>(in args[1]), VariantUtils.ConvertTo<double>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetSpeechBubbleColor)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.CreateInternal)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.SecondsToDisplay)
		{
			SecondsToDisplay = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._contents)
		{
			_contents = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._bubble)
		{
			_bubble = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			_shadow = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._startPos)
		{
			_startPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			_vfxColor = VariantUtils.ConvertTo<VfxColor>(in value);
			return true;
		}
		if (name == PropertyName._style)
		{
			_style = VariantUtils.ConvertTo<DialogueStyle>(in value);
			return true;
		}
		if (name == PropertyName._side)
		{
			_side = VariantUtils.ConvertTo<DialogueSide>(in value);
			return true;
		}
		if (name == PropertyName._text)
		{
			_text = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			_elapsedTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SecondsToDisplay)
		{
			value = VariantUtils.CreateFrom<double>(SecondsToDisplay);
			return true;
		}
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._contents)
		{
			value = VariantUtils.CreateFrom(in _contents);
			return true;
		}
		if (name == PropertyName._bubble)
		{
			value = VariantUtils.CreateFrom(in _bubble);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			value = VariantUtils.CreateFrom(in _shadow);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._startPos)
		{
			value = VariantUtils.CreateFrom(in _startPos);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			value = VariantUtils.CreateFrom(in _vfxColor);
			return true;
		}
		if (name == PropertyName._style)
		{
			value = VariantUtils.CreateFrom(in _style);
			return true;
		}
		if (name == PropertyName._side)
		{
			value = VariantUtils.CreateFrom(in _side);
			return true;
		}
		if (name == PropertyName._text)
		{
			value = VariantUtils.CreateFrom(in _text);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			value = VariantUtils.CreateFrom(in _elapsedTime);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._contents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bubble, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._style, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._side, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.SecondsToDisplay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.SecondsToDisplay, Variant.From<double>(SecondsToDisplay));
		info.AddProperty(PropertyName._container, Variant.From(in _container));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._contents, Variant.From(in _contents));
		info.AddProperty(PropertyName._bubble, Variant.From(in _bubble));
		info.AddProperty(PropertyName._shadow, Variant.From(in _shadow));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._startPos, Variant.From(in _startPos));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
		info.AddProperty(PropertyName._style, Variant.From(in _style));
		info.AddProperty(PropertyName._side, Variant.From(in _side));
		info.AddProperty(PropertyName._text, Variant.From(in _text));
		info.AddProperty(PropertyName._elapsedTime, Variant.From(in _elapsedTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.SecondsToDisplay, out var value))
		{
			SecondsToDisplay = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._container, out var value2))
		{
			_container = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._contents, out var value4))
		{
			_contents = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._bubble, out var value5))
		{
			_bubble = value5.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._shadow, out var value6))
		{
			_shadow = value6.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value8))
		{
			_tween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._startPos, out var value9))
		{
			_startPos = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value10))
		{
			_vfxColor = value10.As<VfxColor>();
		}
		if (info.TryGetProperty(PropertyName._style, out var value11))
		{
			_style = value11.As<DialogueStyle>();
		}
		if (info.TryGetProperty(PropertyName._side, out var value12))
		{
			_side = value12.As<DialogueSide>();
		}
		if (info.TryGetProperty(PropertyName._text, out var value13))
		{
			_text = value13.As<string>();
		}
		if (info.TryGetProperty(PropertyName._elapsedTime, out var value14))
		{
			_elapsedTime = value14.As<float>();
		}
	}
}
